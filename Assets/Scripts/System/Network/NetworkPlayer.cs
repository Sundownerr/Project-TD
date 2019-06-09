using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Game.Systems;
using System;
using Game;
using Game.Data.NetworkRequests;
using Game.Utility.Creator;
using Game.Managers;
using Game.Utility;
using Game.Utility.Serialization;
using Steamworks;

namespace Game.Systems.Network
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SyncVar] public UserData PlayerData;
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
                localPlayer.SpiritPlaceSystem.SpiritCreationRequested += OnSpiritCreatingRequest;
                localPlayer.WaveSystem.EnemyCreationRequested += OnEnemyCreatingRequest;
            }
        }

        List<NetworkWaveData> networkWaveDatas;
        public List<NetworkWaveData> NetworkWaveDatas
        {
            get => networkWaveDatas;
            set
            {
                networkWaveDatas = value;
                GameData.Instance.NetworkPlayer = this;
            }
        }

        public override void OnStartLocalPlayer()
        {
            if (!isLocalPlayer) return;

            var localMaps = GameObject.FindGameObjectsWithTag("map");
            var uiCanvasPrefab = Instantiate(UICanvasPrefab);

            LocalMap = localMaps[MapID];
            UiControlSystem = uiCanvasPrefab.GetComponent<UIControlSystem>();
            UiControlSystem.IncreaseLevelButtonClicked += () => CmdIncreaseLevel(PlayerData);
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
            var sendData = (NetworkManager.singleton as ExtendedNetworkManager).NetworkGameManager.NetworkWaveDatas.Serializer();
            WaitAndDo(delay, () => TargetGetWaves(connectionToClient, sendData));
        }

        [TargetRpc]
        void TargetGetWaves(NetworkConnection conn, byte[] byteData)
        {
            var receivedData = byteData.Deserializer<List<NetworkWaveData>>();
            WaitAndDo(delay, () => NetworkWaveDatas = receivedData);
        }

        #endregion

        #region Spirit creating request

        void OnSpiritCreatingRequest(SpiritCreationRequest e)
        {
            var sendData = e.Serializer();
            WaitAndDo(delay, () => CmdCreateSpirit(sendData));
        }

        [Command] void CmdCreateSpirit(byte[] byteRequest) => RpcCreateSpirit(byteRequest);

        [ClientRpc]
        void RpcCreateSpirit(byte[] byteRequest)
        {
            var request = byteRequest.Deserializer<SpiritCreationRequest>();
            WaitAndDo(delay, () => Create.Spirit(request, isLocalPlayer));
        }

        #endregion

        #region EnemyCreatingRequest

        void OnEnemyCreatingRequest(EnemyCreationRequest e)
        {
            var sendData = e.Serializer();
            WaitAndDo(delay, () => CmdCreateEnemy(sendData));
        }

        [Command] public void CmdCreateEnemy(byte[] byteRequest) => RpcCreateEnemy(byteRequest);

        [ClientRpc]
        public void RpcCreateEnemy(byte[] byteRequest)
        {
            var request = byteRequest.Deserializer<EnemyCreationRequest>();
            WaitAndDo(delay, () => Create.Enemy(request, isLocalPlayer));
        }

        #endregion

        #region Save-load methods

        void OnDestroy() => SaveData();
        void OnApplicationQuit() => SaveData();

        void LoadData()
        {
            if (!isLocalPlayer) return;

            var data = GameData.Instance.UserData;
            var userName = SteamClient.Name;

            CmdSendData(gameObject, data, userName);
        }

        void SaveData()
        {
            if (!isLocalPlayer) return;

            GameData.Instance.SaveData(new UserData(PlayerData.Level));
            // FPClientExt.LeaveGame();
        }

        [Command]
        void CmdSendData(GameObject player, UserData data, string name)
        {
            var manager = NetworkManager.singleton as ExtendedNetworkManager;

            for (int i = 0; i < manager.NetworkGameManager.UserDatas.Length; i++)
            {
                if (!manager.NetworkGameManager.UserDatas[i].IsNotEmpty)
                {
                    manager.NetworkGameManager.UserDatas[i] = new UserData(data.Level, data.SteamID);
                    manager.NetworkGameManager.UserNames[i] = name;
                    break;
                }
            }

            manager.NetworkGameManager.Users.Add(gameObject);
            player.GetComponent<NetworkPlayer>().PlayerData = new UserData(data.Level, data.SteamID);

            UpdatePlayersUI(manager);
        }

        #endregion

        #region UI methods

        public void UpdatePlayersUI(ExtendedNetworkManager manager)
        {
            for (int i = 0; i < manager.NetworkGameManager.Users.Count; i++)
            {
                var player = manager.NetworkGameManager.Users[i].GetComponent<NetworkPlayer>();
                player.RpcUpdateUI(manager.NetworkGameManager.UserDatas, manager.NetworkGameManager.UserNames);
            }
        }


        [ClientRpc]
        public void RpcUpdateUI(UserData[] playerDatas, string[] playerNames)
        {
            if (!isLocalPlayer) return;

            UiControlSystem.UpdateUI(playerDatas, playerNames);
        }

        #endregion

        [Command]
        void CmdIncreaseLevel(UserData data)
        {
            var manager = NetworkManager.singleton as ExtendedNetworkManager;

            for (int i = 0; i < manager.NetworkGameManager.UserDatas.Length; i++)
                if (manager.NetworkGameManager.UserDatas[i].Equals(data))
                {
                    manager.NetworkGameManager.UserDatas[i] = new UserData(data.Level + 1, data.SteamID);
                    manager.NetworkGameManager.Users[i].GetComponent<NetworkPlayer>().PlayerData = new UserData(data.Level + 1, data.SteamID);
                    break;
                }

            UpdatePlayersUI(manager);
        }
    }
}