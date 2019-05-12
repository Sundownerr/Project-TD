using System.Collections.Generic;
using UnityEngine;
using Game.Systems;
using System;
using Game.Data.Databases;
using Game.Data.Effects;

#if UNITY_EDITOR
using NaughtyAttributes;
using UnityEditor;
#endif

namespace Game.Data.Abilities
{
    [CreateAssetMenu(fileName = "Neww Ability", menuName = "Data/Ability")]

    [Serializable]
    public class Ability : Entity
    {
        public float Cooldown, TriggerChance;
        public int ManaCost;

        [Expandable]
        public List<Effect> Effects;
        

#if UNITY_EDITOR
        [Button("Add to DataBase")]
        public void AddToDataBase()
        {
            if (DataControlSystem.LoadDatabase<AbilityDatabase>() is AbilityDatabase dataBase)
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
                Debug.LogError($"{typeof(AbilityDatabase)} not found");
            }
        }
#endif
    }
}