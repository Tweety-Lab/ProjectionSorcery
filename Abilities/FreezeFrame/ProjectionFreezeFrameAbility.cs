using AliLib.Core.Abilities;
using AliLib.Core.Assets;
using AliLib.Core.GC;
using ProjectionSorcery.Abilities.Plotting;
using ThunderRoad;
using UnityEngine;


namespace ProjectionSorcery.Abilities.FreezeFrame;

public class ProjectionFreezeFrameAbility : Ability
{
    [Addressable("ProjectionSorcery.FreezeFrame")]
    public static GameObject? FreezeFrame { get; private set; }

    /// <inheritdoc/>
    public ProjectionFreezeFrameAbility(AbilitySpell spell) : base(spell) { }

    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.OnStartCast += OnStartCast;
    }

    public void OnStartCast()
    {
        if (FreezeFrame == null)
            return;

        foreach (var creature in Creature.allActive)
        {
            if (creature.isPlayer)
                continue;

            GameObject instance = GameObject.Instantiate(FreezeFrame, creature.transform.position, Quaternion.identity);
            instance.GetComponent<Components.FreezeFrame>().Target = creature;
        }
    }
}
