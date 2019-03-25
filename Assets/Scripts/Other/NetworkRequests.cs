using Game.Data;
using Game.Enemy;
using Game.Spirit.Data;
using Game.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Game.Cells;
using Game.Spirit;

[Serializable]
public readonly struct Coordinates3D
{
    [SerializeField] public readonly float X, Y, Z;

    public Coordinates3D(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

[Serializable]
public class NumeralAttributeList : List<NumeralAttribute> { }

[Serializable]
public class SpiritCreationRequest
{
    [SerializeField] public Coordinates3D Position;
    [SerializeField] public ID ID;
    [SerializeField] public int Rarity, Element;

    public SpiritCreationRequest(ID id, int rarity, int element, Coordinates3D position)
    {
        ID = id;
        Rarity = rarity;
        Element = element;
        Position = new Coordinates3D(position.X, position.Y, position.Z);
    }
}

[Serializable]
public class EnemyCreationRequest
{
    [SerializeField] public Coordinates3D Position;
    [SerializeField] public ID ID;
    [SerializeField] public int Race, WaveNumber;
    [SerializeField] public ListID AbilityIDs;
    [SerializeField] public ListID TraitIDs;
}
