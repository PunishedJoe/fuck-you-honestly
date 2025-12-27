using Content.Server.Atmos.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Projectiles;
using Content.Shared.Projectiles.Components;
using Robust.Server.GameObjects;

namespace Content.Server._Battlefield14.Projectiles;

/// <summary>
/// System for handling projectiles that spawn plasma gas and heat the environment after penetration stops.
/// </summary>
public sealed class PlasmaGasSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly TriggerSystem _triggerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlasmaGasComponent, TriggerEvent>(OnPlasmaGasTriggered);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PlasmaGasComponent, ProjectileComponent>();
        while (query.MoveNext(out var uid, out var plasmaGas, out var projectile))
        {
            // Check if projectile penetration has stopped
            if (!projectile.ProjectileSpent || plasmaGas.ReleaseTriggered)
                continue;

            plasmaGas.ReleaseTriggered = true;
            Dirty(uid, plasmaGas);

            // Release plasma gas (with delay if specified)
            if (plasmaGas.ReleaseDelay <= 0)
            {
                ReleasePlasmaGas(uid, plasmaGas);
            }
            else
            {
                // Delayed release using timer
                _triggerSystem.HandleTimerTrigger(uid, null, plasmaGas.ReleaseDelay, 0, null, null);
            }
        }
    }

    private void OnPlasmaGasTriggered(EntityUid uid, PlasmaGasComponent component, TriggerEvent args)
    {
        // Only release if this was triggered by our delay timer
        if (component.ReleaseTriggered)
        {
            ReleasePlasmaGas(uid, component);
        }
    }

    private void ReleasePlasmaGas(EntityUid uid, PlasmaGasComponent component)
    {
        var environment = _atmosphereSystem.GetContainingMixture(uid, false, true);
        if (environment == null)
            return;

        // Create plasma gas mixture with high temperature
        var plasmaMixture = new GasMixture(1) { Temperature = component.PlasmaTemperature };
        plasmaMixture.SetMoles(Gas.Plasma, component.PlasmaAmount);

        // Merge into environment - the high temperature will rapidly heat the area
        _atmosphereSystem.Merge(environment, plasmaMixture);
    }
}

