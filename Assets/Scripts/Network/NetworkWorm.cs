using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkWorm : NetworkBehaviour
{
    private const float EatRadius = 0.6f;
    private const float HeadRadius = 0.25f;
    private const float SegmentRadius = 0.2f;
    private const int SegmentsPerFood = 1;
    private const int DeathFoodStep = 2;
    private const float RespawnDelaySeconds = 3f;
    private const float RespawnAreaFactor = 0.8f;

    public static readonly List<NetworkWorm> ActiveWorms = new List<NetworkWorm>();

    [SerializeField] private WormHeadMover headMover;
    [SerializeField] private WormBody body;
    [SerializeField] private NetworkTransform networkTransform;
    [SerializeField] private int initialSegments = 5;

    [Networked] public int SegmentCount { get; set; }
    [Networked] public int Score { get; set; }
    [Networked] public NetworkBool IsAlive { get; set; }
    [Networked] private TickTimer RespawnTimer { get; set; }

    private FoodSpawner foodSpawner;
    private CameraFollow cameraFollow;

    public override void Spawned()
    {
        ActiveWorms.Add(this);

        if (Object.HasStateAuthority)
        {
            SegmentCount = initialSegments;
            IsAlive = true;
            foodSpawner = FindObjectOfType<FoodSpawner>();
        }

        if (Object.HasInputAuthority)
        {
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
            cameraFollow.SetTarget(transform);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        ActiveWorms.Remove(this);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        if (!IsAlive)
        {
            TryRespawn();
            return;
        }

        if (GetInput(out NetworkInputData input))
        {
            Vector2 direction = input.AimPoint - (Vector2)transform.position;
            headMover.Step(direction.normalized, Runner.DeltaTime);
        }

        Eat();
        CheckCollisions();
    }

    public override void Render()
    {
        body.SetHeadVisible(IsAlive);
        body.SetSegmentCount(IsAlive ? SegmentCount : 0);

        if (Object.HasInputAuthority)
        {
            cameraFollow.SetGrowthProgress(body.GrowthProgress);
        }
    }

    private void Eat()
    {
        int eaten = foodSpawner.ConsumeFoodAt(transform.position, EatRadius * body.Thickness);
        SegmentCount += eaten * SegmentsPerFood;
    }

    private void CheckCollisions()
    {
        float headHitRadius = HeadRadius * body.Thickness;

        foreach (NetworkWorm other in ActiveWorms)
        {
            if (other == this || !other.IsAlive)
            {
                continue;
            }

            float hitRadius = headHitRadius + SegmentRadius * other.body.Thickness;

            if (other.body.OverlapsPoint(transform.position, hitRadius))
            {
                Die(other);
                return;
            }
        }
    }

    private void Die(NetworkWorm killer)
    {
        IsAlive = false;
        killer.Score += SegmentCount;

        foreach (Vector2 position in body.GetSegmentPositions(DeathFoodStep))
        {
            foodSpawner.SpawnFoodAt(position);
        }

        RespawnTimer = TickTimer.CreateFromSeconds(Runner, RespawnDelaySeconds);
    }

    // Temporary: replaced by the round system in the next stage.
    private void TryRespawn()
    {
        if (!RespawnTimer.Expired(Runner))
        {
            return;
        }

        Vector2 position = Random.insideUnitCircle * foodSpawner.MapRadius * RespawnAreaFactor;
        networkTransform.Teleport(position);
        SegmentCount = initialSegments;
        IsAlive = true;
    }

    // Temporary debug label: replaced by the real HUD later.
    private void OnGUI()
    {
        if (!Object.IsValid || !Object.HasInputAuthority)
        {
            return;
        }

        GUI.Label(new Rect(10, 110, 400, 30), $"Score: {Score}  Length: {SegmentCount}  Alive: {IsAlive}");
    }
}