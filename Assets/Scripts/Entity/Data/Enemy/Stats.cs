using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Enemy.Data
{

    [Serializable]
    public enum ArmorType
    {
        Cloth = 0,
        Plate = 1,
        Chainmail = 2,
        Magic = 3
    }

    [Serializable]
    public struct Armor
    {
        public ArmorType Type { get; set; }
        public float Value { get; set; }
    }

    [Serializable]
    public class Race
    {
        [SerializeField, Expandable]
        public List<EnemyData> Enemies;

        public Race()
        {
            Enemies = new List<EnemyData>();
        }
    }
}