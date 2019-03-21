using System.Collections;
using System.Collections.Generic;
using Game.Cells;
using Game.Enemy;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
    public class SpiritControlSystem
    {
        public List<SpiritSystem> Spirits { get; private set; }

        private PlayerSystem player;

        public SpiritControlSystem(PlayerSystem player)
        {
            this.player = player;
            Spirits = new List<SpiritSystem>();
        }

        public void SetSystem()
        {
            player.SpiritPlaceSystem.SpiritPlaced += OnSpiritCreated;
            player.PlayerInputSystem.SpiritUpgraded += OnSpiritCreated;
            player.PlayerInputSystem.SpiritSold += OnSpiritRemoved;
        }

        public void UpdateSystem()
        {
            for (int i = 0; i < Spirits.Count; i++)
                Spirits[i].UpdateSystem();
        }

        private void OnSpiritCreated(object sender, SpiritSystem spirit) => AddSpirit(spirit);
        private void OnSpiritRemoved(object sender, SpiritSystem spirit) => RemoveSpirit(spirit);

        private void AddSpirit(SpiritSystem spirit)
        {
            Spirits.Add(spirit);

            spirit.OwnerSystem = player;
            spirit.UsedCell.GetComponent<Cell>().IsBusy = true;
            spirit.Prefab.layer = 14;
            spirit.IsOn = true;
        }

        private void RemoveSpirit(SpiritSystem spirit)
        {
            if (spirit.UsedCell != null)
                spirit.UsedCell.GetComponent<Cell>().IsBusy = false;

            Spirits.Remove(spirit);

            Object.Destroy(spirit.Prefab);
            Object.Destroy(spirit.Data);
        }
    }
}
		
