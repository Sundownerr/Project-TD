using System;
using UnityEngine;
using Game.Enums;

namespace Game.Data.Attributes
{
    [Serializable]
    public class EntityAttribute<EnumType, ValueType>
    {
        [SerializeField] protected EnumType type;
        [SerializeField] protected ValueType value;

        public ValueType Value { get => value; set => this.value = value; }
        public EnumType Type { get => type; set => type = value; }
    }

    ///<summary>
    /// A is Apply-able: have appliedValue
    ///</summary>
    [Serializable]
    public class EntityAttribute_A<EnumType, ValueType> : EntityAttribute<EnumType, double>
    {
        public double AppliedValue { get; set; }
        public double Sum => Value + AppliedValue;
    }

    ///<summary>
    /// L is Level-Up-able: have per level values 
    ///
    /// A is Apply-able: have appliedValue
    ///</summary>
    [Serializable]
    public class EntityAttribute_A_L<EnumType, ValueType> : EntityAttribute_A<EnumType, double>
    {
        [SerializeField] public Increase IncreasePerLevel;
        [SerializeField] public double ValuePerLevel;

        public void LevelUp()
        {
            Value = IncreasePerLevel == Increase.ByValue ?
                Value + ValuePerLevel :
                Value + Value.GetPercent(ValuePerLevel);
        }
    }

    [Serializable]
    public class NumeralAttribute : EntityAttribute_A_L<Numeral, double> { }

    [Serializable]
    public class SpiritAttribute : EntityAttribute_A_L<Enums.Spirit, double> { }

    [Serializable]
    public class EnemyAttribute : EntityAttribute_A<Enums.Enemy, double> { }

    [Serializable]
    public class SpiritFlagAttribute : EntityAttribute<SpiritFlag, bool> { }
}