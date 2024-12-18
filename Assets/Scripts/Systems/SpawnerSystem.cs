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

            int n = spawner.armySize;
            float y = 0f;
            var aTag = new ArmyATag();
            int width = spawner.armyColumnWidth;

            for (int i = 0; i < n; i++)
            {
                var e = ECB.Instantiate(spawner.armyUnitPrefab);
                float z = (i % width) * 4f;
                float x = (i / width) * 4f;

                var trans = new LocalTransform
                {
                    Position = spawner.armyOffset + new float3(x, y, z),
                    Rotation = quaternion.identity,
                    Scale = 2
                };

                ECB.AddComponent<LocalTransform>(e, trans);
                ECB.AddComponent(e, aTag);
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
        int n = spawner.armySize;
        float y = 0f;
        int width = spawner.armyColumnWidth;
        var aTag = new ArmyATag();

        for (int i = 0; i < n; i++)
        {
            var e = ecb.Instantiate(key, spawner.armyUnitPrefab);
            float z = (i % width) * 4f;
            float x = (i / width) * 4f;

            var trans = new LocalTransform
            {
                Position = spawner.armyOffset + new float3(x, y, z),
                Rotation = quaternion.identity,
                Scale = 2
            };

            ecb.AddComponent<LocalTransform>(key, e, trans);
            ecb.AddComponent(key, e, aTag);
            ecb.AddComponent(key,e, new SoldierTag { });
        }
    }
}

[BurstCompile]
public partial struct SpawnUnits : IJobEntity
{

    public EntityCommandBuffer ecb;
    public void Execute(in SpawnerComponent spawner)
    {
        int n = spawner.armySize;
        float y = 0f;
        int width = spawner.armyColumnWidth;
        var aTag = new ArmyATag();

        for (int i = 0; i < n; i++)
        {
            var e = ecb.Instantiate(spawner.armyUnitPrefab);
            float z = (i % width) * 4f;
            float x = (i / width) * 4f;


            var trans = new LocalTransform
            {
                Position = spawner.armyOffset + new float3(x, y, z),
                Rotation = quaternion.identity,
                Scale = 2
            };

            ecb.AddComponent<LocalTransform>(e, trans);
            ecb.AddComponent(e, aTag);
            ecb.AddComponent(e, new SoldierTag { });
        }

    }
}
