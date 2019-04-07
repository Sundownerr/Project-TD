
using System;
using Game.Enemy;
using Game.Data;
using Game.Spirit;
using UnityEngine;
using Game.Enums;

namespace Game.Systems
{
    public class ResourceSystem
    {
        public event EventHandler ResourcesChanged = delegate { };

        PlayerSystem Owner;

        public ResourceSystem(PlayerSystem player)
        {
            Owner = player;
        }

        enum ResourceType
        {
            Resource,
            MagicCrystal,
            SpiritLimit,
            MaxSpiritLimit
        }

        public void SetSystem()
        {
            Owner.EnemyControlSystem.EnemyDied += OnEnemyDied;
            Owner.SpiritPlaceSystem.SpiritPlaced += OnSpiritCreated;
            Owner.PlayerInputSystem.SpiritSold += OnSpiritSold;
            Owner.WaveSystem.AllWaveEnemiesKilled += OnAllEnemiesKilled;
            Owner.ElementSystem.LearnedElement += OnElementLearned;
            Owner.ItemDropSystem.ItemUICreated += OnItemUICreated;
        }

        void OnItemUICreated(object _, ItemUISystem e)
        {
            e.System.ConsumedMagicCrystals += OnMagicCrystalsConsumed;
            e.System.ConsumedSpiritVessels += OnSpiritVesselsConsumed;
        }

        void OnSpiritSold(object _, SpiritSystem spirit)
        {
            AddResource(ResourceType.Resource, spirit.Data.Get(Numeral.ResourceCost).Value);
            AddResource(ResourceType.SpiritLimit, -spirit.Data.Get(Enums.Spirit.SpiritLimit).Value);
        }

        void OnSpiritCreated(object _, SpiritSystem spirit)
        {
            AddResource(ResourceType.Resource, -spirit.Data.Get(Numeral.ResourceCost).Value);
            AddResource(ResourceType.SpiritLimit, spirit.Data.Get(Enums.Spirit.SpiritLimit).Value);
        }

        void OnEnemyDied(object _, EnemySystem enemy)
        {
            if (enemy.LastDamageDealer != null)
                AddResource(ResourceType.Resource, enemy.Data.Get(Numeral.ResourceCost).Sum);
        }

        void OnElementLearned(object _, int learnCost) => AddResource(ResourceType.MagicCrystal, -learnCost);
        void OnAllEnemiesKilled(object _, EventArgs e)
        {
            AddResource(ResourceType.MagicCrystal, 5f);
            AddResource(ResourceType.Resource, Owner.WaveSystem.WaveNumber * 10 + Owner.Data.Resources.Resource.GetPercent(10));
        }

        void OnMagicCrystalsConsumed(object _, double value) => AddResource(ResourceType.MagicCrystal, value);
        void OnSpiritVesselsConsumed(object _, double value) => AddResource(ResourceType.MaxSpiritLimit, value);

        void AddResource(ResourceType type, double amount)
        {
            if (type == ResourceType.Resource)
                Owner.Data.Resources.Resource += amount;

            else if (type == ResourceType.MagicCrystal)
                Owner.Data.Resources.MagicCrystals += amount;

            else if (type == ResourceType.SpiritLimit)
                Owner.Data.Resources.CurrentSpiritLimit += amount;

            else if (type == ResourceType.MaxSpiritLimit)
                Owner.Data.Resources.MaxSpiritLimit += amount;

            //DataLoadingSystem.Save(GM.I.Players[0]);
            ResourcesChanged?.Invoke(null, null);
        }

        public bool CheckHaveResources(double spiritLimitCost, double goldCost, double magicCrystalCost) =>
            (Owner.Data.Resources.CurrentSpiritLimit + spiritLimitCost) <= Owner.Data.Resources.MaxSpiritLimit &&
            goldCost <= Owner.Data.Resources.Resource &&
            magicCrystalCost <= Owner.Data.Resources.MagicCrystals;
    }
}
