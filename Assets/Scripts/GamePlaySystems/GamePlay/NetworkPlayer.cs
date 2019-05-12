using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FPClient = Facepunch.Steamworks.Client;
using Game.Systems;
using System;
using Game;
using Game.Data.NetworkRequests;
using Game.Utility;
using Game.Managers;

namespace Game.Network
{
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

        List<NetworkWaveData> networkWaveDatas;
        public List<NetworkWaveData> NetworkWaveDatas
        {
            get => networkWaveDatas;
            set
            {
                networkWaveDatas = value;
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
            var sendData = (NetworkManager.singleton as ExtendedNetworkManager).NetworkGameManager.NetworkWaveDatas.Serializer();

            WaitAndDo(delay, () =>
            {
                TargetGetWaves(connectionToClient, sendData);
            });
        }

        [TargetRpc]
        void TargetGetWaves(NetworkConnection conn, byte[] byteData)
        {
            var receivedData = byteData.Deserializer<List<NetworkWaveData>>();
            WaitAndDo(delay, () =>
            {
                NetworkWaveDatas = receivedData;
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
                var spirit = ReferenceHolder.Get.SpiritDB.Data[request.Index];

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
                {
                    Debug.LogError("enemyfromdb is null");
                }
                else
                {
                    var newEnemy = StaticMethods.CreateEnemy(enemyFromDB, spawnPos, waypoints, isLocalPlayer);

                    SetAbilities();
                    SetTraits();

                    ReferenceHolder.Get.Player.WaveSystem.NetworkSpawnEnemy(newEnemy, isLocalPlayer);

                    void SetAbilities()
                    {
                        request.AbilityIndexes?.ForEach(index =>
                        {
                            var abilityFromDB = ReferenceHolder.Get.AbilityDB.Data.Find(abilityInDataBase => abilityInDataBase.Index == index);

                            if (abilityFromDB == null)
                            {
                                Debug.LogError($"can't find ability with index {index}");
                            }
                            else
                            {
                                newEnemy.Data.Abilities.Add(abilityFromDB);
                            }
                        });
                    }

                    void SetTraits()
                    {
                        request.TraitIndexes?.ForEach(index =>
                        {
                            var traitFromDB = ReferenceHolder.Get.TraitDB.Data.Find(traitInDataBase => traitInDataBase.Index == index);

                            if (traitFromDB == null)
                            {
                                Debug.LogError($"can't find trait with index {index}");
                            }
                            else
                            {
                                newEnemy.Data.Traits.Add(traitFromDB);
                            }
                        });
                    }
                }
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
}