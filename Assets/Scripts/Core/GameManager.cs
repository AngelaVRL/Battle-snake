using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public enum GameState
{
    Lobby,
    Playing,
    Results
}

public class GameManager : NetworkBehaviour
{
    public const int MinPlayersToStart = 2;

    private const float RoundDurationSeconds = 180f;
    private const float ResultsDurationSeconds = 10f;
    private const float SpawnAreaFactor = 0.8f;

    [SerializeField] private FoodSpawner foodSpawner;

    public static GameManager Instance { get; private set; }

    [Networked] public GameState State { get; set; }
    [Networked] public PlayerRef Winner { get; set; }
    [Networked] private TickTimer StateTimer { get; set; }

    public float TimeLeft => StateTimer.RemainingTime(Runner) ?? 0f;

    public override void Spawned()
    {
        Instance = this;

        if (Object.HasStateAuthority)
        {
            State = GameState.Lobby;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
        {
            return;
        }

        switch (State)
        {
            case GameState.Playing:
                CheckRoundEnd();
                break;
            case GameState.Results:
                if (StateTimer.Expired(Runner))
                {
                    ReturnToLobby();
                }
                break;
        }
    }

    public void StartRound()
    {
        if (!Object.HasStateAuthority || State != GameState.Lobby)
        {
            return;
        }

        if (NetworkWorm.ActiveWorms.Count < MinPlayersToStart)
        {
            return;
        }

        foreach (NetworkWorm worm in NetworkWorm.ActiveWorms)
        {
            Vector2 position = Random.insideUnitCircle * foodSpawner.MapRadius * SpawnAreaFactor;
            worm.PrepareForRound(position);
        }

        Winner = PlayerRef.None;
        StateTimer = TickTimer.CreateFromSeconds(Runner, RoundDurationSeconds);
        State = GameState.Playing;
    }

    private void CheckRoundEnd()
    {
        int aliveCount = NetworkWorm.ActiveWorms.Count(worm => worm.IsAlive);

        if (aliveCount <= 1 || StateTimer.Expired(Runner))
        {
            EndRound(aliveCount);
        }
    }

    private void EndRound(int aliveCount)
    {
        NetworkWorm winner = aliveCount == 1
            ? NetworkWorm.ActiveWorms.First(worm => worm.IsAlive)
            : NetworkWorm.ActiveWorms.OrderByDescending(worm => worm.Score).FirstOrDefault();

        Winner = winner != null ? winner.Object.InputAuthority : PlayerRef.None;
        StateTimer = TickTimer.CreateFromSeconds(Runner, ResultsDurationSeconds);
        State = GameState.Results;
    }

    private void ReturnToLobby()
    {
        foreach (NetworkWorm worm in NetworkWorm.ActiveWorms)
        {
            worm.SetWaiting();
        }

        State = GameState.Lobby;
    }

    public bool IsHost => Object != null && Object.HasStateAuthority;
    
}