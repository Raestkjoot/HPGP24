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
                    // Catapult arm retracts, rotating towards the retracted position.

                    if ((math.Euler(transform.ValueRO.Rotation).z * math.TODEGREES) < catapultData.ValueRO.retractedRotation)
                    {
                        var rot = quaternion.RotateZ(catapultData.ValueRO.retractionSpeed * dt);
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
                    // Catapult loads projectile, projectile is instantiated and set as child of the arm.

                    if (catapultData.ValueRO.loadingTimer > 0.0f)
                    {
                        catapultData.ValueRW.loadingTimer -= dt;

                        // projectile spawn time hardcoded to 0.5, should probably change that
                        if (catapultData.ValueRO.loadingTimer < 0.5f && catapultData.ValueRO.isProjectileLoaded == false)
                        {
                            Vector3 spawnPos = new(-0.5f, 0.0f, 0.0f);

                            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                                .CreateCommandBuffer(state.WorldUnmanaged);
                            var e = ecb.Instantiate(catapultData.ValueRO.loadedProjectile);
                            ecb.AddComponent(e, LocalTransform.FromPosition(spawnPos));
                            ecb.AddComponent(e, new Parent { Value = entity });
                            catapultData.ValueRW.isProjectileLoaded = true;
                        }
                    }
                    else
                    {
                        catapultData.ValueRW.state = CatapultState.Launching;
                        catapultData.ValueRW.launchSpeed = UnityEngine.Random.Range(
                            Catapult.launchSpeedRange.x,
                            Catapult.launchSpeedRange.y);
                    }
                    break;

                case CatapultState.Launching:
                    // Catapult launches projectile, the arm rotates towards launched position. Once reached, the
                    // projectile is un-childed, a rigidbody component is added, and a velocity applied.

                    if ((math.Euler(transform.ValueRO.Rotation).z * math.TODEGREES) > catapultData.ValueRO.launchedRotation)
                    {
                        var rot = quaternion.RotateZ(-catapultData.ValueRO.launchSpeed * dt);
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
                        ecb.AddComponent(launchedProjectile, LocalTransform.FromPosition(projectilePosition));
                        // TODO: Set linear velocity in baker
                        ecb.SetComponent<PhysicsVelocity>(launchedProjectile, new PhysicsVelocity { Linear = new float3(30.0f, 30.0f, 0.0f) });
                        ecb.DestroyEntity(loadedProjectile);

                    }
                    break;
            }
        }
    }
}
