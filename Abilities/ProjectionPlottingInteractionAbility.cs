using AliLib.Core.Abilities;
using ThunderRoad;

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
    }

    public void OnEnterPlotting()
    {
        foreach (RagdollHand hand in new  RagdollHand[] {Spell.spellCaster.ragdollHand.creature.handLeft, Spell.spellCaster.ragdollHand.creature.handRight})
            hand.colliderGroup.colliders.ForEach(col => col.enabled = false);
    }

    public void OnExitPlotting()
    {
        foreach (RagdollHand hand in new RagdollHand[] { Spell.spellCaster.ragdollHand.creature.handLeft, Spell.spellCaster.ragdollHand.creature.handRight })
            hand.colliderGroup.colliders.ForEach(col => col.enabled = true);
    }
}
