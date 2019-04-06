using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Systems
{
    public class DescriptionBlock : MonoBehaviour, IHaveDescription, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        protected string title;

        [SerializeField, NaughtyAttributes.ResizableTextArea]
        protected string description;

        [SerializeField, NaughtyAttributes.ShowAssetPreview()]
        protected Image image;

        public string Description { get => description; set => description = value; }
        public string Title { get => title; set => title = value; }
        public Image Image { get => image; set => image = value; }

        public void GetDescription() => ReferenceHolder.Get.DescriptionUISystem.ShowDescription(Description);

        public void OnPointerEnter(PointerEventData eventData) => GetDescription();

        public void OnPointerExit(PointerEventData eventData) => ReferenceHolder.Get.DescriptionUISystem.CloseDescription();

    }
}
