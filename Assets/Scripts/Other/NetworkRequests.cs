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
public class ListCoordinates3D : List<Coordinates3D>
{
    public Vector3[] ToVector3Array()
    {
        var newArray = new Vector3[this.Count];
        for (int i = 0; i < this.Count; i++)        
            newArray[i] = new Vector3(this[i].X, this[i].Y, this[i].Z);     
        return newArray;
    }

    public ListCoordinates3D(Vector3[] positions)
    { 
        for (int i = 0; i < positions.Length; i++)
            this.Add(positions[i].ToCoordinates3D());   
    }
}

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

// [Serializable]
// public class NumeralAttributeList : List<NumeralAttribute> { }

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
    [SerializeField] public int Race, WaveNumber, PositionInWave;
    [SerializeField] public ListID AbilityIDs;
    [SerializeField] public ListID TraitIDs;
    [SerializeField] public ListCoordinates3D Waypoints;
}
