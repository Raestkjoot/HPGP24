using Unity.Entities;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    class ProjectileBaker : Baker<Projectile>
    {
        public override void Bake(Projectile authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ProjectileComponent
            {

            });
        }
    }
}

public struct ProjectileComponent : IComponentData
{
}