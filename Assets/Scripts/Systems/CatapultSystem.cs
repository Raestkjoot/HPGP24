using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

partial struct CatapultLaunchingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (transform, catapultData, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<CatapultComponent>>().WithEntityAccess())
        {
            switch (catapultData.ValueRO.state)
            {
                case CatapultState.Retracting:
                    if ((math.Euler(transform.ValueRO.Rotation).x * math.TODEGREES) < catapultData.ValueRO.retractedRotation)
                    {
                        var rot = quaternion.RotateX(-catapultData.ValueRO.retractionSpeed * dt);
                        transform.ValueRW.Rotation = math.mul(transform.ValueRO.Rotation, rot);
                    }
                    else
                    {
                        catapultData.ValueRW.state = CatapultState.Loading;
                        catapultData.ValueRW.loadingTimer = UnityEngine.Random.Range(
                            Catapult.loadingTimeRange.x,
                            Catapult.loadingTimeRange.y);
                    }
                    break;

                case CatapultState.Loading:
                    if (catapultData.ValueRO.loadingTimer > 0.0f)
                    {
                        catapultData.ValueRW.loadingTimer -= dt;

                        // TODO: projectile spawn time hardcoded, should probably change that
                        if (catapultData.ValueRO.loadingTimer < 0.45f && catapultData.ValueRO.isProjectileLoaded == false)
                        {
                            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                                .CreateCommandBuffer(state.WorldUnmanaged);
                            var e = ecb.Instantiate(catapultData.ValueRO.loadedProjectile);
                            ecb.AddComponent(e, LocalTransform.FromPosition(Catapult.ProjectileSpawnOffset));
                            ecb.AddComponent(e, new Parent { Value = entity });
                            catapultData.ValueRW.isProjectileLoaded = true;
                        }
                    }
                    else
                    {
                        catapultData.ValueRW.state = CatapultState.Launching;
                        catapultData.ValueRW.launchSpeed = Catapult.launchSpeed;
                    }
                    break;

                case CatapultState.Launching:
                    if ((math.Euler(transform.ValueRO.Rotation).x * math.TODEGREES) > catapultData.ValueRO.launchedRotation)
                    {
                        var rot = quaternion.RotateX(catapultData.ValueRO.launchSpeed * dt);
                        transform.ValueRW.Rotation = math.mul(transform.ValueRO.Rotation, rot);
                    }
                    else
                    {
                        catapultData.ValueRW.state = CatapultState.Retracting;
                        catapultData.ValueRW.launchSpeed = UnityEngine.Random.Range(
                            Catapult.retractionSpeedRange.x,
                            Catapult.retractionSpeedRange.y);

                        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                            .CreateCommandBuffer(state.WorldUnmanaged);

                        catapultData.ValueRW.isProjectileLoaded = false;
                        var loadedProjectile = SystemAPI.GetBuffer<Child>(entity).ElementAt(0).Value;
                        var projectilePosition = SystemAPI.GetComponent<LocalToWorld>(loadedProjectile).Position;
                        var launchedProjectile = ecb.Instantiate(catapultData.ValueRO.launchedProjectile);
                        ecb.AddComponent<LocalTransform>(launchedProjectile, LocalTransform.FromPosition(projectilePosition));
                        ecb.AddComponent<AirTimeTag>(launchedProjectile);
                        var vel = new PhysicsVelocity
                        {
                            Linear = new float3(
                            UnityEngine.Random.Range(Catapult.projectileSideVelocityRange.x, Catapult.projectileSideVelocityRange.y),
                            UnityEngine.Random.Range(Catapult.projectileVelocityRange.x, Catapult.projectileVelocityRange.y),
                            UnityEngine.Random.Range(Catapult.projectileVelocityRange.x, Catapult.projectileVelocityRange.y))
                        };
                        ecb.SetComponent<PhysicsVelocity>(launchedProjectile, vel);
                        ecb.DestroyEntity(loadedProjectile);

                    }
                    break;
            }
        }
    }
}
