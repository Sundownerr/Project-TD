using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Systems;
using Game.Utility;

namespace Game.UI
{
    public enum DraggedFrom
    {
        PlayerInventory = 0,
        SpiritInventory = 1
    }

    public class ItemUISystem : DescriptionBlock, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerClickHandler
    {
        public PlayerSystem Owner { get; set; }
        public event Action<ItemDragEventArgs> BeingDragged;
        public event Action<ItemDragEventArgs> DragEnd;
        public event Action<ItemUISystem> DoubleClickedInPlayerInventory;
        public event Action<ItemUISystem> DoubleClickedInSpiritInventory;

        public int SlotNumber { get; set; }
        public ItemSystem System { get; set; }
        public DraggedFrom DraggedFrom { get; set; }
        Vector3 startPos;
        RectTransform rectTransform;
        Transform parent;
        GameObject overlappedSlot;
        List<GameObject> inventorySlots;
        List<SlotWithCooldown> spiritSlots;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            
        }

        public void SetSystem(IEntitySystem owner)
        {
            Owner = owner as PlayerSystem;
            spiritSlots = Owner.SpiritUISystem.ItemSlots;
            inventorySlots = Owner.InventoryUISystem.Slots;
            Description = System.Data.Description;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPos = transform.position;
            parent = transform.parent;

            BeingDragged?.Invoke(new ItemDragEventArgs(this));
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = Input.mousePosition;
            transform.SetParent(Owner.InventoryUISystem.transform);
            overlappedSlot = null;


            for (int i = 0; i < spiritSlots.Count; i++)
                if (transform.position.GetDistanceTo(spiritSlots[i].transform.position) < 14)
                    overlappedSlot = spiritSlots[i].gameObject;

            for (int i = 0; i < inventorySlots.Count; i++)
                if (transform.position.GetDistanceTo(inventorySlots[i].transform.position) < 14)
                    overlappedSlot = inventorySlots[i].gameObject;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (overlappedSlot != null && overlappedSlot.activeSelf)
            {
                transform.position = overlappedSlot.transform.position;
                transform.SetParent(overlappedSlot.transform.parent);
            }
            else
            {
                transform.position = startPos;
                transform.SetParent(parent);
            }

            DragEnd?.Invoke(new ItemDragEventArgs(this, overlappedSlot));

            overlappedSlot = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
                if (DraggedFrom == DraggedFrom.PlayerInventory)
                    DoubleClickedInPlayerInventory?.Invoke(this);
                else
                    DoubleClickedInSpiritInventory?.Invoke(this);
        }
    }
}