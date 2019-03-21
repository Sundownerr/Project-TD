using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using FPClient = Facepunch.Steamworks.Client;
using Game.Systems;

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

        if (freeMapID == -1)
        {
            conn.Disconnect();
            return;
        }
       
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
