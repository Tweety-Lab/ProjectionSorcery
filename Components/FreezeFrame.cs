using AliLib.Core;
using AliLib.Core.Assets;
using AliLib.Core.Events;
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
        if (collision.impulse.magnitude > 5)
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
    }

    public void UnFreeze(GameObject target)
    {
        Debug.Log("Unfreezing " + target.name);
    }
}
