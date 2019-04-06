using Game.Systems;
using Game.Spirit;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Multishot", menuName = "Data/Spirit/Trait/Multishot")]

    public class Multishot : Trait
    {
        public int Count;

        private void Awake()
        {
            Name = "Multishot";
            Description = $"Shoot {Count} additional targets";
        }

        public override ITraitHandler GetSystem(ITraitSystem owner) => new MultishotSystem(this, owner);        
    }
}
