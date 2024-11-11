using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct CatapultLaunchingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;

        foreach (var (transform, catapultData) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<CatapultRotationPointComponent>>())
        {
            switch (catapultData.ValueRO.state)
            {
                case CatapultState.Retracting:
                    if ((math.Euler(transform.ValueRO.Rotation).z * math.TODEGREES) < catapultData.ValueRO.retractedRotation)
                    {
                        var rot = quaternion.RotateZ(catapultData.ValueRO.retractionSpeed * dt);
                        transform.ValueRW.Rotation = math.mul(transform.ValueRO.Rotation, rot);
                    }
                    else
                    {
                        catapultData.ValueRW.state = CatapultState.Loading;
                        catapultData.ValueRW.loadingTimer = UnityEngine.Random.Range(
                            CatapultRotationPoint.loadingTimeRange.x,
                            CatapultRotationPoint.loadingTimeRange.y);
                    }
                    break;

                case CatapultState.Loading:
                    if (catapultData.ValueRO.loadingTimer > 0.0f)
                    {
                        catapultData.ValueRW.loadingTimer -= dt;
                        // If loadingTimer at halfway point, spawn projectile
                        // Make projectile follow the catapult arm
                    }
                    else
                    {
                        catapultData.ValueRW.state = CatapultState.Launching;
                        catapultData.ValueRW.launchSpeed = UnityEngine.Random.Range(
                            CatapultRotationPoint.launchSpeedRange.x,
                            CatapultRotationPoint.launchSpeedRange.y);
                    }
                    break;

                case CatapultState.Launching:
                    if ((math.Euler(transform.ValueRO.Rotation).z * math.TODEGREES) > catapultData.ValueRO.launchedRotation)
                    {
                        var rot = quaternion.RotateZ(-catapultData.ValueRO.launchSpeed * dt);
                        transform.ValueRW.Rotation = math.mul(transform.ValueRO.Rotation, rot);
                    }
                    else
                    {
                        catapultData.ValueRW.state = CatapultState.Retracting;
                        catapultData.ValueRW.launchSpeed = UnityEngine.Random.Range(
                            CatapultRotationPoint.retractionSpeedRange.x,
                            CatapultRotationPoint.retractionSpeedRange.y);
                    }
                    break;
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
