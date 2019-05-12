﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using Game.Consts;

namespace Game.UI
{
    public class LobbyPlayerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public event EventHandler ReadyClicked;
        public event EventHandler ChangeMageClicked;
        public TextMeshProUGUI PlayerNameText, LevelText, ReadyText, MageNameText;
        public RawImage Avatar;
        public Button ChangeMageButton, ReadyButton;

        bool isReady;
        bool isOwner;

        void Awake()
        {
            ReadyText.text = $"<color=red>{LocaleKeys.ReadyNo.GetLocalized()}</color>";
            ReadyButton.interactable = false;

            ChangeMageButton.gameObject.SetActive(false);
        }

        public void Set()
        {
            ReadyButton.interactable = true;
            ReadyButton.onClick.AddListener(() =>
            {
                isReady = !isReady;
                ReadyText.text = isReady ?
                    $"<color=green>{LocaleKeys.ReadyYes.GetLocalized()}</color>" :
                    $"<color=red>{LocaleKeys.ReadyNo.GetLocalized()}</color>";
                ReadyClicked?.Invoke(null, null);
            });

            ChangeMageButton.gameObject.SetActive(true);
            ChangeMageButton.onClick.AddListener(() => ChangeMageClicked?.Invoke(null, null));
            ChangeMageButton.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            ReadyButton.onClick.RemoveAllListeners();
            ChangeMageButton.onClick.RemoveAllListeners();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!ReadyButton.interactable) return;

            ChangeMageButton.gameObject.SetActive(false);

        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!ReadyButton.interactable) return;

            ChangeMageButton.gameObject.SetActive(true);
        }
    }
}