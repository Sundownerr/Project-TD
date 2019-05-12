using System;
using System.Collections;
using System.Collections.Generic;
using Game.Data.Databases;
using Game.Enums;
using Game.Systems;
using UnityEditor;

#if UNITY_EDITOR
using NaughtyAttributes;
using UnityEngine;
#endif

namespace Game.Data.Items
{
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "Consumable", menuName = "Data/Item/Consumable")]
#endif

    [Serializable]
    public class Consumable : Item
    {
        public ConsumableType Type;

#if UNITY_EDITOR
        [Button("Add to DataBase")]
        public void AddToDataBase()
        {
            if (DataControlSystem.LoadDatabase<ItemDataBase>() is ItemDataBase dataBase)
            {
                if (dataBase.Data.Find(entity => entity.Index == Index) == null)
                {
                    Index = dataBase.Data.Count;

                    dataBase.Data.Add(this);
                    EditorUtility.SetDirty(this);
                    DataControlSystem.Save(dataBase);
                }
                else
                {
                    Debug.LogWarning($"{this} already in data base");
                }
            }
            else
            {
                Debug.LogError($"{typeof(ItemDataBase)} not found");
            }
        }
#endif
    }
}
