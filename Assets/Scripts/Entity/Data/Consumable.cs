using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "Consumable", menuName = "Data/Item/Consumable")]

    [Serializable]
    public class Consumable : Item
    {
        public ConsumableType Type;
    }
}
