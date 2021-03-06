﻿using System;
using System.Collections.Generic;
using Game.Data;
using UnityEngine;
using U = UnityEngine.Object;
using Game.Enums;
using Game.Utility;
using Game.Managers;
using Game.Data.Traits;
using Game.Data.EnemyEntity;
using Game.Data.EnemyEntity.Internal;
using Game.Data.Abilities;
using Game.Data.NetworkRequests;

namespace Game.Systems.Waves
{
    public static class CreatingSystem
    {
        public static Queue<Wave> GenerateWaves(int waveAmount)
        {
            var randomWaveIDs = new int[waveAmount];
            var raceTypes = Enum.GetValues(typeof(RaceType));
            var generatedWaves = new Queue<Wave>(waveAmount);
            var standartWaves =  ReferenceHolder.Instance.WaveDB.Waves;
            var riftBreakerWaves =  ReferenceHolder.Instance.WaveDB.RiftbreakersWaves;

            for (int i = 0; i < waveAmount; i++)
            {
                var isRiftbreakerWave = i % 8 == 0;
                var maxID = isRiftbreakerWave ? riftBreakerWaves.Count : standartWaves.Count;

                randomWaveIDs[i] = StaticRandom.Instance.Next(0, maxID);
            }

            for (int i = 0; i < waveAmount; i++)
            {
                var isRiftbreakerWave = i % 8 == 0;
                var waves = isRiftbreakerWave ? riftBreakerWaves : standartWaves;

                generatedWaves.Enqueue(CreateWave(waves[randomWaveIDs[i]], i));
            }

            return generatedWaves;
        }

        public static Queue<Wave> GenerateWaves(List<NetworkWaveData> networkWaveDatas)
        {
            var generatedWaves = new Queue<Wave>();

            for (int i = 0; i < networkWaveDatas.Count; i++)
            {
                generatedWaves.Enqueue(CreateWave(networkWaveDatas[i], i));
            }

            return generatedWaves;
        }
 
        static Data.EnemyEntity.Enemy CreateEnemyData(Data.EnemyEntity.Enemy choosedData, int waveNumber, List<Ability> abilities, List<Trait> traits, ArmorType armor)
        {
            var newData = U.Instantiate(choosedData);
            newData.Index = choosedData.Index;

            CalculateStats();

            newData.Traits = traits;
            newData.Abilities = newData.IsBossOrCommander ? abilities : null;
            newData.ArmorType = armor;

            return newData;

            void CalculateStats()
            {
                newData.CreateNewAttributes();

                SetGoldAndExp();

                newData.Get(Enums.Enemy.ArmorValue).Value += waveNumber;
                newData.Get(Numeral.ResourceCost).Value += 1 + waveNumber;        // waveCount / 7;
                newData.Get(Enums.Enemy.Health).Value += 500 + waveNumber * 10;

                if (waveNumber % 8 == 0)
                {
                    newData.Get(Enums.Enemy.ArmorValue).Value *= 2;
                    newData.Get(Enums.Enemy.Health).Value *= 2;
                    newData.Get(Numeral.ResourceCost).Value = 0;
                }

                newData.Get(Enums.Enemy.MoveSpeed).Value += 120 + waveNumber * 5;
                newData.Get(Enums.Enemy.MaxHealth).Value = newData.Get(Enums.Enemy.Health).Value;
                newData.Get(Enums.Enemy.DefaultMoveSpeed).Value = newData.Get(Enums.Enemy.MoveSpeed).Value;

                void SetGoldAndExp()
                {
                    var settings =  ReferenceHolder.Instance.EnemySettings;
                    var gold = 0d;
                    var exp = 0d;

                    switch (newData.Type)
                    {
                        case EnemyType.Small:
                            gold = settings.SmallGold;
                            exp = settings.SmallExp;
                            break;

                        case EnemyType.Normal:
                            gold = settings.NormalGold;
                            exp = settings.NormalExp;
                            break;

                        case EnemyType.Commander:
                            gold = settings.CommanderGold;
                            exp = settings.ChampionExp;
                            break;

                        case EnemyType.Boss:
                            gold = settings.BossGold;
                            exp = settings.BossExp;
                            break;

                        case EnemyType.Flying:
                            gold = settings.FlyingGold;
                            exp = settings.FlyingExp;
                            break;

                        default:
                            break;
                    }

                    newData.Get(Numeral.Exp).Value = exp;
                    newData.Get(Numeral.ResourceCost).Value = gold;
                }
            }
        }


