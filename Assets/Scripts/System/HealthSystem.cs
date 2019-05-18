using System;
using Game.Enums;
using UnityEngine;
using Game.Systems.Enemy;
using Game.Systems.Spirit;

namespace Game.Systems
{
    public class HealthSystem
    {

        public event Action<IHealthComponent> ZeroHealth;
        public bool IsVulnerable { get; set; }

        IHealthComponent owner;
        double maxHealth, healthRegen, regenTimer;

        public HealthSystem(IHealthComponent owner)
        {
            this.owner = owner;
            ZeroHealth += owner.OnZeroHealth;

            if (owner is EnemySystem enemy)
            {
                maxHealth = enemy.Data.Get(Enums.Enemy.MaxHealth).Sum;
            }
        }

        public void UpdateSystem()
        {
            if (owner is EnemySystem enemy)
            {
                var health = enemy.Data.Get(Enums.Enemy.Health).Sum;
                healthRegen = enemy.Data.Get(Enums.Enemy.HealthRegen).Sum;

                if (health < maxHealth)
                {
                    regenTimer = regenTimer > 1 ? 0 : regenTimer += Time.deltaTime;

                    if (regenTimer == 1)
                        health += healthRegen;
                }
                else
                    if (health > maxHealth)
                    health = maxHealth;
            }
        }

        public void ChangeHealth(IDamageDealer changer, double damage)
        {
            if (!IsVulnerable)
                return;

            if (owner is EnemySystem enemy)
            {


                enemy.LastDamageDealer = changer;

                if (enemy.Data.Get(Enums.Enemy.Health).AppliedValue > 0)
                    enemy.Data.Get(Enums.Enemy.Health).AppliedValue -= damage;
                else
                    enemy.Data.Get(Enums.Enemy.Health).Value -= damage;

                var remainingHealth =
                    enemy.Data.Get(Enums.Enemy.Health).AppliedValue > 0 ?
                    enemy.Data.Get(Enums.Enemy.Health).AppliedValue :
                    enemy.Data.Get(Enums.Enemy.Health).Value;

                if (remainingHealth <= 0)
                {
                    GiveResources();
                    IsVulnerable = false;

                    ZeroHealth?.Invoke(owner);
                }
            }

            #region  Helper functions

            void GiveResources()
            {
                if (enemy.LastDamageDealer is SpiritSystem spirit)
                    spirit.AddExp((int)enemy.Data.Get(Numeral.Exp).Sum);
            }

            #endregion
        }
    }
}