using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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
        buttonText.text = "Back to Desktop";

        GetComponentInChildren<Button>().onClick.AddListener(() => { Clicked?.Invoke(null, null); });
    }

    void Start()
    {
        GameManager.Instance.StateChanged += OnGameStateChanged;
    }

    void OnGameStateChanged(object _, GameState e)
    {
        ChangeText();

        void ChangeText()
        {
            var toMainMenu =
                e == GameState.BrowsingLobbies ||
                e == GameState.MultiplayerInGame ||
                e == GameState.SingleplayerInGame;

            var toLobbyList =
                e == GameState.InLobby ||
                e == GameState.CreatingLobby;

            if (toMainMenu) { buttonText.text = LocaleKeys.BackToMainMenu.GetLocalized(); return; }
            if (toLobbyList) { buttonText.text = LocaleKeys.BackToLobbyList.GetLocalized(); return; }
        }
    }
}
