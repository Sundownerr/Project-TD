using Game.Systems;
using Game.Spirit;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Chainshot", menuName = "Data/Spirit/Trait/Chainshot")]

    public class Chainshot : Trait
    {
        public int BounceCount, DecreaseDamagePerBounce;

        void Awake()
        {
            Name = "Chainshot";
            Description = $"Bounce between {BounceCount} targets";
        }

        public override ITraitHandler GetSystem(ITraitSystem owner) => new ChainshotSystem(this, owner);
    }
}
