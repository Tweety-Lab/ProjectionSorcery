using AliLib.Core.Abilities;
using UnityEngine;

namespace ProjectionSorcery.Abilities;

public class ProjectionPlottingLogicAbility : Ability
{
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
        Debug.Log("OnStartCast");
    }
}
