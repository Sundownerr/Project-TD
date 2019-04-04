using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Spirit.Data.Stats;
using Game.Enums;
using RotaryHeart.Lib.SerializableDictionary;

namespace Game.Wrappers
{
    [Serializable]
    public class DamageToArmor
    {
        public DamageType Type;
        public List<double> Percents;
    }



}

