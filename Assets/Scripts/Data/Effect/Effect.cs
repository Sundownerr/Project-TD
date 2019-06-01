using UnityEngine;
using Game.Systems;
using System;
using Game.Data.Databases;
using Game.Systems.Effects;

#if UNITY_EDITOR
using UnityEditor;
using NaughtyAttributes;
#endif

namespace Game.Data
{
    [Serializable]
    public abstract class Effect : Entity
    {
        [SerializeField] float duration;
        [SerializeField] float nextInterval;

#if UNITY_EDITOR
        [MinValue(1), MaxValue(1000)]
#endif
        [SerializeField] int maxStackCount;

        public abstract Systems.Effect EffectSystem { get; }
        public float NextInterval { get => nextInterval; set => nextInterval = value; }
        public float Duration { get => duration; set => duration = value; }
        public int MaxStackCount { get => maxStackCount; set => maxStackCount = value; }

#if UNITY_EDITOR
        [Button("Add to DataBase")]
        public void AddToDataBase()
        {
            if (DataControlSystem.LoadDatabase<EffectDataBase>() is EffectDataBase dataBase)
            {
                if (dataBase.Data.Find(effect => effect.Index == Index) == null)
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
