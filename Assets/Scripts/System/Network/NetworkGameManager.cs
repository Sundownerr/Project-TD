using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Game.Systems;
using Game.Data;
using Game.Data.NetworkRequests;
using Game.Data.Network;

namespace Game.Systems.Network
{
    [SerializeField]
    public class GameObjectSyncList : SyncList<GameObject>
    {
        protected override GameObject DeserializeItem(NetworkReader reader)
        {
            return reader.ReadGameObject();
        }

        protected override void SerializeItem(NetworkWriter writer, GameObject item)
        {
            writer.Write(item);
        }
    }

    public class NetworkGameManager : NetworkBehaviour
    {
        public GameObject MapComponentPrefab;
        public GameObjectSyncList NetworkMaps;
        public GameObjectSyncList Users;
        public UserData[] UserDatas;
        public string[] UserNames;
        public List<NetworkWaveData> NetworkWaveDatas;
        int waveAmount;

        public void Set()
        {
            var maxPlayers = NetworkManager.singleton.maxConnections;

            UserNames = new string[maxPlayers];
            UserDatas = new UserData[maxPlayers];
            Users = new GameObjectSyncList();
            NetworkMaps = new GameObjectSyncList();
            NetworkWaveDatas = new List<NetworkWaveData>();

            //waveAmount = int.Parse(LobbyExtension.GetData(LobbyData.Waves) ?? "100");

            waveAmount = 100;

            AddMapComponentsOnServer();
            GenerateWaves();

            #region Helper functions

            void GenerateWaves()
            {
                var waves = new List<Wave>(WaveCreatingSystem.GenerateWaves(waveAmount));

                for (int wave = 0; wave < waves.Count; wave++)
                {
                    NetworkWaveDatas.Add(new NetworkWaveData()
                    {
                        EnemyIndexes = waves[wave].EnemyTypes.GetIndexes(),
                        ArmorIndex = (int)waves[wave].EnemyTypes[0].ArmorType
                    });

                    for (int j = 0; j < waves[wave].EnemyTypes.Count; j++)
                    {
                        var enemy = waves[wave].EnemyTypes[j];

                        if (enemy.Abilities != null)
                        {
                            NetworkWaveDatas[wave].AbilityIndexes = enemy.Abilities.GetIndexes();
                        }

                        if (enemy.Traits != null)
                        {
                            NetworkWaveDatas[wave].TraitIndexes = enemy.Traits.GetIndexes();
                        }
                    }
                }
            }

            void AddMapComponentsOnServer()
            {
                var localMaps = GameObject.FindGameObjectsWithTag("map");

                for (int i = 0; i < localMaps.Length; i++)
                {
                    var mapPrefab = Instantiate(MapComponentPrefab, localMaps[i].transform.position, Quaternion.identity);

                    NetworkServer.Spawn(mapPrefab);
                    NetworkMaps.Add(mapPrefab);
                }
            }

            #endregion
        }

        public int GetFreeMapID()
        {
            for (int i = 0; i < NetworkMaps.Count; i++)
            {
                var map = NetworkMaps[i].GetComponent<NetworkMap>();

                if (!map.IsUsed)
                {
                    map.IsUsed = true;
                    return i;
                }
            }
            return -1;
        }

        public void RemovePlayer(GameObject userGO, UserData userData)
        {
            var user = userGO.GetComponent<NetworkPlayer>();
            Users.Remove(userGO);

            FreeMap();
            RemovePlayerData();
            UpdateUsersUI();

            #region Helper functions

            void FreeMap()
            {
                for (int i = 0; i < NetworkMaps.Count; i++)
                {
                    var map = NetworkMaps[i].GetComponent<NetworkMap>();

                    if (i == user.MapID)
                    {
                        user.MapID = -1;
                        map.IsUsed = false;

                        break;
                    }
                }
            }

            void RemovePlayerData()
            {
                for (int i = 0; i < UserDatas.Length; i++)
                {
                    if (UserDatas[i].IsNotEmpty)
                        if (UserDatas[i].Equals(userData))
                        {
                            UserDatas[i] = new UserData();
                            UserDatas[i].IsNotEmpty = false;
                            break;
                        }
                }
            }

            void UpdateUsersUI()
            {
                for (int i = 0; i < Users.Count; i++)
                {
                    Users[i].GetComponent<NetworkPlayer>().RpcUpdateUI(UserDatas, UserNames);
                }
            }

            #endregion
        }
    }
}