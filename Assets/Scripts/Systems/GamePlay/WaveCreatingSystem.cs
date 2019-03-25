using System;
using System.Collections.Generic;
using Game.Enemy;
using Game.Enemy.Data;
using Game.Data;
using UnityEngine;
using U = UnityEngine.Object;

namespace Game.Systems
{
    public static class WaveCreatingSystem
    {
        public static List<Wave> GenerateWaves(int waveAmount)
        {
            var randomWaveIds = new List<int>(waveAmount);
            var raceTypes = Enum.GetValues(typeof(RaceType));
            var generatedWaves = new List<Wave>(waveAmount);
            var waves = ReferenceHolder.Get.WaveDataBase.Waves;

            for (int i = 0; i < waveAmount; i++)
                randomWaveIds.Add(StaticRandom.Instance.Next(0, waves.Count));

            for (int i = 0; i < waveAmount; i++)
                generatedWaves.Add(CreateWave(waves[randomWaveIds[i]], i));

            return generatedWaves;
        }

        public static List<Wave> GenerateWaves(List<WaveEnemyID> waveEnemyIDs)
        {
            var generatedWaves = new List<Wave>();

            for (int i = 0; i < waveEnemyIDs.Count; i++)
                generatedWaves.Add(CreateWave(waveEnemyIDs[i], i));

            return generatedWaves;
        }

        private static Wave CreateWave(WaveEnemyID waveIDs, int waveNumber)
        {
            var wave = ScriptableObject.CreateInstance<Wave>();
            wave.EnemyTypes = new List<EnemyData>();

            for (int i = 0; i < waveIDs.IDs.Count; i++)
            {
                var raceID = waveIDs.IDs[i][0];
                var enemyID = waveIDs.IDs[i][1];
                var enemyFromDB = ReferenceHolder.Get.EnemyDataBase.Races[raceID].Enemies[enemyID];
                var newEnemy = U.Instantiate(enemyFromDB);

                newEnemy.ID = new ID();
                newEnemy.ID.AddRange(enemyFromDB.ID);

                CalculateStats(newEnemy, waveNumber, false);

                newEnemy.Traits = GetTraitsByID();
                newEnemy.Abilities = new List<Ability>();

                if (newEnemy.Type == EnemyType.Boss || newEnemy.Type == EnemyType.Commander)
                    newEnemy.Abilities = GetAbilitiesByID();

                wave.EnemyTypes.Add(newEnemy);
            }

            return wave;

            #region Helper functions

            List<Ability> GetAbilitiesByID()
            {
                var abilities = new List<Ability>();

                for (int i = 0; i < waveIDs.AbilityIDs.Count; i++)
                {
                    var neededAbilityID = waveIDs.AbilityIDs[i];
                    var abilityFromDB = ReferenceHolder.Get.EnemyAbilityDataBase.Abilities.Find(x => x.ID.Compare(neededAbilityID));

                    if (abilityFromDB == null)
                    {
                        Debug.LogError($"ability not found in db");
                        return null;
                    }

                    if (!abilities.Contains(abilityFromDB))
                        abilities.Add(abilityFromDB);
                }
                return abilities;
            }

            List<Trait> GetTraitsByID()
            {
                var traits = new List<Trait>();

                for (int i = 0; i < waveIDs.TraitIDs.Count; i++)
                {
                    var neededTraitID = waveIDs.TraitIDs[i];
                    var traitFromDB = ReferenceHolder.Get.EnemyTraitDataBase.Traits.Find(x => x.ID.Compare(neededTraitID));

                    if (traitFromDB == null)
                    {
                        Debug.LogError($"trait not found in db");
                        return null;
                    }

                    if (!traits.Contains(traitFromDB))
                        traits.Add(traitFromDB);
                }
                return traits;
            }

            #endregion
        }

