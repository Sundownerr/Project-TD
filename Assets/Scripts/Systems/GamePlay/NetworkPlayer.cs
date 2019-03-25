﻿using System.Collections;
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
    public int MapID;
    public GameObject LocalMap;
    public GameObject UICanvasPrefab;
    public UIControlSystem UiControlSystem { get; private set; }
    public event EventHandler<SpiritSystem> SpiritCreatingRequestDone = delegate { };
    public event EventHandler<EnemySystem> EnemyCreatingRequestDone = delegate { };
    public event EventHandler WavesReceived = delegate { };
    private List<WaveEnemyID> waveEnenmyIDs;
    public PlayerSystem LocalPlayer
    {
        //client rpc cant see this. not setted? 
        get => localPlayer;
        set
        {
            if (!isLocalPlayer) return;

            localPlayer = value;
            localPlayer.SpiritPlaceSystem.SpiritCreationRequested += OnSpiritCreatingRequest;
            localPlayer.WaveSystem.EnemyCreationRequested += OnEnemyCreatingRequest;
        }
    }

    public List<WaveEnemyID> WaveEnenmyIDs
    {
        get => waveEnenmyIDs;
        set
        {
            waveEnenmyIDs = value;
            ReferenceHolder.Get.NetworkPlayer = this;
        }
    }

    private PlayerSystem localPlayer;
    private float delay = 0.07f;


    public override void OnStartLocalPlayer()
    {
        if (!isLocalPlayer) return;

        var localMaps = GameObject.FindGameObjectsWithTag("map");
        var uiCanvasPrefab = Instantiate(UICanvasPrefab);

        LocalMap = localMaps[MapID];
        UiControlSystem = uiCanvasPrefab.GetComponent<UIControlSystem>();
        UiControlSystem.IncreaseLevelButtonClicked += (s, e) => CmdIncreaseLevel(PlayerData);

        LoadData();
        CmdGetWaves();
        base.OnStartLocalPlayer();
    }



    public void WaitAndDo(float delay, Action function)
    {
        StartCoroutine(Wait());

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(delay);
            function.Invoke();
        }
    }

    #region Getting waves from server

    [Command]
    private void CmdGetWaves()
    {
        var sendData = (NetworkManager.singleton as ExtendedNetworkManager).NetworkGameManager.WaveEnenmyIDs.Serializer();
        TargetGetWaves(connectionToClient, sendData);
    }

    [TargetRpc]
    private void TargetGetWaves(NetworkConnection conn, byte[] byteData)
    {
        var receivedData = byteData.Deserializer<List<WaveEnemyID>>();
        WaveEnenmyIDs = receivedData;
    }

    #endregion

    #region Spirit creating request

    private void OnSpiritCreatingRequest(object _, SpiritCreationRequest e)
    {
        var sendData = e.Serializer();
        CmdCreateSpirit(sendData);
    }

    [Command] private void CmdCreateSpirit(byte[] byteRequest) => RpcCreateSpirit(byteRequest);

    [ClientRpc]
    private void RpcCreateSpirit(byte[] byteRequest)
    {
        var request = byteRequest.Deserializer<SpiritCreationRequest>();
        var pos = request.Position.ToVector3();
        var choosedCell = ReferenceHolder.Get.Player.CellControlSystem.Cells.Find(x => x.transform.position == pos);
        var spirit = ReferenceHolder.Get.SpiritDataBase.Spirits.Elements[request.Element].Rarities[request.Rarity].Spirits.Find(x => x.ID.Compare(request.ID));

        var newSpirit = choosedCell != null ?
            StaticMethods.CreateSpirit(spirit, choosedCell.GetComponent<Cell>(), LocalPlayer) :
            StaticMethods.CreateSpirit(spirit, pos, LocalPlayer);

        SpiritCreatingRequestDone?.Invoke(null, newSpirit);
    }

    #endregion

    #region EnemyCreatingRequest

    private void OnEnemyCreatingRequest(object _, EnemyCreationRequest e)
    {
        var sendData = e.Serializer();
        CmdCreateEnemy(sendData);
    }

    [Command] private void CmdCreateEnemy(byte[] byteRequest) => RpcCreateEnemy(byteRequest);
    [ClientRpc]
    private void RpcCreateEnemy(byte[] byteRequest)
    {
        var request = byteRequest.Deserializer<EnemyCreationRequest>();

        request.WaveNumber -= 1;
        var enemy = ReferenceHolder.Get.Player.WaveSystem.Waves[request.WaveNumber].EnemyTypes.Find(x => x.ID.Compare(request.ID));
        var newEnemy = StaticMethods.CreateEnemy(enemy, request.Position.ToVector3(), LocalPlayer);
        EnemyCreatingRequestDone?.Invoke(null, newEnemy);
    }

    #endregion

    #region Save-load methods

    private void OnDestroy() => SaveData();
    private void OnApplicationQuit() => SaveData();

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
