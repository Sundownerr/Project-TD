using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Game.Systems
{


    public class BackButtonUI : MonoBehaviour
    {
        public GameObject ThisWindow, PreviousWindow;
        public string PreviousSceneName;

        public event EventHandler Clicked;

        static BackButtonUI instance;
        public static BackButtonUI Instance
        {
            get => instance;
            private set
            {
                if (instance == null) instance = value;
            }
        }

        static TextMeshProUGUI buttonText;

        void Awake()
        {
            DontDestroyOnLoad(this);
            Instance = this;
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = "Back";

            GetComponentInChildren<Button>().onClick.AddListener(() => { Clicked?.Invoke(null, null); });
            UIManager.Instance.BackButton = this;
        }
    }
}