using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Components.FreezeManagers;

public class CreatureFreezeManager : FreezeManager<Creature>
{
    public Ragdoll.State SavedState { get; private set; }
    public Vector3 SavedVelocity { get; private set; }
    public Vector3 SavedAngularVelocity { get; private set; }

    public Dictionary<PhysicBody, bool> SavedPhysicBodyStates { get; private set; } = new();

    /// <inheritdoc/>
    public override void Freeze()
    {
        if (Target.isPlayer)
            return;

        ToggleFreezeCreature(Target, true);
    }

    /// <inheritdoc/>
    public override void UnFreeze()
    {
        if (Target.isPlayer)
            return;

        ToggleFreezeCreature(Target, false);
    }

    public void ToggleFreezeCreature(Creature creature, bool shouldFreeze)
    {
        creature.brain.enabled = !shouldFreeze;

        if (shouldFreeze)
        {
            SavedState = creature.ragdoll.state;

            SavedVelocity = creature.RootPhysicBody.velocity;
            SavedAngularVelocity = creature.RootPhysicBody.angularVelocity;

            SavedPhysicBodyStates.Clear();
            SavedPhysicBodyStates = creature.GetPhysicBodiesInChildren().ToDictionary(pb => pb, pb => pb.isEnabled);

            creature.ragdoll.SetState(Ragdoll.State.Frozen);
        }
        else
        {
            creature.ragdoll.SetState(SavedState);

            foreach (var pb in SavedPhysicBodyStates)
                pb.Key.isEnabled = pb.Value;

            creature.RootPhysicBody.velocity = SavedVelocity;
            creature.RootPhysicBody.angularVelocity = SavedAngularVelocity;
        }

        creature.RootPhysicBody.isEnabled = !shouldFreeze;
    }
}