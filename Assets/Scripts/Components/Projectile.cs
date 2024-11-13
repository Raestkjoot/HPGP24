using Unity.Entities;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float maxForce;
    public float minForce;

    class ProjectileBaker : Baker<Projectile>
    {
        public override void Bake(Projectile authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ProjectileComponent
            {
                maxForce = authoring.maxForce,
                minForce = authoring.minForce               
            });
        }
    }
}

public struct ProjectileComponent : IComponentData
{
    public float maxForce;
    public float minForce;
}
