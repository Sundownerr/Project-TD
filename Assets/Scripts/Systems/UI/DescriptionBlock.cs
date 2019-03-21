using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Systems
{
    public class DescriptionBlock : MonoBehaviour, IHaveDescription, IPointerEnterHandler, IPointerExitHandler
    {
        [NaughtyAttributes.ResizableTextArea]
        public string Description;

        public void GetDescription() => ReferenceHolder.Get.DescriptionUISystem.ShowDescription(Description);

        public void OnPointerEnter(PointerEventData eventData) => GetDescription();

        public void OnPointerExit(PointerEventData eventData) => ReferenceHolder.Get.DescriptionUISystem.CloseDescription();
  
    }
}
