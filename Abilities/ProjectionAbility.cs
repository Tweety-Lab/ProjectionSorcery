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
public class ProjectionAbility : Ability
{
    [ModOption]
    [ModOptionCategory("Projection Sorcery", 0)]
    [ModOptionFloatValues(0.1f, 1f, 0.1f)]
    public static float PlottingInterval = 0.2f;

    [ModOption]
    [ModOptionCategory("Projection Sorcery", 0)]
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

    /// <summary> Whether the creature is currently in plotting mode. </summary>
    public bool IsPlotting => PlottedFrames.Count > 0;

    /// <summary> The creatures velocity after launching, or null if not currently launching. </summary>
    public float? LaunchVelocity { get; set; } = null;

    /// <summary> The creatures accumulated speed multiplier after launching. </summary>
    public float AccumulatedSpeedMultiplier { get; set; } = 1f;

    /// <inheritdoc/>
    public ProjectionAbility(AbilitySpell spell) : base(spell) { }

    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.OnStartCast += OnStartCast;
    }

    public void OnStartCast()
    {
        // TODO: Clean velocity stacking up
        if (IsPlotting)
        {
            if (PlottedFrames.Count > 5)
                return;

            Debug.Log("Launching Player");
            PlottedFrame frame = PlottedFrames.Last.Value;

            CoroutineRunner.Instance.PlayAfterYield(() =>
            {
                Vector3 forward = frame.RootRotation * Vector3.forward;
                Vector3 velocity = forward * 2000f * AccumulatedSpeedMultiplier;

                Spell.spellCaster.ragdollHand.creature.AddForce(velocity, ForceMode.Impulse);

                LaunchVelocity = velocity.magnitude;

                CoroutineRunner.Instance.PlayAfterDelay(() =>
                {
                    LaunchVelocity = null;
                }, 1f);
            }, new WaitUntil(() => !IsPlotting));

            return;
        }

        if (LaunchVelocity != null)
        {
            AccumulatedSpeedMultiplier += 0.2f;

            float modifier = LaunchVelocity.Value * 0.001f * AccumulatedSpeedMultiplier;
            Spell.spellCaster.ragdollHand.creature.locomotion.SetSpeedModifier(this, modifier, modifier, modifier, modifier, 1f, 1f);

            Debug.Log(modifier);
        } else
        {
            AccumulatedSpeedMultiplier = 1f;
        }

        Coroutine plotting = CoroutineRunner.Instance.StartCoroutine(PlottingTick());

        // Since we only need to run this logic if StartCast has already fired we can subscribe here
        void StopCast()
        {
            Spell.spellCaster.ragdollHand.creature.locomotion.RemoveSpeedModifier(this);

            // Prevent enumeration issues
            CoroutineRunner.Instance.PlayAfterYield(() => Spell.OnStopCast -= StopCast, new WaitForEndOfFrame());

            CoroutineRunner.Instance.StopCoroutine(plotting);
            StartPlayback();

            OnExitPlotting.Invoke();
        }

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

            PlottedFrame.Hand lhand = new PlottedFrame.Hand(Spell.spellCaster.ragdollHand.creature.handLeft.transform.position, Spell.spellCaster.ragdollHand.creature.handLeft.transform.rotation, Spell.spellCaster.ragdollHand.creature.handLeft.physicBody.velocity);
            PlottedFrame.Hand rhand = new PlottedFrame.Hand(Spell.spellCaster.ragdollHand.creature.handRight.transform.position, Spell.spellCaster.ragdollHand.creature.handRight.transform.rotation, Spell.spellCaster.ragdollHand.creature.handRight.physicBody.velocity);

            PlottedFrame frame = new PlottedFrame(Spell.spellCaster.ragdollHand.creature.transform.position, Spell.spellCaster.ragdollHand.creature.transform.rotation, lhand, rhand);
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
    public struct Hand
    {
        public Hand(Vector3 position, Quaternion rotation, Vector3 velocity = default)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
        }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Velocity { get; set; }
    }

    /// <summary> Initializes a new instance of the <see cref="PlottedFrame"/> class. </summary>
    public PlottedFrame(Vector3 rootPos, Quaternion rootRot, Hand leftHand, Hand rightHand)
    {
        RootPosition = rootPos;
        RootRotation = rootRot;
        LeftHand = leftHand;
        RightHand = rightHand;
    }

    public Vector3 RootPosition { get; set; }
    public Quaternion RootRotation { get; set; }

    public Hand LeftHand { get; set; }
    public Hand RightHand { get; set; }
}