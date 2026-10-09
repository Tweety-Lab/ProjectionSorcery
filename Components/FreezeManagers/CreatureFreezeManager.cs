using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Components.FreezeManagers;

public class CreatureFreezeManager : FreezeManager<Creature>
{
    public bool IsFrozen { get; private set; } = false;

    public Ragdoll.State SavedState { get; private set; }
    public Dictionary<RagdollPart, Vector3> SavedVelocities { get; private set; } = new();
    public Dictionary<RagdollPart, Vector3> SavedAngularVelocities { get; private set; } = new();

    public Dictionary<PhysicBody, bool> SavedPhysicBodyStates { get; private set; } = new();

    public Dictionary<Handle, (bool colliderEnabled, bool touchActive)> SavedHandleStates { get; private set; } = new();

    /// <inheritdoc/>
    public override void Freeze()
    {
        if (Target.isPlayer)
            return;

        if (IsFrozen)
            return;

        ToggleFreezeCreature(Target, true);

        IsFrozen = true;
    }

    /// <inheritdoc/>
    public override void UnFreeze()
    {
        if (Target.isPlayer)
            return;

        if (!IsFrozen)
            return;

        ToggleFreezeCreature(Target, false);

        IsFrozen = false;
        ShouldUnfreeze = true;
    }

    public void ToggleFreezeCreature(Creature creature, bool shouldFreeze)
    {
        creature.brain.enabled = !shouldFreeze;

        if (shouldFreeze)
        {
            SavedVelocities.Clear();
            SavedAngularVelocities.Clear();
            SavedHandleStates.Clear();

            SavedState = creature.ragdoll.state;

            creature.ragdoll.parts.ForEach(part => SavedVelocities.Add(part, part.physicBody.velocity));
            creature.ragdoll.parts.ForEach(part => SavedAngularVelocities.Add(part, part.physicBody.angularVelocity));

            SavedPhysicBodyStates = creature.GetPhysicBodiesInChildren().ToDictionary(pb => pb, pb => pb.isEnabled);

            foreach (var handle in creature.ragdoll.parts.SelectMany(part => part.handles))
                SavedHandleStates[handle] = (handle.touchCollider != null && handle.touchCollider.enabled, handle.touchActive);

            foreach (var pb in creature.GetPhysicBodiesInChildren())
                pb.isEnabled = false;

            creature.ragdoll.SetState(Ragdoll.State.Frozen);

            foreach (var handle in SavedHandleStates.Keys)
            {
                handle.Release();
                handle.ReleaseAllTkHandlers();

                if (handle.touchCollider != null)
                    handle.touchCollider.enabled = false;

                handle.touchActive = false;
            }
        }
        else
        {
            creature.ragdoll.SetState(SavedState);

            creature.ragdoll.parts.ForEach(part => part.physicBody.velocity = SavedVelocities[part]);
            creature.ragdoll.parts.ForEach(part => part.physicBody.angularVelocity = SavedAngularVelocities[part]);

            foreach (var pb in SavedPhysicBodyStates)
                pb.Key.isEnabled = pb.Value;

            foreach (var pb in creature.GetPhysicBodiesInChildren())
                if (SavedPhysicBodyStates.TryGetValue(pb, out var state))
                    pb.isEnabled = state;

            foreach (var pair in SavedHandleStates)
            {
                if (pair.Key.touchCollider != null)
                    pair.Key.touchCollider.enabled = pair.Value.colliderEnabled;

                pair.Key.touchActive = pair.Value.touchActive;
            }
        }

        creature.RootPhysicBody.isEnabled = !shouldFreeze;
    }
}