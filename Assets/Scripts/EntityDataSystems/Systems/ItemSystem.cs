using Game.Data.Attributes;
using Game.Data.Items;
using Game.Enums;
using Game.Systems.Spirit;
using Game.Utility;
using System;
using U = UnityEngine.Object;

namespace Game.Systems
{
    public class ItemSystem : IEntitySystem, IIndexComponent
    {
        public Item Data { get; set; }
        public IEntitySystem Owner { get; set; }

        public event Action<double> ConsumedMagicCrystals;
        public event Action<double> ConsumedSpiritVessels;
        public event Action StatsApplied;

        public int Index { get; private set; }

        Item defaultData;
        bool isStatsApplied;

        public ItemSystem(Item data, IEntitySystem owner)
        {
            Owner = owner;
            Data = U.Instantiate(data);
            defaultData = data;
            Index = (owner as PlayerSystem).ItemsCount;
        }

        public void OnSpiritLevelUp(SpiritSystem e) =>
            IncreaseStatsPerLevel();

        public void OnConsumableApplied(ConsumableEventArgs e)
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
                StatsApplied?.Invoke();
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
                        ConsumedMagicCrystals?.Invoke(item.NumeralAttributes[0].Value);
                    else
                        if (item.Type == ConsumableType.SpiritVessel)
                        ConsumedSpiritVessels?.Invoke(item.NumeralAttributes[0].Value);
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