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
                    if ((math.Euler(transform.ValueRO.Rotation).z * math.TODEGREES) < catapultData.ValueRO.loadedRotation)
                    {
                        var rot = quaternion.RotateZ(catapultData.ValueRO.loadSpeed * dt);
                        transform.ValueRW.Rotation = math.mul(transform.ValueRO.Rotation, rot);
                    }
                    else
                    {
                        catapultData.ValueRW.state = CatapultState.Loading;
                    }
                    break;

                case CatapultState.Loading:
                    break;

                case CatapultState.Launching:
                    break;
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
