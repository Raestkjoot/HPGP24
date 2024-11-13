using System.Numerics;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;


partial struct ApplyForceOnImpactSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
        .CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (trans, force, projectileEntity) in SystemAPI.Query<RefRO<LocalTransform>,RefRO<ProjectileComponent>>().WithAll<ImpactTag>().WithEntityAccess())
        {
            foreach (var (enemyTrans, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<SoldierTag>().WithEntityAccess())
            {
                var difference = trans.ValueRO.Position - enemyTrans.ValueRO.Position;
                float distance = math.sqrt(
                  math.pow(difference.x, 2f) +
                  math.pow(difference.y, 2f) +
                  math.pow(difference.z, 2f));
                if (distance < 10) {
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

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    
}
