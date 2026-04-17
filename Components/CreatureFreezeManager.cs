using System.Linq;
using ThunderRoad;

namespace ProjectionSorcery.Components;

public class CreatureFreezeManager : FreezeManager<Creature>
{
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
            creature.ragdoll.SetState(Ragdoll.State.Frozen);
        else
            creature.ragdoll.SetState(Ragdoll.State.NoPhysic);

        creature.RootPhysicBody.isEnabled = !shouldFreeze;
    }
}
