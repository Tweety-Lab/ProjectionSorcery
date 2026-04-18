using AliLib.Core;
using AliLib.Core.Abilities;
using AliLib.Core.Assets;
using AliLib.Core.GC;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectionSorcery.Abilities.Plotting;

public class ProjectionOutlineAbility : Ability
{
    public enum OutlineType
    {
        Outlined,
        Solid
    }

    [ModOption]
    [ModOptionCategory("Outline", 1)]
    public static bool Outlines = true;

    [ModOption]
    [ModOptionCategory("Outline", 1)]
    public static OutlineType OutlineStyle = OutlineType.Outlined;

    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Outline", 1)]
    [ModOptionFloatValues(0f, 1f, 0.05f)]
    public static float OutlineColorR = 1f;

    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Outline", 1)]
    [ModOptionFloatValues(0f, 1f, 0.05f)]
    public static float OutlineColorG = 1f;

    [ModOption(interactionType = ModOption.InteractionType.Slider)]
    [ModOptionCategory("Outline", 1)]
    [ModOptionFloatValues(0f, 1f, 0.05f)]
    public static float OutlineColorB = 1f;

    [ModOption]
    [ModOptionCategory("Outline", 1)]
    public static bool ChromaticColor = false;

    /// <summary> The current chromatic hue offset if <see cref="ChromaticColor"/> is enabled. </summary>
    public static float ChromaticHueOffset {  get; set; } = 0;

    /// <summary> The color to tint the screen when in Hack Mode. </summary>
    public static Color OutlineColor
    {
        get
        {
            if (!ChromaticColor)
                return new Color(OutlineColorR, OutlineColorG, OutlineColorB);
            
            ChromaticHueOffset = (ChromaticHueOffset + 0.1f) % 6;
            return Color.HSVToRGB(ChromaticHueOffset / 6f, 0.4f, 1f);
        }
    }

    [Addressable("ProjectionSorcery.Outline")]
    public static Shader? OutlineShader { get; set; }

    /// <inheritdoc/>
    public ProjectionOutlineAbility(AbilitySpell spell) : base(spell) { }

    private int currentFrame = 0;

    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.GetAbility<ProjectionAbility>()?.OnPlottingTick += OnPlottingTick;
        Spell.GetAbility<ProjectionAbility>()?.OnEnterPlotting += () => currentFrame = 0;
    }

    public void OnPlottingTick(PlottedFrame plottedFrame)
    {
        if (OutlineShader == null || !Outlines)
            return;

        // IK Solves at the end of the frame
        CoroutineRunner.Instance.PlayAfterYield(() => SpawnOutline(Spell.spellCaster.ragdollHand.creature, currentFrame++ * (1f / ProjectionAbility.FramesPerSecond)), new WaitForEndOfFrame());
    }

    /// <summary> Spawns a plotting outline for a creautre. </summary>
    public GameObject? SpawnOutline(Creature creature, float disposalDelay = 10f, Material? material = null)
    {
        if (OutlineShader == null)
            return null;

        GameObject outline = new GameObject("Outline");

        material ??= new Material(OutlineShader); // This gets disposed by the SmartObject<Mesh>
        if (OutlineStyle == OutlineType.Outlined)
            material.SetFloat("_Power", 4f);
        else
            material.SetFloat("_Power", 0.1f);

        material.color = OutlineColor * 2f;

        foreach(SkinnedMeshRenderer renderer in creature.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            SmartObject<Mesh> bakedMesh = new Mesh();

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
            mr.sharedMaterials = new Material[] { material };

            // When the Garbage Collector wants to dispose the mesh we politely ask it to delay disposal until we reach the relative frame
            bakedMesh.OnDisposed.AddFirst(() =>
            {
                bakedMesh.OnDisposed.Cancelled = true;

                CoroutineRunner.Instance.PlayAfterDelay(() =>
                {
                    bakedMesh.ForceDispose();

                    Object.Destroy(meshObject);
                    Object.Destroy(material);
                    Object.Destroy(outline);

                }, disposalDelay);
            });
        }

        return outline;
    }
}
