using System.Collections;
using System.Collections.Generic;
using Game;
using Game.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class SlotWithCooldown : DescriptionBlock
    {
        public GameObject CooldownPrefab;
        public Image CooldownImage;

        public int EntityIndex { get; set; }

        void Awake()
        {
            CooldownImage = Instantiate(CooldownPrefab, transform.position, Quaternion.identity, transform).GetComponent<Image>();
        }

        public void Set(string description, Sprite sprite, int entityIndex)
        {
            base.Set(description, sprite);

            CooldownImage.fillAmount = 0;
            EntityIndex = entityIndex;
        }
    }
}