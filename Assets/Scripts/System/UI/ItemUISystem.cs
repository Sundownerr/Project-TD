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

    public class ItemUISystem : DescriptionBlock, IPointerClickHandler
    {
        public PlayerSystem Owner { get; private set; }
        public event Action<ItemUISystem> DoubleClickedInPlayerInventory;
        public event Action<ItemUISystem> DoubleClickedInSpiritInventory;

        public int SlotNumber { get; set; }
        public ItemSystem System { get; set; }
        public DraggedFrom DraggedFrom { get; set; }

        public void SetSystem(IEntitySystem owner)
        {
            Owner = owner as PlayerSystem;
            Description = System.Data.Description;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 2)
            {
                if (DraggedFrom == DraggedFrom.PlayerInventory)
                {
                    DoubleClickedInPlayerInventory?.Invoke(this);
                }
                else
                {
                    DoubleClickedInSpiritInventory?.Invoke(this);
                }
            }
        }
    }
}