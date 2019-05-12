using UnityEngine;
using Game.Systems.Traits;

namespace Game.Data.Traits
{
    [CreateAssetMenu(fileName = "Chainshot", menuName = "Data/Spirit/Trait/Chainshot")]

    public class Chainshot : Trait
    {
        public int BounceCount, DecreaseDamagePerBounce;

        protected void Awake()
        {
            Name = "Chainshot";
            Description = $"Bounce between {BounceCount} targets";
        }

        public override ITraitHandler GetSystem(ITraitSystem owner) => new ChainshotSystem(this, owner);
    }
}
