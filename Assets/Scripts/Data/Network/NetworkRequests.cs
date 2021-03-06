﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data.NetworkRequests
{
    [Serializable]
    public class ListCoordinates3D : List<Coordinates3D>
    {
        public List<Vector3> ToVector3List()
        {
            var newArray = new List<Vector3>(this.Count);

            for (int i = 0; i < this.Count; i++)
            {
                newArray[i] = new Vector3(this[i].X, this[i].Y, this[i].Z);
            }

            return newArray;
        }

        public ListCoordinates3D(List<Vector3> positions)
        {
            positions.ForEach(position => this.Add(position.ToCoordinates3D()));
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

    [Serializable]
    public class NetworkWaveData
    {
        [SerializeField] public List<int> EnemyIndexes;
        [SerializeField] public List<int> AbilityIndexes;
        [SerializeField] public List<int> TraitIndexes;
        [SerializeField] public int ArmorIndex;
    }

    [Serializable]
    public class SpiritCreationRequest
    {
        [SerializeField] public Coordinates3D Position;
        [SerializeField] public int Index;
        [SerializeField] public int Rarity;
        [SerializeField] public int Element;
        [SerializeField] public int CellIndex;
    }

    [Serializable]
    public class EnemyCreationRequest
    {
        [SerializeField] public Coordinates3D Position;
        [SerializeField] public int Index;
        [SerializeField] public int Race;
        [SerializeField] public int WaveNumber;
        [SerializeField] public int PositionInWave;
        [SerializeField] public List<int> AbilityIndexes;
        [SerializeField] public List<int> TraitIndexes;
        [SerializeField] public ListCoordinates3D Waypoints;
    }
}