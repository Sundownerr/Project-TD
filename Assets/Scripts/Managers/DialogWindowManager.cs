﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

namespace Game.UI
{
    public class DialogWindowManager : UIWindow
    {
        TextMeshProUGUI windowText;
        Image background;
        Button buttonYes, buttonNo;
        RectTransform windowRect;

        static DialogWindowManager instance;
        public static DialogWindowManager Instance
        {
            get => instance;
            private set
            {
                if (instance == null) instance = value;
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            Instance = this;

            defaultYs = new float[] { transform.GetChild(0).localPosition.y };

            windowRect = transform.GetChild(0).GetComponent<RectTransform>();
            windowText = transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            buttonYes = transform.GetChild(0).GetChild(1).GetComponent<Button>();
            buttonNo = transform.GetChild(0).GetChild(2).GetComponent<Button>();
        }

        public void Show(string text, UnityAction yesAction, UnityAction noAction = null)
        {
            buttonYes.onClick.RemoveAllListeners();
            buttonNo.onClick.RemoveAllListeners();

            buttonYes.onClick.AddListener(yesAction);
            buttonNo.onClick.AddListener(noAction ?? OnButtonClicked);
         
            var wordsCount = text.Split(' ').Length;
            var width = Mathf.Clamp(text.Length * 11, 300, 500);
            var height = Mathf.Clamp(wordsCount * 2, 100, 500);

            windowText.text = text;
            windowRect.sizeDelta = new Vector2(width, height);

            Open(0.05f);

            void OnButtonClicked() => Close(UIWindow.Move.Up, 0.05f);
        }
    }
}