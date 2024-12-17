using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;


partial struct ApplyForceOnImpactSystem : ISystem
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

            foreach (var (trans, force, projectileEntity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<ProjectileComponent>>().WithAll<ImpactTag>().WithEntityAccess())
            {
                foreach (var (enemyTrans, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<SoldierTag>().WithEntityAccess())
                {
                    var difference = trans.ValueRO.Position - enemyTrans.ValueRO.Position;
                    float distance = math.sqrt(
                      math.pow(difference.x, 2f) +
                      math.pow(difference.y, 2f) +
                      math.pow(difference.z, 2f));
                    if (distance < 10)
                    {
                        difference.y = difference.y + 25;
                        var calImpact = -(difference * math.lerp(force.ValueRO.minForce, force.ValueRO.maxForce, math.abs((distance - 10) / 10)));
                        var vel = new PhysicsVelocity
                        {
                            Linear = calImpact
                        };
                        ECB.SetComponent<PhysicsVelocity>(entity, vel);
                    }
                }
                ECB.RemoveComponent<ImpactTag>(projectileEntity);
            }
        }

        if (data.schedulingType == SchedulingType.Schedule)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            var soldierQuery = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, SoldierTag>()
            .Build();

            var soldierEntities = soldierQuery.ToEntityArray(Allocator.TempJob);
            var soldierPositions = soldierQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

            state.Dependency = new ImpactJob
            {
                ecb = ECB,
                soldierEntities = soldierEntities,
                soldierPositions = soldierPositions
                
            }.Schedule(state.Dependency);

            state.Dependency = soldierPositions.Dispose(state.Dependency);
            state.Dependency = soldierEntities.Dispose(state.Dependency);
        }

        if (data.schedulingType == SchedulingType.ScheduleParallel)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var soldierQuery = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, SoldierTag>()
            .Build();

            var soldierEntities = soldierQuery.ToEntityArray(Allocator.TempJob);
            var soldierPositions = soldierQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);

            state.Dependency = new ImpactJobParallel
            {
                ecb = ECB,
                soldierEntities = soldierEntities,
                soldierPositions = soldierPositions

            }.ScheduleParallel(state.Dependency);

            state.Dependency = soldierPositions.Dispose(state.Dependency);
            state.Dependency = soldierEntities.Dispose(state.Dependency);
        }

    }

}


[WithAll(typeof(ImpactTag))]
[BurstCompile]
public partial struct ImpactJob : IJobEntity
{
    public EntityCommandBuffer ecb;
    [ReadOnly] public NativeArray<Entity> soldierEntities;
    [ReadOnly] public NativeArray<LocalTransform> soldierPositions;

    public void Execute(Entity projectileEntity, in LocalTransform projectileTransform, in ProjectileComponent projectileForce)
    {
        float3 projectilePos = projectileTransform.Position;

        for (int i = 0; i < soldierPositions.Length; i++)
        {
            var soldierPos = soldierPositions[i].Position;

            float distance = math.distance(projectilePos, soldierPos);

            if (distance < 10f) // Apply impact if within range
            {
                float3 difference = projectilePos - soldierPos;
                difference.y += 25f;

                float forceMagnitude = math.lerp(
                    projectileForce.minForce,
                    projectileForce.maxForce,
                    math.abs((distance - 10f) / 10f));

                float3 calImpact = -(difference * forceMagnitude);

                var newVelocity = new PhysicsVelocity
                {
                    Linear = calImpact
                };
                ecb.SetComponent<PhysicsVelocity>(soldierEntities[i], newVelocity);
            }
        }

        // Remove ImpactTag from the projectile
        ecb.RemoveComponent<ImpactTag>(projectileEntity);
    }
}


[WithAll(typeof(ImpactTag))]
[BurstCompile]
public partial struct ImpactJobParallel : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter ecb;
    [ReadOnly] public NativeArray<Entity> soldierEntities;
    [ReadOnly] public NativeArray<LocalTransform> soldierPositions;

    public void Execute([ChunkIndexInQuery] int key, Entity projectileEntity, in LocalTransform projectileTransform, in ProjectileComponent projectileForce)
    {
        float3 projectilePos = projectileTransform.Position;

        for (int i = 0; i < soldierPositions.Length; i++)
        {
            var soldierPos = soldierPositions[i].Position;

            float distance = math.distance(projectilePos, soldierPos);

            if (distance < 10f) // Apply impact if within range
            {
                float3 difference = projectilePos - soldierPos;
                difference.y += 25f;

                float forceMagnitude = math.lerp(
                    projectileForce.minForce,
                    projectileForce.maxForce,
                    math.abs((distance - 10f) / 10f));

                float3 calImpact = -(difference * forceMagnitude);

                var newVelocity = new PhysicsVelocity
                {
                    Linear = calImpact
                };
                ecb.SetComponent<PhysicsVelocity>(key, soldierEntities[i], newVelocity);
            }
        }

        // Remove ImpactTag from the projectile
        ecb.RemoveComponent<ImpactTag>(key, projectileEntity);
    }
}
