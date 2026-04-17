using AliLib.Core;
using AliLib.Core.Abilities;
using AliLib.Core.Assets;
using AliLib.Core.GC;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace ProjectionSorcery.Abilities;

public class ProjectionPlottingEffectsAbility : Ability
{
    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Effects", 2)]
    [ModOptionFloatValues(0f, 1f, 0.05f)]
    public static float PlottingColorR = 0.7f;

    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Effects", 2)]
    [ModOptionFloatValues(0f, 1f, 0.05f)]
    public static float PlottingColorG = 0.7f;

    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Effects", 2)]
    [ModOptionFloatValues(0f, 1f, 0.05f)]
    public static float PlottingColorB = 0.7f;

    /// <summary> The color to tint the screen when in Hack Mode. </summary>
    public static Color PlottingModeColor => new Color(PlottingColorR, PlottingColorG, PlottingColorB);

    [Addressable("ProjectionSorcery.Outline")]
    public static Shader? OutlineShader { get; set; }

    /// <summary> The Post Process Volume effects are applied to. </summary>
    public Volume PostProcessVolume { get; set; }

    /// <inheritdoc/>
    public ProjectionPlottingEffectsAbility(AbilitySpell spell) : base(spell) { }

    private Coroutine? plotTintCoroutine;
    private Coroutine? plotTintUndoCoroutine;

    private int outlineCount = 0;

    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.GetAbility<ProjectionPlottingAbility>()?.OnEnterPlotting += OnEnterPlotting;
        Spell.GetAbility<ProjectionPlottingAbility>()?.OnExitPlotting += OnExitPlotting;

        Spell.GetAbility<ProjectionPlottingAbility>()?.OnPlottingTick += OnPlottingTick;
    }

    /// <inheritdoc/>
    public override void OnEquip()
    {
        base.OnEquip();

        PostProcessVolume = GameObject.FindAnyObjectByType<Volume>();
    }

    public void OnEnterPlotting() => TintScreen();
    public void OnExitPlotting() => UnTintScreen();

    public void OnPlottingTick(PlottedFrame plottedFrame)
    {
        if (OutlineShader == null)
            return;

        // IK Solves at the end of the frame
        CoroutineRunner.Instance.PlayAfterYield(() =>
        {
            SpawnOutline(Spell.spellCaster.ragdollHand.creature);
        }, new WaitForEndOfFrame());
    }

    /// <summary> Tints the screen to <see cref="PlottingModeColor"/>. </summary>
    public void TintScreen()
    {
        if (!PostProcessVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
            colorAdjustments = PostProcessVolume.profile.Add<ColorAdjustments>(true);

        colorAdjustments.colorFilter.overrideState = true;

        if (plotTintUndoCoroutine != null)
            CoroutineRunner.Instance.StopCoroutine(plotTintUndoCoroutine);

        plotTintCoroutine = CoroutineRunner.Instance.PlaySmooth(t => colorAdjustments.colorFilter.Override(Color.Lerp(Color.white, PlottingModeColor, t)), duration: 0.5f);
    }

    /// <summary> Untints the screen. </summary>
    public void UnTintScreen()
    {
        if (PostProcessVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            if (plotTintCoroutine != null)
                CoroutineRunner.Instance.StopCoroutine(plotTintCoroutine);

            Color from = colorAdjustments.colorFilter.value;
            plotTintUndoCoroutine = CoroutineRunner.Instance.PlaySmooth(t => colorAdjustments.colorFilter.Override(Color.Lerp(from, Color.white, t)), duration: 0.5f);
        }
    }

    /// <summary> Spawns a plotting outline for a creautre. </summary>
    public GameObject? SpawnOutline(Creature creature)
    {
        if (OutlineShader == null)
            return null;

        GameObject outline = new GameObject("Outline");
        Material material = new Material(OutlineShader); // This gets disposed by the SmartObject<Mesh>

        material.color = Color.white * 2f;
        material.SetInt("_Power", 3);

        foreach(SkinnedMeshRenderer renderer in creature.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            SmartObject<Mesh> bakedMesh = new Mesh();

            // When the Garbage Collector wants to dispose the mesh we politely ask it to delay disposal until we reach the relative frame
            bakedMesh.OnDisposed.AddFirst(() =>
            {
                bakedMesh.OnDisposed.Cancelled = true;

                CoroutineRunner.Instance.PlayAfterYield(() =>
                {
                    bakedMesh.ForceDispose();

                    Object.Destroy(material);
                    Object.Destroy(outline);

                }, new WaitForSeconds(0.1f));
            });

            renderer.BakeMesh(bakedMesh, true);

            GameObject meshObject = new GameObject(renderer.name);
            meshObject.transform.SetParent(outline.transform, false);

            meshObject.transform.position = renderer.transform.position;
            meshObject.transform.rotation = renderer.transform.rotation;
            meshObject.transform.localScale = renderer.transform.lossyScale;

            MeshFilter mf = meshObject.AddComponent<MeshFilter>();
            mf.mesh = bakedMesh;

            MeshRenderer mr = meshObject.AddComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.materials = new Material[] { material };
        }

        return outline;
    }
}
