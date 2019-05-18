using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Enums;
using OneLine;

namespace Game.Data.Spirit.Internal
{
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
