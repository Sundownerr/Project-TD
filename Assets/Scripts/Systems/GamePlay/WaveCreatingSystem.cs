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
        public static List<EnemyData> CreateWave(Wave wave, int waveNumber)
        {
            var races           = ReferenceHolder.Get.EnemyDataBase.Races;   
            var waveRace        = RaceType.Humanoid;              
            var fittingEnemies   = new List<EnemyData>();               
            var armorTypes      = Enum.GetValues(typeof(Armor.ArmorType));
            var randomArmorId   = StaticRandom.Instance.Next(0, armorTypes.Length);  

           
            for (int i = 0; i < races.Count; i++)
                if (i == (int)waveRace)
                    for (int j = 0; j < races[i].Enemies.Count; j++)
                        if (races[i].Enemies[j].WaveLevel >= ReferenceHolder.Get.Player.WaveSystem.WaveNumber)
                            fittingEnemies.Add(races[i].Enemies[j]);                  
                               
            return GetFittingEnemies();

            #region Helper functions

            List<EnemyData> GetFittingEnemies()
            {
                var sortedEnemies = new List<EnemyData>();
                var choosedEnemies = new EnemyData[]
                {
                    ChooseEnemy(EnemyType.Small),
                    ChooseEnemy(EnemyType.Boss),
                    ChooseEnemy(EnemyType.Flying),
                    ChooseEnemy(EnemyType.Commander),
                    ChooseEnemy(EnemyType.Normal),
                };

                var traits = GetRandomTraits();
                var abilities = GetRandomAbilities();

                for (int i = 0; i < wave.EnemyTypes.Count; i++)       
                { 
                    var enemy = GetEnemyOfType(wave.EnemyTypes[i]);
                    var newEnemy = U.Instantiate(enemy);

                    CalculateStats(newEnemy);

                    sortedEnemies.Add(newEnemy);  
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

                    for (int i = 0; i < fittingEnemies.Count; i++)
                        if (fittingEnemies[i].Type == enemyType)                            
                                tempChoosedEnemies.Add(fittingEnemies[i]);                        
                    
                    var random = StaticRandom.Instance.Next(0, tempChoosedEnemies.Count);
                    return tempChoosedEnemies.Count > 0 ? tempChoosedEnemies[random] : null;                                                                   
                }

                void CalculateStats(EnemyData data)
                {
                    data.ArmorType = (Armor.ArmorType)armorTypes.GetValue(randomArmorId);

                    var gold = 0d;
                    var exp = 0d;
                    var settings = ReferenceHolder.Get.EnemySettings;

                    if (data.Type == EnemyType.Small) { gold = settings.SmallGold; exp = settings.SmallExp; }
                    else
                    if (data.Type == EnemyType.Normal) { gold = settings.NormalGold; exp = settings.NormalExp; }
                    else
                    if (data.Type == EnemyType.Commander) { gold = settings.CommanderGold; exp = settings.ChampionExp; }
                    else
                    if (data.Type == EnemyType.Boss) { gold = settings.BossGold; exp = settings.BossExp; }
                    else
                    if (data.Type == EnemyType.Flying) { gold = settings.FlyingGold; exp = settings.FlyingExp; }


                    data.Get(Numeral.ArmorValue, From.Base).Value        = waveNumber;
                    data.Get(Numeral.DefaultMoveSpeed, From.Base).Value  = 120 + waveNumber * 5;
                    data.Get(Numeral.GoldCost, From.Base).Value          = 1 + waveNumber;        // waveCount / 7;
                    data.Get(Numeral.Health, From.Base).Value            = 500 + waveNumber * 10;
                    data.Get(Numeral.MaxHealth, From.Base).Value         = 500 + waveNumber * 10;
                    data.Get(Numeral.MoveSpeed, From.Base).Value         = 120 + waveNumber * 5;
                    data.Get(Numeral.Exp, From.Base).Value = exp;
                    data.Get(Numeral.GoldCost, From.Base).Value = gold;

                    data.Traits = new List<Trait>();
                    data.Traits.AddRange(traits);

                    if (data.Type == EnemyType.Boss || data.Type == EnemyType.Commander)
                    {
                        data.Abilities = new List<Ability>();
                        data.Abilities.AddRange(abilities);
                    }

                    data.IsInstanced = true;
                }

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
                            if (randomAbilities[j].CompareId(randomAbility.Id))
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
                            if (randomTraits[j].CompareId(randomTrait.Id))
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
            #endregion
        }
    }
}
