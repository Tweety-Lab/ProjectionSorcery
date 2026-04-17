using AliLib.Core;
using AliLib.Core.Abilities;
using AliLib.Core.Assets;
using AliLib.Core.GC;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectionSorcery.Abilities;

public class ProjectionPlottingOutlineAbility : Ability
{
    [Addressable("ProjectionSorcery.Outline")]
    public static Shader? OutlineShader { get; set; }

    /// <inheritdoc/>
    public ProjectionPlottingOutlineAbility(AbilitySpell spell) : base(spell) { }


    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.GetAbility<ProjectionPlottingAbility>()?.OnPlottingTick += OnPlottingTick;
    }

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
