using Content.Server.Explosion.EntitySystems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Explosion.Components;
using Content.Shared.Projectiles;
using Content.Shared.Projectiles.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server._Battlefield14.Projectiles;

/// <summary>
/// System for handling HEAT (High Explosive Anti-Tank) projectiles.
/// </summary>
public sealed class HeatSystem : EntitySystem
{
    [Dependency] private readonly TriggerSystem _triggerSystem = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly GunSystem _gunSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HeatComponent, ProjectileComponent>();
        while (query.MoveNext(out var uid, out var heat, out var projectile))
        {
            // Check if projectile penetration has stopped
            if (!projectile.ProjectileSpent || heat.ExplosionTriggered)
                continue;

            heat.ExplosionTriggered = true;
            Dirty(uid, heat);

            // Prevent deletion until after explosion
            projectile.DeleteOnCollide = false;
            Dirty(uid, projectile);

            // Fragment if configured (before explosion)
            if (heat.FragmentationPrototype != null && heat.FragmentationCount > 0)
            {
                FragmentProjectile(uid, heat);
            }

            // Trigger explosion (with delay if specified)
            // Make sure ExplodeOnTrigger component exists for delayed explosions
            if (heat.ExplosionDelay <= 0)
            {
                // Instant explosion
                if (TryComp<ExplosiveComponent>(uid, out var explosive))
                {
                    _explosionSystem.TriggerExplosive(uid, explosive, delete: true);
                }
            }
            else
            {
                // Delayed explosion using timer - requires ExplodeOnTrigger component
                _triggerSystem.HandleTimerTrigger(uid, null, heat.ExplosionDelay, 0, null, null);
            }
        }
    }

    private void FragmentProjectile(EntityUid uid, HeatComponent component)
    {
        var projectileCoord = _transformSystem.GetMapCoordinates(uid);
        var segmentAngle = 360f / component.FragmentationCount;

        for (int i = 0; i < component.FragmentationCount; i++)
        {
            var angleMin = segmentAngle * i;
            var angleMax = segmentAngle * (i + 1);
            var angle = Angle.FromDegrees(_random.NextFloat(angleMin, angleMax));
            var direction = angle.ToVec().Normalized();
            var velocity = _random.NextVector2(5f, 10f);

            var fragUid = Spawn(component.FragmentationPrototype, projectileCoord);
            _gunSystem.ShootProjectile(fragUid, direction, velocity, uid, null);
        }
    }
}


