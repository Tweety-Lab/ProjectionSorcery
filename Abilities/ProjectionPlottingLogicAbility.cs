using AliLib.Core;
using AliLib.Core.Abilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Abilities;

public class ProjectionPlottingLogicAbility : Ability
{
    [ModOption]
    [ModOptionCategory("Projection Sorcery", 1)]
    [ModOptionFloatValues(0.1f, 1f, 0.1f)]
    public static float PlottingInterval = 0.1f;

    [ModOption]
    [ModOptionCategory("Projection Sorcery", 1)]
    [ModOptionIntValues(4, 200, 1)]
    public static int FramesPerSecond = 24;

    /// <summary> All currently plotted frames. </summary>
    public LinkedList<PlottedFrame> PlottedFrames { get; } = new LinkedList<PlottedFrame>(); // Linked List is slightly more perfomant 

    /// <inheritdoc/>
    public ProjectionPlottingLogicAbility(AbilitySpell spell) : base(spell) { }

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
        }

        Spell.OnStopCast -= StopCast;
        Spell.OnStopCast += StopCast;
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
                Spell.spellCaster.ragdollHand.creature.player.Teleport(frame.RootPosition, Quaternion.identity);
            else
                Spell.spellCaster.ragdollHand.creature.Teleport(frame.RootPosition, Quaternion.identity);

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
            PlottedFrames.AddLast(new PlottedFrame { RootPosition = Spell.spellCaster.ragdollHand.creature.transform.position });
        }
    }
}

/// <summary>
/// Data representation of a plotted frame point.
/// </summary>
public class PlottedFrame
{
    public Vector3 RootPosition { get; set; }
}