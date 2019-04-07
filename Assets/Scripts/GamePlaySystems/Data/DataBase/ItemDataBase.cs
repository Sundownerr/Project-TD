using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "ItemDB", menuName = "Data/Data Base/Item DataBase")]

    [Serializable]
    public class ItemDataBase : ScriptableObject
    {
        public Item[] Items;

        void Awake()
        {
            if (Items == null)
                Items = new Item[1];
            else
                UpdateItemId();
        }

        void OnValidate() => UpdateItemId();

        void UpdateItemId()
        {
            for (int i = 0; i < Items.Length; i++)
                if (Items[i].ID.Count == 0)              
                    Items[i].ID = new ID() { i };              
        }
    }
}