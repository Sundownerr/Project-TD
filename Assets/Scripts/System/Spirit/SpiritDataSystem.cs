using System;
using System.Collections.Generic;
using UnityEngine;
using U = UnityEngine.Object;
using Game.Enums;
using Game.Managers;
using Game.Data.SpiritEntity;
using Game.Data.Items;
using Game.Data.Attributes;

namespace Game.Systems.Spirit.Internal
{
    [Serializable]
    public class SpiritDataSystem
    {
        public SpiritData CurrentData { get; set; }
        public SpiritData BaseData { get; set; }
        public event Action StatsChanged;
        public event Action<SpiritSystem> LeveledUp;

        SpiritSystem ownerSpirit;
        List<Consumable> appliedConsumables = new List<Consumable>();

        public SpiritDataSystem(SpiritSystem spirit) => ownerSpirit = spirit;

        public void Set() => Set(CurrentData);

        public void Set(SpiritData data)
        {
            CurrentData = U.Instantiate(data);
            CurrentData.SetData();

            BaseData = data;

            StatsChanged?.Invoke();
        }

        public void Upgrade(SpiritSystem previousSpirit, SpiritData newData)
        {
            Set(newData);

            CurrentData.Index = previousSpirit.Data.Index;
            CurrentData.GradeCount = previousSpirit.Data.GradeCount + 1;
            CurrentData.Get(Numeral.Exp).Value = previousSpirit.Data.Get(Numeral.Exp).Value;

            for (int i = 0; i < previousSpirit.Data.Get(Numeral.Level).Value; i++)
            {
                LevelUp(isUpgrading: true);
            }

            StatsChanged?.Invoke();
        }

        void LevelUp(bool isUpgrading = false)
        {
            CurrentData.NumeralAttributes
                .FindAll(numeralAttribute => numeralAttribute.ValuePerLevel != 0)
                    .ForEach(attribute => attribute.LevelUp());

            CurrentData.SpiritAttributes
                .FindAll(spiritAttribute => spiritAttribute.ValuePerLevel != 0)
                    .ForEach(attribute => attribute.LevelUp());

            ownerSpirit.TraitControlSystem.IncreaseStatsPerLevel();

            if (!isUpgrading)
            {
                var effect = U.Instantiate(
                    ReferenceHolder.Instance.LevelUpEffect,
                    ownerSpirit.Prefab.transform.position,
                    Quaternion.identity);

                U.Destroy(effect, effect.GetComponent<ParticleSystem>().main.duration);
                LeveledUp?.Invoke(ownerSpirit);
            }
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
                {
                    LevelUp();
                }
            }

            if (ownerSpirit.GetOwnerOfType<PlayerSystem>().PlayerInputSystem.ChoosedSpirit == ownerSpirit)
            {
                StatsChanged?.Invoke();
            }
        }
    }
}