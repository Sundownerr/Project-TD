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

namespace Game
{
    [Serializable, CreateAssetMenu(fileName = "New Mage", menuName = "Data/Mage")]
    public class MageData : Entity
    {
        private enum Attribute { Positive, Negative }

        private enum Value { Base, PerLevel }

        [SerializeField, OneLine, OneLine.HideLabel]
        private List<NumeralAttribute> numeralAttributes;

        [SerializeField, OneLine, OneLine.HideLabel]
        private List<SpiritAttribute> spiritAttributes;

        [SerializeField, OneLine, OneLine.HideLabel]
        private List<SpiritFlagAttribute> spiritFlagAttributes;

        [SerializeField, OneLine, OneLine.HideLabel]
        private List<EnemyAttribute> enemyAttributes;

        [NaughtyAttributes.ResizableTextArea]
        public string AdvancedDescription;

        public List<NumeralAttribute> NumeralAttributes { get => numeralAttributes; set => numeralAttributes = value; }
        public List<SpiritAttribute> SpiritAttributes { get => spiritAttributes; set => spiritAttributes = value; }
        public List<SpiritFlagAttribute> SpiritFlagAttributes { get => spiritFlagAttributes; set => spiritFlagAttributes = value; }
        public List<EnemyAttribute> EnemyAttributes { get => enemyAttributes; set => enemyAttributes = value; }

        private StringBuilder bonusesBuilder;

        private void Awake()
        {
            var descriptionBuilder = new StringBuilder().AppendLine("Bonuses:");
            bonusesBuilder = new StringBuilder();

            descriptionBuilder
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
                .Append(BuildDescription(enemyAttributes, Attribute.Negative));

            AdvancedDescription = descriptionBuilder.ToString();

        }

        private string BuildDescription(List<SpiritAttribute> list, Attribute type, Value valueType)
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
                .Append(LeanLocalization.GetTranslationText(notNullAttribute.Type.GetStringKey()))
                .AppendLine("</color>");
            });

            var result = bonusesBuilder.ToString();
            bonusesBuilder.Clear();

            return result;
        }

        private string BuildDescription(List<NumeralAttribute> list, Attribute type, Value valueType)
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
                .Append(LeanLocalization.GetTranslationText(notNullAttribute.Type.GetStringKey()))
                .AppendLine("</color>");
            });

            var result = bonusesBuilder.ToString();
            bonusesBuilder.Clear();

            return result;
        }

        private string BuildDescription(List<EnemyAttribute> list, Attribute type)
        {
            var isPositive = type == Attribute.Positive;

            list.FindAll(attribute => isPositive ? attribute.Value < 0 : attribute.Value > 0)?
                .ForEach(notNullAttribute =>
                {
                    bonusesBuilder
                    .Append(isPositive ? "<color=green>- " : "<color=red>+ ")
                    .Append(Math.Abs(notNullAttribute.Value).ToString())
                    .Append($" {LeanLocalization.GetTranslationText(notNullAttribute.Type.GetStringKey())} for enemies")
                    .AppendLine("</color>");
                });

            var result = bonusesBuilder.ToString();
            bonusesBuilder.Clear();

            return result;
        }
    }
}
