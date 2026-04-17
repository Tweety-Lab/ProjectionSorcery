using AliLib.Core.Abilities;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Abilities;

/// <summary>
/// Handles interaction with the world during plotting mode.
/// </summary>
public class ProjectionPlottingInteractionAbility : Ability
{
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
        Spell.spellCaster.ragdollHand.ragdoll.ForBothHands(hand => hand.colliderGroup.colliders.ForEach(col => col.enabled = false));
    }

    public void OnExitPlotting()
    {
        Spell.spellCaster.ragdollHand.ragdoll.ForBothHands(hand => hand.colliderGroup.colliders.ForEach(col => col.enabled = true));
    }

    public void OnPlaybackTick(PlottedFrame enteredFrame)
    {
        FakeFrameCollision(Spell.spellCaster.ragdollHand.creature.handLeft, enteredFrame.LeftHand);
        FakeFrameCollision(Spell.spellCaster.ragdollHand.creature.handRight, enteredFrame.RightHand);
    }

    public void FakeFrameCollision(RagdollHand hand, PlottedFrame.Hand frameHand)
    {
        Collider[] rightHits = Physics.OverlapSphere(frameHand.Position, 0.5f);
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
                velocity,
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
