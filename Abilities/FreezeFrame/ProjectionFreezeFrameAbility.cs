using AliLib.Core.Abilities;
using AliLib.Core.Assets;
using AliLib.Core;
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

    private readonly Dictionary<RagdollHand, bool> lastTriggers = new Dictionary<RagdollHand, bool>();

    public void OnUpdateCast()
    {
        if (FreezeMode != FreezeType.GrabAndTrigger)
            return;

        // This is kinda hacky but AliLib quirks means we have to manually handle multi-hand states
        UpdateGrabAndTrigger(Spell.spellCaster.ragdollHand.creature.handLeft);
        UpdateGrabAndTrigger(Spell.spellCaster.ragdollHand.creature.handRight);
    }

    private void UpdateGrabAndTrigger(RagdollHand hand)
    {
        if (hand.caster.spellInstance is not ProjectionSorcerySpell)
        {
            lastTriggers.Remove(hand);
            return;
        }

        bool triggerPressed = hand.playerHand.controlHand.usePressed;
        lastTriggers.TryGetValue(hand, out bool lastTrigger);
        lastTriggers[hand] = triggerPressed;

        if (FreezeFrame != null && triggerPressed && !lastTrigger)
        {
            var grabbedHandle = hand.grabbedHandle;
            if (grabbedHandle == null)
                return;

            Creature creature = grabbedHandle.GetComponentInParent<Creature>();
            if (creature != null && !FrozenCreatures.Contains(creature) && !RecentlyFrozenCreatures.Contains(creature))
            {
                GameObject instance = GameObject.Instantiate(FreezeFrame, grabbedHandle.transform.position, Quaternion.identity);
                instance.GetComponent<Components.FreezeFrame>().Target = creature;
                instance.GetComponent<Components.FreezeFrame>().OnFrameBroken += () => OnFrameBroken(creature);
                FrozenCreatures.Add(creature);
                hand.TryRelease();
            }
        }
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
