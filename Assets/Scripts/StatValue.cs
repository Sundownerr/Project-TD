using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OneLine;
using Game;
using Game.Enums;


[Serializable]
public class EntityAttribute<EnumType, ValueType>
{
    [SerializeField] protected EnumType type;
    [SerializeField] protected ValueType value;

    public ValueType Value { get => value; set => this.value = value; }
    public EnumType Type { get => type; set => type = value; }
}

[Serializable]
public class EntityAttributeApplyable<EnumType, ValueType> : EntityAttribute<EnumType, ValueType>
{
    public ValueType AppliedValue { get; set; }
}


[Serializable]
public class EntityAttributeApplyableLevelUpable<EnumType, ValueType> : EntityAttributeApplyable<EnumType, ValueType>
{
    [SerializeField] public Increase IncreasePerLevel;
    [SerializeField] public double ValuePerLevel;
}

[Serializable]
public class NumeralAttribute : EntityAttributeApplyableLevelUpable<Numeral, double>
{
    public double Sum => Value + AppliedValue;
}

[Serializable]
public class SpiritAttribute : EntityAttributeApplyableLevelUpable<Spirit, double>
{
    public double Sum => Value + AppliedValue;
}

[Serializable]
public class EnemyAttribute : EntityAttributeApplyable<Enemy, double>
{
    public double Sum => Value + AppliedValue;
}

[Serializable]
public class SpiritFlagAttribute : EntityAttribute<SpiritFlag, bool>
{

}

