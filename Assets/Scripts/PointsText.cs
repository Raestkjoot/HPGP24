using System.Collections;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class PointsText : MonoBehaviour
{
    private EntityManager _entityManager;
    private Entity _goalEntity;
    private TextMeshProUGUI _text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private IEnumerator Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        // Wait for ECS world to load
        yield return new WaitForSeconds(0.2f);
        _goalEntity = _entityManager.CreateEntityQuery(typeof(GoalTagComponent)).GetSingletonEntity();
    }

    // Update is called once per frame
    private void Update()
    {
        int curPoints = _entityManager.GetComponentData<GoalTagComponent>(_goalEntity).points;
        _text.text = "Points: " + curPoints;
    }
}
