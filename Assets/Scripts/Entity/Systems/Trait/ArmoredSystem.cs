using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Data;
using Game.Systems;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
	public class ArmoredSystem : ITraitHandler
    {
        public ITraitSystem Owner { get; set; }

        private Armored trait;

        public ArmoredSystem(Armored trait, ITraitSystem owner) 
        {
            this.trait = trait;
            Owner = owner;
        }

        public void IncreaseStatsPerLevel()
        {
            //Debug.Log("increase stats per level");
        }

		public void Apply(IPrefabComponent entity)
		{
			
		}

        
        public void Set()
        {
            (Owner as EnemySystem).Data.Get(Numeral.ArmorValue, From.Base).Value += trait.AdditionalArmor;
        }
    }
}
