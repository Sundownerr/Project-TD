﻿using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Enums;
using OneLine;

namespace Game.Spirit.Data.Stats
{

    [Serializable]
    public class ElementList
    {
        [SerializeField]
        public Element[] Elements;
    }

    [Serializable]
    public class Element
    {
        [SerializeField]
        public Rarity[] Rarities;

        [SerializeField]
        public string Name;

        public Element(string name)
        {
            Name = name;
            Rarities = new Rarity[4]
            {
                new Rarity(RarityType.Common.ToString()),
                new Rarity(RarityType.Uncommon.ToString()),
                new Rarity(RarityType.Rare.ToString()),
                new Rarity(RarityType.Unique.ToString())
            };
        }
    }

    [Serializable]
    public class Rarity
    {
        [SerializeField, OneLine.HideLabel]
        public List<SpiritData> Spirits;

        [SerializeField]
        public string Name;

        public Rarity(string name)
        {
            Name = name;
            Spirits = new List<SpiritData>();
        }
    }

    [Serializable]
    public enum DamageType
    {
        Elemental = 0,
        Magic = 1,
        Energy = 2,
        Essence = 3,
        Decay = 4,
        Physical = 5,
        Spell = 6
    }

    [Serializable]
    public class Damage
    {
        [SerializeField]
        public DamageType Type;

        [SerializeField]
        public float Value;
    }
}