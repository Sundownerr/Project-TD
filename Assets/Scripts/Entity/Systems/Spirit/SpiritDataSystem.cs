using System;
using System.Collections.Generic;
using Game.Data;
using Game.Systems;
using Game.Spirit.Data;
using UnityEngine;
using U = UnityEngine.Object;

namespace Game.Spirit.System
{
    [Serializable]
    public class SpiritDataSystem 
    {
        public SpiritData CurrentData { get; set; }
        public SpiritData BaseData { get; set; }
        public event EventHandler StatsChanged = delegate { };
        public event EventHandler<SpiritSystem> LeveledUp = delegate { };

        private SpiritSystem ownerSpirit;
        private List<Consumable> appliedConsumables = new List<Consumable>();

        public SpiritDataSystem(SpiritSystem spirit) => ownerSpirit = spirit;

        public void Set() => Set(CurrentData);

        public void Set(SpiritData data)
        {
            CurrentData = U.Instantiate(data);
            CurrentData.SetData();

            BaseData = data;
   
            StatsChanged?.Invoke(null, null);
        }

        public void Upgrade(SpiritSystem previousSpirit, SpiritData newData)
        {
            Set(newData);

            CurrentData.ID = previousSpirit.Data.ID;
            CurrentData.GradeCount = previousSpirit.Data.GradeCount + 1;
            CurrentData.Get(Numeral.Exp, From.Base).Value = previousSpirit.Data.Get(Numeral.Exp, From.Base).Value;
            
            ownerSpirit.UsedCell = previousSpirit.UsedCell;

            for (int i = 0; i < previousSpirit.Data.Get(Numeral.Level, From.Base).Value; i++)
                IncreaseStatsPerLevel();

            StatsChanged?.Invoke(null, null);
        }

        private void IncreaseStatsPerLevel()
        {
            for (int i = 0; i < CurrentData.BaseAttributes.Count; i++)
            {
                var stat = CurrentData.Get(CurrentData.BaseAttributes[i].Type, From.Base);
                
                    stat.Value += stat.IncreacePerLevel == Change.ByValue ?
                        stat.ValuePerLevel :
                        stat.Value.GetPercent(stat.ValuePerLevel);
            }
           
            ownerSpirit.TraitControlSystem.IncreaseStatsPerLevel();

            var effect = U.Instantiate(
                ReferenceHolder.Get.LevelUpEffect, 
                ownerSpirit.Prefab.transform.position, 
                Quaternion.identity);

            U.Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);

            LeveledUp?.Invoke(null, ownerSpirit);
        }

        public void AddExp(int amount)
        {
            CurrentData.Get(Numeral.Exp, From.Base).Value += amount;

            for (int i = (int)CurrentData.Get(Numeral.Level, From.Base).Value; i < 25; i++)
            {
                var nextLevel = (int)CurrentData.Get(Numeral.Level, From.Base).Value;
                var currentExp = CurrentData.Get(Numeral.Exp, From.Base).Value;
                var neededExp = ReferenceHolder.ExpToLevelUp[nextLevel];

                if (currentExp >= neededExp && nextLevel < 25)
                    IncreaseStatsPerLevel();
            }

            if (ownerSpirit.GetOwnerOfType<PlayerSystem>().PlayerInputSystem.ChoosedSpirit == ownerSpirit)
                StatsChanged?.Invoke(null, null);
        }
    }
}