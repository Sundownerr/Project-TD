
using System;
using Game.Enemy;
using Game.Data;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
    public class ResourceSystem
    {
        public event EventHandler ResourcesChanged = delegate { };

        private PlayerSystem Owner;

        public ResourceSystem(PlayerSystem player)
        {
            Owner = player;
        }

        private enum ResourceType
        {
            Gold,
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

        private void OnItemUICreated(object _, ItemUISystem e)
        {
            e.System.ConsumedMagicCrystals += OnMagicCrystalsConsumed;
            e.System.ConsumedSpiritVessels += OnSpiritVesselsConsumed;
        }

        private void OnSpiritSold(object _, SpiritSystem spirit)
        {
            AddResource(ResourceType.Gold, spirit.Data.Get(Numeral.GoldCost, From.Base).Value);
            AddResource(ResourceType.SpiritLimit, -spirit.Data.Get(Numeral.SpiritLimit, From.Base).Value);
        }

        private void OnSpiritCreated(object _, SpiritSystem spirit)
        {
            AddResource(ResourceType.Gold, -spirit.Data.Get(Numeral.GoldCost, From.Base).Value);
            AddResource(ResourceType.SpiritLimit, spirit.Data.Get(Numeral.SpiritLimit, From.Base).Value);
        }

        private void OnEnemyDied(object _, EnemySystem enemy)
        {
            if (enemy.LastDamageDealer != null)
                AddResource(ResourceType.Gold, enemy.Data.GetValue(Numeral.GoldCost));
        }

        private void OnElementLearned(object _, int learnCost) => AddResource(ResourceType.MagicCrystal, -learnCost);
        private void OnAllEnemiesKilled(object _, EventArgs e)
        {
            AddResource(ResourceType.MagicCrystal, 5f);
            AddResource(ResourceType.Gold, Owner.WaveSystem.WaveNumber * 10 + Owner.Data.Resources.Gold.GetPercent(10));
        }

        private void OnMagicCrystalsConsumed(object _, double value) => AddResource(ResourceType.MagicCrystal, value);
        private void OnSpiritVesselsConsumed(object _, double value) => AddResource(ResourceType.MaxSpiritLimit, value);

        private void AddResource(ResourceType type, double amount)
        {
            if (type == ResourceType.Gold)
                Owner.Data.Resources.Gold += amount;

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
            goldCost <= Owner.Data.Resources.Gold &&
            magicCrystalCost <= Owner.Data.Resources.MagicCrystals;
    }
}
