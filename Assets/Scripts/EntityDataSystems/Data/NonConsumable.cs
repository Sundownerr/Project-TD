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
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "NonConsumable", menuName = "Data/Item/NonConsumable")]
#endif

    public class NonConsumable : Item
    {
        [SerializeField] List<Ability> abilities;

        public List<Ability> Abilities { get => abilities; private set => abilities = value; }

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
