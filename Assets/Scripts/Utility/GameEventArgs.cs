using UnityEngine;
using Game.UI;
using Game.Systems.Spirit;
using System;
using Game.Data.SpiritEntity.Internal;

namespace Game.Utility
{

    public class DamageEventArgs : EventArgs
    {
        public IHealthComponent Target { get; set; }
        public double Damage { get; set; }
        public int CritCount { get; set; }

        public DamageEventArgs(IHealthComponent target, (double value, int critCount, DamageType type) damageInstance)
        {
            Target = target;
            Damage = damageInstance.value;
            CritCount = damageInstance.critCount;
        }
    }

    public class ConsumableEventArgs : EventArgs
    {
        public IEntitySystem Entity { get; set; }
        public ItemUISystem ItemUI { get; set; }

        public ConsumableEventArgs(IEntitySystem entity, ItemUISystem itemUI)
        {
            Entity = entity;
            ItemUI = itemUI;
        }
    }

    public class ItemDragEventArgs : EventArgs
    {
        public ItemUISystem ItemUI { get; set; }
        public GameObject OverlappedSlot { get; set; }

        public ItemDragEventArgs(ItemUISystem itemUI)
        {
            ItemUI = itemUI;
        }

        public ItemDragEventArgs(ItemUISystem itemUISystem, GameObject overlappedSlot)
        {
            ItemUI = itemUISystem;
            OverlappedSlot = overlappedSlot;
        }
    }

    public class SpiritItemEventArgs : EventArgs
    {
        public ItemUISystem ItemUI { get; set; }
        public SpiritSystem Spirit { get; set; }

        public SpiritItemEventArgs(SpiritSystem spirit, ItemUISystem item)
        {
            Spirit = spirit;
            ItemUI = item;
        }
    }
}