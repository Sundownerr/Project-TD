﻿using System;
using Game.Managers;
using Game.Systems.Enemy;
using Game.Systems.Spirit;
using Game.Utility;

namespace Game.Systems
{
    public static class DamageSystem
    {
        public static event Action<DamageEventArgs> DamageDealt;

        public static void DealDamage(this IDamageDealer damageDealer, IHealthComponent target, double damage)
        {
            if (target == null)
                return;

            target.ChangeHealth(GetDealerEntity(), CalculateDamage());

            #region Helper functions

            IDamageDealer GetDealerEntity()
            {
                var dealerEntity = damageDealer;

                if (dealerEntity is IDamageDealerChild dealerChild)
                {
                    dealerEntity = dealerChild.GetOwnerOfType<IDamageDealer>();
                }

                return dealerEntity;
            }

            double CalculateDamage()
            {
                var critCount = 0;

                CalculateRaceDamageModifier();
                CalculateArmorDamageModifier();
                CalculateCrit();

                DamageDealt?.Invoke(new DamageEventArgs(target, damage, critCount));
                return damage;

                #region Helper functions

                void CalculateArmorDamageModifier()
                {
                    if (damageDealer is SpiritSystem spirit)
                    {
                        if (target is EnemySystem enemy)
                        {
                            var armorValue = enemy.Data.Get(Enums.Enemy.ArmorValue).Sum;
                            if (armorValue == 0) return;

                            var armorType = enemy.Data.ArmorType;
                            var damageToArmor = ReferenceHolder.Instance.DamageToArmorSettings.DamageToArmorList.Find(x => x.Type == spirit.Data.Base.DamageType).Percents[(int)armorType];
                            damage = damage.GetPercent(damageToArmor);

                            if (armorValue > 0)
                            {
                                var damageReductionPerArmorPoint = 0.06;
                                var diminishedDamageReduction = armorValue * damageReductionPerArmorPoint;
                                var damageReduction = (diminishedDamageReduction) / (1 + damageReductionPerArmorPoint * (armorValue));

                                damage -= damage.GetPercent(damageReduction * 100);
                                return;
                            }

                            if (armorValue < 0)
                            {
                                var damageIncrease = 1 - (2 - Math.Pow(0.94, -armorValue));
                                damage += damage.GetPercent(damageIncrease * 100);
                            }
                        }
                    }
                }

                void CalculateRaceDamageModifier()
                {
                    if (damageDealer is SpiritSystem spirit)
                    {
                        if (target is EnemySystem enemy)
                        {
                            damage = damage.GetPercent(spirit.Data.DamageToRace[(int)enemy.Data.Race]);
                        }
                    }
                }

                void CalculateCrit()
                {
                    if (damageDealer is SpiritSystem spirit)
                    {
                        var critChance = spirit.Data.Get(Enums.Spirit.CritChance).Sum;
                        var isCrit = new double[] { critChance, 100 - critChance }.RollDice() == 0;

                        if (isCrit)
                        {
                            critCount = 1;

                            var multicritCount = spirit.Data.Get(Enums.Spirit.MulticritCount).Sum;
                            var critMultiplier = 1 + spirit.Data.Get(Enums.Spirit.CritMultiplier).Sum;
                            var multicritChances = new double[(int)multicritCount];

                            CalculateMulticritChances();
                            CalculateCritMultiplier();

                            damage *= critMultiplier;

                            #region Helper functions

                            void CalculateMulticritChances()
                            {
                                for (int multicritNumber = 0; multicritNumber < multicritCount; multicritNumber++)
                                {
                                    // TODO: rename
                                    var value1 = Math.Pow(critChance * 0.1d, critCount + multicritNumber);
                                    var value2 = Math.Pow(0.1d, multicritNumber - critCount);
                                    var chanceToMulticrit = value1 * value2;

                                    multicritChances[multicritNumber] = chanceToMulticrit;
                                }
                            }

                            void CalculateCritMultiplier()
                            {
                                var chances = new double[2];

                                for (int multicritNumber = multicritChances.Length - 1; multicritNumber > 0; multicritNumber--)
                                {
                                    chances[0] = multicritChances[multicritNumber];
                                    chances[1] = 100 - multicritChances[multicritNumber];

                                    var isMulticrit = chances.RollDice() == 0;

                                    if (isMulticrit)
                                    {
                                        critCount = multicritNumber + 1;

                                        for (int i = 0; i < multicritNumber; i++)
                                        {
                                            critMultiplier += critMultiplier;
                                        }

                                        break;
                                    }
                                }
                            }

                            #endregion
                        }
                    }
                }

                #endregion
            }

            #endregion  
        }
    }
}