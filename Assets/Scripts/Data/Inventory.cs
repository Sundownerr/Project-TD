using Game.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory 
{
    public List<ItemSystem> Items { get; set; }
    public double MaxSlotCount { get; set; }

    public Inventory(double maxSlotCount)
    {
        Items = new List<ItemSystem>();
        MaxSlotCount = maxSlotCount;
    }
}
