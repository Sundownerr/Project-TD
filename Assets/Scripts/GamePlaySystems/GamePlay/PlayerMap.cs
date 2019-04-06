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

}
