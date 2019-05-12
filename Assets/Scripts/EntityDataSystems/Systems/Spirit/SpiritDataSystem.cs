using System;
using System.Collections.Generic;
using UnityEngine;
using U = UnityEngine.Object;
using Game.Enums;
using Game.Managers;
using Game.Data.Spirit;
using Game.Data.Items;

namespace Game.Systems.Spirit.Internal
{
    [Serializable]
    public class SpiritDataSystem
    {
        public SpiritData CurrentData { get; set; }
        public SpiritData BaseData { get; set; }
        public event EventHandler StatsChanged;
        public event EventHandler<SpiritSystem> LeveledUp;

        SpiritSystem ownerSpirit;
        List<Consumable> appliedConsumables = new List<Consumable>();

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

            CurrentData.Index = previousSpirit.Data.Index;
            CurrentData.GradeCount = previousSpirit.Data.GradeCount + 1;
            CurrentData.Get(Numeral.Exp).Value = previousSpirit.Data.Get(Numeral.Exp).Value;

            ownerSpirit.UsedCell = previousSpirit.UsedCell;

            for (int i = 0; i < previousSpirit.Data.Get(Numeral.Level).Value; i++)
                IncreaseStatsPerLevel();

            StatsChanged?.Invoke(null, null);
        }

        void IncreaseStatsPerLevel()
        {
            for (int i = 0; i < CurrentData.NumeralAttributes.Count; i++)
            {
                var attribute = CurrentData.NumeralAttributes[i];
                if (attribute.ValuePerLevel == 0) continue;

                attribute.Value += attribute.IncreasePerLevel == Increase.ByValue ?
                       attribute.ValuePerLevel :
                       attribute.Value.GetPercent(attribute.ValuePerLevel);
            }

            for (int i = 0; i < CurrentData.SpiritAttributes.Count; i++)
            {
                var attribute = CurrentData.SpiritAttributes[i];
                if (attribute.ValuePerLevel == 0) continue;

                attribute.Value += attribute.IncreasePerLevel == Increase.ByValue ?
                       attribute.ValuePerLevel :
                       attribute.Value.GetPercent(attribute.ValuePerLevel);
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
            CurrentData.Get(Numeral.Exp).Value += amount;

            for (int i = (int)CurrentData.Get(Numeral.Level).Value; i < 25; i++)
            {
                var nextLevel = (int)CurrentData.Get(Numeral.Level).Value;
                var currentExp = CurrentData.Get(Numeral.Exp).Value;
                var neededExp = ReferenceHolder.ExpToLevelUp[nextLevel];

                if (currentExp >= neededExp && nextLevel < 25)
                    IncreaseStatsPerLevel();
            }

            if (ownerSpirit.GetOwnerOfType<PlayerSystem>().PlayerInputSystem.ChoosedSpirit == ownerSpirit)
                StatsChanged?.Invoke(null, null);
        }
    }
}