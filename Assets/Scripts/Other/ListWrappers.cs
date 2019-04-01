using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Spirit.Data.Stats;

namespace Game.Data
{

    
    [Serializable]
    public class DamageToArmor
    {
        public DamageType Type;
        public List<double> Percents;
    }
}

