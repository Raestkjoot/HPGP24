using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;

partial struct ColissionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
        var random = Random.Range(0.0f, 1.0f);

        state.Dependency = new TriggerJob
        {
            allArmyA = state.GetComponentLookup<ArmyATag>(),
            allArmyB = state.GetComponentLookup<ArmyBTag>(),
            ecb = ECB,
            random = random
        }.Schedule(simulationSingleton, state.Dependency);

    }
}

[BurstCompile]
public struct TriggerJob : ITriggerEventsJob
{
    public ComponentLookup<ArmyATag> allArmyA;
    public ComponentLookup<ArmyBTag> allArmyB;
    public EntityCommandBuffer ecb;
    public float random;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        if (allArmyA.HasComponent(entityA) && allArmyB.HasComponent(entityB))
        {
            if(random >= 0.5f){
                ecb.DestroyEntity(entityA);
            }
            else
            {
                ecb.DestroyEntity(entityB);
            }
        }
        else if (allArmyB.HasComponent(entityA) && allArmyA.HasComponent(entityB))
        {
            if (random >= 0.5f)
            {
                ecb.DestroyEntity(entityA);
            }
            else
            {
                ecb.DestroyEntity(entityB);
            }
        }
    }
}
