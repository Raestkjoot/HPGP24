using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int armySizeA = 100;
    public int armySizeB = 100;
    public float3 armyAOffset = new float3(100, 0, 0);
    public float3 armyBOffset = new float3(0, 0, 0);
    public GameObject armyUnitPrefab;

    class SpawnerBaker : Baker<Spawner>
    {
        public override void Bake(Spawner authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new SpawnerComponent
            {
                armySizeA = authoring.armySizeA,
                armySizeB = authoring.armySizeB,
                armyAOffset = authoring.armyAOffset,
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