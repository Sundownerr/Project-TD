using Game.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Game.Data
{
    public class PlayerMap : MonoBehaviour
    {
        public List<GameObject> CellAreas;
        public PlayerSystem Owner;
        public GameObject GroundSpawnPoint;
        public GameObject FlyingSpawnPoint;
        public List<GameObject> GroundWaypoints;
        public List<GameObject> FlyingWaypoints;

    }
}