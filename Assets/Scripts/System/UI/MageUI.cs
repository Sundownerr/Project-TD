﻿using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Systems;
using System;
using TMPro;
using Game.Data.Mage;

namespace Game.UI
{
    public class MageUI : MonoBehaviour
    {
        public event Action<MageData> Selected;
        public MageData MageData;

        Button button;

        void Awake()
        {
            MageData = Instantiate(MageData);
            MageData.GenerateDescription();
            button = transform.GetChild(0).GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            GetComponentInChildren<Image>().sprite = MageData.Image;
            GetComponentInChildren<TextMeshProUGUI>().text = MageData.Name;

            void OnClick() => Selected?.Invoke(MageData);
        }

        void OnDestroy()
        {
            Destroy(MageData);
            Selected = null;
            button.onClick.RemoveAllListeners();
        }
    }
}