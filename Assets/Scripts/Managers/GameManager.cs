using Game;
using Game.Systems;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Transport.Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Lean.Localization;
using Game.Utility;
using Game.UI;
using Game.Enums;
using Game.Consts;

namespace Game.Managers
{
    public class GameManager : SingletonDDOL<GameManager>
    {
        public event Action<GameState> StateChanged;
        public event Action SteamLostConnection, SteamConnected;
        public GameObject[] Managers;

        MenuUISystem menu;
        public MenuUISystem Menu
        {
            get => menu;
            set
            {
                if (menu != null) return;

                menu = value;

                if (UIManager.Instance != null)
                    UIManager.Instance.Menu = value;
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
                StateChanged?.Invoke(currentGameState);
                Debug.Log(value);
            }
        }

        GameState previousGameState;
        public GameState PreviousGameState { get; private set; } = GameState.MainMenu;

        bool managersInstanced;

        protected override void Awake()
        {
            base.Awake();

            Application.targetFrameRate = 70;
            QualitySettings.vSyncCount = 0;
            Cursor.lockState = CursorLockMode.Confined;

            GetComponent<LeanLocalizationLoader>().LoadFromSource();

            SceneManager.sceneLoaded += OnSceneLoaded;
            Lean.Localization.LeanLocalization.OnLocalizationChanged += OnLocalizationChanged;

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
        }

        void Start()
        {
            Steam.Instance.ConnectionLost += OnSteamLostConnection;
            Steam.Instance.Connected += OnSteamConnected;
            UIManager.Instance.ReturnToMenu += OnReturnToMenu;
            UIManager.Instance.GameStarted += OnGameStarted;

            void OnGameStarted()
            {
                GameState = GameState.LoadingGame;
                SceneManager.LoadSceneAsync(StringConsts.SingleplayerMap);
            }

            void OnSteamConnected() => SteamConnected?.Invoke();

            void OnReturnToMenu()
            {
                GameState = GameState.UnloadingGame;
                SceneManager.LoadSceneAsync(StringConsts.MainMenu);
            }

            void OnSteamLostConnection()
            {
                var isUsingSteam =
                    GameState == GameState.InGameMultiplayer ||
                    GameState == GameState.InLobby ||
                    GameState == GameState.BrowsingLobbies ||
                    GameState == GameState.CreatingLobby;

                SteamLostConnection?.Invoke();

                if (isUsingSteam)
                {
                    GoToMainMenu();
                }
            }
        }

        public void GoToMainMenu()
        {
            Destroy(Steam.Instance.gameObject);
            Destroy(NetworkManager.singleton.gameObject);

            NetworkManager.Shutdown();
            SceneManager.LoadSceneAsync(StringConsts.MainMenu);
        }
    }
}