using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Game.Utility;
using Game.Managers;

namespace Game.UI
{
    public class BackButtonUI : SingletonDDOL<BackButtonUI>
    {
        public GameObject ThisWindow, PreviousWindow;
        public string PreviousSceneName;

        public event EventHandler Clicked;

        static TextMeshProUGUI buttonText;

        protected override void Awake()
        {
            base.Awake();

            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = "Back";

            GetComponentInChildren<Button>().onClick.AddListener(() => { Clicked?.Invoke(null, null); });
            UIManager.Instance.BackButton = this;
        }
    }
}