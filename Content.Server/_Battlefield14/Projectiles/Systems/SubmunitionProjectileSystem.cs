using System.Numerics;
using Content.Shared.Projectiles;
using Content.Shared.Projectiles.Components;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Server._Battlefield14.Projectiles;

/// <summary>
/// System for handling submunition projectiles that spawn submunitions after a delay or distance.
/// </summary>
public sealed class SubmunitionProjectileSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SubmunitionProjectileComponent, ProjectileComponent, PhysicsComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var submunition, out var projectile, out var physics, out var xform))
        {
            if (submunition.SubmunitionsSpawned)
                continue;

            var currentVelocity = physics.LinearVelocity;
            var distanceThisFrame = currentVelocity.Length() * frameTime;

            // Update tracking values
            submunition.TimeElapsed += frameTime;
            submunition.DistanceTraveled += distanceThisFrame;

            // Check if we should spawn submunitions
            bool shouldSpawn = false;
            if (submunition.DistanceDelay > 0)
            {
                shouldSpawn = submunition.DistanceTraveled >= submunition.DistanceDelay;
            }
            else
            {
                // If delay is 0 or negative, spawn immediately, otherwise wait
                shouldSpawn = submunition.SubmunitionDelay <= 0 || submunition.TimeElapsed >= submunition.SubmunitionDelay;
            }

            if (!shouldSpawn)
                continue;

            // Spawn submunitions
            SpawnSubmunitions(uid, submunition, projectile, physics, xform);
        }
    }

    private void SpawnSubmunitions(EntityUid uid, SubmunitionProjectileComponent component, 
        ProjectileComponent projectile, PhysicsComponent physics, TransformComponent xform)
    {
        component.SubmunitionsSpawned = true;
        Dirty(uid, component);

        var currentPosition = _transform.GetWorldPosition(xform);
        var currentVelocity = physics.LinearVelocity;
        var baseDirection = currentVelocity.Normalized();
        var baseAngle = baseDirection.ToWorldAngle();
        var mapCoords = new MapCoordinates(currentPosition, xform.MapID);

        // Calculate spread angles
        var spreadPerSub = component.SpreadAngle / Math.Max(1, component.SubmunitionCount - 1);
        var startAngle = baseAngle - Angle.FromDegrees(component.SpreadAngle / 2f);

        for (int i = 0; i < component.SubmunitionCount; i++)
        {
            // Calculate direction for this submunition
            Angle subAngle;
            if (component.SubmunitionCount == 1)
            {
                subAngle = baseAngle;
            }
            else
            {
                subAngle = startAngle + Angle.FromDegrees(spreadPerSub * i);
            }

            var subDirection = subAngle.ToVec();
            
            // Spawn submunition slightly in front
            var spawnOffset = subDirection * 0.3f;
            var spawnPosition = currentPosition + spawnOffset;
            var spawnMapCoords = new MapCoordinates(spawnPosition, xform.MapID);

            var subUid = Spawn(component.SubmunitionPrototype, spawnMapCoords);

            // Transfer shooter/weapon info
            if (TryComp<ProjectileComponent>(subUid, out var subProjectile))
            {
                subProjectile.Shooter = projectile.Shooter;
                subProjectile.Weapon = projectile.Weapon;
                Dirty(subUid, subProjectile);
            }

            // Set velocity (apply multiplier if specified)
            if (TryComp<PhysicsComponent>(subUid, out var subPhysics))
            {
                var baseSpeed = currentVelocity.Length();
                var subSpeed = baseSpeed * component.VelocityMultiplier;
                var subVelocity = subDirection * subSpeed;
                _physics.SetLinearVelocity(subUid, subVelocity, body: subPhysics);
                
                // Set rotation to match velocity direction
                if (TryComp<TransformComponent>(subUid, out var subXform))
                {
                    _transform.SetWorldRotation(subXform, subAngle);
                }
            }
        }

        // Delete the parent projectile
        QueueDel(uid);
    }
}


