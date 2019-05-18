using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField, Serializable]
public struct UserData
{
    [SerializeField] int level;
    [SerializeField] bool isNotEmpty;
    [SerializeField] ulong steamID;

    public int Level { get => level; private set => level = value; }
    public bool IsNotEmpty { get => isNotEmpty; set => isNotEmpty = value; }
    public ulong SteamID { get => steamID; private set => steamID = value; }

    public UserData(int level)
    {
        this.steamID = 0;
        this.level = level;
        isNotEmpty = true;
    }

    public UserData(int level, ulong steamID)
    {
        this.steamID = steamID;
        this.level = level;
        isNotEmpty = true;
    }
}