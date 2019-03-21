using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Enemy.Data
{
    [Serializable]
    public struct Armor
    {
        public ArmorType Type { get; set; }
        public float Value { get; set; }

        public enum ArmorType
        {
            Cloth,
            Plate,
            Chainmail,
            Magic
        }
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