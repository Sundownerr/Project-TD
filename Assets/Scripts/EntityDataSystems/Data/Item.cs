using Game.Systems;
using Game.Spirit.Data.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Enums;
using Game.Wrappers;
using OneLine;

namespace Game.Data
{
    public class Item : Entity
    {
        public RarityType Rarity;
        public int Weigth;
        public int WaveLevel;
        [OneLine, OneLine.HideLabel] public List<NumeralAttribute> NumeralAttributes;
        [OneLine, OneLine.HideLabel] public List<SpiritAttribute> SpiritAttributes;

        // public NumeralAttribute Get(Numeral type) =>
        //     Attributes.Find(attribute => attribute.Type == type);
    }
}
