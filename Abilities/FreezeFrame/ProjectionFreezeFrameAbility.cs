using AliLib.Core.Abilities;
using AliLib.Core.Assets;
using AliLib.Core.GC;
using ProjectionSorcery.Abilities.Plotting;
using ThunderRoad;
using UnityEngine;


namespace ProjectionSorcery.Abilities.FreezeFrame;

public class ProjectionFreezeFrameAbility : Ability
{
    public enum FreezeType
    {
        GrabAndTrigger,
        Collision
    }

    [ModOption]
    [ModOptionCategory("Freeze Frames", 0)]
    public static FreezeType FreezeMode { get; set; }

    [Addressable("ProjectionSorcery.FreezeFrame")]
    public static GameObject? FreezeFrame { get; private set; }

    /// <inheritdoc/>
    public ProjectionFreezeFrameAbility(AbilitySpell spell) : base(spell) { }
    
    /// <inheritdoc/>
    public override void Init()
    {
        base.Init();

        Spell.OnUpdateCast += OnUpdateCast;
    }

    /// <inheritdoc/>
    public override void OnEquip() => Spell.spellCaster.ragdollHand.collisionHandler.OnCollisionStartEvent += OnCollideStart;

    /// <inheritdoc/>
    public override void OnUnequip() => Spell.spellCaster.ragdollHand.collisionHandler.OnCollisionStartEvent -= OnCollideStart;

    public void OnUpdateCast()
    {
        if (FreezeFrame == null || FreezeMode != FreezeType.GrabAndTrigger)
            return;

        // TODO: Test this works
        if (Spell.spellCaster.ragdollHand.grabbedWithTrigger)
        {
            Creature creature = Spell.spellCaster.ragdollHand.grabbedHandle.GetComponentInParent<Creature>();
            if (creature == null)
                return;

            GameObject instance = GameObject.Instantiate(FreezeFrame, Spell.spellCaster.ragdollHand.grabbedHandle.item.transform.position, Quaternion.identity);
            instance.GetComponent<Components.FreezeFrame>().Target = creature;
        }
    }

    public void OnCollideStart(CollisionInstance col)
    {
        if (FreezeFrame == null || FreezeMode != FreezeType.Collision)
            return;

        if (col.targetCollider.transform.root.TryGetComponent<Creature>(out var creature))
        {
            GameObject instance = GameObject.Instantiate(FreezeFrame, creature.transform.position, Quaternion.identity);
            instance.GetComponent<Components.FreezeFrame>().Target = creature;
        }
    }
}
