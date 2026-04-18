using AliLib.Core.Abilities;
using AliLib.Core.Assets;
using AliLib.Core.GC;
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

        foreach (var item in Item.allActive)
        {
            GameObject.Instantiate(FreezeFrame, item.transform.position, item.transform.rotation);
            FreezeFrame.GetComponent<Components.FreezeFrame>().Target = item.gameObject;
        }
    }
}
