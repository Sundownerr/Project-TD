using Game.Data.Items;
using Game.Enums;
using Game.Systems;
using Game.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class InventoryUISystem : MonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public event Action<ItemUISystem> MoveItemToSpirit;
        public event Action<ItemUISystem> ItemAddedToPlayer;
        public event Action<ItemUISystem> ItemRemovedFromPlayer;
        public event Action<ConsumableEventArgs> ApplyConsumable;
        public Button InventoryButton;
        public GameObject BGPanel;
        public List<GameObject> Slots;
        public List<ItemUISystem> ItemsUI;

        List<bool> isSlotEmpty = new List<bool>();

        void Awake()
        {
            InventoryButton.onClick.AddListener(ShowButton);

            for (int i = 0; i < Slots.Count; i++)
                isSlotEmpty.Add(true);

            #region Helper functions

            void ShowButton() =>
               BGPanel.gameObject.SetActive(!BGPanel.gameObject.activeSelf);

            #endregion
        }

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;
            Owner.ItemDropSystem.ItemCreated += OnItemCreated;
            Owner.ItemDropSystem.ItemUICreated += OnItemUICreated;
            Owner.SpiritUISystem.MoveItemToPlayer += OnItemMovedToPlayer;
        }

        void OnItemUICreated(ItemUISystem itemUI)
        {
            itemUI.BeingDragged += OnItemBeingDragged;
            itemUI.DragEnd += OnItemDragEnd;
            itemUI.DoubleClickedInPlayerInventory += OnItemDoubleClicked;
            ApplyConsumable += itemUI.System.OnConsumableApplied;
        }

        void OnItemMovedToPlayer(ItemUISystem itemUI)
        {
            var freeSlotIndex = isSlotEmpty.IndexOf(true);
            if (freeSlotIndex == -1) return;

            var freeSlot = Slots[freeSlotIndex];

            itemUI.transform.position = freeSlot.transform.position;
            itemUI.transform.SetParent(freeSlot.transform.parent);

            AddItemToPlayer(itemUI, freeSlotIndex);
        }

        void OnItemCreated(ItemSystem item)
        {
            var freeSlotIndex = isSlotEmpty.IndexOf(true);
            if (freeSlotIndex == -1) return;

            ItemsUI.Add(
                Owner.ItemDropSystem.CreateItemUI(item.Data, freeSlotIndex, Slots[freeSlotIndex].transform, Owner));

            isSlotEmpty[freeSlotIndex] = false;
            Slots[freeSlotIndex].SetActive(false);

            ItemAddedToPlayer?.Invoke(ItemsUI[ItemsUI.Count - 1]);
            return;

        }

        public void OnItemBeingDragged(ItemDragEventArgs e) => RemoveItemFromPlayer(e.ItemUI);

        public void OnItemDoubleClicked(ItemUISystem itemUI)
        {
            var isSpiritChoosed = Owner.PlayerInputSystem.ChoosedSpirit != null;

            if (itemUI.System.Data is Consumable item)
                if (item.Type != ConsumableType.Essence)
                {
                    ApplyConsumable?.Invoke(new ConsumableEventArgs(Owner, itemUI));
                    RemoveItemFromPlayer(itemUI);
                    Destroy(itemUI.gameObject);
                    return;
                }
                else
                    if (isSpiritChoosed && item.Type == ConsumableType.Essence)
                {
                    ApplyConsumable?.Invoke(new ConsumableEventArgs(Owner.PlayerInputSystem.ChoosedSpirit, itemUI));
                    RemoveItemFromPlayer(itemUI);
                    Destroy(itemUI.gameObject);
                    return;
                }

            if (isSpiritChoosed)
                if (itemUI.DraggedFrom == DraggedFrom.PlayerInventory)
                {
                    RemoveItemFromPlayer(itemUI);
                    MoveItemToSpirit?.Invoke(itemUI);
                }

        }

        public void OnItemDragEnd(ItemDragEventArgs e)
        {
            if (e.OverlappedSlot == null && e.ItemUI.DraggedFrom == DraggedFrom.PlayerInventory)
                AddItemToPlayer(e.ItemUI, e.ItemUI.SlotNumber);

            for (int i = 0; i < Slots.Count; i++)
                if (e.OverlappedSlot == Slots[i])
                    AddItemToPlayer(e.ItemUI, i);
        }

        void RemoveItemFromPlayer(ItemUISystem itemUI)
        {
            if (itemUI.DraggedFrom == DraggedFrom.PlayerInventory)
            {
                isSlotEmpty[itemUI.SlotNumber] = true;
                Slots[itemUI.SlotNumber].SetActive(true);
                ItemsUI.Remove(itemUI);

                ItemRemovedFromPlayer?.Invoke(itemUI);
            }

        }

        void AddItemToPlayer(ItemUISystem itemUI, int slotNumber)
        {
            ItemsUI.Add(itemUI);
            itemUI.DraggedFrom = DraggedFrom.PlayerInventory;
            itemUI.SlotNumber = slotNumber;
            Slots[slotNumber].SetActive(false);
            isSlotEmpty[slotNumber] = false;

            ItemAddedToPlayer?.Invoke(itemUI);
        }
    }
}
