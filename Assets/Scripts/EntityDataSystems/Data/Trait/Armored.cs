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
        public int AdditionalArmor;

        protected override void Awake()
        {
            base.Awake();
            
            Name = "Armored";
            Description = $"Add {AdditionalArmor} armor";
        }

        public override ITraitHandler GetSystem(ITraitSystem owner) => new ArmoredSystem(this, owner);
    }
}


