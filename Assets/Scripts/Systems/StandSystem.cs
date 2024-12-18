using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct StandSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DataSingletonComponent>();
    }
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var data = SystemAPI.GetSingleton<DataSingletonComponent>();

        if (data.schedulingType == SchedulingType.Run)
        {

            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (localTransform, entity) in
                SystemAPI.Query<RefRW<LocalTransform>>().WithNone<StoppedTag>().WithAll<EnemyComponent>().WithEntityAccess())
            {

                quaternion currentRotation = localTransform.ValueRO.Rotation;

                float3 entityUp = math.mul(currentRotation, math.up());

                if (math.dot(entityUp, math.up()) < 0.5f)
                {
                    ECB.AddComponent<RotatingTag>(entity);
                    quaternion targetRotation = quaternion.identity;
                    localTransform.ValueRW.Rotation = targetRotation;
                }
            }

            foreach (var (localTransform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithAll<RotatingTag>().WithEntityAccess())
            {
                quaternion currentRotation = localTransform.ValueRO.Rotation;

                float3 entityUp = math.mul(currentRotation, math.up());

                if (math.dot(entityUp, math.up()) >= 0.7f)
                {
                    ECB.RemoveComponent<RotatingTag>(entity);
                }
            }
        }

        if (data.schedulingType == SchedulingType.Schedule)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var jobHandle = new RotateArmy
            {
                ecb = ECB
            }.Schedule(state.Dependency);

            state.Dependency = new RemoveRotateTag
            {
                ecb = ECB
            }.Schedule(jobHandle);
        }

        if (data.schedulingType == SchedulingType.ScheduleParallel)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            var jobHandle = new RotateArmyParallel
            {
                ecb = ECB
            }.ScheduleParallel(state.Dependency);

            state.Dependency = new RemoveRotateTagParallel
            {
                ecb = ECB
            }.ScheduleParallel(jobHandle);
        }
    }
}


[WithNone(typeof(StoppedTag))]
[WithAll(typeof(EnemyComponent))]
[BurstCompile]
public partial struct RotateArmy : IJobEntity
{

    public EntityCommandBuffer ecb;
    public void Execute(Entity entity, ref LocalTransform transform)
    {
        quaternion currentRotation = transform.Rotation;

        float3 entityUp = math.mul(currentRotation, math.up());

        if (math.dot(entityUp, math.up()) < 0.5f)
        {
            ecb.AddComponent<RotatingTag>(entity);
            quaternion targetRotation = quaternion.identity;
            transform.Rotation = targetRotation;
        }
    }
}


[WithAll(typeof(RotatingTag))]
[BurstCompile]
public partial struct RemoveRotateTag : IJobEntity
{

    public EntityCommandBuffer ecb;
    public void Execute(Entity entity, ref LocalTransform transform)
    {
        quaternion currentRotation = transform.Rotation;

        float3 entityUp = math.mul(currentRotation, math.up());

        if (math.dot(entityUp, math.up()) > 0.9f)
        {
            ecb.RemoveComponent<RotatingTag>(entity);
        }
    }
}

[WithNone(typeof(StoppedTag))]
[WithAll(typeof(EnemyComponent))]
[BurstCompile]
public partial struct RotateArmyParallel : IJobEntity
{

    public EntityCommandBuffer.ParallelWriter ecb;
    public void Execute([ChunkIndexInQuery] int key, Entity entity, ref LocalTransform transform)
    {
        quaternion currentRotation = transform.Rotation;

        float3 entityUp = math.mul(currentRotation, math.up());

        if (math.dot(entityUp, math.up()) < 0.5f)
        {
            ecb.AddComponent<RotatingTag>(key, entity);
            quaternion targetRotation = quaternion.identity;
            transform.Rotation = targetRotation;
        }

    }
}


[WithAll(typeof(RotatingTag))]
[BurstCompile]
public partial struct RemoveRotateTagParallel : IJobEntity
{

    public EntityCommandBuffer.ParallelWriter ecb;
    public void Execute([ChunkIndexInQuery] int key, Entity entity, ref LocalTransform transform)
    {
        quaternion currentRotation = transform.Rotation;

        float3 entityUp = math.mul(currentRotation, math.up());

        if (math.dot(entityUp, math.up()) > 0.9f)
        {
            ecb.RemoveComponent<RotatingTag>(key, entity);
        }
    }
}
