using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using OneLine;
using UnityEngine;

namespace Game
{
    [Serializable, CreateAssetMenu(fileName = "New Mage", menuName = "Data/Mage")]
    public class MageHero : Entity
    {
        [SerializeField, OneLine, OneLine.HideLabel]
        private List<NumeralAttribute> numeralAttributes;

        [SerializeField, OneLine, OneLine.HideLabel]
        private List<SpiritAttribute> spiritAttributes;

        [SerializeField, OneLine, OneLine.HideLabel]
        private List<SpiritFlagAttribute> spiritFlagAttributes;
        
        [SerializeField, OneLine, OneLine.HideLabel]
        private List<EnemyAttribute> enemyAttributes;

        public List<NumeralAttribute> NumeralAttributes { get => numeralAttributes; set => numeralAttributes = value; }
        public List<SpiritAttribute> SpiritAttributes { get => spiritAttributes; set => spiritAttributes = value; }
        public List<SpiritFlagAttribute> SpiritFlagAttributes { get => spiritFlagAttributes; set => spiritFlagAttributes = value; }
        public List<EnemyAttribute> EnemyAttributes { get => enemyAttributes; set => enemyAttributes = value; }
    }
}