        private static Wave CreateWave(Wave wave, int waveNumber)
        {
            var races = ReferenceHolder.Get.EnemyDataBase.Races;
            var waveRace = RaceType.Humanoid;
            var fittingEnemies = ScriptableObject.CreateInstance<Wave>();
            var armorTypes = Enum.GetValues(typeof(Armor.ArmorType));
            var randomArmorId = StaticRandom.Instance.Next(0, armorTypes.Length);
            fittingEnemies.EnemyTypes = new List<EnemyData>();

            for (int i = 0; i < races.Length; i++)
                if (i == (int)waveRace)
                    for (int j = 0; j < races[i].Enemies.Count; j++)
                        if (races[i].Enemies[j].WaveLevel <= waveNumber)
                            fittingEnemies.EnemyTypes.Add(races[i].Enemies[j]);

            return GetFittingEnemies();

            #region Helper functions

            Wave GetFittingEnemies()
            {
                var sortedEnemies = ScriptableObject.CreateInstance<Wave>();
                sortedEnemies.EnemyTypes = new List<EnemyData>();

                var choosedEnemies = new EnemyData[]
                {
                    ChooseEnemy(EnemyType.Small),
                    ChooseEnemy(EnemyType.Boss),
                    ChooseEnemy(EnemyType.Flying),
                    ChooseEnemy(EnemyType.Commander),
                    ChooseEnemy(EnemyType.Normal),
                };

                for (int i = 0; i < wave.EnemyTypes.Count; i++)
                {
                    var enemy = GetEnemyOfType(wave.EnemyTypes[i]);
                    var newEnemy = U.Instantiate(enemy);

                    newEnemy.ID = new ID();
                    newEnemy.ID.AddRange(enemy.ID);

                    CalculateStats(newEnemy, waveNumber, true);

                    sortedEnemies.EnemyTypes.Add(newEnemy);
                }

                return sortedEnemies;

                #region Helper functions

                EnemyData GetEnemyOfType(EnemyData enemy)
                {
                    for (int i = 0; i < choosedEnemies.Length; i++)
                        if (choosedEnemies[i].Type == enemy.Type)
                            return choosedEnemies[i];
                    return null;
                }

                EnemyData ChooseEnemy(EnemyType enemyType)
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

        private static void CalculateStats(EnemyData enemy, int waveNumber, bool addRandomTraitsAndAbilities)
        {
            var armorTypes = Enum.GetValues(typeof(Armor.ArmorType));
            var randomArmorId = StaticRandom.Instance.Next(0, armorTypes.Length);

            enemy.AppliedAttributes = ExtensionMethods.CreateAttributeList();
            enemy.BaseAttributes = ExtensionMethods.CreateAttributeList();
            enemy.ArmorType = (Armor.ArmorType)armorTypes.GetValue(randomArmorId);

            var gold = 0d;
            var exp = 0d;
            var settings = ReferenceHolder.Get.EnemySettings;

            if (enemy.Type == EnemyType.Small) { gold = settings.SmallGold; exp = settings.SmallExp; }
            else
            if (enemy.Type == EnemyType.Normal) { gold = settings.NormalGold; exp = settings.NormalExp; }
            else
            if (enemy.Type == EnemyType.Commander) { gold = settings.CommanderGold; exp = settings.ChampionExp; }
            else
            if (enemy.Type == EnemyType.Boss) { gold = settings.BossGold; exp = settings.BossExp; }
            else
            if (enemy.Type == EnemyType.Flying) { gold = settings.FlyingGold; exp = settings.FlyingExp; }

            enemy.Get(Numeral.ArmorValue, From.Base).Value = waveNumber;
            enemy.Get(Numeral.DefaultMoveSpeed, From.Base).Value = 120 + waveNumber * 5;
            enemy.Get(Numeral.GoldCost, From.Base).Value = 1 + waveNumber;        // waveCount / 7;
            enemy.Get(Numeral.Health, From.Base).Value = 500 + waveNumber * 10;
            enemy.Get(Numeral.MaxHealth, From.Base).Value = 500 + waveNumber * 10;
            enemy.Get(Numeral.MoveSpeed, From.Base).Value = 120 + waveNumber * 5;
            enemy.Get(Numeral.Exp, From.Base).Value = exp;
            enemy.Get(Numeral.GoldCost, From.Base).Value = gold;

            if (addRandomTraitsAndAbilities)
            {
                enemy.Traits = new List<Trait>(GetRandomTraits());
                enemy.Abilities = new List<Ability>();

                if (enemy.Type == EnemyType.Boss || enemy.Type == EnemyType.Commander)               
                    enemy.Abilities = GetRandomAbilities();
                
            }

            #region Helper functions

            List<Ability> GetRandomAbilities()
            {
                var randomAbilityCount = StaticRandom.Instance.Next(0, 2);
                var randomAbilities = new List<Ability>();

                for (int i = 0; i < randomAbilityCount; i++)
                {
                    var randomAbilityId = StaticRandom.Instance.Next(0, ReferenceHolder.Get.EnemyAbilityDataBase.Abilities.Count);
                    var randomAbility = ReferenceHolder.Get.EnemyAbilityDataBase.Abilities[randomAbilityId];

                    if (!randomAbilities.Contains(randomAbility))
                        randomAbilities.Add(randomAbility);
                }
                return randomAbilities;
            }

            List<Trait> GetRandomTraits()
            {
                var randomTraitCount = StaticRandom.Instance.Next(0, 2);
                var randomTraits = new List<Trait>();

                for (int i = 0; i < randomTraitCount; i++)
                {
                    var randomTraitId = StaticRandom.Instance.Next(0, ReferenceHolder.Get.EnemyTraitDataBase.Traits.Count);
                    var randomTrait = ReferenceHolder.Get.EnemyTraitDataBase.Traits[randomTraitId];

                    if (!randomTraits.Contains(randomTrait))
                        randomTraits.Add(randomTrait);
                }
                return randomTraits;
            }
            #endregion
        }
    }
}


