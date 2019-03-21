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
        public bool IsGradeSpirit;
        public bool CanAttackFlying;
        public List<NumeralAttribute> Attributes;
        public List<Ability> AbilityList;
        public List<Trait> TraitList;

        [SerializeField]
        public RarityType Rarity;

        [SerializeField]
        public ElementType Element;

        public List<SpiritData> Grades;
        public List<float> DamageToRace;
        public int GradeCount { get; set; } = -1;
        public Inventory Inventory { get; set; }
        public List<NumeralAttribute> BaseAttributes { get => Attributes; set => Attributes = value; }
        public List<NumeralAttribute> AppliedAttributes { get; set; }
        public List<Ability> Abilities { get => AbilityList; set => AbilityList = value; }
        public List<Trait> Traits { get => TraitList; set => TraitList = value; }

        private int numberInList;

        private void Awake()
        {
            if (Attributes == null || Attributes.Count == 0)
                Attributes = Attributes.CreateAttributeList();
        }

        public void SetData()
        {
            Grades = new List<SpiritData>();
            Inventory = new Inventory(this.Get(Numeral.MaxInventorySlots, From.Base).Value);

            AppliedAttributes = AppliedAttributes.CreateAttributeList();         

            if (DamageToRace == null)
                for (int i = 0; i < 5; i++)
                    DamageToRace.Add(100f);
        }

        public void SetId()
        {
            Id = new List<int>
            {
                (int)Element,
                (int)Rarity,
                numberInList,
            };
        }

        #region IF UNITY EDITOR

#if UNITY_EDITOR
        [Button("Add to DataBase")]
        private void AddToDataBase()
        {
            if (!IsGradeSpirit)
            {
                var dataBase = DataControlSystem.Load<SpiritDataBase>() as SpiritDataBase;
                var elements = dataBase.Spirits.Elements;

                for (int i = 0; i < elements.Count; i++)
                    if ((int)Element == i)
                        for (int j = 0; j < elements[i].Rarities.Count; j++)
                            if ((int)Rarity == j)
                            {
                                var spirits = elements[i].Rarities[j].Spirits;
                                for (int k = 0; k < spirits.Count; k++)
                                    if (this.CompareId(spirits[k].Id))
                                        return;

                                numberInList = spirits.Count;
                                SetId();
                                elements[i].Rarities[j].Spirits.Add(this);
                                DataControlSystem.Save(dataBase);
                                EditorUtility.SetDirty(this);
                                return;
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

        private void OnValuesChanged() => SetId();
#endif 

        #endregion   
    }
}