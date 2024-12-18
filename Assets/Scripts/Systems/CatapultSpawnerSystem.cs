using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct CatapultSpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CatapultSpawnerComponent>();
        state.RequireForUpdate<DataSingletonComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var spawner = SystemAPI.GetSingleton<CatapultSpawnerComponent>();

        var data = SystemAPI.GetSingleton<DataSingletonComponent>();

        if (data.schedulingType == SchedulingType.Run)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            int n = spawner.spawnAmmount;
            int width = spawner.catapultRowWidth;
            float y = 0f;

            for (int i = 0; i < n; i++)
            {
                var e = ECB.Instantiate(spawner.catapultPrefab);
                float x = (i % width) * 7.5f;
                float z = -(i / width) * 15f;

                ECB.AddComponent(e, LocalTransform.FromPosition(spawner.catapultOffset + new float3(x, y, z)));
            }
        }

        if (data.schedulingType == SchedulingType.Schedule)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            state.Dependency = new spawnCatapults
            {
                ecb = ECB
            }.Schedule(state.Dependency);
        }

        if (data.schedulingType == SchedulingType.ScheduleParallel)
        {
            var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
            state.Dependency = new spawnCatapultsParallel
            {
                ecb = ECB
            }.ScheduleParallel(state.Dependency);
        }
    }
}

[BurstCompile]
public partial struct spawnCatapults : IJobEntity
{

    public EntityCommandBuffer ecb;
    public void Execute(in CatapultSpawnerComponent spawner)
    {
        int n = spawner.spawnAmmount;
        int width = spawner.catapultRowWidth;
        float y = 0f;

        for (int i = 0; i < n; i++)
        {
            var e = ecb.Instantiate(spawner.catapultPrefab);
            float x = (i % width) * 7.5f;
            float z = -(i / width) * 15f;

            ecb.AddComponent(e, LocalTransform.FromPosition(spawner.catapultOffset + new float3(x, y, z)));
        }
    }
}

[BurstCompile]
public partial struct spawnCatapultsParallel : IJobEntity
{

    public EntityCommandBuffer.ParallelWriter ecb;
    public void Execute([ChunkIndexInQuery] int key, in CatapultSpawnerComponent spawner)
    {
        int n = spawner.spawnAmmount;
        int width = spawner.catapultRowWidth;
        float y = 0f;

        for (int i = 0; i < n; i++)
        {
            var e = ecb.Instantiate(key, spawner.catapultPrefab);
            float x = (i % width) * 7.5f;
            float z = -(i / width) * 15f;

            ecb.AddComponent(key, e, LocalTransform.FromPosition(spawner.catapultOffset + new float3(x, y, z)));
        }
    }
}