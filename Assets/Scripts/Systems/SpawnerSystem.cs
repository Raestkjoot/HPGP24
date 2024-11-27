using System.Security.Cryptography;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;

partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SpawnerComponent>();
        state.RequireForUpdate<DataSingletonComponent>();       
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var spawner = SystemAPI.GetSingleton<SpawnerComponent>();

        var data = SystemAPI.GetSingleton<DataSingletonComponent>();

        if(data.schedulingType == SchedulingType.Run)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            int n = spawner.armySizeA;
            float y = 0f;
            int armyWidth = (int)math.sqrt(n);
            var aTag = new ArmyATag();

            for (int i = 0; i < armyWidth * armyWidth; i++)
            {
                var e = ECB.Instantiate(spawner.armyUnitPrefab);
                float x = (i % armyWidth) * 2f;
                float z = ((i / armyWidth) % armyWidth) * 2f;

                ECB.AddComponent(e, LocalTransform.FromPosition(spawner.armyAOffset + new float3(x, y, z)));
                ECB.AddComponent(e, aTag);
                ECB.AddComponent(e, new SoldierTag { });
            }

            n = spawner.armySizeB;

            armyWidth = (int)math.sqrt(n);
            var bTag = new ArmyBTag();

            for (int i = 0; i < armyWidth * armyWidth; i++)
            {
                var e = ECB.Instantiate(spawner.armyUnitPrefab);
                float x = (i % armyWidth) * 2f;
                float z = ((i / armyWidth) % armyWidth) * 2f;

                ECB.AddComponent(e, LocalTransform.FromPosition(spawner.armyBOffset + new float3(x, y, z)));
                ECB.AddComponent(e, bTag);
                ECB.AddComponent(e, new SoldierTag { });
            }
        }

        if(data.schedulingType == SchedulingType.Schedule)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new SpawnUnits
            {
                ecb = ECB
            }.Schedule(state.Dependency);
        }

        if(data.schedulingType == SchedulingType.ScheduleParallel)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            state.Dependency = new SpawnUnitsParallel
            {
                ecb = ECB
            }.ScheduleParallel(state.Dependency);
        }
    }
}

[BurstCompile]
public partial struct SpawnUnitsParallel : IJobEntity
{
    
    public EntityCommandBuffer.ParallelWriter ecb;
    public void Execute([ChunkIndexInQuery] int key, in SpawnerComponent spawner)
    {
        int n = spawner.armySizeA;
        float y = 0f;
        int armyWidth = (int)math.sqrt(n);
        var aTag = new ArmyATag();

        for (int i = 0; i < armyWidth * armyWidth; i++)
        {
            var e = ecb.Instantiate(key, spawner.armyUnitPrefab);
            float x = (i % armyWidth) * 2f;
            float z = ((i / armyWidth) % armyWidth) * 2f;

            ecb.AddComponent(key, e, LocalTransform.FromPosition(spawner.armyAOffset + new float3(x, y, z)));
            ecb.AddComponent(key, e, aTag);
            ecb.AddComponent(key,e, new SoldierTag { });
        }

        n = spawner.armySizeB;

        armyWidth = (int)math.sqrt(n);
        var bTag = new ArmyBTag();

        for (int i = 0; i < armyWidth * armyWidth; i++)
        {
            var e = ecb.Instantiate(key, spawner.armyUnitPrefab);
            float x = (i % armyWidth) * 2f;
            float z = ((i / armyWidth) % armyWidth) * 2f;

            ecb.AddComponent(key, e, LocalTransform.FromPosition(spawner.armyBOffset + new float3(x, y, z)));
            ecb.AddComponent(key, e, bTag);
            ecb.AddComponent(key, e, new SoldierTag { });
        }
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
        int armyWidth = (int)math.sqrt(n);
        var aTag = new ArmyATag();

        for (int i = 0; i < armyWidth * armyWidth; i++)
        {
            var e = ecb.Instantiate(spawner.armyUnitPrefab);
            float x = (i % armyWidth) * 2f;
            float z = ((i / armyWidth) % armyWidth) * 2f;

            ecb.AddComponent(e, LocalTransform.FromPosition(spawner.armyAOffset + new float3(x, y, z)));
            ecb.AddComponent(e, aTag);
            ecb.AddComponent(e, new SoldierTag { });
        }

        n = spawner.armySizeB;

        armyWidth = (int)math.sqrt(n);
        var bTag = new ArmyBTag();

        for (int i = 0; i < armyWidth * armyWidth; i++)
        {
            var e = ecb.Instantiate(spawner.armyUnitPrefab);
            float x = (i % armyWidth) * 2f;
            float z = ((i / armyWidth) % armyWidth) * 2f;

            ecb.AddComponent(e, LocalTransform.FromPosition(spawner.armyBOffset + new float3(x, y, z)));
            ecb.AddComponent(e, bTag);
            ecb.AddComponent(e, new SoldierTag { });
        }
    }
}
