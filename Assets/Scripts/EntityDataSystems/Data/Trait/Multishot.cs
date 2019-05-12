using UnityEngine;
using Game.Systems.Traits;

namespace Game.Data.Traits
{
    [CreateAssetMenu(fileName = "Multishot", menuName = "Data/Spirit/Trait/Multishot")]

    public class Multishot : Trait
    {
        [SerializeField] int count;

        public int Count { get => count; private set => count = value; }

        protected void Awake()
        {            
            Name = "Multishot";
            Description = $"Shoot {Count} additional targets";
        }

        public override ITraitHandler GetSystem(ITraitSystem owner) => new MultishotSystem(this, owner);        
    }
}
