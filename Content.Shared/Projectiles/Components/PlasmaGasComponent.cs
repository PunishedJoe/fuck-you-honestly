using Robust.Shared.GameStates;

namespace Content.Shared.Projectiles.Components;

/// <summary>
/// Component for projectiles that spawn plasma gas and rapidly heat the environment after penetration stops.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PlasmaGasComponent : Component
{
    /// <summary>
    /// Delay in seconds before releasing plasma after penetration stops. If 0, releases instantly.
    /// </summary>
    [DataField]
    public float ReleaseDelay = 0f;

    /// <summary>
    /// Whether the plasma release has been triggered.
    /// </summary>
    [DataField]
    public bool ReleaseTriggered = false;

    /// <summary>
    /// Amount of plasma gas to spawn in moles.
    /// </summary>
    [DataField]
    public float PlasmaAmount = 100f;

    /// <summary>
    /// Temperature of the plasma gas in Kelvin. High temperatures will rapidly heat the environment.
    /// </summary>
    [DataField]
    public float PlasmaTemperature = 1000f;
}



