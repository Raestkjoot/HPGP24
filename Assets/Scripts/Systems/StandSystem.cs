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
        foreach (var (localTransform, enemy) in
            SystemAPI.Query<RefRW<LocalTransform>, RefRO<EnemyComponent>>().WithNone<StoppedTag>())
        {
            quaternion currentRotation = localTransform.ValueRO.Rotation;

            float3 entityUp = math.mul(currentRotation, math.up());

            if (math.dot(entityUp, math.up()) < 0.5f) {
                quaternion targetRotation = quaternion.identity;
                localTransform.ValueRW.Rotation = math.slerp(currentRotation,targetRotation, SystemAPI.Time.DeltaTime * enemy.ValueRO.rotationSpeed);
            }
        }
    }
}
