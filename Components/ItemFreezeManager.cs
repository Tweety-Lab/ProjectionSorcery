using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Components;

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

        SavedVelocity = Target.physicBody.velocity;
        SavedAngularVelocity = Target.physicBody.angularVelocity;

        Target.physicBody.isEnabled = false;

        IsFrozen = true;
    }

    /// <inheritdoc/>
    public override void UnFreeze()
    {
        if (IsFrozen)
        {
            Target.physicBody.isEnabled = true;
            Target.physicBody.velocity = SavedVelocity;
            Target.physicBody.angularVelocity = SavedAngularVelocity;

            IsFrozen = false;
        }
    }
}