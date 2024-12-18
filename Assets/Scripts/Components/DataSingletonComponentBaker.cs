using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
public class DataSingletonComponentBaker : MonoBehaviour
{
    public SchedulingType schedulingType;
    public int spawnAmount;
    public int spawnAmountCatapult;
    public int catapultRowWidth;


    class Baker : Baker<DataSingletonComponentBaker>
    {
        public override void Bake(DataSingletonComponentBaker authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new DataSingletonComponent
            {
                schedulingType = authoring.schedulingType
            });
        }
    }
}

public struct DataSingletonComponent : IComponentData
{
    public SchedulingType schedulingType;
}


public enum SchedulingType
{
    Run,
    Schedule,
    ScheduleParallel
}