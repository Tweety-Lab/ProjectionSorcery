using AliLib.Core.Abilities;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Abilities;

/// <summary>
/// Handles interaction with the world during plotting mode.
/// </summary>
public class ProjectionPlottingInteractionAbility : Ability
{
    public enum InteractionType
    {
        DuringPlayback,
        DuringPlotting
    }

    [ModOption(tooltip = "Determines how Projection Sorcery interacts with the world.\n\nDuring Playback: All interactions that happen during plotting are delayed until the relative frame is played.\n\nDuring Plotting: Interactions are enabled during plotting.")]
    [ModOptionCategory("Projection Sorcery", 1)]
    public static InteractionType InteractionMode = InteractionType.DuringPlayback;

    [ModOption]
    [ModOptionCategory("Projection Sorcery", 1)]
    [ModOptionFloatValues(0.1f, 5f, 0.1f)]
    public static float PlaybackDamageMultiplier = 1f;

    /// <inheritdoc/>
    public ProjectionPlottingInteractionAbility(AbilitySpell spell) : base(spell) { }

    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.GetAbility<ProjectionPlottingAbility>()?.OnEnterPlotting += OnEnterPlotting;
        Spell.GetAbility<ProjectionPlottingAbility>()?.OnExitPlotting += OnExitPlotting;

        Spell.GetAbility<ProjectionPlottingAbility>()?.OnPlaybackTick += OnPlaybackTick;
    }

    public void OnEnterPlotting()
    {
        if (InteractionMode == InteractionType.DuringPlotting)
            return;

        Spell.spellCaster.ragdollHand.ragdoll.ForBothHands(hand => hand.colliderGroup.colliders.ForEach(col => col.enabled = false));
    }

    public void OnExitPlotting()
    {
        if (InteractionMode == InteractionType.DuringPlotting)
            return;

        Spell.spellCaster.ragdollHand.ragdoll.ForBothHands(hand => hand.colliderGroup.colliders.ForEach(col => col.enabled = true));
    }

    public void OnPlaybackTick(PlottedFrame enteredFrame)
    {
        if (InteractionMode == InteractionType.DuringPlotting)
            return;

        FakeFrameCollision(Spell.spellCaster.ragdollHand.creature.handLeft, enteredFrame.LeftHand);
        FakeFrameCollision(Spell.spellCaster.ragdollHand.creature.handRight, enteredFrame.RightHand);
    }

    public void FakeFrameCollision(RagdollHand hand, PlottedFrame.Hand frameHand)
    {
        Collider[] rightHits = Physics.OverlapSphere(frameHand.Position, 0.2f);
        foreach (Collider col in rightHits)
        {
            ColliderGroup target = col.GetComponentInParent<ColliderGroup>();
            if (target == null)
                continue;

            if (target.collisionHandler?.ragdollPart?.ragdoll == hand.ragdoll)
                continue;

            ColliderGroup source = hand.colliderGroup;
            Collider sourceCollider = hand.palmCollider;

            Vector3 sourcePoint = frameHand.Position;
            Vector3 targetPoint = col.ClosestPoint(sourcePoint);
            Vector3 velocity = frameHand.Velocity;
            Vector3 normal = (sourcePoint - targetPoint).normalized;

            CollisionInstance.FakeCollision(
                source,
                target,
                sourceCollider,
                col,
                velocity * PlaybackDamageMultiplier,
                sourcePoint,
                targetPoint,
                normal,
                null,
                null,
                null,
                0.6f,
                0.5f
            );
        }
    }
}
