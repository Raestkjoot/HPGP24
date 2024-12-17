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
        state.RequireForUpdate<DataSingletonComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = (float)SystemAPI.Time.DeltaTime;

        var data = SystemAPI.GetSingleton<DataSingletonComponent>();

        if(data.schedulingType == SchedulingType.Run)
        {
            foreach (var (transform, component) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyComponent>>().WithAll<ArmyATag>().WithNone<StoppedTag>())
            {
                var x_movement = transform.ValueRO.Position.x - (component.ValueRO.speed * deltaTime);
                transform.ValueRW.Position = new float3(x_movement, transform.ValueRO.Position.y, transform.ValueRO.Position.z);
            }
            foreach (var (transform, component) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyComponent>>().WithAll<ArmyBTag>().WithNone<StoppedTag>())
            {
                var x_movement = transform.ValueRO.Position.x + (component.ValueRO.speed * deltaTime);
                transform.ValueRW.Position = new float3(x_movement, transform.ValueRO.Position.y, transform.ValueRO.Position.z);
            }
        }
        
        if(data.schedulingType == SchedulingType.Schedule)
        {
            var moveArmyADependency = new MoveArmyAJob
            {
                deltaTime = deltaTime,

            }.Schedule(state.Dependency);
            state.Dependency = new MoveArmyBJob
            {
                deltaTime = deltaTime,

            }.Schedule(moveArmyADependency);
        }


        if(data.schedulingType == SchedulingType.ScheduleParallel)
        {
            var moveArmyADependency = new MoveArmyAJob
            {
                deltaTime = deltaTime,

            }.ScheduleParallel(state.Dependency);
            state.Dependency = new MoveArmyBJob
            {
                deltaTime = deltaTime,

            }.ScheduleParallel(moveArmyADependency);
        }

    }

}

[WithAll(typeof(ArmyATag))]
[WithNone(typeof(StoppedTag))]
public partial struct MoveArmyAJob : IJobEntity
{
    public float deltaTime;
    public void Execute(ref LocalTransform trans, in EnemyComponent component)
    {
        var x_movement = trans.Position.x - (component.speed * deltaTime);
        trans.Position = new float3(x_movement, trans.Position.y, trans.Position.z);
    }
}

[WithAll(typeof(ArmyBTag))]
[WithNone(typeof(StoppedTag))]
public partial struct MoveArmyBJob : IJobEntity
{
    public float deltaTime;
    public void Execute(ref LocalTransform trans, in EnemyComponent component)
    {
        var x_movement = trans.Position.x + (component.speed * deltaTime);
        trans.Position = new float3(x_movement, trans.Position.y, trans.Position.z);
    }
}