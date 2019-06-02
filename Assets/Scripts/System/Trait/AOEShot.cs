using UnityEngine;
using Game.Systems.Spirit;
using Game.Data.Traits;

namespace Game.Systems.Traits
{
    public class AOEShot : ITraitSystem
    {
        public ITraitComponent Owner { get; set; }

        Data.Traits.AOEShot trait;

        public AOEShot(Data.Traits.AOEShot trait, ITraitComponent owner)
        {
            this.trait = trait;
            Owner = owner;
        }

        public void LevelUp()
        {
            // Debug.Log("increase stats per level");
        }

        public void Apply(IPrefabComponent bullet)
        {
            var enemyLayer = 1 << 12;
            var colliders = new Collider[40];
            var spirit = Owner as SpiritSystem;
            var player = spirit.GetOwnerOfType<PlayerSystem>();

            var hitTargetCount = Physics.OverlapSphereNonAlloc(
                bullet.Prefab.transform.position,
                trait.Range,
                colliders,
                enemyLayer);

            for (int i = 0; i < hitTargetCount; i++)
            {
                var findedEnemy = player.EnemyControlSystem.AllEnemies.Find(enemy => colliders[i].transform.gameObject == enemy.Prefab);

                if (findedEnemy != null)
                {
                    spirit.DealDamage(findedEnemy, spirit.DamageInstance);
                }
            }
        }

        public void Set()
        {

        }
    }
}