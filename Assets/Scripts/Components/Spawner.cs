using Unity.Entities;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public int armySizeA = 100;
    public int armySizeB = 100;
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
                armyUnitPrefab = GetEntity(authoring.armyUnitPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct SpawnerComponent : IComponentData
{
    public int armySizeA;
    public int armySizeB;
    public Entity armyUnitPrefab;
}