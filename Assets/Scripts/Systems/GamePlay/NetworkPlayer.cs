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

    #region Spirit creating request

    private void OnSpiritCreatingRequest(object _, SpiritCreationRequest e) => CmdCreateSpirit(JsonUtility.ToJson(e));
    [Command]
    private void CmdCreateSpirit(string jsonRequest) => RpcCreateSpirit(jsonRequest);
    [ClientRpc]
    private void RpcCreateSpirit(string jsonRequest)
    {
        var request = JsonUtility.FromJson<SpiritCreationRequest>(jsonRequest);
        var choosedCell = ReferenceHolder.Get.Player.CellControlSystem.Cells.Find(x => x.transform.position == request.Position);
        var newSpirit = choosedCell != null ?
            StaticMethods.CreateSpirit(request.Data, choosedCell.GetComponent<Cell>(), ReferenceHolder.Get.Player) :
            StaticMethods.CreateSpirit(request.Data, request.Position, ReferenceHolder.Get.Player);

        SpiritCreatingRequestDone?.Invoke(null, newSpirit);
    }

    #endregion

    #region EnemyCreatingRequest

    private void OnEnemyCreatingRequest(object _, EnemyCreationRequest e) => CmdCreateEnemy(JsonUtility.ToJson(e));
    [Command]
    private void CmdCreateEnemy(string jsonRequest) => RpcCreateEnemy(jsonRequest);
    [ClientRpc]
    private void RpcCreateEnemy(string jsonRequest)
    {
        var request = JsonUtility.FromJson<EnemyCreationRequest>(jsonRequest);
        var newEnemy = StaticMethods.CreateEnemy(request.Data, request.Position, ReferenceHolder.Get.Player);

        EnemyCreatingRequestDone?.Invoke(null, newEnemy);
    }

    #endregion

    #region Save-load methods

    private void LoadData()
    {
        if (!isLocalPlayer) return;

        var data = GameData.Instance.PlayerData;
        var userName = FPClient.Instance.Username;


        CmdSendData(gameObject, data, userName);
    }

    private void SaveData()
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
