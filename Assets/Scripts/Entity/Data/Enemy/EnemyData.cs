﻿using UnityEngine;
using Game.Enemy.Data;
using System.Collections.Generic;
using Game.Data;
using NaughtyAttributes;
using Game.Systems;
using System.Text;
using System;
using Game.Enums;
using Game.Wrappers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Enemy
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Data/Enemy New")]

    [Serializable]
    public class EnemyData : Entity, IEnemyAttributes, IAbilityComponent, ITraitComponent, IPrefabComponent
    {
        [SerializeField, ShowAssetPreview()] private GameObject prefab;
        [SerializeField] public int WaveLevel;
        [SerializeField] public RaceType Race;
        [SerializeField] public EnemyType Type;
        [SerializeField] public int numberInList;

        public GameObject Prefab { get => prefab; set => prefab = value; }
        public List<Trait> Traits { get; set; }
        public List<Ability> Abilities { get; set; }
        public List<NumeralAttribute> BaseAttributes { get; set; }
        public List<NumeralAttribute> AppliedAttributes { get; set; }
        public ArmorType ArmorType { get; set; }

        public List<NumeralAttribute> NumeralAttributes { get ; private set ; }
        public List<EnemyAttribute> EnemyAttributes { get ; private set; }

        private void Awake()
        {
            ID.Add((int)Race);
            ID.Add(numberInList);
        }

        public void CreateNewAttributes()
        {
            NumeralAttributes = NumeralAttributes.CreateAttributeList();
            EnemyAttributes = EnemyAttributes.CreateAttributeList();
        }

#if UNITY_EDITOR

        [Button]
        public void AddToDataBase()
        {
            if (DataControlSystem.Load<EnemyDataBase>() is EnemyDataBase dataBase)
            {
                var thisEnemyRace = dataBase.Races[(int)Race];

                if (!thisEnemyRace.Enemies.Contains(this))
                {
                    numberInList = thisEnemyRace.Enemies.Count;

                    ID = new ID() { (int)Race, numberInList };
                    SetName();

                    thisEnemyRace.Enemies.Add(this);
                    EditorUtility.SetDirty(this);
                    DataControlSystem.Save(dataBase);
                }
            }
        }

        private void RemoveFromDataBase()
        {
            if (DataControlSystem.Load<EnemyDataBase>() is EnemyDataBase dataBase)
            {
                dataBase.Races[(int)Race].Enemies.RemoveAt(numberInList);
                DataControlSystem.Save(dataBase);
            }
        }

        private void SetName()
        {
            var tempName = new StringBuilder();

            tempName.Append(Race.ToString());
            tempName.Append((int)Race);
            tempName.Append(numberInList);

            Name = tempName.ToString();
        }

#endif 
    }
}
