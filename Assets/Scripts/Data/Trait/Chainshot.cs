using UnityEngine;
using Game.Systems.Traits;

namespace Game.Data.Traits
{
    [CreateAssetMenu(fileName = "Chainshot", menuName = "Data/Spirit/Trait/Chainshot")]

    public class Chainshot : Trait
    {
        [SerializeField] int bounceCount;
        [SerializeField] int decreaseDamagePerBounce;

        public int BounceCount { get => bounceCount; private set => bounceCount = value; }
        public int DecreaseDamagePerBounce { get => decreaseDamagePerBounce; private set => decreaseDamagePerBounce = value; }

        protected void Awake()
        {
            Name = "Chainshot";
            Description = $"Bounce between {BounceCount} targets";
        }

        public override ITraitHandler GetSystem(ITraitSystem owner) => new ChainshotSystem(this, owner);
    }
}
