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
using TMPro;
using UnityEngine;
using Unity.Transforms;

partial struct ColissionSystem : ISystem
{
    public ComponentLookup<ArmyATag> army;
    public ComponentLookup<GoalTagComponent> goal;

    public void OnCreate(ref SystemState state)
    {
        // If we spawn more soldiers dynamically we can move this to update
        army = state.GetComponentLookup<ArmyATag>();
        goal = state.GetComponentLookup<GoalTagComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
        var random = Random.Range(0.0f, 1.0f);

        army.Update(ref state);
        goal.Update(ref state);

        state.Dependency = new TriggerJob
        {
            army = army,
            goal = goal,
            ecb = ECB
        }.Schedule(simulationSingleton, state.Dependency);
    }
}

[BurstCompile]
public struct TriggerJob : ITriggerEventsJob
{
    public ComponentLookup<ArmyATag> army;
    [NativeDisableParallelForRestrictionAttribute]
    public ComponentLookup<GoalTagComponent> goal;
    public EntityCommandBuffer ecb;

    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;

        if (army.HasComponent(entityA) && goal.HasComponent(entityB))
        {
            ecb.DestroyEntity(entityA);
            goal.GetRefRW(entityB).ValueRW.points += 1;

        }
        else if (goal.HasComponent(entityA) && army.HasComponent(entityB))
        {
            ecb.DestroyEntity(entityB);
            goal.GetRefRW(entityA).ValueRW.points += 1;
        }
    }
}
