using Game.Enemy;
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
        public event EventHandler<ItemSystem> ItemCreated = delegate { };
        public event EventHandler<ItemUISystem> ItemUICreated = delegate { };

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
                var allItems = ReferenceHolder.Get.ItemDataBase.Items;
                var fittingItems = new ItemSystem[allItems.Length];
                var fittingItemsWeigths = new double[allItems.Length];

                for (int i = 0; i < allItems.Length; i++)
                    if (allItems[i].WaveLevel <= Owner.WaveSystem.WaveNumber)
                    {
                        var newItem = new ItemSystem(allItems[i], Owner);
                        fittingItems[i] = newItem;
                        fittingItemsWeigths[i] = allItems[i].Weigth;
                    }

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

            itemUI.SetSystem(owner);
            itemUIGO.GetComponent<Image>().sprite = item.Image;
            itemUI.DraggedFrom = owner is PlayerSystem ? DraggedFrom.PlayerInventory : DraggedFrom.SpiritInventory;
            itemUI.SlotNumber = slotNumber;

            itemUI.System = new ItemSystem(item, owner);

            ItemUICreated?.Invoke(null, itemUI);
            return itemUI;
        }
    }
}
