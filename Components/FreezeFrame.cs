using AliLib.Core;
using AliLib.Core.Assets;
using AliLib.Core.Events;
using ProjectionSorcery.Components.FreezeManagers;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Components;

[AddressableComponent("ProjectionSorcery.FreezeFrame")]
public class FreezeFrame : MonoBehaviour
{
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

    /// <summary> The object that is frozen. </summary>
    public GameObject? Target
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.impulse.magnitude > 5 && IsBreakable)
            ForceBreak(collision.impulse);
    }

    public void ForceBreak(Vector3 impulse = default)
    {
        if (BreakSound != null)
            Audio.PlayNoBlend(BreakSound, ThunderRoad.AudioMixerName.Effect);

        if (Target != null)
            UnFreeze(Target);

        OnFrameBroken.Invoke();

        Destroy(gameObject);
    }

    public void Freeze(GameObject target)
    {
        Debug.Log("Freezing " + target.name);

        if (target.GetComponent<Item>())
            target.AddComponent<ItemFreezeManager>().Freeze();

        foreach (Collider col in target.GetComponentsInChildren<Collider>())
            col.enabled = false;

        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;

        Quaternion flatRotation = Quaternion.LookRotation(-forward);

        target.transform.rotation = flatRotation;
        target.transform.localScale = new Vector3(target.transform.localScale.x, target.transform.localScale.y, 0.01f);

        transform.position = target.transform.position;
        transform.rotation = Quaternion.LookRotation(-forward) * Quaternion.Euler(0, -90, 0);
    }

    public void UnFreeze(GameObject target)
    {
        Debug.Log("Unfreezing " + target.name);
        if (target.GetComponent<Item>())
            target.GetComponent<ItemFreezeManager>().UnFreeze();

        foreach (Collider col in target.GetComponentsInChildren<Collider>())
            col.enabled = true;

        target.transform.localScale = new Vector3(target.transform.localScale.x, target.transform.localScale.y, 1f);
    }
}
