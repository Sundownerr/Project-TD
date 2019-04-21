using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Game.Spirit.Data.Stats;
using Game.Data;
using Game.Systems;
using Game.Enemy.Data;
using OneLine;
using Game.Enums;
using Game.Wrappers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Spirit.Data
{

    [CreateAssetMenu(fileName = "New Spirit", menuName = "Data/Spirit/Spirit")]
    [Serializable]
    public class SpiritData : Entity, ISpiritAttributes, IAbilityComponent, ITraitComponent, IPrefabComponent
    {
        [Serializable]
        public struct BaseData
        {
            [SerializeField] public RarityType Rarity;
            [SerializeField] public DamageType DamageType;
            [SerializeField] public ElementType Element;
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
        [SerializeField] public List<SpiritData> Grades;
        [SerializeField] public List<float> DamageToRace;
        [SerializeField, HideInInspector] public int NumberInList;

        public int GradeCount { get; set; } = -1;
        public Inventory Inventory { get; set; }
        public List<Ability> Abilities { get => AbilityList; set => AbilityList = value; }
        public List<Trait> Traits { get => TraitList; set => TraitList = value; }
        public GameObject Prefab { get => prefab; set => prefab = value; }
        public List<SpiritAttribute> SpiritAttributes { get => spiritAttributes; set => spiritAttributes = value; }
        public List<SpiritFlagAttribute> FlagAttributes { get => flagAttributes; set => flagAttributes = value; }
        public List<NumeralAttribute> NumeralAttributes { get => numeralAttributes; set => numeralAttributes = value; }

        public void SetData()
        {
            Grades = new List<SpiritData>();
            Inventory = new Inventory(this.Get(Enums.Spirit.MaxInventorySlots).Value);
        }

        void Awake()
        {

            if (numeralAttributes == null || numeralAttributes.Count == 0)
                numeralAttributes = Ext.CreateAttributeList_N();

            if (flagAttributes == null || flagAttributes.Count == 0)
                flagAttributes = Ext.CreateAttributeList_SF();

            if (spiritAttributes == null || spiritAttributes.Count == 0)
                spiritAttributes = Ext.CreateAttributeList_S();

            ID.Add((int)Base.Element);
            ID.Add((int)Base.Rarity);
            ID.Add(NumberInList);

            if (DamageToRace == null || DamageToRace.Count == 0)
            {
                DamageToRace = new List<float>();
                var races = Enum.GetValues(typeof(RaceType));
                for (int i = 0; i < races.Length; i++)
                    DamageToRace.Add(100f);
            }
        }

#if UNITY_EDITOR

        [Button("Add to DataBase")]
        void AddToDataBase()
        {
            if (!this.Get(Enums.SpiritFlag.IsGradeSpirit).Value)
                if (DataControlSystem.Load<SpiritDataBase>() is SpiritDataBase dataBase)
                {
                    var thisElementAndRarityList = dataBase.Spirits.Elements[(int)Base.Element].Rarities[(int)Base.Rarity].Spirits;

                    if (!thisElementAndRarityList.Contains(this))
                    {
                        NumberInList = thisElementAndRarityList.Count;

                        ID = new ID() { (int)Base.Element, (int)Base.Rarity, NumberInList };

                        thisElementAndRarityList.Add(this);
                        EditorUtility.SetDirty(this);
                        DataControlSystem.Save(dataBase);
                    }
                    else Debug.LogWarning($"{this} already in data base");
                }
                else Debug.LogError($"{typeof(SpiritDataBase)} not found");
        }

        void RemoveFromDataBase()
        {
            if (!this.Get(Enums.SpiritFlag.IsGradeSpirit).Value)
                if (DataControlSystem.Load<SpiritDataBase>() is SpiritDataBase dataBase)
                {
                    dataBase.Spirits.Elements[(int)Base.Element].Rarities[(int)Base.Rarity].Spirits.RemoveAt(NumberInList);
                    DataControlSystem.Save(dataBase);
                }
        }
#endif 
    }
}