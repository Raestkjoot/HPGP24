using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class CatapultSpawner : MonoBehaviour
{
    public DataSingletonComponentBaker data;
    public GameObject catapultPrefab;

    class CatapultSpawnerBaker : Baker<CatapultSpawner>
    {
        public override void Bake(CatapultSpawner authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new CatapultSpawnerComponent
            {
                spawnAmmount = authoring.data.spawnAmountCatapult,
                catapultRowWidth = authoring.data.catapultRowWidth,
                catapultOffset = authoring.data.spawnPositionCatapult,
                catapultPrefab = GetEntity(authoring.catapultPrefab, TransformUsageFlags.Dynamic)

            });
        }

    }
}


public struct CatapultSpawnerComponent : IComponentData
{
    public int spawnAmmount;
    public int catapultRowWidth;
    public float3 catapultOffset;
    public Entity catapultPrefab;
}
