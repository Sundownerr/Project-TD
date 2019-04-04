using System;
using System.Collections.Generic;
using Game.Enemy;
using Game.Enemy.Data;
using Game.Data;
using UnityEngine;
using U = UnityEngine.Object;
using Game.Enums;
using Game.Wrappers;

namespace Game.Systems
{
    public static class WaveCreatingSystem
    {
        public static Queue<Wave> GenerateWaves(int waveAmount)
        {
            var randomWaveIDs = new int[waveAmount];
            var raceTypes = Enum.GetValues(typeof(RaceType));
            var generatedWaves = new Queue<Wave>(waveAmount);
            var standartWaves = ReferenceHolder.Get.WaveDataBase.Waves;
            var riftBreakerWaves = ReferenceHolder.Get.WaveDataBase.RiftbreakersWaves;

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

        public static Queue<Wave> GenerateWaves(List<WaveEnemyID> waveEnemyIDs)
        {
            var generatedWaves = new Queue<Wave>();

            for (int i = 0; i < waveEnemyIDs.Count; i++)
                generatedWaves.Enqueue(CreateWave(waveEnemyIDs[i], i));

            return generatedWaves;
        }

        private static EnemyData CreateNewEnemyData(EnemyData choosedData, int waveNumber, List<Ability> abilities, List<Trait> traits)
        {
            var newData = U.Instantiate(choosedData);
            newData.ID = new ID(choosedData.ID);

            CalculateStats();

            newData.Traits = traits;
            newData.Abilities = newData.IsBossOrCommander() ? abilities : new List<Ability>();

            return newData;

            #region Helper functions

            void CalculateStats()
            {
                newData.CreateNewAttributes();

                SetArmor();
                SetGoldAndExp();


                newData.Get(Enums.Enemy.ArmorValue).Value += waveNumber;
                newData.Get(Numeral.GoldCost).Value += 1 + waveNumber;        // waveCount / 7;
                newData.Get(Enums.Enemy.Health).Value += 500 + waveNumber * 10;

                if (waveNumber % 8 == 0)
                {
                    newData.Get(Enums.Enemy.ArmorValue).Value *= 2;
                    newData.Get(Enums.Enemy.Health).Value *= 2;
                    newData.Get(Numeral.GoldCost).Value = 0;
                }

                newData.Get(Enums.Enemy.MoveSpeed).Value += 120 + waveNumber * 5;
                newData.Get(Enums.Enemy.MaxHealth).Value = newData.Get(Enums.Enemy.Health).Value;
                newData.Get(Enums.Enemy.DefaultMoveSpeed).Value = newData.Get(Enums.Enemy.MoveSpeed).Value;

                #region Helper functions

                void SetGoldAndExp()
                {
                    var settings = ReferenceHolder.Get.EnemySettings;
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
                    newData.Get(Numeral.GoldCost).Value = gold;
                }

                void SetArmor()
                {
                    var armorTypes = Enum.GetValues(typeof(ArmorType));
                    var randomArmorId = StaticRandom.Instance.Next(0, armorTypes.Length);
                    newData.ArmorType = (ArmorType)armorTypes.GetValue(randomArmorId);
                }

                #endregion
            }

            #endregion
        }

        private static Wave CreateWave(WaveEnemyID waveIDs, int waveNumber)
        {
            var wave = ScriptableObject.CreateInstance<Wave>();
            var waveTraits = GetTraitsByID();
            var waveAbilities = GetAbilitiesByID();
            wave.EnemyTypes = new List<EnemyData>();

            for (int i = 0; i < waveIDs.IDs.Count; i++)
            {
                var raceID = waveIDs.IDs[i][0];
                var enemyID = waveIDs.IDs[i][1];
                var enemyFromDB = ReferenceHolder.Get.EnemyDataBase.Races[raceID].Enemies[enemyID];

                wave.EnemyTypes.Add(
                    CreateNewEnemyData(enemyFromDB, waveNumber, waveAbilities, waveTraits));
            }

            return wave;

            #region Helper functions

            List<Ability> GetAbilitiesByID()
            {
                var abilities = new HashSet<Ability>();

                for (int i = 0; i < waveIDs.AbilityIDs.Count; i++)
                {
                    var neededAbilityID = waveIDs.AbilityIDs[i];
                    var abilityFromDB = ReferenceHolder.Get.EnemyAbilityDataBase.Abilities.Find(x => x.ID.Compare(neededAbilityID));

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
                var traits = new HashSet<Trait>();

                for (int i = 0; i < waveIDs.TraitIDs.Count; i++)
                {
                    var neededTraitID = waveIDs.TraitIDs[i];
                    var traitFromDB = ReferenceHolder.Get.EnemyTraitDataBase.Traits.Find(x => x.ID.Compare(neededTraitID));

                    if (traitFromDB == null)
                    {
                        Debug.LogError($"trait not found in db");
                        return null;
                    }

                    traits.Add(traitFromDB);
                }
                return new List<Trait>(traits);
            }

            #endregion
        }

        private static Wave CreateWave(Wave wave, int waveNumber)
        {
            var races = ReferenceHolder.Get.EnemyDataBase.Races;
            var randomRace = RaceType.Humanoid;
            var waveRace = races[(int)randomRace];
            var fittingEnemies = ScriptableObject.CreateInstance<Wave>();
            var waveTraits = GetRandomTraits();
            var waveAbilities = GetRandomAbilities();

            if (waveNumber % 8 == 0)
            {
                waveRace = races[(int)RaceType.RiftCreature];
                waveTraits = new List<Trait>();
                waveAbilities = new List<Ability>();
            }

            fittingEnemies.EnemyTypes = new List<EnemyData>();

            for (int i = 0; i < waveRace.Enemies.Count; i++)
                if (waveRace.Enemies[i].WaveLevel <= waveNumber)
                    fittingEnemies.EnemyTypes.Add(waveRace.Enemies[i]);

            return GetFittingEnemies();

            #region Helper functions

            List<Ability> GetRandomAbilities()
            {
                var randomAbilityCount = StaticRandom.Instance.Next(0, 2);
                var randomAbilities = new HashSet<Ability>();

                for (int i = 0; i < randomAbilityCount; i++)
                {
                    var randomAbilityId = StaticRandom.Instance.Next(0, ReferenceHolder.Get.EnemyAbilityDataBase.Abilities.Count);
                    var randomAbility = ReferenceHolder.Get.EnemyAbilityDataBase.Abilities[randomAbilityId];

                    randomAbilities.Add(randomAbility);
                }
                return new List<Ability>(randomAbilities);
            }

            List<Trait> GetRandomTraits()
            {
                var randomTraitCount = StaticRandom.Instance.Next(0, 2);
                var randomTraits = new HashSet<Trait>();

                for (int i = 0; i < randomTraitCount; i++)
                {
                    var randomTraitId = StaticRandom.Instance.Next(0, ReferenceHolder.Get.EnemyTraitDataBase.Traits.Count);
                    var randomTrait = ReferenceHolder.Get.EnemyTraitDataBase.Traits[randomTraitId];

                    randomTraits.Add(randomTrait);
                }
                return new List<Trait>(randomTraits);
            }

            Wave GetFittingEnemies()
            {
                var sortedEnemies = ScriptableObject.CreateInstance<Wave>();
                sortedEnemies.EnemyTypes = new List<EnemyData>();

                var choosedEnemies = new EnemyData[]
                {
                    ChooseEnemyDataFrom(EnemyType.Small),
                    ChooseEnemyDataFrom(EnemyType.Boss),
                    ChooseEnemyDataFrom(EnemyType.Flying),
                    ChooseEnemyDataFrom(EnemyType.Commander),
                    ChooseEnemyDataFrom(EnemyType.Normal),
                };

                for (int i = 0; i < wave.EnemyTypes.Count; i++)
                    sortedEnemies.EnemyTypes.Add(
                        CreateNewEnemyData(GetEnemyDataOfType(wave.EnemyTypes[i]), waveNumber, waveAbilities, waveTraits));

                return sortedEnemies;

                #region Helper functions

                EnemyData GetEnemyDataOfType(EnemyData enemy)
                {
                    for (int i = 0; i < choosedEnemies.Length; i++)
                        if (choosedEnemies[i].Type == enemy.Type)
                            return choosedEnemies[i];
                    return null;
                }

                EnemyData ChooseEnemyDataFrom(EnemyType enemyType)
                {
                    var tempChoosedEnemies = new List<EnemyData>();

                    for (int i = 0; i < fittingEnemies.EnemyTypes.Count; i++)
                        if (fittingEnemies.EnemyTypes[i].Type == enemyType)
                            tempChoosedEnemies.Add(fittingEnemies.EnemyTypes[i]);

                    var random = StaticRandom.Instance.Next(0, tempChoosedEnemies.Count);
                    return tempChoosedEnemies.Count > 0 ? tempChoosedEnemies[random] : null;
                }

                #endregion
            }
            #endregion
        }


    }
}