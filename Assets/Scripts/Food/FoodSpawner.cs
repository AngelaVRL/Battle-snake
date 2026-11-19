using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class FoodSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef foodPrefab;
    [SerializeField] private int targetFoodCount = 80;
    [SerializeField] private float mapRadius = 20f;

    private readonly List<NetworkObject> foodItems = new List<NetworkObject>();

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        while (foodItems.Count < targetFoodCount)
        {
            Vector2 position = Random.insideUnitCircle * mapRadius;
            foodItems.Add(Runner.Spawn(foodPrefab, position, Quaternion.identity));
        }
    }

    public int ConsumeFoodAt(Vector2 position, float radius)
    {
        int consumed = 0;
        float sqrRadius = radius * radius;

        for (int i = foodItems.Count - 1; i >= 0; i--)
        {
            Vector2 foodPosition = foodItems[i].transform.position;

            if ((foodPosition - position).sqrMagnitude <= sqrRadius)
            {
                Runner.Despawn(foodItems[i]);
                foodItems.RemoveAt(i);
                consumed++;
            }
        }

        return consumed;
    }

    public float MapRadius => mapRadius;

    public void SpawnFoodAt(Vector2 position)
    {
        foodItems.Add(Runner.Spawn(foodPrefab, position, Quaternion.identity));
    }
}