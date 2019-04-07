using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Enums;
using Game.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Lean.Localization;

namespace Game.Systems
{
    public class NumeralStatValueUI : DescriptionBlock
    {
        public Numeral NumeralValue;
        TextMeshProUGUI value;

        public TextMeshProUGUI Value { get; set; }

        void Awake()
        {
            Value = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            
            GetComponent<LeanLocalizedText>().PhraseName = NumeralValue.GetStringKey();
        }
    }
}

