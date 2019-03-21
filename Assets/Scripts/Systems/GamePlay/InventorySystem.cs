using Game.Systems;
using Game.Spirit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    public class InventorySystem
    {
        public PlayerSystem Owner { get; set; }

        public InventorySystem(PlayerSystem player)
        {
            Owner = player;
        }

        public void SetSystem()
        {
            Owner.SpiritUISystem.ItemAddedToSpirit     += OnItemAddedToSpirit;
            Owner.SpiritUISystem.ItemRemovedFromSpirit += OnItemRemovedFromSpirit;
            Owner.InventoryUISystem.ItemRemovedFromPlayer += OnItemRemovedFromPlayer;
            Owner.InventoryUISystem.ItemAddedToPlayer += OnItemAddedToPlayer;
            Owner.InventoryUISystem.ApplyConsumable += OnConsumbleApplied;
        }

        private void OnConsumbleApplied(object sender, ConsumableEventArgs e)
        {
            if (e.Entity is SpiritSystem spirit)
            {
                e.ItemUI.System.OwnerSystem = spirit;
                spirit.DataSystem.LeveledUp += e.ItemUI.System.OnSpiritLevelUp;
            }

            if (e.Entity is PlayerSystem player)
                e.ItemUI.System.OwnerSystem = player;
        }

        private void OnItemAddedToSpirit(object sender, SpiritItemEventArgs e)
        {          
            e.ItemUI.System.OwnerSystem = e.Spirit;
            AddItem(e.Spirit.Data.Inventory, e.ItemUI.System);
            e.ItemUI.System.ApplyStats();           
        }

        private void OnItemRemovedFromSpirit(object sender, SpiritItemEventArgs e)
        {          
            RemoveItem(e.Spirit.Data.Inventory, e.ItemUI.System);            
            e.ItemUI.System.RemoveStats();           
        }

        private void OnItemAddedToPlayer(object sender, ItemUISystem itemUI)
        {          
            itemUI.System.OwnerSystem = Owner;
            AddItem(Owner.Data.Inventory, itemUI.System);
        }

        public void OnItemRemovedFromPlayer(object sender, ItemUISystem itemUI)
        {
            RemoveItem(Owner.Data.Inventory, itemUI.System);
        }

        private void AddItem(Inventory inventory, ItemSystem item)
        {
            if (inventory.Items.Count < inventory.MaxSlotCount)
            {
                inventory.Items.Add(item);

                if (item.OwnerSystem is SpiritSystem spirit)               
                    spirit.DataSystem.LeveledUp += item.OnSpiritLevelUp;              
            }
        }

        private void RemoveItem(Inventory inventory, ItemSystem item)
        {
            inventory.Items.Remove(item);

            if (item.OwnerSystem is SpiritSystem spirit)
                spirit.DataSystem.LeveledUp -= item.OnSpiritLevelUp;
        }
    }
}
