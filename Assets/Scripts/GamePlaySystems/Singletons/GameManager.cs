﻿using Game;
using Game.Systems;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Transport.Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Lean.Localization;

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
    public event EventHandler<GameState> StateChanged;
    public event EventHandler SteamLostConnection, SteamConnected;
    public GameObject[] Managers;

    MenuUISystem menu;
    public MenuUISystem Menu
    {
        get => menu;
        set
        {
            menu = value;

            if (UIManager.Instance != null)
                UIManager.Instance.Menu = value;
        }
    }

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
        if (GameManager.Instance != null) { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);
        Instance = this;

        Application.targetFrameRate = 70;
        QualitySettings.vSyncCount = 0;
        Cursor.lockState = CursorLockMode.Confined;

        GetComponent<LeanLocalizationLoader>().LoadFromSource();

        SceneManager.sceneLoaded += OnSceneLoaded;
        Lean.Localization.LeanLocalization.OnLocalizationChanged += OnLocalizationChanged;
    }

    void Start()
    {
        Steam.Instance.ConnectionLost += OnSteamLostConnection;
        Steam.Instance.Connected += OnSteamConnected;
        UIManager.Instance.ReturnToMenu += OnReturnToMenu;
        UIManager.Instance.GameStarted += OnGameStarted;

    }

    void OnGameStarted(object sender, EventArgs e)
    {
        GameState = GameState.LoadingGame;
        SceneManager.LoadSceneAsync(StringConsts.SingleplayerMap);
    }

    void OnSteamConnected(object sender, EventArgs e) => SteamConnected?.Invoke(null, null);

    void OnLocalizationChanged()
    {
        if (!managersInstanced)
        {
            for (int i = 0; i < Managers.Length; i++)
                Instantiate(Managers[i]);
            managersInstanced = true;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == StringConsts.SingleplayerMap)
        {
            GameState = GameState.InGameSingleplayer;
            return;
        }

        if (scene.name == StringConsts.MultiplayerMap1)
        {
            GameState = GameState.InGameMultiplayer;
            return;
        }

        if (scene.name == StringConsts.MainMenu)
        {
            RefreshReferences();
            GameState = GameState.MainMenu;
        }

        void RefreshReferences()
        {
            Menu = GameObject.FindGameObjectWithTag(StringConsts.MenuUITag).GetComponent<MenuUISystem>();

        }
    }

    void OnReturnToMenu(object _, EventArgs e)
    {
        GameState = GameState.UnloadingGame;
        SceneManager.LoadSceneAsync(StringConsts.MainMenu);
    }

    void OnSteamLostConnection(object _, EventArgs e)
    {
        var isUsingSteam =
            GameState == GameState.InGameMultiplayer ||
            GameState == GameState.InLobby ||
            GameState == GameState.BrowsingLobbies ||
            GameState == GameState.CreatingLobby;

        SteamLostConnection?.Invoke(null, null);

        if (isUsingSteam)
            GoToMainMenu();
    }

    public void GoToMainMenu()
    {
        Destroy(Steam.Instance.gameObject);
        Destroy(NetworkManager.singleton.gameObject);

        NetworkManager.Shutdown();
        SceneManager.LoadSceneAsync(StringConsts.MainMenu);
    }
}
