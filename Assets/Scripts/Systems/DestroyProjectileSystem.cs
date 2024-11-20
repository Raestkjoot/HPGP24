using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct DestroyProjectileSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
        .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (timer, entity) in SystemAPI.Query<RefRW<Timer>>().WithEntityAccess()) {
            timer.ValueRW.timer -= Time.deltaTime;
            if (timer.ValueRO.timer < 0) { 
                ECB.DestroyEntity(entity);
            }
        }   
    }
}
