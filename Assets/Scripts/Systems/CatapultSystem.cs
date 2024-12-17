using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;
using System;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;

partial struct CatapultLaunchingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var data = SystemAPI.GetSingleton<DataSingletonComponent>();

        if (data.schedulingType == SchedulingType.Run)
        {
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
        if (data.schedulingType == SchedulingType.Schedule)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new UpdateCatapult
            {
                dt = dt,
                ecb = ECB,
                childLookup = SystemAPI.GetBufferLookup<Child>()
            }.Schedule(state.Dependency);
        }

        if (data.schedulingType == SchedulingType.ScheduleParallel)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new UpdateCatapultParallel
            {
                dt = dt,
                ecb = ECB,
                childLookup = SystemAPI.GetBufferLookup<Child>()
            }.ScheduleParallel(state.Dependency);
        }
    } // End of Update

    //[BurstCompile] - Not burst because we use random
    public partial struct UpdateCatapult : IJobEntity
    {
        public float dt;
        public EntityCommandBuffer ecb;
        public BufferLookup<Child> childLookup;


        public void Execute(ref LocalTransform transform, ref CatapultComponent catapultData, Entity entity)
        {
            switch (catapultData.state)
            {
                case CatapultState.Retracting:
                    if ((math.Euler(transform.Rotation).x * math.TODEGREES) < catapultData.retractedRotation)
                    {
                        var rot = quaternion.RotateX(-catapultData.retractionSpeed * dt);
                        transform.Rotation = math.mul(transform.Rotation, rot);
                    }
                    else
                    {
                        catapultData.state = CatapultState.Loading;
                        var rand = new System.Random();
                        catapultData.loadingTimer = Catapult.loadingTimeRange.x + (float)rand.NextDouble() * Catapult.loadingTimeRange.y;
                    }
                    break;

                case CatapultState.Loading:
                    if (catapultData.loadingTimer > 0.0f)
                    {
                        catapultData.loadingTimer -= dt;

                        // TODO: projectile spawn time hardcoded, should probably change that
                        if (catapultData.loadingTimer < 0.45f && catapultData.isProjectileLoaded == false)
                        {
                            var e = ecb.Instantiate(catapultData.loadedProjectile);
                            ecb.AddComponent(e, LocalTransform.FromPosition(Catapult.ProjectileSpawnOffset));
                            ecb.AddComponent(e, new Parent { Value = entity });
                            catapultData.isProjectileLoaded = true;
                        }
                    }
                    else
                    {
                        catapultData.state = CatapultState.Launching;
                        catapultData.launchSpeed = Catapult.launchSpeed;
                    }
                    break;

                case CatapultState.Launching:
                    if ((math.Euler(transform.Rotation).x * math.TODEGREES) > catapultData.launchedRotation)
                    {
                        var rot = quaternion.RotateX(catapultData.launchSpeed * dt);
                        transform.Rotation = math.mul(transform.Rotation, rot);
                    }
                    else
                    {
                        catapultData.state = CatapultState.Retracting;
                        var rand = new System.Random();
                        catapultData.launchSpeed = Catapult.retractionSpeedRange.x + (float)rand.NextDouble() * Catapult.retractionSpeedRange.y;
                        catapultData.isProjectileLoaded = false;

                        var loadedProjectile = childLookup[entity].ElementAt(0).Value;

                        var linear = new float3(
                            Catapult.projectileSideVelocityRange.x + (float)rand.NextDouble() * Catapult.projectileSideVelocityRange.y,
                            40.0f,
                            Catapult.projectileVelocityRange.x + (float)rand.NextDouble() * Catapult.projectileVelocityRange.y);

                        var vel = new PhysicsVelocity { Linear = linear };

                        var grav = new PhysicsGravityFactor { Value = 1 };

                        var trans = new LocalTransform
                        {
                            Position = transform.Position + (float3)Catapult.ProjectileLaunchOffset,
                            Rotation = quaternion.identity,
                            Scale = 1.0f
                        };

                        ecb.AddComponent<AirTimeTag>(loadedProjectile);
                        ecb.SetComponent<PhysicsVelocity>(loadedProjectile, vel);
                        ecb.SetComponent<PhysicsGravityFactor>(loadedProjectile, grav);
                        ecb.SetComponent<LocalTransform>(loadedProjectile, trans);
                        ecb.RemoveComponent<Parent>(loadedProjectile);
                    }
                    break;
            }
        }
    }

    //[BurstCompile] - Not burst because we use random
    public partial struct UpdateCatapultParallel : IJobEntity
    {
        public float dt;
        public EntityCommandBuffer.ParallelWriter ecb;
        [NativeDisableParallelForRestrictionAttribute]
        public BufferLookup<Child> childLookup;


        public void Execute([ChunkIndexInQuery] int key, ref LocalTransform transform, ref CatapultComponent catapultData, Entity entity)
        {
            switch (catapultData.state)
            {
                case CatapultState.Retracting:
                    if ((math.Euler(transform.Rotation).x * math.TODEGREES) < catapultData.retractedRotation)
                    {
                        var rot = quaternion.RotateX(-catapultData.retractionSpeed * dt);
                        transform.Rotation = math.mul(transform.Rotation, rot);
                    }
                    else
                    {
                        catapultData.state = CatapultState.Loading;
                        var rand = new System.Random();
                        catapultData.loadingTimer = Catapult.loadingTimeRange.x + (float)rand.NextDouble() * Catapult.loadingTimeRange.y;
                    }
                    break;

                case CatapultState.Loading:
                    if (catapultData.loadingTimer > 0.0f)
                    {
                        catapultData.loadingTimer -= dt;

                        // TODO: projectile spawn time hardcoded, should probably change that
                        if (catapultData.loadingTimer < 0.45f && catapultData.isProjectileLoaded == false)
                        {
                            var e = ecb.Instantiate(key, catapultData.loadedProjectile);
                            ecb.AddComponent(key, e, LocalTransform.FromPosition(Catapult.ProjectileSpawnOffset));
                            ecb.AddComponent(key, e, new Parent { Value = entity });
                            catapultData.isProjectileLoaded = true;
                        }
                    }
                    else
                    {
                        catapultData.state = CatapultState.Launching;
                        catapultData.launchSpeed = Catapult.launchSpeed;
                    }
                    break;

                case CatapultState.Launching:
                    if ((math.Euler(transform.Rotation).x * math.TODEGREES) > catapultData.launchedRotation)
                    {
                        var rot = quaternion.RotateX(catapultData.launchSpeed * dt);
                        transform.Rotation = math.mul(transform.Rotation, rot);
                    }
                    else
                    {
                        catapultData.state = CatapultState.Retracting;
                        var rand = new System.Random();
                        catapultData.launchSpeed = Catapult.retractionSpeedRange.x + (float)rand.NextDouble() * Catapult.retractionSpeedRange.y;
                        catapultData.isProjectileLoaded = false;

                        var loadedProjectile = childLookup[entity].ElementAt(0).Value;

                        var linear = new float3(
                            Catapult.projectileSideVelocityRange.x + (float)rand.NextDouble() * Catapult.projectileSideVelocityRange.y,
                            40.0f,
                            Catapult.projectileVelocityRange.x + (float)rand.NextDouble() * Catapult.projectileVelocityRange.y);

                        var vel = new PhysicsVelocity { Linear = linear };

                        var grav = new PhysicsGravityFactor { Value = 1 };

                        var trans = new LocalTransform
                        {
                            Position = transform.Position + (float3)Catapult.ProjectileLaunchOffset,
                            Rotation = quaternion.identity,
                            Scale = 1.0f
                        };

                        ecb.AddComponent<AirTimeTag>(key, loadedProjectile);
                        ecb.SetComponent<PhysicsVelocity>(key, loadedProjectile, vel);
                        ecb.SetComponent<PhysicsGravityFactor>(key, loadedProjectile, grav);
                        ecb.SetComponent<LocalTransform>(key, loadedProjectile, trans);
                        ecb.RemoveComponent<Parent>(key, loadedProjectile);
                    }
                    break;
            }
        }
    }
}
