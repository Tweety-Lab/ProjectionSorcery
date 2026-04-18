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
    [ModOption]
    [ModOptionCategory("Projection Sorcery", 0)]
    public static bool FreezeTime = true;

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
            CreatureFreezeManager freezeManager = creature.GetOrAddComponent<CreatureFreezeManager>();
            freezeManager.UnFreeze();

            GameObject.Destroy(freezeManager);
        }

        foreach (Item item in Item.allActive)
        {
            ItemFreezeManager freezeManager = item.GetOrAddComponent<ItemFreezeManager>();
            freezeManager.UnFreeze();

            GameObject.Destroy(freezeManager);
        }
    }
}
