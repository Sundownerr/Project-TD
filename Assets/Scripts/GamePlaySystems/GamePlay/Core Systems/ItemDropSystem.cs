﻿using Game.Enemy;
using Game.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Systems
{
    public class ItemDropSystem
    {
        public event EventHandler<ItemSystem> ItemCreated;
        public event EventHandler<ItemUISystem> ItemUICreated;

        public PlayerSystem Owner { get; set; }

        // 0 - Item dropped
        // 1 - Item not dropped
        double[] itemDropProbabilities = new double[] { 40, 50 };

        // 0 - Common
        // 1 - Uncommon
        // 2 - Rare
        // 3 - Unique
        double[] itemRarityProbabilities = new double[] { 50, 20, 10, 5 };

        public ItemDropSystem(PlayerSystem player) => Owner = player;

        public void SetSystem()
        {
            Owner.EnemyControlSystem.EnemyDied += OnEnemyDied;
        }

        void OnEnemyDied(object _, EnemySystem enemy)
        {
            var isItemDropped = itemDropProbabilities.RollDice() == 0;

            if (isItemDropped)
            {
                var newItem = CreateItem(itemRarityProbabilities.RollDice());
                Owner.ItemsCount++;

                ItemCreated?.Invoke(null, newItem);
                // Debug.Log($"{newItem.Data.Name} {newItem.Data.Rarity}");
            }

            #region Helper functions

            ItemSystem CreateItem(int rarityId)
            {
                var ItemsFromDB = ReferenceHolder.Get.ItemDB.Data;
                var fittingItems = new List<ItemSystem>(ItemsFromDB.Count);
                var fittingItemsWeigths = new List<double>(ItemsFromDB.Count);

                ItemsFromDB.ForEach(item =>
                {
                    if (item.WaveLevel <= Owner.WaveSystem.WaveNumber)
                    {
                        var newItem = new ItemSystem(item, Owner);
                        fittingItems.Add(newItem);
                        fittingItemsWeigths.Add(item.Weigth);
                    }
                });

                return fittingItems[fittingItemsWeigths.RollDice()];
            }

            #endregion
        }

        public ItemUISystem CreateItemUI(Item item, int slotNumber, Transform slotTransform, IEntitySystem owner)
        {
            var itemUIGO = UnityEngine.Object.Instantiate(
                ReferenceHolder.Get.ItemPrefab,
                slotTransform.position,
                Quaternion.identity,
                slotTransform.parent);

            var itemUI = itemUIGO.GetComponent<ItemUISystem>();
            itemUI.System = new ItemSystem(item, owner);
            itemUI.SetSystem(owner);
            itemUIGO.GetComponent<Image>().sprite = item.Image;
            itemUI.DraggedFrom = owner is PlayerSystem ? DraggedFrom.PlayerInventory : DraggedFrom.SpiritInventory;
            itemUI.SlotNumber = slotNumber;

            ItemUICreated?.Invoke(null, itemUI);
            return itemUI;
        }
    }
}
