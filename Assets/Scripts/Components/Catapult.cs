using Unity.Entities;
using UnityEngine;

public class Catapult : MonoBehaviour
{
    public float retractedRotation;
    public float launchedRotation;
    public GameObject loadedProjectile;
    public GameObject launchedProjectile;

    public static readonly Vector2 retractionSpeedRange = new(0.5f, 1.0f);
    public static readonly Vector2 loadingTimeRange = new(0.6f, 1.4f);
    public static readonly float launchSpeed = 12.0f;
    public static readonly Vector2 projectileVelocityRange = new(40.0f, 50.0f);
    public static readonly Vector2 projectileSideVelocityRange = new(-0.4f, 0.4f);
    public static readonly Vector3 ProjectileSpawnOffset = new(0.0f, -8.8f, 0.0f);

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
                loadedProjectile = GetEntity(authoring.loadedProjectile, TransformUsageFlags.Dynamic),
                launchedProjectile = GetEntity(authoring.launchedProjectile, TransformUsageFlags.Dynamic)
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
    public Entity loadedProjectile;
    public Entity launchedProjectile;
}

public enum CatapultState
{
    Retracting,
    Loading,
    Launching
}