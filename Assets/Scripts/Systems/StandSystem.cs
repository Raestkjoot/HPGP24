using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct StandSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
        foreach (var (localTransform, entity) in
            SystemAPI.Query<RefRW<LocalTransform>>().WithNone<StoppedTag>().WithAll<EnemyComponent>().WithEntityAccess())
        {

            quaternion currentRotation = localTransform.ValueRO.Rotation;

            float3 entityUp = math.mul(currentRotation, math.up());

            if (math.dot(entityUp, math.up()) < 0.5f) {
                ECB.AddComponent<RotatingTag>(entity);
                quaternion targetRotation = quaternion.identity;
                localTransform.ValueRW.Rotation = targetRotation;
            }

            if (math.dot(entityUp, math.up()) > 0.9f)
            {
                ECB.RemoveComponent<RotatingTag>(entity);
            }

        }
    }
}
