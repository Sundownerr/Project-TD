using System.Collections;
using System.Collections.Generic;
using Game.Data.Abilities;
using Game.Data.Databases;
using Game.Systems;
using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
using NaughtyAttributes;
#endif

namespace Game.Data.Items
{
    [CreateAssetMenu(fileName = "NonConsumable", menuName = "Data/Item/NonConsumable")]

    public class NonConsumable : Item
    {
        public List<Ability> Abilities;

#if UNITY_EDITOR
        [Button("Add to DataBase")]
        public void AddToDataBase()
        {
            if (DataControlSystem.LoadDatabase<ItemDataBase>() is ItemDataBase dataBase)
            {
                var isInDataBase = dataBase.Data.Find(element => element.Compare(this));

                if (isInDataBase == null)
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
