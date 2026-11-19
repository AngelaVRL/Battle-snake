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

    public static readonly List<NetworkWorm> ActiveWorms = new List<NetworkWorm>();

    [SerializeField] private WormHeadMover headMover;
    [SerializeField] private WormBody body;
    [SerializeField] private NetworkTransform networkTransform;
    [SerializeField] private GameVisuals visuals;
    [SerializeField] private ColorPalette palette;
    [SerializeField] private int initialSegments = 5;

    [Networked] public int SegmentCount { get; set; }
    [Networked] public int Score { get; set; }
    [Networked] public NetworkBool IsAlive { get; set; }
    [Networked] public int ShapeIndex { get; set; }
    [Networked] public int ColorIndex { get; set; }
    [Networked] public NetworkString<_16> PlayerName { get; set; }

    private FoodSpawner foodSpawner;
    private CameraFollow cameraFollow;
    private int appliedShape = -1;
    private int appliedColor = -1;

    public static NetworkWorm Local { get; private set; }

    public override void Spawned()
    {
        ActiveWorms.Add(this);

        if (Object.HasStateAuthority)
        {
            SegmentCount = initialSegments;
            IsAlive = false;
            foodSpawner = FindObjectOfType<FoodSpawner>();
        }

        if (Object.HasInputAuthority)
        {
            Local = this;
            cameraFollow = Camera.main.GetComponent<CameraFollow>();
            cameraFollow.SetTarget(transform);
            RPC_SetProfile(PlayerProfile.ShapeIndex, PlayerProfile.ColorIndex, PlayerProfile.Name);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        ActiveWorms.Remove(this);

        if (Local == this)
        {
            Local = null;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || !IsAlive)
        {
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
        {
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
        RefreshStyle();
        body.SetHeadVisible(IsAlive);
        body.SetSegmentCount(IsAlive ? SegmentCount : 0);

        if (Object.HasInputAuthority && cameraFollow != null)
        {
            cameraFollow.SetGrowthProgress(body.GrowthProgress);
        }
    }

    public void PrepareForRound(Vector2 position)
    {
        networkTransform.Teleport(position);
        SegmentCount = initialSegments;
        Score = 0;
        IsAlive = true;
    }

    public void SetWaiting()
    {
        IsAlive = false;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetProfile(int shapeIndex, int colorIndex, NetworkString<_16> playerName)
    {
        ShapeIndex = Mathf.Clamp(shapeIndex, 0, visuals.ShapeCount - 1);
        ColorIndex = Mathf.Clamp(colorIndex, 0, palette.PlayerColorCount - 1);
        PlayerName = playerName;
    }

    private void RefreshStyle()
    {
        if (ShapeIndex == appliedShape && ColorIndex == appliedColor)
        {
            return;
        }

        appliedShape = ShapeIndex;
        appliedColor = ColorIndex;

        body.ApplyStyle(
            visuals.GetHeadSprite(ShapeIndex),
            visuals.GetBodySprite(ShapeIndex),
            palette.GetPlayerColor(ColorIndex));
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
            foodSpawner.SpawnFoodAt(position, ColorIndex);
        }
    }
}