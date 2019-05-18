using System.Collections.Generic;
using System;
using Game.Data.Spirit.Internal;

namespace Game.Wrappers
{
    [Serializable]
    public class DamageToArmor
    {
        public DamageType Type;
        public List<double> Percents;
    }
}

