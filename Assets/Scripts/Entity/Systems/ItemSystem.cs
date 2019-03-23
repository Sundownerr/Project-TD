using Game.Data;
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
        public IEntitySystem OwnerSystem { get ; set ; }
        public ID ID { get ; set ; }

        public event EventHandler<double> ConsumedMagicCrystals = delegate { };
        public event EventHandler<double> ConsumedSpiritVessels = delegate { };
        public event EventHandler StatsApplied = delegate { };

        private Item defaultData;
        private bool isStatsApplied;

        public ItemSystem(Item data, IEntitySystem owner)
        {
            OwnerSystem = owner;
            Data = U.Instantiate(data);          
            defaultData = data;

            ID = new ID();
            ID.AddRange(data.ID);
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
                AddValues(OwnerSystem);
                isStatsApplied = true;
                StatsApplied?.Invoke(null, null);
            }
        }

        private void AddValues(IEntitySystem entity)
        {
            if (entity is SpiritSystem spirit)
            {
                for (int i = 0; i < spirit.Data.Get(Numeral.Level, From.Base).Value; i++)
                    IncreaseStatsPerLevel();

                for (int i = 0; i < Data.Attributes.Count; i++)
                    spirit.Data.Get(Data.Attributes[i].Type, From.Applied).Value += GetValue(Data.Attributes[i], spirit);

                return;
            }

            if (entity is PlayerSystem player)
            {
                if (Data is Consumable item)
                    if (item.Type == ConsumableType.MagicCrystals)
                        ConsumedMagicCrystals?.Invoke(null, item.Attributes[0].Value);
                    else
                        if (item.Type == ConsumableType.SpiritVessel)
                        ConsumedSpiritVessels?.Invoke(null, item.Attributes[0].Value);
            }
        }

        public void RemoveStats()
        {
            if (isStatsApplied)
                if (OwnerSystem is SpiritSystem spirit)
                {
                    isStatsApplied = false;
                    for (int i = 0; i < Data.Attributes.Count; i++)
                        spirit.Data.Get(Data.Attributes[i].Type, From.Applied).Value -= GetValue(Data.Attributes[i], spirit);

                    Data = U.Instantiate(defaultData);
                    U.Destroy(Data);
                }
        }

        private void IncreaseStatsPerLevel()
        {
            if (OwnerSystem is SpiritSystem spirit)
                for (int i = 0; i < Data.Attributes.Count; i++)
                {
                    var valuePerLevel = GetValuePerLevel(Data.Attributes[i]);

                    if (isStatsApplied)
                        spirit.Data.Get(Data.Attributes[i].Type, From.Applied).Value += valuePerLevel;

                    Data.Attributes[i].Value += valuePerLevel;
                }

            double GetValuePerLevel(NumeralAttribute attribute) =>
                attribute.IncreasePerLevel.ChangeType == Change.ByPercent ?
                    attribute.Value.GetPercent(attribute.IncreasePerLevel.Value) :
                    attribute.IncreasePerLevel.Value;
        }

        private double GetValue(NumeralAttribute attribute, IEntitySystem entity) =>
            attribute.ChangeType == Change.ByPercent && entity is SpiritSystem spirit ?
                spirit.Data.Get(attribute.Type, From.Base).Value.GetPercent(attribute.Value) :
                attribute.Value;
    }
}