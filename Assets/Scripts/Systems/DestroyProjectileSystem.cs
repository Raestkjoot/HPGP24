using Unity.Burst;
using Unity.Entities;
using UnityEngine;

partial struct DestroyProjectileSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DataSingletonComponent>();
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var data = SystemAPI.GetSingleton<DataSingletonComponent>();

        var deltaTime = Time.deltaTime;


        if (data.schedulingType == SchedulingType.Run)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (timer, entity) in SystemAPI.Query<RefRW<Timer>>().WithEntityAccess())
            {
                timer.ValueRW.timer -= deltaTime;
                if (timer.ValueRO.timer < 0)
                {
                    ECB.DestroyEntity(entity);
                }
            }
        }

        if (data.schedulingType == SchedulingType.Schedule)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new DestroyProjectile
            {
                ecb = ECB,
                deltaTime = deltaTime
            }.Schedule(state.Dependency);
        }

        if (data.schedulingType == SchedulingType.ScheduleParallel)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            state.Dependency = new DestroyProjectileParallel
            {
                ecb = ECB,
                deltaTime = deltaTime
            }.ScheduleParallel(state.Dependency);
        }
        
    }
}


[BurstCompile]
public partial struct DestroyProjectile : IJobEntity
{
    public EntityCommandBuffer ecb;
    public float deltaTime;

    public void Execute(Entity entity, ref Timer timer)
    {
        timer.timer -= deltaTime;
        if (timer.timer < 0)
        {
            ecb.DestroyEntity(entity);
        }
    }
}

[BurstCompile]
public partial struct DestroyProjectileParallel : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    public float deltaTime;

    public void Execute([ChunkIndexInQuery] int key, Entity entity, ref Timer timer)
    {
        timer.timer -= deltaTime;
        if (timer.timer < 0)
        {
            ecb.DestroyEntity(key, entity);
        }
    }
}