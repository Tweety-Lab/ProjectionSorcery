using AliLib.Core;
using AliLib.Core.Abilities;
using AliLib.Core.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Abilities;

/// <summary>
/// Handles the core logic of the plotting mode + playback.
/// </summary>
public class ProjectionPlottingAbility : Ability
{
    [ModOption]
    [ModOptionCategory("Projection Sorcery", 1)]
    [ModOptionFloatValues(0.1f, 1f, 0.1f)]
    public static float PlottingInterval = 0.1f;

    [ModOption]
    [ModOptionCategory("Projection Sorcery", 1)]
    [ModOptionIntValues(4, 200, 1)]
    public static int FramesPerSecond = 24;

    /// <summary> Called when the casting creature has entered plotting mode. </summary>
    public ModEvent OnEnterPlotting { get; set; } = new ModEvent();

    /// <summary> Called when the casting creature has exited plotting mode. </summary>
    public ModEvent OnExitPlotting { get; set; } = new ModEvent();

    /// <summary> Called when the casting creature has entered a plotted frame during playback. </summary>
    public ModEvent<PlottedFrame> OnPlaybackTick { get; set; } = new ModEvent<PlottedFrame>();

    /// <summary> Called when the casting creature has plotted a new frame. </summary>
    public ModEvent<PlottedFrame> OnPlottingTick { get; set; } = new ModEvent<PlottedFrame>();

    /// <summary> All currently plotted frames. </summary>
    public LinkedList<PlottedFrame> PlottedFrames { get; set; } = new LinkedList<PlottedFrame>(); // Linked List is slightly more perfomant 

    /// <inheritdoc/>
    public ProjectionPlottingAbility(AbilitySpell spell) : base(spell) { }

    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.OnStartCast += OnStartCast;
    }

    public void OnStartCast()
    {
        Coroutine plotting = CoroutineRunner.Instance.StartCoroutine(PlottingTick());

        // Since we only need to run this logic if StartCast has already fired we can subscribe here
        void StopCast()
        {
            CoroutineRunner.Instance.StopCoroutine(plotting);
            StartPlayback();

            OnExitPlotting.Invoke();
        }

        Spell.OnStopCast -= StopCast;
        Spell.OnStopCast += StopCast;

        OnEnterPlotting.Invoke();
    }

    /// <summary> Starts sending the caster through all plotted frames. </summary>
    public void StartPlayback() => CoroutineRunner.Instance.StartCoroutine(PlaybackTick());

    /// <summary> Called once per frame playback. </summary>
    public IEnumerator PlaybackTick()
    {
        PlottedFrame[] frames = PlottedFrames.ToArray();
        foreach (PlottedFrame frame in frames)
        {
            if (Spell.spellCaster.ragdollHand.creature.isPlayer)
                Spell.spellCaster.ragdollHand.creature.player.Teleport(frame.RootPosition, frame.RootRotation);
            else
                Spell.spellCaster.ragdollHand.creature.Teleport(frame.RootPosition, frame.RootRotation);

            OnPlaybackTick.Invoke(frame);

            PlottedFrames.Remove(frame);
            yield return new WaitForSeconds(1f / FramesPerSecond);
        }
    }

    /// <summary> Called once per frame plot. </summary>
    public IEnumerator PlottingTick()
    {
        while (PlottedFrames.Count < FramesPerSecond)
        {
            yield return new WaitForSeconds(PlottingInterval);

            PlottedFrame frame = new PlottedFrame(Spell.spellCaster.ragdollHand.creature.transform.position, Spell.spellCaster.ragdollHand.creature.transform.rotation);
            PlottedFrames.AddLast(frame);

            OnPlottingTick.Invoke(frame);
        }
    }
}

/// <summary>
/// Data representation of a plotted frame point.
/// </summary>
public class PlottedFrame
{
    /// <summary> Initializes a new instance of the <see cref="PlottedFrame"/> class. </summary>
    public PlottedFrame(Vector3 rootPos, Quaternion rootRot)
    {
        RootPosition = rootPos;
        RootRotation = rootRot;
    }

    public Vector3 RootPosition { get; set; }
    public Quaternion RootRotation { get; set; }
}