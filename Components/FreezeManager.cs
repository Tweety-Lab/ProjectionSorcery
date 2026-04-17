using UnityEngine;

namespace ProjectionSorcery.Components;

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

    void Awake() => Target = GetComponent<T>() ?? GetComponentInParent<T>();

    /// <inheritdoc/>
    public abstract void Freeze();

    /// <inheritdoc/>
    public abstract void UnFreeze();
}
