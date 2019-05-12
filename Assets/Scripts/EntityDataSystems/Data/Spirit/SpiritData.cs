using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Game.Systems;
using OneLine;
using Game.Enums;
using Game.Data.Databases;
using Game.Data.Attributes;
using Game.Data.Traits;
using Game.Data.Spirit.Internal;
using Game.Data.Abilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Data.Spirit
{

    [CreateAssetMenu(fileName = "New Spirit", menuName = "Data/Spirit/Spirit")]
    [Serializable]
    public class SpiritData : Entity, ISpiritAttributes, IAbilityComponent, ITraitComponent, IPrefabComponent
    {
        [Serializable]
        public struct BaseData
        {
            [SerializeField] RarityType rarity;
            [SerializeField] DamageType damageType;
            [SerializeField] ElementType element;

            public RarityType Rarity { get => rarity; set => rarity = value; }
            public DamageType DamageType { get => damageType; set => damageType = value; }
            public ElementType Element { get => element; set => element = value; }
        }

        [SerializeField, ShowAssetPreview()]
        GameObject prefab;

        [SerializeField, OneLineWithHeader, OneLine.HideLabel]
        public BaseData Base;

        [SerializeField, OneLine, OneLine.HideLabel]
        List<NumeralAttribute> numeralAttributes;

        [SerializeField, OneLine, OneLine.HideLabel]
        List<SpiritFlagAttribute> flagAttributes;

        [SerializeField, OneLine, OneLine.HideLabel]
        List<SpiritAttribute> spiritAttributes;

        [SerializeField] List<Ability> AbilityList;
        [SerializeField] List<Trait> TraitList;
        [SerializeField] List<SpiritData> grades;
        [SerializeField] List<float> damageToRace;

        public int GradeCount { get; set; } = -1;
        public Inventory Inventory { get; set; }
        public List<Ability> Abilities { get => AbilityList; set => AbilityList = value; }
        public List<Trait> Traits { get => TraitList; set => TraitList = value; }
        public GameObject Prefab { get => prefab; set => prefab = value; }
        public List<SpiritAttribute> SpiritAttributes { get => spiritAttributes; set => spiritAttributes = value; }
        public List<SpiritFlagAttribute> FlagAttributes { get => flagAttributes; set => flagAttributes = value; }
        public List<NumeralAttribute> NumeralAttributes { get => numeralAttributes; set => numeralAttributes = value; }
        public List<SpiritData> Grades { get => grades; private set => grades = value; }
        public List<float> DamageToRace { get => damageToRace; set => damageToRace = value; }

        public void SetData()
        {
            Grades = new List<SpiritData>();
            Inventory = new Inventory(this.Get(Enums.Spirit.MaxInventorySlots).Value);
        }

        protected void Awake()
        {
            if (numeralAttributes == null || numeralAttributes.Count == 0)
            {
                numeralAttributes = Ext.CreateAttributeList_N();
            }

            if (flagAttributes == null || flagAttributes.Count == 0)
            {
                flagAttributes = Ext.CreateAttributeList_SF();
            }

            if (spiritAttributes == null || spiritAttributes.Count == 0)
            {
                spiritAttributes = Ext.CreateAttributeList_S();
            }

            if (DamageToRace == null || DamageToRace.Count == 0)
            {
                var races = Enum.GetValues(typeof(RaceType));

                DamageToRace = new List<float>();

                for (int i = 0; i < races.Length; i++)
                {
                    DamageToRace.Add(100f);
                }
            }
        }

#if UNITY_EDITOR
        [Button("Add to DataBase")]
        void AddToDataBase()
        {
            if (!this.Get(Enums.SpiritFlag.IsGradeSpirit).Value)
            {
                if (DataControlSystem.LoadDatabase<SpiritDataBase>() is SpiritDataBase dataBase)
                {
                    var thisElementAndRarityList = dataBase.Data;

                    if (thisElementAndRarityList.Find(element => element.Index == Index) == null)
                    {
                        Index = thisElementAndRarityList.Count;

                        thisElementAndRarityList.Add(this);
                        EditorUtility.SetDirty(this);
                        DataControlSystem.Save(dataBase);
                    }
                    else
                    {
                        Debug.LogWarning($"{this} already in data base");
                    }
                }
            }
            else
            {
                Debug.LogError($"{typeof(SpiritDataBase)} not found");
            }
        }
#endif 
    }
}