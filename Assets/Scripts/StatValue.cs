using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NumeralValue
{
    [SerializeField]
    public Change ChangeType;

    [SerializeField]
    public double Value;
}

[Serializable]
public class NumeralAttribute 
{
    [SerializeField]
    public Numeral Type;

    [SerializeField]
    public Change ChangeType;

    [SerializeField]
    public double Value;

    public NumeralValue IncreasePerLevel;

    public NumeralAttribute(Numeral type, double value)
    {
        Value = value;
        Type = type;
    }
}


