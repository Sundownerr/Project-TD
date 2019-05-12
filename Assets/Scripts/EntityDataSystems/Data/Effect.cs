using UnityEngine;
using Game.Systems;
using System;
using Game.Data.Databases;
using Game.Systems.Effects;

#if UNITY_EDITOR
using UnityEditor;
using NaughtyAttributes;
#endif

namespace Game.Data.Effects
{
    [Serializable]
    public abstract class Effect : Entity
    {
        public float Duration;
        public float NextInterval;

#if UNITY_EDITOR
        [MinValue(1), MaxValue(1000)]
#endif
        public int MaxStackCount;

        public abstract EffectSystem EffectSystem { get; }

#if UNITY_EDITOR
        [Button("Add to DataBase")]
        public void AddToDataBase()
        {
            if (DataControlSystem.LoadDatabase<EffectDataBase>() is EffectDataBase dataBase)
            {
                var isEffectInDataBase = dataBase.Data.Find(effect => effect.Compare(this));

                if (!isEffectInDataBase)
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
                Debug.LogError($"{typeof(EffectDataBase)} not found");
            }
        }  
#endif 
    }
}
