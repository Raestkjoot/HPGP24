using Unity.Entities;
using UnityEngine;

public class CatapultRotationPoint : MonoBehaviour
{
    public float retractedRotation;
    public float launchedRotation;

    public static readonly Vector2 retractionSpeedRange = new(0.8f, 1.2f);
    public static readonly Vector2 loadingTimeRange = new(0.6f, 2.5f);
    public static readonly Vector2 launchSpeedRange = new(18.0f, 25.0f);

    class CatapultRotationPointBaker : Baker<CatapultRotationPoint>
    {
        public override void Bake(CatapultRotationPoint authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CatapultRotationPointComponent
            {
                retractedRotation = authoring.retractedRotation,
                launchedRotation = authoring.launchedRotation,
                retractionSpeed = Random.Range(retractionSpeedRange.x, retractionSpeedRange.y),
                state = CatapultState.Retracting
            });
        }
    }
}

public struct CatapultRotationPointComponent : IComponentData
{
    public float retractedRotation;
    public float retractionSpeed;
    public float launchedRotation;
    public float launchSpeed;
    public float loadingTimer;
    public CatapultState state;
}

public enum CatapultState
{
    Retracting,
    Loading,
    Launching
}