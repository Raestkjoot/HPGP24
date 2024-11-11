using Unity.Entities;
using UnityEngine;

public class CatapultRotationPoint : MonoBehaviour
{
    public float loadedRotation;
    public float loadSpeed;
    public float launchedRotation;
    public float launchSpeed;

    class CatapultRotationPointBaker : Baker<CatapultRotationPoint>
    {
        public override void Bake(CatapultRotationPoint authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CatapultRotationPointComponent
            {
                loadedRotation = authoring.loadedRotation,
                loadSpeed = authoring.loadSpeed,
                launchedRotation = authoring.launchedRotation,
                launchSpeed = authoring.launchSpeed,
                state = CatapultState.Retracting
            });
        }
    }
}

public struct CatapultRotationPointComponent : IComponentData
{
    public float loadedRotation;
    public float loadSpeed;
    public float launchedRotation;
    public float launchSpeed;
    public CatapultState state;
}

public enum CatapultState
{
    Retracting,
    Loading,
    Launching
}