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

public class NetworkPlayer : NetworkBehaviour
{ 
    [SyncVar] public PlayerData PlayerData;
    [SyncVar] public int MapID;
    public GameObject LocalMap;
    public GameObject UICanvasPrefab;
    private PlayerSystem localPlayer;

    public UIControlSystem UiControlSystem { get; private set; }
    public event EventHandler<SpiritSystem> SpiritCreatingRequestDone = delegate { };
    public event EventHandler<EnemySystem> EnemyCreatingRequestDone = delegate { };

    public PlayerSystem LocalPlayer
    {
        get => localPlayer;
        set
        {
            if (!isLocalPlayer) return;

            localPlayer = value;
            localPlayer.SpiritPlaceSystem.SpiritCreationRequested += OnSpiritCreatingRequest;
            localPlayer.WaveSystem.EnemyCreationRequested += OnEnemyCreatingRequest;
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

        LoadData();

        base.OnStartLocalPlayer();
    }

    private void OnDestroy() => SaveData();
    private void OnApplicationQuit() => SaveData();

    private void Start()
    {
        if (!isLocalPlayer) return;

        ReferenceHolder.Get.NetworkPlayer = this;
    }

    private void OnEnemyCreatingRequest(object sender, EnemyCreationRequest e)
    {
        if (!isLocalPlayer) return;
        NetworkRequest.Send(e);
    }
 
    private void OnSpiritCreatingRequest(object sender, SpiritCreationRequest e)
    {
        if (!isLocalPlayer) return;
        NetworkRequest.Send(e);
    }

    #region Save-load methods

    private void LoadData()
    {
        if (!StaticMethods.CheckLocalPlayer(this)) return;

        var data = new PlayerData(GameData.Instance.PlayerData.Level, FPClient.Instance.SteamId);
        var userName = FPClient.Instance.Username;

        CmdSendData(gameObject, data, userName);
    }

    private void SaveData()
    {
        if (!StaticMethods.CheckLocalPlayer(this))
            return;

        GameData.Instance.SaveData(new PlayerData(PlayerData.Level));

        if (FPClient.Instance == null)
            return;

        FPClient.Instance.Lobby.OnLobbyStateChanged = null;
        FPClient.Instance.Lobby.OnLobbyMemberDataUpdated = null;
        FPClient.Instance.LobbyList.OnLobbiesUpdated = null;
        FPClient.Instance.Lobby.OnLobbyCreated = null;
        FPClient.Instance.Lobby.OnLobbyJoined = null;
        FPClient.Instance.Lobby.OnChatStringRecieved = null;

        FPClient.Instance.Lobby.Leave();
    }

    [Command]
    private void CmdSendData(GameObject player, PlayerData data, string name)
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
