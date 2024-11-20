using UnityEngine;
using Unity.Entities;

public class VfxSystemHelper : MonoBehaviour
{
    public ParticleSystem particles;
    void Start()
    {
        World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<VfxSystem>().Init(particles);
    }
}
