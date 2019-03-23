using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Game.Spirit.Data.Stats;
using Game.Data;
using UnityEditor;
using Game.Systems;


namespace Game.Spirit.Data
{
    [CreateAssetMenu(fileName = "New Spirit", menuName = "Data/Spirit/Spirit")]
    [Serializable]
    public class SpiritData : Entity, IAttributeComponent, IAbilityComponent, ITraitComponent
    {
        [SerializeField] public bool IsGradeSpirit;
        [SerializeField] public bool CanAttackFlying;
        [SerializeField] public List<NumeralAttribute> Attributes;
        [SerializeField] public List<Ability> AbilityList;
        [SerializeField] public List<Trait> TraitList;
        [SerializeField] public RarityType Rarity;
        [SerializeField] public ElementType Element;
        [SerializeField] public List<SpiritData> Grades;
        [SerializeField] public List<float> DamageToRace;
        [SerializeField] public int GradeCount { get; set; } = -1;
        [SerializeField] public Inventory Inventory { get; set; }
        [SerializeField] public List<NumeralAttribute> BaseAttributes { get => Attributes; set => Attributes = value; }
        [SerializeField] public List<NumeralAttribute> AppliedAttributes { get; set; }
        [SerializeField] public List<Ability> Abilities { get => AbilityList; set => AbilityList = value; }
        [SerializeField] public List<Trait> Traits { get => TraitList; set => TraitList = value; }

        [SerializeField] public int numberInList;

        public void SetData()
        {
            Grades = new List<SpiritData>();
            Inventory = new Inventory(this.Get(Numeral.MaxInventorySlots, From.Base).Value);

            AppliedAttributes = ExtensionMethods.CreateAttributeList();

            if (DamageToRace == null)
                for (int i = 0; i < 5; i++)
                    DamageToRace.Add(100f);
        }

        private void Awake()
        {
            if (Attributes == null || Attributes.Count == 0)
                Attributes = ExtensionMethods.CreateAttributeList();

            ID.Add((int)Element);
            ID.Add((int)Rarity);
            ID.Add(numberInList);
        }

#if UNITY_EDITOR

        [Button("Add to DataBase")]
        private void AddToDataBase()
        {
            if (!IsGradeSpirit)
                if (DataControlSystem.Load<SpiritDataBase>() is SpiritDataBase dataBase)
                {
                    var thisElementAndRarityList = dataBase.Spirits.Elements[(int)Element].Rarities[(int)Rarity].Spirits;

                    if (!thisElementAndRarityList.Contains(this))
                    {
                        numberInList = thisElementAndRarityList.Count;

                        ID = new ID() { (int)Element, (int)Rarity, numberInList };

                        thisElementAndRarityList.Add(this);
                        EditorUtility.SetDirty(this);
                        DataControlSystem.Save(dataBase);
                    }
                }
        }

        private void RemoveFromDataBase()
        {
            if (!IsGradeSpirit)
                if (DataControlSystem.Load<SpiritDataBase>() is SpiritDataBase dataBase)
                {
                    dataBase.Spirits.Elements[(int)Element].Rarities[(int)Rarity].Spirits.RemoveAt(numberInList);
                    DataControlSystem.Save(dataBase);
                }
        }
#endif 
    }
}