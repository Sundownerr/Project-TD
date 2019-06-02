using System;
using Game.Data.SpiritEntity.Internal;
using Game.Managers;
using Game.Systems.Enemy;
using Game.Systems.Spirit;
using Game.Utility;

namespace Game.Systems
{
    public static class DamageSystem
    {
        public static event Action<DamageEventArgs> DamageDealt;

        public static void DealDamage(this IDamageDealer damageDealer, IHealthComponent target, (double value, DamageType type) damageInstance)
        {
            if (target == null)
                return;

            var newDamageInstance =
                GetCritModifier(
                    GetArmorModifier(
                        GetRaceModifier(damageInstance.value)));

            target.ChangeHealth(GetDealerEntity(), newDamageInstance.value);

            DamageDealt?.Invoke(new DamageEventArgs(target, (newDamageInstance.value, newDamageInstance.critCount, damageInstance.type)));

            IDamageDealer GetDealerEntity()
            {
                var dealerEntity = damageDealer;

                if (dealerEntity is IDamageDealerChild dealerChild)
                {
                    dealerEntity = dealerChild.GetOwnerOfType<IDamageDealer>();
                }

                return dealerEntity;
            }

            double GetArmorModifier(double currentDamage)
            {
                if (target is EnemySystem enemy)
                {
                    var armorValue = enemy.Data.Get(Enums.Enemy.ArmorValue).Sum;

                    if (armorValue == 0)
                    {
                        return currentDamage;
                    }

                    var damageToArmor = ReferenceHolder.Instance.DamageToArmorSettings.DamageToArmorList.Find(x => x.Type == damageInstance.type).Percents[(int)enemy.Data.ArmorType];
                    var modifiedDamage = currentDamage.GetPercent(damageToArmor);

                    if (armorValue > 0)
                    {
                        return GetDecreasedDamage();
                    }

                    if (armorValue < 0)
                    {
                        return GetIncreasedDamage();
                    }

                    double GetDecreasedDamage()
                    {
                        var damageReductionPerArmorPoint = 0.06;
                        var diminishedDamageReduction = armorValue * damageReductionPerArmorPoint;
                        var damageReduction = (diminishedDamageReduction) / (1 + damageReductionPerArmorPoint * (armorValue));

                        modifiedDamage -= modifiedDamage.GetPercent(damageReduction * 100);

                        return modifiedDamage;
                    }

                    double GetIncreasedDamage()
                    {
                        var damageIncrease = 1 - (2 - Math.Pow(0.94, -armorValue));
                        modifiedDamage += modifiedDamage.GetPercent(damageIncrease * 100);

                        return modifiedDamage;
                    }
                }

                return currentDamage;
            }

            double GetRaceModifier(double currentDamage)
            {
                if (damageDealer is SpiritSystem spirit)
                {
                    if (target is EnemySystem enemy)
                    {
                        return currentDamage.GetPercent(spirit.Data.DamageToRace[(int)enemy.Data.Race]);
                    }
                }
                return currentDamage;
            }

            (double value, int critCount) GetCritModifier(double currentDamage)
            {
                if (damageDealer is SpiritSystem spirit)
                {
                    var critChance = spirit.Data.Get(Enums.Spirit.CritChance).Sum;
                    var isCrit = new double[] { critChance, 100 - critChance }.RollDice() == 0;

                    if (!isCrit)
                    {
                        return (currentDamage, 0);
                    }
                    else
                    {
                        var crit = GetCritInstance();

                        return (currentDamage * crit.multiplier, crit.count);

                        (double multiplier, int count) GetCritInstance()
                        {
                            var critCount = 1;
                            var multicritChances = GetMulticritChances();
                            var chancesToMulticrit = new double[2];
                            var multiplier = 1 + spirit.Data.Get(Enums.Spirit.CritMultiplier).Sum;

                            for (int multicritNumber = multicritChances.Length - 1; multicritNumber > 0; multicritNumber--)
                            {
                                chancesToMulticrit[0] = multicritChances[multicritNumber];
                                chancesToMulticrit[1] = 100 - multicritChances[multicritNumber];

                                var isMulticrit = chancesToMulticrit.RollDice() == 0;

                                if (isMulticrit)
                                {
                                    critCount = multicritNumber + 1;

                                    for (int i = 0; i < multicritNumber; i++)
                                    {
                                        multiplier += multiplier;
                                    }

                                    break;
                                }
                            }

                            return (multiplier, critCount);

                            double[] GetMulticritChances()
                            {
                                var multicritCount = spirit.Data.Get(Enums.Spirit.MulticritCount).Sum;
                                var newMulticritChances = new double[(int)multicritCount];

                                for (int multicritNumber = 0; multicritNumber < multicritCount; multicritNumber++)
                                {
                                    // TODO: rename
                                    var value1 = Math.Pow(critChance * 0.1d, critCount + multicritNumber);
                                    var value2 = Math.Pow(0.1d, multicritNumber - critCount);
                                    var chanceToMulticrit = value1 * value2;

                                    newMulticritChances[multicritNumber] = chanceToMulticrit;
                                }
                                return newMulticritChances;
                            }
                        }
                    }
                }
                return (currentDamage, 0);
            }
        }
    }
}