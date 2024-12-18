using UnityEngine;
using Unity.Entities;

public class VfxSystemHelper : MonoBehaviour
{
    public ParticleSystem particles;
    public ParticleSystem particles2;

    void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<VfxSystem>().Init(particles, particles2);
    }
}
