using Game.Systems;
using Game.Spirit;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Data
{
    public abstract class Trait : Entity
    {
        public abstract ITraitHandler GetSystem(ITraitSystem owner);
    }
}
