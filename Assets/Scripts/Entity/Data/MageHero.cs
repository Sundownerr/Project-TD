using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using OneLine;
using UnityEngine;


[Serializable, CreateAssetMenu(fileName = "New Mage", menuName = "Data/Mage")]
public class MageHero : Entity
{
    [SerializeField, OneLine, OneLine.HideLabel]
    private List<NumeralAttribute> numeralAttributes;

    [SerializeField, OneLine, OneLine.HideLabel]
    private List<SpiritAttribute> spiritAttributes;

    public List<NumeralAttribute> NumeralAttributes { get => numeralAttributes; set => numeralAttributes = value; }
    public List<SpiritAttribute> SpiritAttributes { get => spiritAttributes; set => spiritAttributes = value; }

    
}

