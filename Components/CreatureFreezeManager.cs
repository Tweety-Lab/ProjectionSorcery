using ThunderRoad;

namespace ProjectionSorcery.Components;

public class CreatureFreezeManager : FreezeManager<Creature>
{
    /// <inheritdoc/>
    public override void Freeze()
    {
        if (Target.isPlayer)
            return;

        Target.brain.enabled = false;
        Target.ragdoll.SetState(Ragdoll.State.Frozen);
        Target.RootPhysicBody.isEnabled = false;
    }

    /// <inheritdoc/>
    public override void UnFreeze()
    {
        if (Target.isPlayer)
            return;

        Target.brain.enabled = true;
        Target.ragdoll.SetState(Ragdoll.State.NoPhysic);
        Target.RootPhysicBody.isEnabled = true;
    }
}
