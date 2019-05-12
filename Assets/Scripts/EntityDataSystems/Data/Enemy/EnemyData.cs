using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using Game.Systems;
using System.Text;
using System;
using Game.Enums;
using Game.Data.Databases;
using Game.Data.Traits;
using Game.Data.Attributes;
using Game.Data.Enemy.Internal;
using Game.Data.Abilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Data.Enemy
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Data/Enemy New")]

    [Serializable]
    public class EnemyData : Entity, IEnemyAttributes, IAbilityComponent, ITraitComponent, IPrefabComponent
    {
        [SerializeField, ShowAssetPreview()] GameObject prefab;
        [SerializeField] public int WaveLevel;
        [SerializeField] public RaceType Race;
        [SerializeField] public EnemyType Type;

        public GameObject Prefab { get => prefab; set => prefab = value; }
        public List<Trait> Traits { get; set; }
        public List<Ability> Abilities { get; set; }
        public ArmorType ArmorType { get; set; }

        public List<NumeralAttribute> NumeralAttributes { get; private set; }
        public List<EnemyAttribute> EnemyAttributes { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            ID.Add((int)Race);         
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
            if (DataControlSystem.LoadDatabase<EnemyDataBase>() is EnemyDataBase dataBase)
            {
                var thisEnemyRace = dataBase.Data[(int)Race];

                if (!thisEnemyRace.Enemies.Contains(this))
                {
                    index = thisEnemyRace.Enemies.Count;

                    ID = new ID() { index, (int)Race };
                    SetName();

                    thisEnemyRace.Enemies.Add(this);
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
                Debug.LogError($"{typeof(EnemyDataBase)} not found");
            }
        }

        void SetName()
        {
            var tempName = new StringBuilder();

            tempName.Append(Race.ToString());
            tempName.Append((int)Race);
            tempName.Append(index);

            Name = tempName.ToString();
        }

#endif 
    }
}
