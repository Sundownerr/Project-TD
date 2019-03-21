using System.Collections;
using System.Collections.Generic;
using Game.Data;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
    public class ChainshotSystem : ITraitHandler
    {
        public ITraitSystem Owner { get; set; }

        private Chainshot trait;

        public ChainshotSystem(Chainshot trait, ITraitSystem owner)
        {
            this.trait = trait;
            Owner = owner;
        }

        public void IncreaseStatsPerLevel()
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

            if (bullet.Target != null && enemyCount > 1)
            {
                spirit.DealDamage(bullet.Target, spirit.Data.GetValue(Numeral.Damage));

                if (bullet.RemainingBounceCount <= 0)
                    spirit.ShootSystem.SetTargetReached(bullet);
                else
                {
                    bullet.RemainingBounceCount--;
                    for (int enemyIndexInChain = 0; enemyIndexInChain < enemyCount; enemyIndexInChain++)
                        if (bullet.Target.Prefab == enemies[enemyIndexInChain].gameObject)
                        {
                            if (enemyIndexInChain - 1 >= 0)
                            {
                                for (int j = 0; j < player.Enemies.Count; j++)
                                    if (player.Enemies[j].Prefab == enemies[enemyIndexInChain - 1].transform.gameObject)
                                    {
                                        bullet.Target = player.Enemies[j];
                                        break;
                                    }
                                break;
                            }

                            if (enemyIndexInChain + 1 < enemyCount)
                            {
                                for (int j = 0; j < player.Enemies.Count; j++)
                                    if (player.Enemies[j].Prefab == enemies[enemyIndexInChain + 1].transform.gameObject)
                                    {
                                        bullet.Target = player.Enemies[j];
                                        break;
                                    }
                                break;
                            }
                        }
                }
            }
            else
            {
                spirit.DealDamage(bullet.Target, spirit.Data.GetValue(Numeral.Damage));
                spirit.ShootSystem.SetTargetReached(bullet);
            }
        }

        public void Set()
        {

        }
    }
}