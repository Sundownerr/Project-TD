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
                generatedWaves.Add(CreateWave(waveEnemyIDs[i].IDs, i));
               
            return generatedWaves;
        }

        private static Wave CreateWave(ListID listID, int waveNumber)
        {
            var wave = ScriptableObject.CreateInstance<Wave>();  
            var armorTypes = Enum.GetValues(typeof(Armor.ArmorType));
            var randomArmorId = StaticRandom.Instance.Next(0, armorTypes.Length);

            wave.EnemyTypes = new List<EnemyData>();

            for (int i = 0; i < listID.Count; i++)
            {
                var raceID = listID[i][0];
                var enemyID = listID[i][1];
                var enemyFromDB = ReferenceHolder.Get.EnemyDataBase.Races[raceID].Enemies[enemyID];
                var newEnemy = U.Instantiate(enemyFromDB);

                newEnemy.ID = new ID();
                newEnemy.ID.AddRange(enemyFromDB.ID);

                CalculateStats(enemyFromDB, (Armor.ArmorType)armorTypes.GetValue(randomArmorId), waveNumber);

                wave.EnemyTypes.Add(enemyFromDB);
            }

            return wave;
        }

        private static Wave CreateWave(Wave wave, int waveNumber)
        {
            var races = ReferenceHolder.Get.EnemyDataBase.Races;
            var waveRace = RaceType.Humanoid;
            var fittingEnemies = ScriptableObject.CreateInstance<Wave>();
            var armorTypes = Enum.GetValues(typeof(Armor.ArmorType));
            var randomArmorId = StaticRandom.Instance.Next(0, armorTypes.Length);
            fittingEnemies.EnemyTypes = new List<EnemyData>();

            for (int i = 0; i < races.Count; i++)
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

                    CalculateStats(newEnemy, (Armor.ArmorType)armorTypes.GetValue(randomArmorId), waveNumber);

                   
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

        private static void CalculateStats(EnemyData enemy, Armor.ArmorType armorType, int waveNumber)
        {
            enemy.AppliedAttributes = ExtensionMethods.CreateAttributeList();
            enemy.BaseAttributes = ExtensionMethods.CreateAttributeList();
            enemy.ArmorType = armorType;

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

            enemy.Traits = new List<Trait>(GetRandomTraits());
            enemy.Abilities = new List<Ability>();

            if (enemy.Type == EnemyType.Boss || enemy.Type == EnemyType.Commander)
            {
                enemy.Abilities.AddRange(GetRandomAbilities());
            }
            
            #region Helper functions

            List<Ability> GetRandomAbilities()
            {
                var randomAbilityCount = StaticRandom.Instance.Next(0, 2);
                var randomAbilities = new List<Ability>();

                for (int i = 0; i < randomAbilityCount; i++)
                {
                    var isHaveSameAbility = false;
                    var randomAbilityId = StaticRandom.Instance.Next(0, ReferenceHolder.Get.EnemyAbilityDataBase.Abilities.Count);
                    var randomAbility = ReferenceHolder.Get.EnemyAbilityDataBase.Abilities[randomAbilityId];

                    for (int j = 0; j < randomAbilities.Count; j++)
                        if (randomAbilities[j].ID.Compare(randomAbility.ID))
                        {
                            isHaveSameAbility = true;
                            break;
                        }

                    if (!isHaveSameAbility)
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
                    var isHaveSameTrait = false;
                    var randomTraitId = StaticRandom.Instance.Next(0, ReferenceHolder.Get.EnemyTraitDataBase.Traits.Count);
                    var randomTrait = ReferenceHolder.Get.EnemyTraitDataBase.Traits[randomTraitId];

                    for (int j = 0; j < randomTraits.Count; j++)
                        if (randomTraits[j].ID.Compare(randomTrait.ID))
                        {
                            isHaveSameTrait = true;
                            break;
                        }

                    if (!isHaveSameTrait)
                        randomTraits.Add(randomTrait);
                }
                return randomTraits;
            }

            #endregion
        }

        


    }
}
