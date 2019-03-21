using Game.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMap : MonoBehaviour
{
    public GameObject[] CellAreas;
    public PlayerSystem Owner;
    public NetworkPlayer NetworkOwner;
    public GameObject GroundSpawnPoint;
    public GameObject FlyingSpawnPoint;
    public GameObject[] GroundWaypoints;
    public GameObject[] FlyingWaypoints;

    private void Awake()
    {
        if (GameManager.Instance.GameState == GameState.InLobby)
        {
            // if (!NetworkServer.localClientActive)
            //     Destroy(gameObject);
            
            
        }
    }

    private void Start()
    {
        if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
        {
            // Debug.Log("add c");
            // gameObject.AddComponent<NetworkIdentity>();
            // gameObject.AddComponent<NetworkTransform>();
            // ClientScene.RegisterPrefab(gameObject, GetComponent<NetworkIdentity>().assetId);
            // NetworkManager.singleton.spawnPrefabs.Add(gameObject);
        }
    }
}
