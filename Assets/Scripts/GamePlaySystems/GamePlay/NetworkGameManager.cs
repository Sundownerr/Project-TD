using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using Game.Systems;
using System;
using Game.Enemy;
using FPClient = Facepunch.Steamworks.Client;
using Game.Data;

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
    public GameObjectSyncList Players;
    public PlayerData[] PlayerDatas;
    public string[] PlayerNames;
    public List<WaveEnemyID> WaveEnenmyIDs;
    int waveAmount;

    public void Set()
    {
        var maxPlayers = NetworkManager.singleton.maxConnections;

        PlayerNames = new string[maxPlayers];
        PlayerDatas = new PlayerData[maxPlayers];
        Players = new GameObjectSyncList();
        NetworkMaps = new GameObjectSyncList();
        WaveEnenmyIDs = new List<WaveEnemyID>();

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
                WaveEnenmyIDs.Add(new WaveEnemyID()
                {
                    IDs = waves[wave].EnemyTypes.GetIDs(),
                    AbilityIDs = new ListID(),
                    TraitIDs = new ListID(),
                    ArmorID = (int)waves[wave].EnemyTypes[0].ArmorType
                });

                for (int j = 0; j < waves[wave].EnemyTypes.Count; j++)
                {
                    var enemy = waves[wave].EnemyTypes[j];
                    WaveEnenmyIDs[wave].AbilityIDs.AddRange(enemy.Abilities.GetIDs());
                    WaveEnenmyIDs[wave].TraitIDs.AddRange(enemy.Traits.GetIDs());
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

    public void RemovePlayer(GameObject playerGO, PlayerData playerData)
    {
        var player = playerGO.GetComponent<NetworkPlayer>();
        Players.Remove(playerGO);

        FreeMap();
        RemovePlayerData();
        UpdatePlayersUI();

        #region Helper functions

        void FreeMap()
        {
            for (int i = 0; i < NetworkMaps.Count; i++)
            {
                var map = NetworkMaps[i].GetComponent<NetworkMap>();

                if (i == player.MapID)
                {
                    player.MapID = -1;
                    map.IsUsed = false;

                    break;
                }
            }
        }

        void RemovePlayerData()
        {
            for (int i = 0; i < PlayerDatas.Length; i++)
            {
                if (PlayerDatas[i].IsNotEmpty)
                    if (PlayerDatas[i].Equals(playerData))
                    {
                        PlayerDatas[i] = new PlayerData();
                        PlayerDatas[i].IsNotEmpty = false;
                        break;
                    }
            }
        }

        void UpdatePlayersUI()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].GetComponent<NetworkPlayer>().RpcUpdateUI(PlayerDatas, PlayerNames);
            }
        }

        #endregion
    }
}
