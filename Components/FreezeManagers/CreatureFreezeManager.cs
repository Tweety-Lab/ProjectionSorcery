using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Components.FreezeManagers;

public class CreatureFreezeManager : FreezeManager<Creature>
{
    public Ragdoll.State SavedState { get; private set; }
    public Dictionary<RagdollPart, Vector3> SavedVelocities { get; private set; } = new();
    public Dictionary<RagdollPart, Vector3> SavedAngularVelocities { get; private set; } = new();

    public Dictionary<PhysicBody, bool> SavedPhysicBodyStates { get; private set; } = new();

    /// <inheritdoc/>
    public override void Freeze()
    {
        base.Freeze();

        if (Target.isPlayer)
            return;

        ToggleFreezeCreature(Target, true);
    }

    /// <inheritdoc/>
    public override void UnFreeze()
    {
        base.UnFreeze();

        if (Target.isPlayer)
            return;

        ToggleFreezeCreature(Target, false);

        ShouldUnfreeze = true;
    }

    public void ToggleFreezeCreature(Creature creature, bool shouldFreeze)
    {
        creature.brain.enabled = !shouldFreeze;

        if (shouldFreeze)
        {
            SavedVelocities.Clear();
            SavedAngularVelocities.Clear();

            SavedState = creature.ragdoll.state;

            creature.ragdoll.parts.ForEach(part => SavedVelocities.Add(part, part.physicBody.velocity));
            creature.ragdoll.parts.ForEach(part => SavedAngularVelocities.Add(part, part.physicBody.angularVelocity));

            SavedPhysicBodyStates = creature.GetPhysicBodiesInChildren().ToDictionary(pb => pb, pb => pb.isEnabled);

            creature.ragdoll.SetState(Ragdoll.State.Frozen);

            foreach (var pb in creature.GetPhysicBodiesInChildren())
                pb.isEnabled = false;
        }
        else
        {
            creature.ragdoll.SetState(SavedState);

            foreach (var pb in SavedPhysicBodyStates)
                pb.Key.isEnabled = pb.Value;

            creature.ragdoll.parts.ForEach(part => part.physicBody.velocity = SavedVelocities[part]);
            creature.ragdoll.parts.ForEach(part => part.physicBody.angularVelocity = SavedAngularVelocities[part]);

            foreach (var pb in creature.GetPhysicBodiesInChildren())
                if (SavedPhysicBodyStates.TryGetValue(pb, out var state))
                    pb.isEnabled = state;
        }

        creature.RootPhysicBody.isEnabled = !shouldFreeze;
    }
}