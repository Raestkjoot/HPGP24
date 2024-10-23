using Unity.Entities;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Vector3 position;
    public float speed;
    public float rotationSpeed;

    class EnemyBaker : Baker<Enemy>
    {
        public override void Bake(Enemy authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemyComponent
            {
                position = authoring.position,
                speed = authoring.speed,
                rotationSpeed = authoring.rotationSpeed
            });
        }
    }
}

public struct EnemyComponent : IComponentData
{
    public Vector3 position;
    public float speed;
    public float rotationSpeed;
}