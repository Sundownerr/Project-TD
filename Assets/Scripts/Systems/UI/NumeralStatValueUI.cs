using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Enums;
using Game.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Systems
{
   public class NumeralStatValueUI : MonoBehaviour, IHaveDescription, IPointerEnterHandler, IPointerExitHandler
   {
      public Numeral Numeral;
      public TextMeshProUGUI Value;

      private void Awake()
      {
         Value = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
      }

      [NaughtyAttributes.ResizableTextArea]
      public string Description;

      public void GetDescription() => ReferenceHolder.Get.DescriptionUISystem.ShowDescription(Description);

      public void OnPointerEnter(PointerEventData eventData) => GetDescription();

      public void OnPointerExit(PointerEventData eventData) => ReferenceHolder.Get.DescriptionUISystem.CloseDescription();
      }
}

