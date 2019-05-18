using Game.Data.Traits;

namespace Game.Systems.Traits
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