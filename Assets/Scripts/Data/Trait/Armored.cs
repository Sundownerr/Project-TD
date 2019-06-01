using System.Collections;
using System.Collections.Generic;
using Game.Systems;
using Game.Systems.Traits;
using UnityEngine;


namespace Game.Data.Traits
{
    [CreateAssetMenu(fileName = "Armored", menuName = "Data/Enemy/Trait/Armored")]

    public class Armored : Trait
    {
        [SerializeField] int additionalArmor;

        public int AdditionalArmor { get => additionalArmor; private set => additionalArmor = value; }

        protected void Awake()
        {
            Name = "Armored";
            Description = $"Add {AdditionalArmor} armor";
        }

        public override ITraitSystem GetSystem(ITraitComponent owner) => new Systems.Traits.Armored((Armored)this, (ITraitComponent)owner);
    }
}


