using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField, Serializable]
public struct PlayerData
{
    [SerializeField] public int Level;
    [SerializeField] public bool IsNotEmpty;
    [SerializeField] public ulong SteamID;

    public PlayerData(int level)
    {
        Level = level;
        IsNotEmpty = true;
        SteamID = 0;
    }

    public PlayerData(int level, ulong steamID)
    {
        SteamID = steamID;
        Level = level;
        IsNotEmpty = true;
    }
}