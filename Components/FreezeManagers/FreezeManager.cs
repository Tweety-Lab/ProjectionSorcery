using UnityEngine;

namespace ProjectionSorcery.Components.FreezeManagers;

/// <summary> Non-Generic interface for <see cref="FreezeManager"/> </summary>
public interface IFreezeManager
{
    /// <summary> Freeze the object. </summary>
    void Freeze();

    /// <summary> Unfreeze the object. </summary>
    void UnFreeze();
}

/// <summary>
/// Component responsible for managing freezing/unfreezing of a certain object.
/// </summary>
public abstract class FreezeManager<T> : MonoBehaviour, IFreezeManager where T : MonoBehaviour
{
    /// <summary> Target object to manage. </summary>
    public T Target { get; set; } = null!;

    /// <summary> Is the object frozen? </summary>
    public bool IsFrozen { get; private set; }

    /// <summary> Should the object be unfrozen? </summary>
    public bool ShouldUnfreeze { get; set; } = true;

    /// <inheritdoc/>
    public virtual void Freeze()
    {
        if (IsFrozen)
            return;

        IsFrozen = true;
    }

    /// <inheritdoc/>
    public virtual void UnFreeze()
    {
        if (!IsFrozen)
            return;

        IsFrozen = false;
    }

    public void Awake() => Target = GetComponent<T>() ?? GetComponentInParent<T>();
}
