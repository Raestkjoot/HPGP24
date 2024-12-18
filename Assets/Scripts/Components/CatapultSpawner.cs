using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class CatapultSpawner : MonoBehaviour
{
    public DataSingletonComponentBaker data;
    public float3 catapultOffset;
    public GameObject catapultBasePrefab;
    public Quaternion catapultBaseRotation;
    public GameObject catapultArmPrefab;
    public Quaternion catapultArmRotation;

    class CatapultSpawnerBaker : Baker<CatapultSpawner>
    {
        public override void Bake(CatapultSpawner authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new CatapultSpawnerComponent
            {
                spawnAmmount = authoring.data.spawnAmountCatapult,
                catapultRowWidth = authoring.data.catapultRowWidth,
                catapultOffset = authoring.catapultOffset,
                catapultBasePrefab = GetEntity(authoring.catapultBasePrefab, TransformUsageFlags.Dynamic),
                catapultBaseRotation = authoring.catapultBaseRotation,
                catapultArmPrefab = GetEntity(authoring.catapultArmPrefab, TransformUsageFlags.Dynamic),
                catapultArmRotation = authoring.catapultArmRotation

            });
        }

    }
}


public struct CatapultSpawnerComponent : IComponentData
{
    public int spawnAmmount;
    public int catapultRowWidth;
    public float3 catapultOffset;
    public Entity catapultBasePrefab;
    public Quaternion catapultBaseRotation;
    public Entity catapultArmPrefab;
    public Quaternion catapultArmRotation;
}
