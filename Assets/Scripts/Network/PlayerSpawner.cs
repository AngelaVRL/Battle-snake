using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private const string SessionName = "TestRoom";
    private const float SpawnRadius = 8f;

    [SerializeField] private NetworkPrefabRef playerPrefab;

    private readonly Dictionary<PlayerRef, NetworkObject> spawnedPlayers =
        new Dictionary<PlayerRef, NetworkObject>();

    private NetworkRunner runner;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private async void StartSession(GameMode mode)
    {
        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        SceneRef sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        await runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = SessionName,
            Scene = sceneRef,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    private const string DefaultRoomName = "Room";

public void StartHost(string roomName)
{
    StartSession(GameMode.Host, roomName);
}

public void JoinGame(string roomName)
{
    StartSession(GameMode.Client, roomName);
}

private async void StartSession(GameMode mode, string roomName)
{
    if (runner != null)
    {
        return;
    }

    string sessionName = string.IsNullOrWhiteSpace(roomName) ? DefaultRoomName : roomName.Trim();

    runner = gameObject.AddComponent<NetworkRunner>();
    runner.ProvideInput = true;

    SceneRef sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
    NetworkSceneManagerDefault sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

    StartGameResult result = await runner.StartGame(new StartGameArgs
    {
        GameMode = mode,
        SessionName = sessionName,
        Scene = sceneRef,
        SceneManager = sceneManager
    });

    if (!result.Ok)
    {
        Debug.LogError($"Could not start session: {result.ShutdownReason}");
        Destroy(sceneManager);
        Destroy(runner);
        runner = null;
    }
}

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
        {
            return;
        }

        Vector2 spawnPosition = Random.insideUnitCircle * SpawnRadius;
        NetworkObject playerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        spawnedPlayers.Add(player, playerObject);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (spawnedPlayers.TryGetValue(player, out NetworkObject playerObject))
        {
            runner.Despawn(playerObject);
            spawnedPlayers.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData();
        data.AimPoint = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        input.Set(data);
    }

    #region Unused INetworkRunnerCallbacks

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    #endregion
}