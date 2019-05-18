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
        [SerializeField] RarityType rarity;
        [SerializeField] int weigth;
        [SerializeField] int waveLevel;
        [SerializeField, OneLine, OneLine.HideLabel] List<NumeralAttribute> numeralAttributes;
        [SerializeField, OneLine, OneLine.HideLabel] List<SpiritAttribute> spiritAttributes;

        public RarityType Rarity { get => rarity; private set => rarity = value; }
        public int Weigth { get => weigth; private set => weigth = value; }
        public int WaveLevel { get => waveLevel; private set => waveLevel = value; }
        public List<NumeralAttribute> NumeralAttributes { get => numeralAttributes; private set => numeralAttributes = value; }
        public List<SpiritAttribute> SpiritAttributes { get => spiritAttributes; private set => spiritAttributes = value; }
    }
}
