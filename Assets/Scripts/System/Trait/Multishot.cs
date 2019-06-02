using Game.Data.Traits;

namespace Game.Systems.Traits
{
    public class Multishot : ITraitSystem
    {
        public ITraitComponent Owner { get; set; }

        Data.Traits.Multishot trait;

        public Multishot(Data.Traits.Multishot trait, ITraitComponent owner)
        {
            this.trait = trait;
            Owner = owner;
        }

        public void LevelUp()
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