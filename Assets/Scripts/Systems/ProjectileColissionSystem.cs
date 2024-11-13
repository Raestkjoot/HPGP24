using System.Diagnostics;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine.EventSystems;

partial struct ProjectileColissionSystem : ISystem
{
    public ComponentLookup<FloorTag> floor;
    public ComponentLookup<AirTimeTag> projectile;

    public void OnCreate(ref SystemState state) {
        floor = state.GetComponentLookup<FloorTag>();
        projectile = state.GetComponentLookup<AirTimeTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

        floor.Update(ref state);
        projectile.Update(ref state);
        state.Dependency = new ProjectileCollisionJob
        {
            floor = floor,
            projectile = projectile,
            ecb = ECB,
        }.Schedule(simulationSingleton, state.Dependency);

    }
}

[BurstCompile]
public struct ProjectileCollisionJob : ICollisionEventsJob
{
    public ComponentLookup<FloorTag> floor;
    public ComponentLookup<AirTimeTag> projectile;
    public EntityCommandBuffer ecb;

    public void Execute(CollisionEvent collisionEvent)
    {
        Entity entityA = collisionEvent.EntityA;
        Entity entityB = collisionEvent.EntityB;
        

        if (floor.HasComponent(entityA) && projectile.HasComponent(entityB))
        {
            ecb.RemoveComponent<AirTimeTag>(entityB);
            ecb.AddComponent<ImpactTag>(entityB);
        }
        else if (projectile.HasComponent(entityA) && floor.HasComponent(entityB))
        {
            ecb.RemoveComponent<AirTimeTag>(entityA);
            ecb.AddComponent<ImpactTag>(entityA);
        }
    }
}