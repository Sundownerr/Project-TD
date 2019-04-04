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
public class NumeralAttribute : EntityAttributeApplyable<Numeral, double>
{
    [SerializeField] public Increase IncreacePerLevel;
    [SerializeField] public double ValuePerLevel;
    public double Sum => Value + AppliedValue;
}

[Serializable]
public class SpiritAttribute : EntityAttributeApplyable<Spirit, double>
{
    [SerializeField] public Increase IncreacePerLevel;
    [SerializeField] public double ValuePerLevel;
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

