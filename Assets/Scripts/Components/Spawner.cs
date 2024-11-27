using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public DataSingletonComponentBaker data;
    public int armySizeB = 100;
    public float3 armyBOffset = new float3(0, 0, 0);
    public GameObject armyUnitPrefab;

    class SpawnerBaker : Baker<Spawner>
    {
        public override void Bake(Spawner authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SpawnerComponent
            {
                armySizeA = authoring.data.spawnAmount,
                armySizeB = authoring.armySizeB,
                armyAOffset = authoring.data.spawnPosition,
                armyBOffset = authoring.armyBOffset,
                armyUnitPrefab = GetEntity(authoring.armyUnitPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct SpawnerComponent : IComponentData
{
    public int armySizeA;
    public int armySizeB;
    public float3 armyAOffset;
    public float3 armyBOffset;
    public Entity armyUnitPrefab;
}