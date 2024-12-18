using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Jobs;

public partial class VfxSystem : SystemBase
{
    ParticleSystem particleSystem;
    ParticleSystem particleSystem2;

    Transform particleSystemTransform;
    Transform particleSystemTransform2;

    protected override void OnCreate()
    {
        base.OnCreate();
        Enabled = false;
    }

    protected override void OnUpdate()
    {
        var ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(EntityManager.WorldUnmanaged);

        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<VfxEmitterTag>().WithEntityAccess())
        {
            particleSystemTransform.position = transform.ValueRO.Position;
            particleSystemTransform2.position = transform.ValueRO.Position;
            particleSystem.Emit(50);
            particleSystem2.Emit(50);
            ECB.RemoveComponent<VfxEmitterTag>(entity);
        }

    }

    public void Init(ParticleSystem particleSystem, ParticleSystem particleSystem2) { 
        this.particleSystem = particleSystem;
        this.particleSystem2 = particleSystem2;

        particleSystemTransform = particleSystem.transform;
        particleSystemTransform2 = particleSystem2.transform;

        Enabled = true;
    }
}
