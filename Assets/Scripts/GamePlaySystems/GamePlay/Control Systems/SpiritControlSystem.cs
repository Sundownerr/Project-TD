﻿using System.Collections;
using System.Collections.Generic;
using Game.Cells;
using Game.Enemy;
using Game.Spirit;
using UnityEngine;

namespace Game.Systems
{
    public class SpiritControlSystem
    {
        public List<SpiritSystem> Spirits { get; private set; } = new List<SpiritSystem>();

        PlayerSystem Owner;

        public SpiritControlSystem(PlayerSystem player) => Owner = player;

        public void SetSystem()
        {
            Owner.SpiritPlaceSystem.SpiritPlaced += OnSpiritCreated;
            Owner.PlayerInputSystem.SpiritUpgraded += OnSpiritCreated;
            Owner.PlayerInputSystem.SpiritSold += OnSpiritRemoved;
        }

        public void UpdateSystem()
        {
            for (int i = 0; i < Spirits.Count; i++)
                Spirits[i].UpdateSystem();
        }

        void OnSpiritCreated(object _, SpiritSystem spirit) => AddSpirit(spirit);
        void OnSpiritRemoved(object _, SpiritSystem spirit) => RemoveSpirit(spirit);

        void AddSpirit(SpiritSystem spirit)
        {
            Spirits.Add(spirit);

            if(spirit.UsedCell != null)
                spirit.UsedCell.GetComponent<Cell>().IsBusy = true;

            spirit.IsOn = true;
        }

        void RemoveSpirit(SpiritSystem spirit)
        {
            if (spirit.UsedCell != null)
                spirit.UsedCell.GetComponent<Cell>().IsBusy = false;

            spirit.Dispose();
            Spirits.Remove(spirit);
        }
    }
}
		