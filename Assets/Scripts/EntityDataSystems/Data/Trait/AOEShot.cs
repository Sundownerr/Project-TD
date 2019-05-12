using UnityEngine;
using Game.Data.Traits;
using Game.Systems.Traits;

namespace Game.Data.Traits
{
    [CreateAssetMenu(fileName = "AOE Shot", menuName = "Data/Spirit/Trait/AOE Shot")]

    public class AOEShot : Trait
    {
        public int Range;

        protected void Awake()
        {
            Name = "AOE SHot";
            Description = $"Damage targets in {Range} range";
        }

        public override ITraitHandler GetSystem(ITraitSystem owner) => new AOEShotSystem(this, owner);
    }
}

