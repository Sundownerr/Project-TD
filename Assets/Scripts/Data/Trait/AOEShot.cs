using UnityEngine;
using Game.Data.Traits;
using Game.Systems.Traits;

namespace Game.Data.Traits
{
    [CreateAssetMenu(fileName = "AOE Shot", menuName = "Data/Spirit/Trait/AOE Shot")]

    public class AOEShot : Trait
    {
        [SerializeField] int range;

        public int Range { get => range; private set => range = value; }

        protected void Awake()
        {
            Name = "AOE SHot";
            Description = $"Damage targets in {Range} range";
        }

        public override ITraitSystem GetSystem(ITraitComponent owner) => new Systems.Traits.AOEShot(this, (ITraitComponent)owner);
    }
}

