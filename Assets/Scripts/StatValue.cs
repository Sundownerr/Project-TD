using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NumeralAttribute 
{
    [SerializeField] public Numeral Type;

    [SerializeField] public Change IncreacePerLevel;
    [SerializeField] public double Value;
    [SerializeField] public double ValuePerLevel;

    public NumeralAttribute(Numeral type, double value)
    {
        Value = value;
        Type = type;
    }
}


