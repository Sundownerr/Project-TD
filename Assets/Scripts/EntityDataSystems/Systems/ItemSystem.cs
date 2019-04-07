using Game.Data;
using Game.Enums;
using Game.Spirit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using U = UnityEngine.Object;

namespace Game.Systems
{
    public class ItemSystem : IEntitySystem
    {
        public Item Data { get; set; }
        public IEntitySystem Owner { get ; set ; }
        public ID ID { get ; set ; }

        public event EventHandler<double> ConsumedMagicCrystals = delegate { };
        public event EventHandler<double> ConsumedSpiritVessels = delegate { };
        public event EventHandler StatsApplied = delegate { };

        Item defaultData;
        bool isStatsApplied;

        public ItemSystem(Item data, IEntitySystem owner)
        {
            Owner = owner;
            Data = U.Instantiate(data);          
            defaultData = data;

            ID = new ID(data.ID);
            ID.Add((owner as PlayerSystem).ItemsCount);
        }

        public void OnSpiritLevelUp(object _, SpiritSystem e) =>
            IncreaseStatsPerLevel();

        public void OnConsumableApplied(object _, ConsumableEventArgs e)
        {
            if (e.ItemUI.System == this)
                ApplyStats();
        }

        public void ApplyStats()
        {
            if (!isStatsApplied)
            {
                AddValues(Owner);
                isStatsApplied = true;
                StatsApplied?.Invoke(null, null);
            }
        }

        void AddValues(IEntitySystem entity)
        {
            if (entity is SpiritSystem spirit)
            {
                for (int i = 0; i < spirit.Data.Get(Numeral.Level).Value; i++)
                    IncreaseStatsPerLevel();

                for (int i = 0; i < Data.NumeralAttributes.Count; i++)
                    spirit.Data.Get(Data.NumeralAttributes[i].Type).AppliedValue += GetValue(Data.NumeralAttributes[i], spirit);

                return;
            }

            if (entity is PlayerSystem player)
            {
                if (Data is Consumable item)
                    if (item.Type == ConsumableType.MagicCrystals)
                        ConsumedMagicCrystals?.Invoke(null, item.NumeralAttributes[0].Value);
                    else
                        if (item.Type == ConsumableType.SpiritVessel)
                        ConsumedSpiritVessels?.Invoke(null, item.NumeralAttributes[0].Value);
            }
        }

        public void RemoveStats()
        {
            if (isStatsApplied)
                if (Owner is SpiritSystem spirit)
                {
                    isStatsApplied = false;
                    for (int i = 0; i < Data.NumeralAttributes.Count; i++)
                        spirit.Data.Get(Data.NumeralAttributes[i].Type).AppliedValue -= GetValue(Data.NumeralAttributes[i], spirit);

                    Data = U.Instantiate(defaultData);
                    U.Destroy(Data);
                }
        }

        void IncreaseStatsPerLevel()
        {
            if (Owner is SpiritSystem spirit)
                for (int i = 0; i < Data.NumeralAttributes.Count; i++)
                {
                    var valuePerLevel = GetValuePerLevel(Data.NumeralAttributes[i]);

                    if (isStatsApplied)
                        spirit.Data.Get(Data.NumeralAttributes[i].Type).AppliedValue += valuePerLevel;

                    Data.NumeralAttributes[i].Value += valuePerLevel;
                }

            double GetValuePerLevel(NumeralAttribute attribute) =>
                attribute.IncreasePerLevel == Increase.ByPercent ?
                    attribute.Value.GetPercent(attribute.ValuePerLevel) :
                    attribute.ValuePerLevel;
        }

        double GetValue(NumeralAttribute attribute, IEntitySystem entity) =>
            attribute.IncreasePerLevel == Increase.ByPercent && entity is SpiritSystem spirit ?
                spirit.Data.Get(attribute.Type).Value.GetPercent(attribute.Value) :
                attribute.Value;
    }
}