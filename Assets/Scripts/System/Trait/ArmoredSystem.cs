using Game.Data.Traits;
using Game.Systems.Enemy;

namespace Game.Systems.Traits
{
    public class ArmoredSystem : ITraitHandler
    {
        public ITraitSystem Owner { get; set; }

        Armored trait;

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
            (Owner as EnemySystem).Data.Get(Enums.Enemy.ArmorValue).Value += trait.AdditionalArmor;
        }
    }
}