        static Wave CreateWave(NetworkWaveData networkWaveData, int waveNumber)
        {
            var wave = ScriptableObject.CreateInstance<Wave>();
            var waveTraits = GetTraitsByID();
            var waveAbilities = GetAbilitiesByID();
            var armor = (ArmorType)Enum.GetValues(typeof(ArmorType)).GetValue(networkWaveData.ArmorIndex);

            wave.EnemyTypes = new List<Data.EnemyEntity.Enemy>();

            networkWaveData.EnemyIndexes.ForEach(index =>
            {  
                var enemyFromDB = U.Instantiate( ReferenceHolder.Instance.EnemyDB.Data[index]);
                wave.EnemyTypes.Add(CreateEnemyData(enemyFromDB, waveNumber, waveAbilities, waveTraits, armor));
            });

            return wave;

            List<Ability> GetAbilitiesByID()
            {
                if (networkWaveData.AbilityIndexes == null)
                {
                    return null;
                }

                var abilities = new HashSet<Ability>();

                foreach (var index in networkWaveData.AbilityIndexes)
                {
                    var abilityFromDB =  ReferenceHolder.Instance.AbilityDB.Data.Find(abilityInDataBase => abilityInDataBase.Index == index);

                    if (abilityFromDB == null)
                    {
                        Debug.LogError($"ability not found in db");
                        return null;
                    }

                    abilities.Add(abilityFromDB);
                }
                return new List<Ability>(abilities);
            }

            List<Trait> GetTraitsByID()
            {
                if (networkWaveData.TraitIndexes == null)
                {
                    return null;
                }

                var traits = new HashSet<Trait>();

                foreach (var index in networkWaveData.TraitIndexes)
                {
                    var traitFromDB =  ReferenceHolder.Instance.TraitDB.Data.Find(traitInDataBase => traitInDataBase.Index == index);

                    if (traitFromDB == null)
                    {
                        Debug.LogError($"trait not found in db");
                        return null;
                    }

                    traits.Add(traitFromDB);
                }
                return new List<Trait>(traits);
            }
        }

        static ArmorType GetRandomArmor()
        {
            var armorTypes = Enum.GetValues(typeof(ArmorType));
            var randomArmorId = StaticRandom.Instance.Next(0, armorTypes.Length);

            return (ArmorType)armorTypes.GetValue(randomArmorId);
        }

        static Wave CreateWave(Wave wave, int waveNumber)
        {
            var enemiesInDataBase =  ReferenceHolder.Instance.EnemyDB.Data;
            var randomRace = RaceType.Humanoid;
            var fittingEnemies = ScriptableObject.CreateInstance<Wave>();
            var waveTraits = GetRandomTraits();
            var waveAbilities = GetRandomAbilities();
            var armor = GetRandomArmor();

            if (waveNumber % 8 == 0)
            {
                randomRace = RaceType.RiftCreature;
                waveTraits = null;
                waveAbilities = null;
            }

            fittingEnemies.EnemyTypes = enemiesInDataBase.FindAll(enemy => enemy.Race == randomRace);

            return GetFittingEnemies();

            List<Ability> GetRandomAbilities()
            {
                var randomAbilityCount = StaticRandom.Instance.Next(0, 2);
                var randomAbilities = new HashSet<Ability>();

                for (int i = 0; i < randomAbilityCount; i++)
                {
                    var randomAbilityId = StaticRandom.Instance.Next(0,  ReferenceHolder.Instance.AbilityDB.Data.Count);
                    var randomAbility =  ReferenceHolder.Instance.AbilityDB.Data[randomAbilityId];

                    randomAbilities.Add(randomAbility);
                }
                return randomAbilities.Count > 0 ? new List<Ability>(randomAbilities) : null;
            }

            List<Trait> GetRandomTraits()
            {
                var randomTraitCount = StaticRandom.Instance.Next(0, 2);
                var randomTraits = new HashSet<Trait>();

                for (int i = 0; i < randomTraitCount; i++)
                {
                    var randomTraitId = StaticRandom.Instance.Next(0,  ReferenceHolder.Instance.TraitDB.Data.Count);
                    var randomTrait =  ReferenceHolder.Instance.TraitDB.Data[randomTraitId];

                    randomTraits.Add(randomTrait);
                }
                return randomTraits.Count > 0 ? new List<Trait>(randomTraits) : null;
            }

            Wave GetFittingEnemies()
            {
                var sortedEnemies = ScriptableObject.CreateInstance<Wave>();
                sortedEnemies.EnemyTypes = new List<Data.EnemyEntity.Enemy>();

                var choosedEnemies = new Data.EnemyEntity.Enemy[]
                {
                    ChooseEnemyDataFrom(EnemyType.Small),
                    ChooseEnemyDataFrom(EnemyType.Boss),
                    ChooseEnemyDataFrom(EnemyType.Flying),
                    ChooseEnemyDataFrom(EnemyType.Commander),
                    ChooseEnemyDataFrom(EnemyType.Normal),
                };

                wave.EnemyTypes.ForEach(enemy =>                
                    sortedEnemies.EnemyTypes.Add(CreateEnemyData(GetEnemyDataOfType(enemy.Type), waveNumber, waveAbilities, waveTraits, armor))
                );

                return sortedEnemies;

                Data.EnemyEntity.Enemy GetEnemyDataOfType(EnemyType type)
                {
                    for (int i = 0; i < choosedEnemies.Length; i++)
                    {
                        if (choosedEnemies[i].Type == type)
                        {
                            return choosedEnemies[i];
                        }
                    }
                    return null;
                }

                Data.EnemyEntity.Enemy ChooseEnemyDataFrom(EnemyType enemyType)
                {
                    var tempChoosedEnemies = new List<Data.EnemyEntity.Enemy>();

                    for (int i = 0; i < fittingEnemies.EnemyTypes.Count; i++)
                    {
                        if (fittingEnemies.EnemyTypes[i].Type == enemyType)
                        {
                            tempChoosedEnemies.Add(fittingEnemies.EnemyTypes[i]);
                        }
                    }

                    var random = StaticRandom.Instance.Next(0, tempChoosedEnemies.Count);
                    return tempChoosedEnemies.Count > 0 ? tempChoosedEnemies[random] : null;
                }
            }
        }
    }
}