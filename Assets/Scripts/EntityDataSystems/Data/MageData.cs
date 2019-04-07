using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Game;
using OneLine;
using UnityEngine;
using Game.Systems;
using Lean.Localization;
using Game.Enums;
using NaughtyAttributes;
using Game.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    [Serializable, CreateAssetMenu(fileName = "New Mage", menuName = "Data/Mage")]
    public class MageData : Entity
    {
        enum Attribute { Positive, Negative }
        enum Value { Base, PerLevel }

        [SerializeField, OneLine, OneLine.HideLabel]
        List<NumeralAttribute> numeralAttributes;

        [SerializeField, OneLine, OneLine.HideLabel]
        List<SpiritAttribute> spiritAttributes;

        [SerializeField, OneLine, OneLine.HideLabel]
        List<SpiritFlagAttribute> spiritFlagAttributes;

        [SerializeField, OneLine, OneLine.HideLabel]
        List<EnemyAttribute> enemyAttributes;

        [NaughtyAttributes.ResizableTextArea]
        public string AdvancedDescription;

        public List<NumeralAttribute> NumeralAttributes { get => numeralAttributes; set => numeralAttributes = value; }
        public List<SpiritAttribute> SpiritAttributes { get => spiritAttributes; set => spiritAttributes = value; }
        public List<SpiritFlagAttribute> SpiritFlagAttributes { get => spiritFlagAttributes; set => spiritFlagAttributes = value; }
        public List<EnemyAttribute> EnemyAttributes { get => enemyAttributes; set => enemyAttributes = value; }

        StringBuilder bonusesBuilder;
        [SerializeField, HideInInspector] private int numberInList;

        void Awake()
        {
            ID.ForEach(x => Debug.Log($"mage id: {x}"));
        }

        public void GenerateDescription()
        {
            bonusesBuilder = new StringBuilder();

            AdvancedDescription = new StringBuilder()
                .AppendLine("Bonuses:")
                .Append(BuildDescription(spiritAttributes, Attribute.Positive, Value.Base))
                .Append(BuildDescription(spiritAttributes, Attribute.Positive, Value.PerLevel))
                .Append(BuildDescription(numeralAttributes, Attribute.Positive, Value.Base))
                .Append(BuildDescription(numeralAttributes, Attribute.Positive, Value.PerLevel))
                .AppendLine(BuildDescription(enemyAttributes, Attribute.Positive))
                .AppendLine("Drawbacks:")
                .Append(BuildDescription(spiritAttributes, Attribute.Negative, Value.Base))
                .Append(BuildDescription(spiritAttributes, Attribute.Negative, Value.PerLevel))
                .Append(BuildDescription(numeralAttributes, Attribute.Negative, Value.Base))
                .Append(BuildDescription(numeralAttributes, Attribute.Negative, Value.PerLevel))
                .Append(BuildDescription(enemyAttributes, Attribute.Negative))
                .ToString();
        }

#if UNITY_EDITOR
        [Button("Add to DataBase")]
        void AddToDataBase()
        {
            if (DataControlSystem.Load<MageDataBase>() is MageDataBase dataBase)
            {
                if (!dataBase.Data.Contains(this))
                {
                    numberInList = dataBase.Data.Count;

                    ID = new ID() { numberInList };

                    dataBase.Data.Add(this);
                    EditorUtility.SetDirty(this);
                    DataControlSystem.Save(dataBase);
                }
                else Debug.LogWarning($"{this} already in data base");
            }
            else Debug.LogError($"{typeof(MageDataBase)} not found");
        }
#endif 

        string BuildDescription(List<SpiritAttribute> list, Attribute type, Value valueType)
        {
            var isPositive = type == Attribute.Positive;
            var isBaseValue = valueType == Value.Base;

            list.FindAll(attribute => isPositive ?
                                        isBaseValue ?
                                            attribute.Value > 0 : attribute.ValuePerLevel > 0 :
                                        isBaseValue ?
                                            attribute.Value < 0 : attribute.ValuePerLevel < 0)?
            .ForEach(notNullAttribute =>
            {
                bonusesBuilder
                .Append(isPositive ? "<color=green>+ " : "<color=red>- ")
                .Append($"{Math.Abs(isBaseValue ? notNullAttribute.Value : notNullAttribute.ValuePerLevel).ToString()} ")
                .Append(notNullAttribute.Type.GetLocalized())
                .AppendLine("</color>");
            });

            var result = bonusesBuilder.ToString();
            bonusesBuilder.Clear();

            return result;
        }

        string BuildDescription(List<NumeralAttribute> list, Attribute type, Value valueType)
        {
            var isPositive = type == Attribute.Positive;
            var isBaseValue = valueType == Value.Base;

            list.FindAll(attribute => isPositive ?
                                        isBaseValue ?
                                            attribute.Value > 0 : attribute.ValuePerLevel > 0 :
                                        isBaseValue ?
                                            attribute.Value < 0 : attribute.ValuePerLevel < 0)?
            .ForEach(notNullAttribute =>
            {
                bonusesBuilder
                .Append(isPositive ? "<color=green>+ " : "<color=red>- ")
                .Append($"{Math.Abs(isBaseValue ? notNullAttribute.Value : notNullAttribute.ValuePerLevel).ToString()} ")
                .Append(notNullAttribute.Type.GetLocalized())
                .AppendLine("</color>");
            });

            var result = bonusesBuilder.ToString();
            bonusesBuilder.Clear();

            return result;
        }

        string BuildDescription(List<EnemyAttribute> list, Attribute type)
        {
            var isPositive = type == Attribute.Positive;

            list.FindAll(attribute => isPositive ? attribute.Value < 0 : attribute.Value > 0)?
                .ForEach(notNullAttribute =>
                {
                    bonusesBuilder
                    .Append(isPositive ? "<color=green>- " : "<color=red>+ ")
                    .Append(Math.Abs(notNullAttribute.Value).ToString())
                    .Append($" {notNullAttribute.Type.GetLocalized()} for enemies")
                    .AppendLine("</color>");
                });

            var result = bonusesBuilder.ToString();
            bonusesBuilder.Clear();

            return result;
        }
    }
}
