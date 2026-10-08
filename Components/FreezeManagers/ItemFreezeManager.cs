using AliLib.Core.Assets;
using ProjectionSorcery.Abilities.Plotting;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Components.FreezeManagers;

public class ItemFreezeManager : FreezeManager<Item>
{
    public Vector3 SavedVelocity { get; set; }
    public Vector3 SavedAngularVelocity { get; set; }

    /// <inheritdoc/>
    public override void Freeze()
    {
        base.Freeze();

        if (Target.physicBody.isKinematic)
            return;

        if (Target.IsHeldByPlayer)
        {
            Target.OnHandleReleaseEvent += OnHandleRelease;
            return;
        }


        SavedVelocity = Target.physicBody.velocity;
        SavedAngularVelocity = Target.physicBody.angularVelocity;

        Target.physicBody.isEnabled = false;
        Target.breakable?.canBreak = false;
    }

    /// <inheritdoc/>
    public override void UnFreeze()
    {
        base.UnFreeze();

        Target.OnHandleReleaseEvent -= OnHandleRelease;

        Target.physicBody.isEnabled = true;
        Target.physicBody.velocity = SavedVelocity;
        Target.physicBody.angularVelocity = SavedAngularVelocity;
        Target.breakable?.canBreak = true;
    }

    public void OnHandleRelease(Handle handle, RagdollHand hand, bool _)
    {
        // HACK: We should move this logic to ProjectionInteractionAbility
        if (ProjectionInteractionAbility.InteractionMode == ProjectionInteractionAbility.InteractionType.DuringPlayback)
            handle.item.colliderGroups.ForEach(cg => cg.colliders.ForEach(col => col.enabled = true));

        Freeze();
    }
}