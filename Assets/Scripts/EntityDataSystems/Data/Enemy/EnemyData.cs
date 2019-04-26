using UnityEngine;
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
        [SerializeField, ShowAssetPreview()] GameObject prefab;
        [SerializeField] public int WaveLevel;
        [SerializeField] public RaceType Race;
        [SerializeField] public EnemyType Type;
        [SerializeField] public int numberInList;

        public GameObject Prefab { get => prefab; set => prefab = value; }
        public List<Trait> Traits { get; set; }
        public List<Ability> Abilities { get; set; }
        public ArmorType ArmorType { get; set; }

        public List<NumeralAttribute> NumeralAttributes { get; private set; }
        public List<EnemyAttribute> EnemyAttributes { get; private set; }

        void Awake()
        {
            ID.Add((int)Race);
            ID.Add(numberInList);
        }

        public void CreateNewAttributes()
        {
            NumeralAttributes = Ext.CreateAttributeList_N();
            EnemyAttributes = Ext.CreateAttributeList_E();
        }

#if UNITY_EDITOR

        [Button]
        public void AddToDataBase()
        {
            if (DataControlSystem.Load<EnemyDataBase>() is EnemyDataBase dataBase)
            {
                var thisEnemyRace = dataBase.Data[(int)Race];

                if (!thisEnemyRace.Enemies.Contains(this))
                {
                    numberInList = thisEnemyRace.Enemies.Count;

                    ID = new ID() { (int)Race, numberInList };
                    SetName();

                    thisEnemyRace.Enemies.Add(this);
                    EditorUtility.SetDirty(this);
                    DataControlSystem.Save(dataBase);
                }
                else Debug.LogWarning($"{this} already in data base");
            }
            else Debug.LogError($"{typeof(EnemyDataBase)} not found");
        }

        void RemoveFromDataBase()
        {
            if (DataControlSystem.Load<EnemyDataBase>() is EnemyDataBase dataBase)
            {
                dataBase.Data[(int)Race].Enemies.RemoveAt(numberInList);
                DataControlSystem.Save(dataBase);
            }
        }

        void SetName()
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
