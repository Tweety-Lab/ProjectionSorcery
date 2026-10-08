using AliLib.Core.Assets;
using ProjectionSorcery.Abilities.Plotting;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Components.FreezeManagers;

public class ItemFreezeManager : FreezeManager<Item>
{
    public bool IsFrozen { get; private set; } = false;

    public Vector3 SavedVelocity { get; set; }
    public Vector3 SavedAngularVelocity { get; set; }

    /// <inheritdoc/>
    public override void Freeze()
    {
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

        IsFrozen = true;
    }

    /// <inheritdoc/>
    public override void UnFreeze()
    {
        Target.OnHandleReleaseEvent -= OnHandleRelease;

        if (IsFrozen)
        {
            Target.physicBody.isEnabled = true;
            Target.physicBody.velocity = SavedVelocity;
            Target.physicBody.angularVelocity = SavedAngularVelocity;
            Target.breakable?.canBreak = true;

            IsFrozen = false;
        }
    }

    public void OnHandleRelease(Handle handle, RagdollHand hand, bool _)
    {
        // HACK: We should move this logic to ProjectionInteractionAbility
        if (ProjectionInteractionAbility.InteractionMode == ProjectionInteractionAbility.InteractionType.DuringPlayback)
            handle.item.colliderGroups.ForEach(cg => cg.colliders.ForEach(col => col.enabled = true));

        Freeze();
    }
}