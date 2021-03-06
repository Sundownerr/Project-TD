﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using NetworkPlayer = Game.Systems.Network.NetworkPlayer;
using Game.Systems;
using Game.Systems.Network;

namespace Game.Managers
{
    public class ExtendedNetworkManager : NetworkManager
    {
        public GameObject GamemanagerPrefab;
        public NetworkGameManager NetworkGameManager;


        public override void OnServerSceneChanged(string sceneName)
        {
            var gmPrefab = Instantiate(GamemanagerPrefab);

            NetworkServer.Spawn(gmPrefab);

            NetworkGameManager = gmPrefab.GetComponent<NetworkGameManager>();
            NetworkGameManager.Set();

            base.OnServerSceneChanged(sceneName);
        }

        public override void OnServerAddPlayer(NetworkConnection conn, AddPlayerMessage extraMessage)
        {
            var freeMapID = NetworkGameManager.GetFreeMapID();
            // var maxPlayers = FPClient.Instance != null ? FPClient.Instance.Lobby.NumMembers : 4;
            // var currentPlayers = NetworkServer.connections.Count;

            // if (currentPlayers > maxPlayers)
            // {
            //     conn.Disconnect();
            //     return;
            // }

            var playerGO = Instantiate(playerPrefab);
            var player = playerGO.GetComponent<NetworkPlayer>();

            player.MapID = freeMapID;

            NetworkServer.AddPlayerForConnection(conn, playerGO);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            var player = conn.playerController;

            NetworkGameManager.RemovePlayer(player.gameObject, player.GetComponent<NetworkPlayer>().PlayerData);

            base.OnServerDisconnect(conn);
        }
    }
}
