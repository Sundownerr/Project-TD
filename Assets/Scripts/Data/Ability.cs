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
        [SerializeField] bool isUsedWhenShoot;
        [SerializeField] float cooldown;
        [SerializeField] float triggerChance;
        [SerializeField] int manaCost;
        [SerializeField, Expandable] List<Effect> effects;

        public int ManaCost { get => manaCost; set => manaCost = value; }
        public float TriggerChance { get => triggerChance; set => triggerChance = value; }
        public float Cooldown { get => cooldown; set => cooldown = value; }
        public List<Effect> Effects { get => effects; set => effects = value; }
        public bool IsUsedWhenShoot { get => isUsedWhenShoot; set => isUsedWhenShoot = value; }

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