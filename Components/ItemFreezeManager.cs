using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace ProjectionSorcery.Components;

public class ItemFreezeManager : FreezeManager<Item>
{
    /// <summary> All currently frozen items. </summary>
    public static Dictionary<Item, FrozenItemData> FrozenItems = new Dictionary<Item, FrozenItemData>();

    /// <inheritdoc/>
    public override void Freeze()
    {
        if (Target.physicBody.isKinematic)
            return;

        FrozenItemData data = new FrozenItemData()
        {
            Velocity = Target.physicBody.velocity,
            AngularVelocity = Target.physicBody.angularVelocity
        };

        Target.physicBody.isEnabled = false;

        FrozenItems.Add(Target, data);
    }

    /// <inheritdoc/>
    public override void UnFreeze()
    {
        if (FrozenItems.ContainsKey(Target))
        {
            Target.physicBody.isEnabled = true;
            Target.physicBody.velocity = FrozenItems[Target].Velocity;
            Target.physicBody.angularVelocity = FrozenItems[Target].AngularVelocity;

            FrozenItems.Remove(Target);
        }
    }
}

/// <summary>
/// Data Representation of a frozen item.
/// </summary>
public struct FrozenItemData
{
    public Vector3 Velocity { get; set; }
    public Vector3 AngularVelocity { get; set; }
}
