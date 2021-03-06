﻿using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Data.EnemyEntity;

namespace Game.Data.EnemyEntity.Internal
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
}