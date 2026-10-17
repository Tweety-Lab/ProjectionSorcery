using AliLib.Core;
using AliLib.Core.Abilities;
using ProjectionSorcery.Components.FreezeManagers;
using System.Collections.Generic;
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
        Delayed,
        Instant,
        AfterPlayback,
        LevelDependant
    }

    [ModOption]
    [ModOptionCategory("Plotting", 0)]
    public static bool FreezeTime = true;

    [ModOption(tooltip = "Determines how fast creatures react to Projection Sorecry users.\n\nDelayed: Creatures react after a delay defined by Enemy Reaction Delay.\nInstant: Creatures react as soon as plotting ends.\nAfter Playback: Creatures react as soon as playback ends.\nLevel Dependant: The stronger the creature the faster they react (roughly).")]
    [ModOptionCategory("Plotting", 0)]
    public static ReactionType EnemyReactionType = ReactionType.Instant;

    [ModOption]
    [ModOptionCategory("Plotting", 0)]
    [ModOptionFloatValues(0.1f, 5f, 0.1f)]
    public static float EnemyReactionDelay = 0.3f;

    private HashSet<Creature> awaitingPlaybackReaction = new HashSet<Creature>();

    /// <inheritdoc/>
    public ProjectionTimeFreezeAbility(AbilitySpell spell) : base(spell) { }

    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.GetAbility<ProjectionAbility>()?.OnEnterPlotting += OnEnterPlotting;
        Spell.GetAbility<ProjectionAbility>()?.OnExitPlotting += OnExitPlotting;
        Spell.GetAbility<ProjectionAbility>()?.OnPlaybackFinish += OnPlaybackFinish;
    }

    public void OnPlaybackFinish()
    {
        foreach (Creature creature in awaitingPlaybackReaction)
            creature.brain.enabled = true;

        awaitingPlaybackReaction.Clear();
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

        foreach (Components.FreezeFrame freezeFrame in GameObject.FindObjectsOfType<Components.FreezeFrame>())
        {
            FreezeFrameFreezeManager freezeManager = freezeFrame.GetOrAddComponent<FreezeFrameFreezeManager>();
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
                    CoroutineRunner.Instance.PlayAfterDelay(() => creature.brain.enabled = true, EnemyReactionDelay);

                if (EnemyReactionType == ReactionType.LevelDependant)
                    CoroutineRunner.Instance.PlayAfterDelay(() => creature.brain.enabled = true, Mathf.Max(0f, 2f - DetermineStrength(creature)));

                if (EnemyReactionType == ReactionType.AfterPlayback)
                    awaitingPlaybackReaction.Add(creature);
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

        foreach (Components.FreezeFrame freezeFrame in GameObject.FindObjectsOfType<Components.FreezeFrame>())
        {
            FreezeFrameFreezeManager freezeManager = freezeFrame.GetOrAddComponent<FreezeFrameFreezeManager>();
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
