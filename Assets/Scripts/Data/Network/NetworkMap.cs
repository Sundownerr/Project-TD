﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Game.Data.Network
{
    public class NetworkMap : NetworkBehaviour
    {
        [SyncVar]
        public bool IsUsed;

        [SyncVar]
        public GameObject LocalMap;


    }
}
