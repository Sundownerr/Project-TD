using System.Collections;
using System.Collections.Generic;
using Game.Systems;
using UnityEngine;


namespace Game.Data
{
    [CreateAssetMenu(fileName = "Armored", menuName = "Data/Enemy/Trait/Armored")]

    public class Armored : Trait
    {
        public int AdditionalArmor;

        void Awake()
        {
            Name = "Armored";
            Description = $"Add {AdditionalArmor} armor";
        }

        public override ITraitHandler GetSystem(ITraitSystem owner) => new ArmoredSystem(this, owner);
    }
}


