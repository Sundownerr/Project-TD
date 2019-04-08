﻿using Game;
using Game.Systems;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Transport.Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    LoadingGame,
    UnloadingGame,
    InGameSingleplayer,
    InGameMultiplayer,
    BrowsingLobbies,
    CreatingLobby,
    InLobby,
    SelectingMage
}

public class GameManager : MonoBehaviour
{
    public bool UseLocalTransport;
    public event EventHandler<GameState> StateChanged;
    public GameObject GameDataPrefab, SteamInstancePrefab, ReferenceHolderPrefab, GameLoopPrefab, BackButtonPrefab, UIManagerPrefab, FadeManagerPrefab;
    public MenuUISystem Menu { get; set; }

    static GameManager instance;
    public static GameManager Instance
    {
        get => instance;
        private set
        {
            if (instance == null) instance = value;
        }
    }

    GameState currentGameState;
    public GameState GameState
    {
        get => currentGameState;
        set
        {
            if (value == currentGameState) return;

            PreviousGameState = currentGameState;
            currentGameState = value;
            StateChanged?.Invoke(null, currentGameState);
            Debug.Log(value);
        }
    }

    GameState previousGameState;
    public GameState PreviousGameState { get; private set; } = GameState.MainMenu;

    bool managersInstanced;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;

        Application.targetFrameRate = 70;
        QualitySettings.vSyncCount = 0;
        Cursor.lockState = CursorLockMode.Confined;

        SceneManager.sceneLoaded += (scene, loadMode) =>
        {
            if (scene.name == "SingleplayerMap")
            {
                GameState = GameState.InGameSingleplayer;
                return;
            }

            if (scene.name == "MultiplayerMap1")
            {
                GameState = GameState.InGameMultiplayer;
            }
        };

        Lean.Localization.LeanLocalization.OnLocalizationChanged += () =>
        {
            if (!managersInstanced)
            {
                if (GameData.Instance == null) Instantiate(GameDataPrefab);
                if (Steam.Instance == null) Instantiate(SteamInstancePrefab);
                if (ReferenceHolder.Get == null) Instantiate(ReferenceHolderPrefab);
                if (GameLoop.Instance == null) Instantiate(GameLoopPrefab);
                if (UIManager.Instance == null) Instantiate(UIManagerPrefab);
                if (BackButtonUI.Instance == null) Instantiate(BackButtonPrefab);
                if (FadeManager.Instance == null) Instantiate(FadeManagerPrefab);
                managersInstanced = true;
            }
        };
    }

    void Start()
    {
        Steam.Instance.ConnectionLost += OnLostConnectionToSteam;
        Menu.MageSelected += OnMageSelected;
    }

    private void OnMageSelected(object sender, MageData e)
    {
        SceneManager.LoadSceneAsync("SingleplayerMap");
        GameState = GameState.LoadingGame;
    }

    void OnLostConnectionToSteam(object _, EventArgs e)
    {
        var isUsingSteam =
            GameState == GameState.InGameMultiplayer ||
            GameState == GameState.InLobby ||
            GameState == GameState.BrowsingLobbies ||
            GameState == GameState.CreatingLobby;

        if (isUsingSteam)
            GoToMainMenu();
    }

    public void GoToMainMenu()
    {
        Destroy(Steam.Instance.gameObject);
        Destroy(NetworkManager.singleton.gameObject);

        NetworkManager.Shutdown();
        SceneManager.LoadSceneAsync("MainMenu");
    }
}