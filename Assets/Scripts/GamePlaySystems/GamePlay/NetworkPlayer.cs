using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FPClient = Facepunch.Steamworks.Client;
using Game.Systems;
using Game.Spirit;
using Game.Cells;
using System;
using Game.Enemy;
using U = UnityEngine.Object;
using Game.Data;

public class NetworkPlayer : NetworkBehaviour
{
    [SyncVar] public PlayerData PlayerData;
    [SyncVar] public int MapID;
    [SyncVar] public GameObject NetEnemy;
    public GameObject LocalMap;
    public GameObject UICanvasPrefab;
    public UIControlSystem UiControlSystem { get; private set; }
    
    float delay = 0.07f;
    WaitForSeconds wfsDelay;

    PlayerSystem localPlayer;
    public PlayerSystem LocalPlayer
    {
        get => localPlayer;
        set
        {
            if (!isLocalPlayer) return;

            localPlayer = value;
            ReferenceHolder.Get.Player.SpiritPlaceSystem.SpiritCreationRequested += OnSpiritCreatingRequest;
            ReferenceHolder.Get.Player.WaveSystem.EnemyCreationRequested += OnEnemyCreatingRequest;
        }
    }

    List<WaveEnemyID> waveEnenmyIDs;
    public List<WaveEnemyID> WaveEnenmyIDs
    {
        get => waveEnenmyIDs;
        set
        {
            waveEnenmyIDs = value;
            ReferenceHolder.Get.NetworkPlayer = this;
        }
    }

    public override void OnStartLocalPlayer()
    {
        if (!isLocalPlayer) return;

        var localMaps = GameObject.FindGameObjectsWithTag("map");
        var uiCanvasPrefab = Instantiate(UICanvasPrefab);

        LocalMap = localMaps[MapID];
        UiControlSystem = uiCanvasPrefab.GetComponent<UIControlSystem>();
        UiControlSystem.IncreaseLevelButtonClicked += (s, e) => CmdIncreaseLevel(PlayerData);
        wfsDelay = new WaitForSeconds(delay);

        LoadData();
        CmdGetWaves();
        base.OnStartLocalPlayer();
    }

    public void WaitAndDo(float delay, Action function)
    {
        StartCoroutine(Wait());

        IEnumerator Wait()
        {
            yield return wfsDelay;
            function.Invoke();
        }
    }

    #region Getting waves from server

    [Command]
    void CmdGetWaves()
    {
        var sendData = (NetworkManager.singleton as ExtendedNetworkManager).NetworkGameManager.WaveEnenmyIDs.Serializer();

        WaitAndDo(delay, () =>
        {
            TargetGetWaves(connectionToClient, sendData);
        });
    }

    [TargetRpc]
    void TargetGetWaves(NetworkConnection conn, byte[] byteData)
    {
        var receivedData = byteData.Deserializer<List<WaveEnemyID>>();
        WaitAndDo(delay, () =>
        {
            WaveEnenmyIDs = receivedData;
        });
    }

    #endregion

    #region Spirit creating request

    void OnSpiritCreatingRequest(object _, SpiritCreationRequest e)
    {
        var sendData = e.Serializer();

        WaitAndDo(delay, () =>
        {
            CmdCreateSpirit(sendData);
        });
    }

    [Command] void CmdCreateSpirit(byte[] byteRequest) => RpcCreateSpirit(byteRequest);

    [ClientRpc]
    void RpcCreateSpirit(byte[] byteRequest)
    {
        var request = byteRequest.Deserializer<SpiritCreationRequest>();

        WaitAndDo(delay, () =>
        {
            var pos = request.Position.ToVector3();
            var choosedCell = ReferenceHolder.Get.Player.CellControlSystem.Cells[request.CellIndex];
            var spirit = ReferenceHolder.Get.SpiritDataBase.Spirits.Elements[request.Element].Rarities[request.Rarity].Spirits[request.DataBaseIndex];

            var newSpirit = isLocalPlayer ?
                StaticMethods.CreateSpirit(spirit, choosedCell) :
                StaticMethods.CreateSpirit(spirit, pos, false);

            ReferenceHolder.Get.Player.SpiritPlaceSystem.NetworkCreateSpirit(newSpirit);
        });
    }

    #endregion

    #region EnemyCreatingRequest

