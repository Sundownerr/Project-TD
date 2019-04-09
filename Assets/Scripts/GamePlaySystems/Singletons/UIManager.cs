using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public event EventHandler ReturnToMenu;
    static UIManager instance;
    public static UIManager Instance
    {
        get => instance;
        private set
        {
            if (instance == null) instance = value;
        }
    }

    protected MenuUISystem menu;
    public MenuUISystem Menu
    {
        get => menu;
        set
        {
            menu = value;
            lobbyList = menu.LobbyList;
            lobby = lobbyList.LobbyUI;
            lobbyCreationWindow = lobbyList.LobbyCreationWindow;
            mageSelection = menu.MageSelection;
            state.ChangeState(new InMainMenu(this));
        }
    }

    protected StateMachine state = new StateMachine();
    protected LobbyListUISystem lobbyList;
    protected LobbyUISystem lobby;
    protected MageSelectionUISystem mageSelection;
    protected LobbyCreationWindowUISystem lobbyCreationWindow;

    void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }

    void Start()
    {
        GameManager.Instance.StateChanged += OnGameStateChanged;
        BackButtonUI.Instance.Clicked += OnBackButtonClicked;
        Menu = GameManager.Instance.Menu;
    }

    void OnBackButtonClicked(object _, EventArgs e) => state.Update();
    void OnGameStateChanged(object _, GameState e)
    {
        var newState = GetState();

        if (newState != null)
            state.ChangeState(newState);
    }

    IState GetState()
    {
        if (GameManager.Instance.GameState == GameState.MainMenu) return new InMainMenu(this);
        if (GameManager.Instance.GameState == GameState.BrowsingLobbies) return new InBrowsingLobbies(this);
        if (GameManager.Instance.GameState == GameState.InLobby) return new InLobby(this);
        if (GameManager.Instance.GameState == GameState.SelectingMage) return new InMageSelection(this);
        if (GameManager.Instance.GameState == GameState.CreatingLobby) return new InLobbyCreation(this);
        if (GameManager.Instance.GameState == GameState.InGameMultiplayer) return new InGame(this);
        if (GameManager.Instance.GameState == GameState.InGameSingleplayer) return new InGame(this);
        return null;
    }

    class InMainMenu : IState
    {
        public InMainMenu(UIManager o) => this.o = o; readonly UIManager o;

        public void Enter()
        {
            var isLobbyListOpened =
                GameManager.Instance.PreviousGameState == GameState.InLobby ||
                GameManager.Instance.PreviousGameState == GameState.CreatingLobby;

            if (isLobbyListOpened) o.Menu.LobbyList.Close(UIWindow.Move.Up);

            o.menu.Open();
            GameManager.Instance.GameState = GameState.MainMenu;
        }
        public void Execute()
        {
            if (GameManager.Instance.GameState == GameState.SelectingMage) o.state.ChangeState(new InMageSelection(o));
            else
            if (GameManager.Instance.GameState == GameState.BrowsingLobbies) o.state.ChangeState(new InBrowsingLobbies(o));
            else
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
        public void Exit() { o.menu.Close(UIWindow.Move.Up); }
    }

    class InMageSelection : IState
    {
        public InMageSelection(UIManager o) => this.o = o; readonly UIManager o;

        public void Enter()
        {
            o.mageSelection.Open();
            GameManager.Instance.GameState = GameState.SelectingMage;
        }
        public void Execute()
        {
            if (GameManager.Instance.PreviousGameState == GameState.MainMenu) o.state.ChangeState(new InMainMenu(o));
            else
            if (GameManager.Instance.PreviousGameState == GameState.InLobby) o.state.ChangeState(new InLobby(o));
        }
        public void Exit() { if (o.mageSelection != null) o.mageSelection.Close(UIWindow.Move.Up); }
    }

    class InBrowsingLobbies : IState
    {
        public InBrowsingLobbies(UIManager o) => this.o = o; readonly UIManager o;

        public void Enter()
        {
            o.lobbyList.Open();
            GameManager.Instance.GameState = GameState.BrowsingLobbies;
        }
        public void Execute() { o.state.ChangeState(new InMainMenu(o)); }
        public void Exit() { o.lobbyList.Close(UIWindow.Move.Down); }
    }

    class InLobby : IState
    {
        public InLobby(UIManager o) => this.o = o; readonly UIManager o;

        public void Enter()
        {
            o.lobby.Open();
            GameManager.Instance.GameState = GameState.InLobby;
        }
        public void Execute() { o.state.ChangeState(new InBrowsingLobbies(o)); }
        public void Exit() { if (o.lobby != null) o.lobby.Close(UIWindow.Move.Up); }
    }

    class InLobbyCreation : IState
    {
        public InLobbyCreation(UIManager o) => this.o = o; readonly UIManager o;

        public void Enter()
        {
            o.lobbyCreationWindow.Open();

            GameManager.Instance.GameState = GameState.CreatingLobby;
        }
        public void Execute() { o.state.ChangeState(new InBrowsingLobbies(o)); }
        public void Exit()
        {
            if (GameManager.Instance.PreviousGameState == GameState.BrowsingLobbies)
                o.lobbyCreationWindow.Close(UIWindow.Move.Up);
            else
                o.lobbyCreationWindow.Close(UIWindow.Move.Down);
        }
    }

    class InGame : IState
    {
        public InGame(UIManager o) => this.o = o; readonly UIManager o;

        public void Enter() { }
        public void Execute() { DialogWindowManager.Instance.Show("Back to menu?", () => { o.ReturnToMenu?.Invoke(null, null); }); }
        public void Exit() { }
    }
}
