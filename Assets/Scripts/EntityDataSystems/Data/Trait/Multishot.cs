using UnityEngine;
using Game.Systems.Traits;

namespace Game.Data.Traits
{
    [CreateAssetMenu(fileName = "Multishot", menuName = "Data/Spirit/Trait/Multishot")]

    public class Multishot : Trait
    {
        public int Count;

        protected override void Awake()
        {
            base.Awake();
            Name = "Multishot";
            Description = $"Shoot {Count} additional targets";
        }

        public override ITraitHandler GetSystem(ITraitSystem owner) => new MultishotSystem(this, owner);        
    }
}
