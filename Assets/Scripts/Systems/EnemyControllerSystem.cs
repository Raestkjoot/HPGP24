using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct EnemyControllerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = (float)SystemAPI.Time.DeltaTime;
        Debug.Log(deltaTime);
        foreach (var (transform, component) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyComponent>>())
        {
            var x_movement = transform.ValueRO.Position.x + (component.ValueRO.speed * deltaTime);
            transform.ValueRW.Position = new float3(x_movement, transform.ValueRO.Position.y, transform.ValueRO.Position.z);
        }
    }

}
