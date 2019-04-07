using System.Collections;
using System.Collections.Generic;
using Game.Data;
using Game.Systems;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
	public class MultishotSystem : ITraitHandler
    {
        public ITraitSystem Owner { get; set; }

        Multishot trait;

        public MultishotSystem(Multishot trait, ITraitSystem owner) 
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
            
        }
    }
}