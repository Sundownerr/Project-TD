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
using Game.Utility;

namespace Game.UI
{
    public class SpiritStatValueUI : DescriptionBlock
    {
        public Enums.Spirit SpiritValue;
        public TextMeshProUGUI Value { get; set; }

        void Awake()
        {
            Value = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            GetComponent<LeanLocalizedText>().PhraseName = SpiritValue.GetStringKey();
        }
    }
}

