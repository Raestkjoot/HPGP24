using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public DataSingletonComponentBaker data;
    public float3 armyOffset;
    public GameObject armyUnitPrefab;

    class SpawnerBaker : Baker<Spawner>
    {
        public override void Bake(Spawner authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SpawnerComponent
            {
                armySize = authoring.data.spawnAmount,
                armyOffset = authoring.armyOffset,
                armyUnitPrefab = GetEntity(authoring.armyUnitPrefab, TransformUsageFlags.Dynamic),
                armyColumnWidth = authoring.data.armyColumnWidth
            });
        }
    }
}

public struct SpawnerComponent : IComponentData
{
    public int armySize;
    public float3 armyOffset;
    public Entity armyUnitPrefab;
    public int armyColumnWidth;
}