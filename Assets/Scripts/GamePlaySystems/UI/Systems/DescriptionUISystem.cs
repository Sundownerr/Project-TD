using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Systems
{
    public class DescriptionUISystem : MonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public GameObject DescriptionBlock;

        TextMeshProUGUI descriptionText;
        RectTransform descriptionRect;

        void Awake()
        {           
            descriptionText = DescriptionBlock.GetComponentInChildren<TextMeshProUGUI>();
            descriptionRect = DescriptionBlock.GetComponent<RectTransform>();
        }

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;
        }

        public void ShowDescription(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var wordsCount = text.Split(' ').Length;
            var width = Mathf.Clamp(text.Length * 8, 80, 200);
            var height = 25 + wordsCount * 2;
            var blockPosition = Input.mousePosition + new Vector3(0, 15 + wordsCount * 1.7f);

            DescriptionBlock.SetActive(true);
            DescriptionBlock.transform.position = blockPosition;
            descriptionRect.sizeDelta = new Vector2(width, height);
            descriptionText.text = text;
        }

        public void CloseDescription()
        {
            DescriptionBlock.transform.SetParent(transform);
            DescriptionBlock.SetActive(false);
        }
    }
}