    void OnEnemyCreatingRequest(object _, EnemyCreationRequest e)
    {
        var sendData = e.Serializer();
        WaitAndDo(delay, () => CmdCreateEnemy(sendData));
    }

    [Command]
    public void CmdCreateEnemy(byte[] byteRequest)
    {
        RpcCreateEnemy(byteRequest);
    }

    [ClientRpc]
    public void RpcCreateEnemy(byte[] byteRequest)
    {
        var request = byteRequest.Deserializer<EnemyCreationRequest>();

        WaitAndDo(delay, () =>
        {
            var enemyFromDB = ReferenceHolder.Get.Player.WaveSystem.ListWaves[request.WaveNumber + 1].EnemyTypes[request.PositionInWave];
            var spawnPos = request.Position.ToVector3();
            var waypoints = request.Waypoints.ToVector3Array();

            if (enemyFromDB == null)
                Debug.LogError("enemyfromdb is null");
            else         
                ReferenceHolder.Get.Player.WaveSystem.NetworkSpawnEnemy(
                    StaticMethods.CreateEnemy(enemyFromDB, spawnPos, waypoints, isLocalPlayer), isLocalPlayer);        
        });
    }

    #endregion

    #region Save-load methods

    void OnDestroy() => SaveData();
    void OnApplicationQuit() => SaveData();

    void LoadData()
    {
        if (!isLocalPlayer) return;

        var data = GameData.Instance.PlayerData;
        var userName = FPClient.Instance.Username;

        CmdSendData(gameObject, data, userName);
    }

    void SaveData()
    {
        if (!isLocalPlayer) return;

        GameData.Instance.SaveData(new PlayerData(PlayerData.Level));

        if (FPClient.Instance == null) return;

        FPClient.Instance.Lobby.OnLobbyStateChanged = null;
        FPClient.Instance.Lobby.OnLobbyMemberDataUpdated = null;
        FPClient.Instance.LobbyList.OnLobbiesUpdated = null;
        FPClient.Instance.Lobby.OnLobbyCreated = null;
        FPClient.Instance.Lobby.OnLobbyJoined = null;
        FPClient.Instance.Lobby.OnChatStringRecieved = null;

        FPClient.Instance.Lobby.Leave();
    }

    [Command]
    void CmdSendData(GameObject player, PlayerData data, string name)
    {
        var manager = NetworkManager.singleton as ExtendedNetworkManager;

        for (int i = 0; i < manager.NetworkGameManager.PlayerDatas.Length; i++)
        {
            if (!manager.NetworkGameManager.PlayerDatas[i].IsNotEmpty)
            {
                manager.NetworkGameManager.PlayerDatas[i] = new PlayerData(data.Level, data.SteamID);
                manager.NetworkGameManager.PlayerNames[i] = name;
                break;
            }
        }

        manager.NetworkGameManager.Players.Add(gameObject);
        player.GetComponent<NetworkPlayer>().PlayerData = new PlayerData(data.Level, data.SteamID);

        UpdatePlayersUI(manager);
    }

    #endregion

    #region UI methods

    public void UpdatePlayersUI(ExtendedNetworkManager manager)
    {
        for (int i = 0; i < manager.NetworkGameManager.Players.Count; i++)
        {
            var player = manager.NetworkGameManager.Players[i].GetComponent<NetworkPlayer>();
            player.RpcUpdateUI(manager.NetworkGameManager.PlayerDatas, manager.NetworkGameManager.PlayerNames);
        }
    }


    [ClientRpc]
    public void RpcUpdateUI(PlayerData[] playerDatas, string[] playerNames)
    {
        if (!StaticMethods.CheckLocalPlayer(this)) return;

        UiControlSystem.UpdateUI(playerDatas, playerNames);
    }

    #endregion

    [Command]
    void CmdIncreaseLevel(PlayerData data)
    {
        var manager = NetworkManager.singleton as ExtendedNetworkManager;

        for (int i = 0; i < manager.NetworkGameManager.PlayerDatas.Length; i++)
            if (manager.NetworkGameManager.PlayerDatas[i].Equals(data))
            {
                manager.NetworkGameManager.PlayerDatas[i] = new PlayerData(data.Level + 1, data.SteamID);
                manager.NetworkGameManager.Players[i].GetComponent<NetworkPlayer>().PlayerData = new PlayerData(data.Level + 1, data.SteamID);
                break;
            }

        UpdatePlayersUI(manager);
    }
}
