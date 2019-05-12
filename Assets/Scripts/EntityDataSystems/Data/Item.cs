using System.Collections.Generic;
using Game.Enums;
using OneLine;
using Game.Data.Attributes;
using NaughtyAttributes;
using Game.Data.Databases;
using UnityEngine;
using Game.Systems;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Data.Items
{
    public class Item : Entity
    {
        public RarityType Rarity;
        public int Weigth;
        public int WaveLevel;
        [OneLine, OneLine.HideLabel] public List<NumeralAttribute> NumeralAttributes;
        [OneLine, OneLine.HideLabel] public List<SpiritAttribute> SpiritAttributes;
    }
}
