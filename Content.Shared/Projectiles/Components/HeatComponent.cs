using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Projectiles.Components;

/// <summary>
/// Component for HEAT (High Explosive Anti-Tank) projectiles.
/// Causes the projectile to explode after penetration threshold is reached, with optional delay.
/// Can also fragment into additional projectiles.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HeatComponent : Component
{
    /// <summary>
    /// Delay in seconds before explosion after penetration stops. If 0, explodes instantly.
    /// </summary>
    [DataField]
    public float ExplosionDelay = 0f;

    /// <summary>
    /// Whether the explosion has been triggered.
    /// </summary>
    [DataField]
    public bool ExplosionTriggered = false;

    /// <summary>
    /// If set, the projectile will fragment into this prototype when exploding (requires HeFragProjectileComponent).
    /// </summary>
    [DataField]
    public EntProtoId? FragmentationPrototype = null;

    /// <summary>
    /// Number of fragmentation projectiles to spawn. Only used if FragmentationPrototype is set.
    /// </summary>
    [DataField]
    public int FragmentationCount = 0;
}

