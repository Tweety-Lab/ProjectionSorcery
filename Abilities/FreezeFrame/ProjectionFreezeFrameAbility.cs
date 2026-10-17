using AliLib.Core.Abilities;
using AliLib.Core.Assets;
using AliLib.Core;
using AliLib.Core.GC;
using ProjectionSorcery.Abilities.Plotting;
using System.Collections.Generic;
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
    public static FreezeType FreezeMode;

    [ModOption(tooltip = "The amount of time before a creature just broken out of a freeze frame can be refrozen.")]
    [ModOptionCategory("Freeze Frames", 0)]
    [ModOptionFloatValues(0.1f, 5f, 0.1f)]
    public static float RefreezeGracePeriod = 0.5f;

    [Addressable("ProjectionSorcery.FreezeFrame")]
    public static GameObject? FreezeFrame { get; set; }

    /// <summary> The creatures that are currently frozen. </summary>
    public HashSet<Creature> FrozenCreatures { get; } = new HashSet<Creature>();

    /// <summary> The creatures that have been frozen within the last <see cref="RefreezeGracePeriod"/> seconds. </summary>
    public HashSet<Creature> RecentlyFrozenCreatures { get; } = new HashSet<Creature>();

    private void OnFrameBroken(Creature creature)
    {
        FrozenCreatures.Remove(creature);
        RecentlyFrozenCreatures.Add(creature);
        CoroutineRunner.Instance.PlayAfterDelay(() => RecentlyFrozenCreatures.Remove(creature), RefreezeGracePeriod);
    }

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

    bool lastTrigger = false;
    public void OnUpdateCast()
    {
        if (FreezeFrame == null || FreezeMode != FreezeType.GrabAndTrigger)
            return;

        bool triggerPressed = Spell.spellCaster.ragdollHand.playerHand.controlHand.usePressed;

        if (triggerPressed && !lastTrigger)
        {
            var grabbedHandle = Spell.spellCaster.ragdollHand.grabbedHandle;
            if (grabbedHandle != null)
            {
                Creature creature = grabbedHandle.GetComponentInParent<Creature>();
                if (creature != null && !FrozenCreatures.Contains(creature) && !RecentlyFrozenCreatures.Contains(creature))
                {
                    GameObject instance = GameObject.Instantiate(FreezeFrame, grabbedHandle.item.transform.position, Quaternion.identity);
                    instance.GetComponent<Components.FreezeFrame>().Target = creature;
                    instance.GetComponent<Components.FreezeFrame>().OnFrameBroken += () => OnFrameBroken(creature);
                    FrozenCreatures.Add(creature);
                    Spell.spellCaster.ragdollHand.TryRelease();
                }
            }
        }

        lastTrigger = triggerPressed;
    }

    public void OnCollideStart(CollisionInstance col)
    {
        if (FreezeFrame == null || FreezeMode != FreezeType.Collision)
            return;

        if (Spell.spellCaster.ragdollHand.playerHand.controlHand.gripPressed)
            return;

        if (col.targetCollider.transform.root.TryGetComponent<Creature>(out var creature))
        {
            if (FrozenCreatures.Contains(creature) || RecentlyFrozenCreatures.Contains(creature))
                return;

            GameObject instance = GameObject.Instantiate(FreezeFrame, creature.transform.position, Quaternion.identity);
            instance.GetComponent<Components.FreezeFrame>().Target = creature;
            instance.GetComponent<Components.FreezeFrame>().OnFrameBroken += () => OnFrameBroken(creature);
            FrozenCreatures.Add(creature);
        }
    }
}
