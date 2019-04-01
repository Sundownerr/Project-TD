using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Enemy.Data
{

    [Serializable]
    public enum ArmorType
    {
        Mytherite = 0,
        Luanem = 1,
        Solphyr = 2,
        Helleum = 3,
        Zoddth = 4,
        Siffat = 5,
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