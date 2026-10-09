namespace ProjectionSorcery.Components.FreezeManagers;

// We have a dedicated freeze manager for the FreezeFrame in case we ever want to support frozen creatures AND props
public class FreezeFrameFreezeManager : FreezeManager<FreezeFrame>
{
    /// <inheritdoc/>
    public override void Freeze()
    {
        Target.IsBreakable = false;
        Target.TickAutoBreak = false;
    }

    /// <inheritdoc/>
    public override void UnFreeze()
    {
        Target.IsBreakable = true;
        Target.TickAutoBreak = true;
    }
}
