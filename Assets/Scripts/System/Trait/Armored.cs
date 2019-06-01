using Game.Data.Traits;
using Game.Systems.Enemy;

namespace Game.Systems.Traits
{
    public class Armored : ITraitSystem
    {
        public ITraitComponent Owner { get; set; }

        Data.Traits.Armored trait;

        public Armored(Data.Traits.Armored trait, ITraitComponent owner)
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
