using Unity.Entities;
using UnityEngine;

class Floor : MonoBehaviour
{
    
}

class FloorBaker : Baker<Floor>
{
    public override void Bake(Floor authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new FloorTag
        {
            
        });
    }
}
