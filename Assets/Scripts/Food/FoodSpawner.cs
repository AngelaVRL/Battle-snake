using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class FoodSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef foodPrefab;
    [SerializeField] private GameVisuals visuals;
    [SerializeField] private ColorPalette palette;
    [SerializeField] private int targetFoodCount = 80;
    [SerializeField] private float mapRadius = 20f;

    private readonly List<NetworkObject> foodItems = new List<NetworkObject>();

    public float MapRadius => mapRadius;

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        while (foodItems.Count < targetFoodCount)
        {
            Vector2 position = Random.insideUnitCircle * mapRadius;
            SpawnFoodAt(position, Random.Range(0, palette.PlayerColorCount));
        }
    }

    public void SpawnFoodAt(Vector2 position, int colorIndex)
    {
        int spriteIndex = Random.Range(0, visuals.FoodSpriteCount);

        NetworkObject food = Runner.Spawn(foodPrefab, position, Quaternion.identity, null,
            (spawnRunner, spawned) =>
            {
                FoodItem item = spawned.GetComponent<FoodItem>();
                item.SpriteIndex = spriteIndex;
                item.ColorIndex = colorIndex;
            });

        foodItems.Add(food);
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
}