using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class NetworkMap : NetworkBehaviour
{
    [SyncVar]
    public bool IsUsed;

    [SyncVar]
    public GameObject LocalMap;

  
}
