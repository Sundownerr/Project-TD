using Game.Data.Traits;
using Game.Systems.Spirit;
using Game.Systems.Spirit.Internal;
using UnityEngine;

namespace Game.Systems.Traits
{
    public class Chainshot : ITraitSystem
    {
        public ITraitComponent Owner { get; set; }

        Data.Traits.Chainshot trait;

        public Chainshot(Data.Traits.Chainshot trait, ITraitComponent owner)
        {
            this.trait = trait;
            Owner = owner;
        }

        public void LevelUp()
        {
            //Debug.Log("incresa");
        }

        public void Apply(IPrefabComponent entity)
        {
            var bullet = entity as BulletSystem;
            var spirit = Owner as SpiritSystem;
            var player = spirit.GetOwnerOfType<PlayerSystem>();
            var enemies = new Collider[40];
            var enemyLayer = 1 << 12;

            var enemyCount = Physics.OverlapSphereNonAlloc(
                bullet.Prefab.transform.position,
                150,
                enemies,
                enemyLayer);

            spirit.DealDamage(bullet.Target, spirit.DamageInstance);

            if (bullet.Target == null || enemyCount <= 1)
            {
                spirit.ShootSystem.SetTargetReached(bullet);
            }
            else
            {
                if (bullet.RemainingBounceCount <= 0)
                {
                    spirit.ShootSystem.SetTargetReached(bullet);
                }
                else
                {
                    bullet.RemainingBounceCount--;

                    for (int enemyIndexInChain = 0; enemyIndexInChain < enemyCount; enemyIndexInChain++)
                    {
                        if (bullet.Target.Prefab == enemies[enemyIndexInChain].gameObject)
                        {
                            if (enemyIndexInChain - 1 >= 0)
                            {
                                bullet.Target = GetEnemyInChain(enemies[enemyIndexInChain - 1].transform.gameObject);
                            }

                            if (enemyIndexInChain + 1 < enemyCount)
                            {
                                bullet.Target = GetEnemyInChain(enemies[enemyIndexInChain + 1].transform.gameObject);
                            }
                        }
                    }
                }
            }


            IHealthComponent GetEnemyInChain(GameObject affectedEnemy) =>
                player.EnemyControlSystem.AllEnemies.Find(enemy => enemy.Prefab == affectedEnemy);
        }

        public void Set()
        {

        }
    }
}

// if (bullet.Target != null && enemyCount > 1)
//         {
//             spirit.DealDamage(bullet.Target, spirit.DamageInstance);

//             if (bullet.RemainingBounceCount <= 0)
//                 spirit.ShootSystem.SetTargetReached(bullet);
//             else
//             {
//                 bullet.RemainingBounceCount--;
//                 for (int enemyIndexInChain = 0; enemyIndexInChain < enemyCount; enemyIndexInChain++)
//                 {
//                     if (bullet.Target.Prefab == enemies[enemyIndexInChain].gameObject)
//                     {
//                         if (enemyIndexInChain - 1 >= 0)
//                         {
//                             bullet.Target = GetEnemyInChain(enemies[enemyIndexInChain - 1].transform.gameObject);

//                             for (int j = 0; j < player.EnemyControlSystem.AllEnemies.Count; j++)
//                                 if (player.EnemyControlSystem.AllEnemies[j].Prefab == enemies[enemyIndexInChain - 1].transform.gameObject)
//                                 {
//                                     bullet.Target = player.EnemyControlSystem.AllEnemies[j];
//                                     break;
//                                 }
//                             break;
//                         }

//                         if (enemyIndexInChain + 1 < enemyCount)
//                         {
//                             for (int j = 0; j < player.EnemyControlSystem.AllEnemies.Count; j++)
//                                 if (player.EnemyControlSystem.AllEnemies[j].Prefab == enemies[enemyIndexInChain + 1].transform.gameObject)
//                                 {
//                                     bullet.Target = player.EnemyControlSystem.AllEnemies[j];
//                                     break;
//                                 }
//                             break;
//                         }
//                     }
//                 }
//             }
//         }
//         else
//         {
//             spirit.DealDamage(bullet.Target, spirit.DamageInstance);
//             spirit.ShootSystem.SetTargetReached(bullet);
//         }