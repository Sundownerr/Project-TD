using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "NonConsumable", menuName = "Data/Item/NonConsumable")]

    public class NonConsumable : Item
    {
        public List<Ability> Abilities;   
    }
}
