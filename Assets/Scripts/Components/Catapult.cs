using Unity.Entities;
using UnityEngine;

public class Catapult : MonoBehaviour
{
    public float retractedRotation;
    public float launchedRotation;
    public GameObject projectile;

    public static readonly Vector2 retractionSpeedRange = new(0.5f, 1.0f);
    public static readonly Vector2 loadingTimeRange = new(0.6f, 2.3f);
    public static readonly Vector2 launchSpeedRange = new(18.0f, 25.0f);

    class CatapultBaker : Baker<Catapult>
    {
        public override void Bake(Catapult authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CatapultComponent
            {
                retractedRotation = authoring.retractedRotation,
                launchedRotation = authoring.launchedRotation,
                retractionSpeed = Random.Range(retractionSpeedRange.x, retractionSpeedRange.y),
                state = CatapultState.Retracting,
                projectile = GetEntity(authoring.projectile, TransformUsageFlags.Dynamic)
            });
        }
    }
}

public struct CatapultComponent : IComponentData
{
    public float retractedRotation;
    public float retractionSpeed;
    public float launchedRotation;
    public float launchSpeed;
    public float loadingTimer;
    public bool isProjectileLoaded;
    public CatapultState state;

    // TODO: Can we make this into a static readonly somewhere?
    // I guess if we spawn the catapults like we do the armies...
    public Entity projectile;
}

public enum CatapultState
{
    Retracting,
    Loading,
    Launching
}