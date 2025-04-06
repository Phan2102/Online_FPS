using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;

public class Spawner : SimulationBehaviour, INetworkRunnerCallbacks
{
    public NetworkPlayer playerPrefab;

    Dictionary<int, NetworkPlayer> mapTokenIDWithNetworkPlayer;

    //list of bots
    List<NetworkPlayer> botList = new List<NetworkPlayer>();

    bool isBotSpawned = false;

    //component
    CharacterInputHandler characterInputHandler;
    SessionListUIHandler sessionListUIHandler;

    void Awake()
    {
        mapTokenIDWithNetworkPlayer = new Dictionary<int, NetworkPlayer>();

        sessionListUIHandler = FindObjectOfType<SessionListUIHandler>(true);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void SpawnBots()
    {
        if(isBotSpawned)
            return;

        int numberOfBotsToSpawn = 3;

        Debug.Log($"so bot dc spawn {numberOfBotsToSpawn}. {botList}. {Runner.SessionInfo.PlayerCount}");

        for(int i = 0; i < numberOfBotsToSpawn; i++)
        {
            NetworkPlayer spawnedAIPlayer = Runner.Spawn(playerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, null, InitializeBotBeforeSpawn);

            botList.Add(spawnedAIPlayer);
        }
        isBotSpawned = true;
    }

    void InitializeBotBeforeSpawn(NetworkRunner runner, NetworkObject networkObject)
    {
        networkObject.GetComponent<NetworkPlayer>().isBot = true;
    }

    int GetPlayerToken(NetworkRunner runner, PlayerRef player)
    {
        if(runner.LocalPlayer == player)
        {
            return ConnectionTokeUtils.HashToken(GameManager.instance.GetConnectionToken());
        }
        else
        {
            var token = runner.GetPlayerConnectionToken(player);

            if(token != null)
                return ConnectionTokeUtils.HashToken(token);

            Debug.LogError("GetPlayerToken return invalid token");

            return 0;
        }
    }

    public void SetConnectionTokenMapping(int token, NetworkPlayer networkPlayer)
    {
        mapTokenIDWithNetworkPlayer.Add(token, networkPlayer);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            int playerToken = GetPlayerToken(runner, player);


            Debug.Log($"OnplayerJoined we are server. connection token {playerToken}");

            if(mapTokenIDWithNetworkPlayer.TryGetValue(playerToken, out NetworkPlayer networkPlayer))
            {
                //Debug.LogError($"found old connecction token {playerToken}. assigning controlls to that player ");

                networkPlayer.GetComponent<NetworkObject>().AssignInputAuthority(player);

                networkPlayer.Spawned();
            }
            else
            {
                Debug.Log($"spawning new player with connection token {playerToken}");

                bool isReadyScene = SceneManager.GetActiveScene().name == "Ready";

                Vector3 spawnPosition = Utils.GetRandomSpawnPoint();

                if (isReadyScene)
                {
                    if (runner.SessionInfo.MaxPlayers - player.PlayerId == 1)
                        spawnPosition = new Vector3(-1 * 3, 1, 0);
                    else
                        spawnPosition = new Vector3(player.PlayerId * 3, 1, 0);
                        
                }



                NetworkPlayer spawnedNetworkPlayer = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
                spawnedNetworkPlayer.transform.position = spawnPosition;

                spawnedNetworkPlayer.token = playerToken;

                mapTokenIDWithNetworkPlayer[playerToken] = spawnedNetworkPlayer;
            }
        }
        else Debug.Log("OnPlayerJoined");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (characterInputHandler == null && NetworkPlayer.Local != null)
            characterInputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();

        if (characterInputHandler != null)
            input.Set(characterInputHandler.GetNetworkInput());
    }
   

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("OnShutdown");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log("OnDisconnectedFromServer");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("OnConnectRequest");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log("OnConnectFailed");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) 
    {
        if (sessionListUIHandler == null)
            return;

        if(sessionList.Count == 0)
        {
            Debug.Log("Joined lobby no session found");

            sessionListUIHandler.OnNoSessionsFound();
        }
        else
        {
            sessionListUIHandler.ClearList();

            foreach (SessionInfo sessionInfo in sessionList)
            {
                sessionListUIHandler.AddToList(sessionInfo);

                Debug.Log($"Found session {sessionInfo.Name} playerCount {sessionInfo.PlayerCount}");
            }
        }
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("OnHostMigration");

        await runner.Shutdown(shutdownReason: ShutdownReason.HostMigration);

        FindObjectOfType<NetworkRunnerHandler>().StartHostMigration(hostMigrationToken);
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }

    public void OnSceneLoadDone(NetworkRunner runner) 
    {
        /*if (SceneManager.GetActiveScene().name != "Ready" && runner.IsServer)
            SpawnBots();*/
    }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnHostMigrationCleanup()
    {
        Debug.Log("OnHostMigrationCleanup started ");

        foreach(KeyValuePair<int, NetworkPlayer> entry in mapTokenIDWithNetworkPlayer)
        {
            NetworkObject networkObjectInDictionary = entry.Value.GetComponent<NetworkObject>();

            if(networkObjectInDictionary.InputAuthority.IsNone)
            {
                Debug.Log($"Destroying network object with token {entry.Value.nickName}");
                networkObjectInDictionary.Runner.Despawn(networkObjectInDictionary);
            }
        }

        Debug.Log("OnHostMigrationCleanup completed ");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
       
    }

    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }
}
