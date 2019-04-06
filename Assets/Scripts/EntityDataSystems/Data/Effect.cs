using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Systems;
using NaughtyAttributes;
using System;
using Game.Enemy;
using Game.Spirit;

namespace Game.Data
{
    [Serializable]
    public abstract class Effect : Entity
    {
        public float Duration, NextInterval;
        public bool IsStackable;

        [ShowIf("IsStackable")]
        [MinValue(1), MaxValue(1000)]
        public int MaxStackCount;

        public abstract EffectSystem EffectSystem {get; } 
    }
}
