using AliLib.Core;
using AliLib.Core.Abilities;
using ProjectionSorcery.Components.FreezeManagers;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Abilities.Plotting;

/// <summary>
/// Handles the time-freezing during plotting.
/// </summary>
public class ProjectionTimeFreezeAbility : Ability
{
    public enum ReactionType
    {
        Instant,
        Delayed,
        LevelDependant
    }

    [ModOption]
    [ModOptionCategory("Projection Sorcery", 0)]
    public static bool FreezeTime = true;

    [ModOption(tooltip = "Determines how fast creatures react to Projection Sorecry users.\n\nInstant: Creatures react as soon as playback ends.\nDelayed: Creatures react after a short delay.\nLevelDependant: The stronger the creature the faster they react.")]
    [ModOptionCategory("Projection Sorcery", 0)]
    public static ReactionType EnemyReactionType = ReactionType.Instant;

    /// <inheritdoc/>
    public ProjectionTimeFreezeAbility(AbilitySpell spell) : base(spell) { }

    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.GetAbility<ProjectionAbility>()?.OnEnterPlotting += OnEnterPlotting;
        Spell.GetAbility<ProjectionAbility>()?.OnExitPlotting += OnExitPlotting;
    }

    public void OnEnterPlotting()
    {
        if (!FreezeTime)
            return;

        foreach (Creature creature in Creature.allActive)
        {
            CreatureFreezeManager freezeManager = creature.GetOrAddComponent<CreatureFreezeManager>();
            freezeManager.Freeze();
        }

        foreach (Item item in Item.allActive)
        {
            ItemFreezeManager freezeManager = item.GetOrAddComponent<ItemFreezeManager>();
            freezeManager.Freeze();
        }
    }

    public void OnExitPlotting()
    {
        if (!FreezeTime)
            return;

        foreach (Creature creature in Creature.allActive)
        {
            if (creature.isPlayer)
                continue;

            CreatureFreezeManager freezeManager = creature.GetOrAddComponent<CreatureFreezeManager>();
            if (freezeManager.ShouldUnfreeze)
            {
                freezeManager.UnFreeze();
                GameObject.Destroy(freezeManager);
            }

            if (EnemyReactionType != ReactionType.Instant)
            {
                creature.brain.enabled = false;

                if (EnemyReactionType == ReactionType.Delayed)
                    CoroutineRunner.Instance.PlayAfterDelay(() => creature.brain.enabled = true, 0.5f);

                if (EnemyReactionType == ReactionType.LevelDependant)
                    CoroutineRunner.Instance.PlayAfterDelay(() => creature.brain.enabled = true, Mathf.Max(0f, 2f - DetermineStrength(creature)));
            }
        }

        foreach (Item item in Item.allActive)
        {
            ItemFreezeManager freezeManager = item.GetOrAddComponent<ItemFreezeManager>();
            if (freezeManager.ShouldUnfreeze)
            {
                freezeManager.UnFreeze();
                GameObject.Destroy(freezeManager);
            }
        }
    }

    public static float DetermineStrength(Creature creature)
    {
        // TODO: Tweak these values
        float strength = 0;

        if (creature.maxHealth > 100f)
            strength += 0.5f;

        if (creature.currentLocomotion.forwardSpeed > 0.3f)
            strength += 0.5f;

        if (creature.damageMultipliers.Count > 2)
            strength += 0.5f;

        if (creature.brain.instance.modules.Count > 20)
            strength += 1f;

        return strength;
    }
}
