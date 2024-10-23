using Unity.Entities;
using UnityEngine;

public class EnemyPrefabToEntity : MonoBehaviour
{
    public GameObject prefab;

    class EnemyPrefabBaker : Baker<EnemyPrefabToEntity>
    {

        public override void Bake(EnemyPrefabToEntity authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyPrefabComponent
            {
                entity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct EnemyPrefabComponent : IComponentData
{
    public Entity entity;
}