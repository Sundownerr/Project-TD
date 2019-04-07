using Game.Systems;
using Game.Spirit;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "AOE Shot", menuName = "Data/Spirit/Trait/AOE Shot")]

    public class AOEShot : Trait
    {
        public int Range;

        void Awake()
        {
            Name = "AOE SHot";
            Description = $"Damage targets in {Range} range";
        }

        public override ITraitHandler GetSystem(ITraitSystem owner) => new AOEShotSystem(this, owner);
    }
}

