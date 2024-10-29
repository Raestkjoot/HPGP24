using System.Security.Cryptography;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
       state.RequireForUpdate<SpawnerComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var spawner = SystemAPI.GetSingleton<SpawnerComponent>();

        var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

        state.Dependency = new SpawnUnits
        {
            ecb = ECB
        }.Schedule(state.Dependency);
    }
}

[BurstCompile]
public partial struct SpawnUnits : IJobEntity
{
    public EntityCommandBuffer ecb;
    public void Execute(in SpawnerComponent spawner)
    {
        int n = spawner.armySizeA;
        float y = 0f;

        int armyWidth = (int)math.log2(n);
        var aTag = new ArmyATag();

        for (int i = 0; i < n; i++)
        {
            var e = ecb.Instantiate(spawner.armyUnitPrefab);
            float x = i % armyWidth * 2f;
            float z = i / armyWidth;

            ecb.AddComponent(e, LocalTransform.FromPosition(spawner.armyAOffset + new float3(x, y, z)));
            ecb.AddComponent(e, aTag);
        }

        n = spawner.armySizeB;

        armyWidth = (int)math.log2(n);
        var bTag = new ArmyBTag();

        for (int i = 0; i < n; i++)
        {
            var e = ecb.Instantiate(spawner.armyUnitPrefab);
            float x = i % armyWidth * 2f;
            float z = i / armyWidth;

            ecb.AddComponent(e, LocalTransform.FromPosition(spawner.armyBOffset + new float3(x, y, z)));
            ecb.AddComponent(e, bTag);
        }
    }
}
