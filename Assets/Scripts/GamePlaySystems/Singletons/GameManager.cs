using Game;
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
    SingleplayerInGame,
    MultiplayerInGame,
    BrowsingLobbies,
    CreatingLobby,
    InLobby,
    SelectingMage
}

public class GameManager : MonoBehaviour
{
    public bool UseLocalTransport;
    public event EventHandler<GameState> StateChanged;
    public GameObject GameDataPrefab, SteamInstancePrefab, ReferenceHolderPrefab, GameLoopPrefab, BackButtonPrefab;
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

    protected GameState previousGameState = GameState.MainMenu;
    GameState currentGameState;
    public GameState GameState
    {
        get => currentGameState;
        private set
        {
            previousGameState = currentGameState;
            currentGameState = value;
            StateChanged?.Invoke(null, currentGameState);
            Debug.Log(value);
        }
    }

    protected StateMachine state;

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
                GameState = GameState.SingleplayerInGame;
                return;
            }

            if (scene.name == "MultiplayerMap1")
            {
                GameState = GameState.MultiplayerInGame;
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
                if (GameLoop.Instance == null) Instantiate(GameLoopPrefab);
                if (BackButtonUI.Instance == null) Instantiate(BackButtonPrefab);
                managersInstanced = true;
            }
        };
    }

    void Start()
    {
        Steam.Instance.ConnectionLost += OnLostConnectionToSteam;
        BackButtonUI.Instance.Clicked += OnBackButtonClicked;

        Menu.Activated += OnMenuActivated;
        Menu.MageSelection.Activated += OnMageSelectionActivated;
        Menu.LobbyList.Activated += OnLobbyListActivated;
        Menu.LobbyList.LobbyUI.Activated += OnLobbyActivated;
        Menu.LobbyList.LobbyCreationWindow.Activated += OnLobbyCreationWindowActivated;

        state = new StateMachine();
        state.ChangeState(new InMainMenu(this));
    }

    void OnBackButtonClicked(object _, EventArgs e) => state.Update();
    void OnMageSelectionActivated(object sender, EventArgs e) => state.ChangeState(new InMageSelection(this));
    void OnLobbyCreationWindowActivated(object _, EventArgs e) => state.ChangeState(new InLobbyCreation(this));
    void OnLobbyActivated(object _, EventArgs e) => state.ChangeState(new InLobby(this));
    void OnLobbyListActivated(object _, EventArgs e) => state.ChangeState(new InBrowsingLobbies(this));
    void OnMenuActivated(object _, EventArgs e) => state.ChangeState(new InMainMenu(this));

    void OnLostConnectionToSteam(object _, EventArgs e)
    {
        var isUsingSteam =
            GameState == GameState.MultiplayerInGame ||
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

    class InMainMenu : IState
    {
        public InMainMenu(GameManager o) => this.o = o; readonly GameManager o;

        public void Enter()
        {
            if (o.previousGameState == GameState.InLobby || o.previousGameState == GameState.MainMenu || o.previousGameState == GameState.CreatingLobby)
                o.Menu.LobbyList.gameObject.SetActive(false);
            o.GameState = GameState.MainMenu;
        }
        public void Execute()
        {
            if (o.GameState == GameState.SelectingMage) o.state.ChangeState(new InMageSelection(o));
            else
            if (o.GameState == GameState.BrowsingLobbies) o.state.ChangeState(new InBrowsingLobbies(o));
            else
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
        public void Exit() { }
    }

    class InMageSelection : IState
    {
        public InMageSelection(GameManager o) => this.o = o; readonly GameManager o;

        public void Enter()
        {
            o.Menu.MageSelection.gameObject.SetActive(true);
            o.GameState = GameState.SelectingMage;
        }
        public void Execute()
        {
            if (o.previousGameState == GameState.MainMenu) o.state.ChangeState(new InMainMenu(o));
            else
            if (o.previousGameState == GameState.InLobby) o.state.ChangeState(new InLobby(o));
        }
        public void Exit() { o.Menu.MageSelection.gameObject.SetActive(false); }
    }

    class InBrowsingLobbies : IState
    {
        public InBrowsingLobbies(GameManager o) => this.o = o; readonly GameManager o;

        public void Enter()
        {
            o.Menu.LobbyList.gameObject.SetActive(true);
            o.GameState = GameState.BrowsingLobbies;
        }
        public void Execute() { o.state.ChangeState(new InMainMenu(o)); }
        public void Exit() { }
    }

    class InLobby : IState
    {
        public InLobby(GameManager o) => this.o = o; readonly GameManager o;

        public void Enter()
        {
            o.Menu.LobbyList.LobbyUI.gameObject.SetActive(true);
            o.GameState = GameState.InLobby;
        }
        public void Execute() { o.state.ChangeState(new InBrowsingLobbies(o)); }
        public void Exit() { o.Menu.LobbyList.LobbyUI.gameObject.SetActive(false); }
    }

    class InLobbyCreation : IState
    {
        public InLobbyCreation(GameManager o) => this.o = o; readonly GameManager o;

        public void Enter()
        {
            o.Menu.LobbyList.LobbyCreationWindow.gameObject.SetActive(true);
            o.GameState = GameState.CreatingLobby;
        }
        public void Execute() { o.state.ChangeState(new InBrowsingLobbies(o)); }
        public void Exit() { o.Menu.LobbyList.LobbyCreationWindow.gameObject.SetActive(false); }
    }
}
