using Game.Data;
using Game.Spirit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Systems
{
   

    public enum DraggedFrom
    {
        PlayerInventory = 0,
        SpiritInventory = 1
    }

    public class ItemUISystem : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler, IPointerClickHandler, IPointerEnterHandler, IHaveDescription, IPointerExitHandler
    {
        public PlayerSystem Owner { get; set; }
        public event EventHandler<ItemDragEventArgs> BeingDragged = delegate { };
        public event EventHandler<ItemDragEventArgs> DragEnd = delegate { };
        public event EventHandler<ItemUISystem> DoubleClickedInPlayerInventory = delegate { };
        public event EventHandler<ItemUISystem> DoubleClickedInSpiritInventory = delegate { };

        public int SlotNumber { get; set; }
        public ItemSystem System { get; set; }
        public DraggedFrom DraggedFrom { get; set; }

        private Vector3 startPos;
        private RectTransform rectTransform;
        private Transform parent;
        private GameObject overlappedSlot;
        private List<GameObject> inventorySlots;
        private List<SlotWithCooldown> spiritSlots;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void SetSystem(IEntitySystem owner)
        {
            Owner = owner as PlayerSystem;
            spiritSlots = Owner.SpiritUISystem.ItemSlots;
            inventorySlots = Owner.InventoryUISystem.Slots;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {          
            startPos = transform.position;
            parent = transform.parent;

            BeingDragged?.Invoke(null, new ItemDragEventArgs(this));
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

            DragEnd?.Invoke(null, new ItemDragEventArgs(this, overlappedSlot));

            overlappedSlot = null;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
                if (DraggedFrom == DraggedFrom.PlayerInventory)
                    DoubleClickedInPlayerInventory?.Invoke(null, this);
                else
                    DoubleClickedInSpiritInventory?.Invoke(null, this);
        }

        public void GetDescription()
        {
            Owner.DescriptionUISystem.ShowDescription(System.Data.Description);            
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GetDescription();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Owner.DescriptionUISystem.CloseDescription();
        }
    }
}