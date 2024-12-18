using TMPro;
using Unity.Entities;
using UnityEngine;

public class GoalTag : MonoBehaviour
{
    class GoalTagBaker : Baker<GoalTag>
    {
        public override void Bake(GoalTag authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GoalTagComponent
            {
                points = 0
            });
        }
    }
}

public struct GoalTagComponent : IComponentData
{
    public int points;
}
