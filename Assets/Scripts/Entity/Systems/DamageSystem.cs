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

        public static void DealDamage(this IDamageDealer dealer, IHealthComponent target, double damage)
        {
            if (target == null || !target.HealthSystem.IsVulnerable)
                return;

            var dealerEntity = dealer;

            if (dealerEntity is IDamageDealerChild dealerChild)
                if (dealerChild.Owner is IDamageDealer)
                    dealerEntity = dealerChild.Owner;
                else
                    while (dealerChild.Owner != null && !(dealerEntity is IDamageDealer))
                        dealerEntity = dealerChild.Owner;

            target.HealthSystem.ChangeHealth(dealerEntity, CalculateDamage());
            
            double CalculateDamage()
            {
                var critCount = 0;
                if (dealer is SpiritSystem spirit)
                {
                    var critChance = spirit.Data.GetValue(Numeral.CritChance);
                    var isCrit = new List<double>() { critChance, 100 - critChance }.RollDice();

                    if (target is EnemySystem enemy)
                        damage = damage.GetPercent(spirit.Data.DamageToRace[(int)enemy.Data.Race]);

                    if (isCrit == 0)
                    {
                        var multicritCount = spirit.Data.GetValue(Numeral.MulticritCount);
                        var critMultiplier = 1 + spirit.Data.GetValue(Numeral.CritMultiplier);
                        var multicritChances = new List<double>();

                        critCount = 1;

                        for (int multicritNumber = 0; multicritNumber < multicritCount; multicritNumber++)
                        {
                            var chanceToMulticrit = Math.Pow(critChance * 0.1d, 1 + multicritNumber) * Math.Pow(0.1d, multicritNumber - 1);
                            multicritChances.Add(chanceToMulticrit);
                        }

                        for (int multicritNumber = multicritChances.Count - 1; multicritNumber > 0; multicritNumber--)
                        {
                            var isMulticrit =
                                new List<double>() { multicritChances[multicritNumber], 100 - multicritChances[multicritNumber] }.RollDice();

                            if (isMulticrit == 0)
                            {
                                critCount = multicritNumber + 1;
                                for (int j = 0; j < multicritNumber; j++)
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
        }
    }
}