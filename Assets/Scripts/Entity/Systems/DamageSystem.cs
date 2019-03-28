using System;
using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
    public static class DamageSystem
    {
        public static event EventHandler<DamageEventArgs> DamageDealt = delegate { };

        public static void DealDamage(this IDamageDealer damageDealer, IHealthComponent target, double damage)
        {
            if (target == null || !target.HealthSystem.IsVulnerable)
                return;

            target.HealthSystem.ChangeHealth(GetDealerEntity(), CalculateDamage());

            #region Helper functions

            IDamageDealer GetDealerEntity()
            {
                var dealerEntity = damageDealer;
                if (dealerEntity is IDamageDealerChild dealerChild)
                    dealerEntity = dealerChild.GetOwnerOfType<IDamageDealer>();
                return dealerEntity;
            }

            double CalculateDamage()
            {
                var critCount = 0;

                if (damageDealer is SpiritSystem spirit)
                {
                    var critChance = spirit.Data.GetValue(Numeral.CritChance);
                    var isCrit = new double[] { critChance, 100 - critChance }.RollDice();

                    if (target is EnemySystem enemy)
                        damage = damage.GetPercent(spirit.Data.DamageToRace[(int)enemy.Data.Race]);

                    if (isCrit == 0)
                    {
                        var multicritCount = spirit.Data.GetValue(Numeral.MulticritCount);
                        var critMultiplier = 1 + spirit.Data.GetValue(Numeral.CritMultiplier);
                        var multicritChances = new double[(int)multicritCount];

                        critCount = 1;

                        for (int multicritNumber = 0; multicritNumber < multicritCount; multicritNumber++)
                        {
                            var chanceToMulticrit = Math.Pow(critChance * 0.1d, 1 + multicritNumber) * Math.Pow(0.1d, multicritNumber - 1);
                            multicritChances[multicritNumber] = chanceToMulticrit;
                        }

                        var chances = new double[2];

                        for (int multicritNumber = multicritChances.Length - 1; multicritNumber > 0; multicritNumber--)
                        {
                            chances[0] = multicritChances[multicritNumber];
                            chances[1] = 100 - multicritChances[multicritNumber];

                            var isMulticrit = chances.RollDice();

                            if (isMulticrit == 0)
                            {
                                critCount = multicritNumber + 1;
                                for (int i = 0; i < multicritNumber; i++)
                                    critMultiplier += critMultiplier;
                                break;
                            }
                        }

                        damage *= critMultiplier;
                    }
                    // add armor modificator
                }

                DamageDealt?.Invoke(null, new DamageEventArgs(target, damage, critCount));
                return damage;
            }

            #endregion  
        }
    }
}