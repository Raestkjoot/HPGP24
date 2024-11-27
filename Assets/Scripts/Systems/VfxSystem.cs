using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

public partial class VfxSystem : SystemBase
{
    ParticleSystem particleSystem;
    Transform particleSystemTransform;
    float timer;
    float interval;
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
            particleSystem.Emit(50);
            ECB.RemoveComponent<VfxEmitterTag>(entity);
        }
    }

    public void Init(ParticleSystem particleSystem) { 
        this.particleSystem = particleSystem;
        particleSystemTransform = particleSystem.transform;
        interval = 1f / particleSystem.emission.rateOverTimeMultiplier;
        Enabled = true;
    }
}
