using AliLib.Core;
using AliLib.Core.Assets;
using AliLib.Core.Events;
using ProjectionSorcery.Components.FreezeManagers;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Components;

[AddressableComponent("ProjectionSorcery.FreezeFrame")]
public class FreezeFrame : MonoBehaviour
{
    [ModOption]
    [ModOptionCategory("Freeze Frames", 0)]
    [ModOptionFloatValues(0.5f, 10f, 0.5f)]
    public static float MinimumBreakForce = 3f;

    [Addressable("ProjectionSorcery.Sounds.GlassSpawn")]
    public static AudioClip? SpawnSound { get; set; }

    [Addressable("ProjectionSorcery.Sounds.GlassShatter")]
    public static AudioClip? BreakSound { get; set; }

    /// <summary> Called when an object is frozen. </summary>
    public ModEvent OnObjectFrozen { get; set; } = new ModEvent();

    /// <summary> Called when a frame is broken. </summary>
    public ModEvent OnFrameBroken { get; set; } = new ModEvent();

    /// <summary> Is the frame currently breakable? </summary>
    public bool IsBreakable { get; set; } = true;

    /// <summary> The creature that is frozen. </summary>
    public Creature? Target
    {
        get => field;
        set
        {
            field = value;
            if (field == null)
                return;

            Freeze(field);

            OnObjectFrozen.Invoke();
        }
    }

    void Start()
    {
        if (SpawnSound == null)
            return;

        Audio.PlayNoBlend(SpawnSound, ThunderRoad.AudioMixerName.Effect);
    }

    void OnCollisionEnter(Collision collision) => TryBreak(collision.impulse);
    
    /// <summary> Tries to break the frame. </summary>
    /// <param name="impulse"> The impulse. </param>
    /// <returns> True if the frame was broken. </returns>
    public bool TryBreak(Vector3 impulse = default)
    {
        if (impulse.magnitude > MinimumBreakForce && IsBreakable)
        {
            ForceBreak(impulse);
            return true;
        }
        
        return false;
    }

    /// <summary> Forces the frame to break. <see cref="IsBreakable"/>. </summary>
    public void ForceBreak(Vector3 impulse = default)
    {
        if (BreakSound != null)
            Audio.PlayNoBlend(BreakSound, ThunderRoad.AudioMixerName.Effect);

        if (Target != null)
            UnFreeze(Target);

        OnFrameBroken.Invoke();

        Destroy(gameObject);
    }

    public void Freeze(Creature target)
    {
        CreatureFreezeManager manager = target.GetOrAddComponent<CreatureFreezeManager>();
        manager.ShouldUnfreeze = false;

        if (!manager.IsFrozen)
            manager.Freeze();

        GameObject flat = CreateFlatFrame(target);

        target.renderers.ForEach(r => r.renderer.enabled = false);

        target.ragdoll.parts.ForEach(part => part.handles.ForEach(handle => handle.enabled = false));

        target.ragdoll.SetColliders(false);
        target.ragdoll.ForBothHands(hand => hand.grabbedHandle?.item.colliderGroups.ForEach(col => col.colliders.ForEach(col => col.enabled = false)));
        target.ragdoll.ForBothHands(hand => hand.grabbedHandle?.item.renderers.ForEach(r => r.enabled = false));

        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;

        Bounds totalBounds = target.Bounds;

        var heldItems = new[] { target.handLeft, target.handRight }.Where(hand => hand?.grabbedHandle?.item != null).Select(hand => hand.grabbedHandle.item);

        foreach (var item in heldItems)
            foreach (var r in item.renderers)
                totalBounds.Encapsulate(r.bounds);

        transform.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 90, 0);
        transform.position = new Vector3(totalBounds.center.x, totalBounds.center.y, totalBounds.center.z);
        transform.localScale = new Vector3(0.03f, totalBounds.size.y, totalBounds.size.x);

        flat.transform.position = transform.position + Vector3.up * 0.25f;
    }

    public void UnFreeze(Creature target)
    {
        target.renderers.ForEach(r => r.renderer.enabled = true);

        target.ragdoll.parts.ForEach(part => part.handles.ForEach(handle => handle.enabled = true));

        target.ragdoll.SetColliders(true);
        target.ragdoll.ForBothHands(hand => hand.grabbedHandle?.item.colliderGroups.ForEach(col => col.colliders.ForEach(col => col.enabled = true)));
        target.ragdoll.ForBothHands(hand => hand.grabbedHandle?.item.renderers.ForEach(r => r.enabled = true));

        CreatureFreezeManager manager = target.GetComponent<CreatureFreezeManager>();
        manager.UnFreeze();
    }

    /// <summary> Creates a flat freeze frame for a target. </summary>
    /// <param name="target"> The target. </param>
    /// <returns> The root of the frame. </returns>
    public GameObject CreateFlatFrame(Creature target, bool disposeOnBreak = true)
    {
        GameObject root = new GameObject("FlatFrame");

        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        root.transform.rotation = Quaternion.LookRotation(forward);
        root.transform.position = target.Center;

        void AddToFrame(Mesh mesh, Material[] materials, Transform source)
        {
            GameObject meshObj = new GameObject();
            meshObj.transform.SetParent(root.transform, true);
            meshObj.transform.SetPositionAndRotation(source.position, source.rotation);
            meshObj.transform.localScale = source.lossyScale;
            meshObj.AddComponent<MeshFilter>().mesh = mesh;
            meshObj.AddComponent<MeshRenderer>().sharedMaterials = materials;
        }

        var itemRenderers = new[] { target.handLeft, target.handRight }.Where(hand => hand?.grabbedHandle?.item != null).SelectMany(hand => hand.grabbedHandle.item.renderers);

        foreach (MeshRenderer mr in itemRenderers)
        {
            MeshFilter mf = mr.GetComponent<MeshFilter>();
            if (mf?.sharedMesh == null)
                continue;

            AddToFrame(mf.sharedMesh, mr.sharedMaterials, mr.transform);
        }

        foreach (SkinnedMeshRenderer smr in target.renderers.Select(r => r.renderer))
        {
            Mesh baked = new Mesh();
            smr.BakeMesh(baked, true);
            AddToFrame(baked, smr.sharedMaterials, smr.transform);

            if (disposeOnBreak)
                OnFrameBroken += () => Destroy(baked);
        }

        if (disposeOnBreak)
            OnFrameBroken += () => Destroy(root);


        root.transform.localScale = new Vector3(1f, 1f, 0.02f);

        return root;
    }
}
