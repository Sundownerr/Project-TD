using System.Collections.Generic;
using UnityEngine;
using System;

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
        [SerializeField]
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
    public class Damage
    {
        [SerializeField]
        public DamageType Type;

        [SerializeField]
        public float Value;

        [Serializable]
        public enum DamageType
        {
            Spell,
            Decay,
            Energy,
            Physical,
            Elemental,
        }
    }
}
