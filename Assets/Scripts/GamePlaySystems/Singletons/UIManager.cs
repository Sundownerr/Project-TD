using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
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
    }

    void OnBackButtonClicked(object _, EventArgs e) => state.Update();
    void OnGameStateChanged(object _, GameState e) => state.ChangeState(GetState()); 

    IState GetState()
    {
        if (GameManager.Instance.GameState == GameState.MainMenu) return new InMainMenu(this);
        if (GameManager.Instance.GameState == GameState.BrowsingLobbies) return new InBrowsingLobbies(this);
        if (GameManager.Instance.GameState == GameState.InLobby) return new InLobby(this);
        if (GameManager.Instance.GameState == GameState.SelectingMage) return new InMageSelection(this);
        if (GameManager.Instance.GameState == GameState.CreatingLobby) return new InLobbyCreation(this);
        return null;
    }

    class InMainMenu : IState
    {
        public InMainMenu(UIManager o) => this.o = o; readonly UIManager o;

        public void Enter()
        {
            var isLobbyListOpened = 
                GameManager.Instance.PreviousGameState == GameState.InLobby || 
                GameManager.Instance.PreviousGameState == GameState.MainMenu || 
                GameManager.Instance.PreviousGameState == GameState.CreatingLobby;

            if (isLobbyListOpened) o.Menu.LobbyList.Close();

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
        public void Exit() { }
    }

    class InMageSelection : IState
    {
        public InMageSelection(UIManager o) => this.o = o; readonly UIManager o;

        public void Enter()
        {
            GameManager.Instance.GameState = GameState.SelectingMage;
        }
        public void Execute()
        {
            if (GameManager.Instance.PreviousGameState == GameState.MainMenu) o.state.ChangeState(new InMainMenu(o));
            else
            if (GameManager.Instance.PreviousGameState == GameState.InLobby) o.state.ChangeState(new InLobby(o));
        }
        public void Exit() { o.mageSelection.Close(); }
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
        public void Exit() { }
    }

    class InLobby : IState
    {
        public InLobby(UIManager o) => this.o = o; readonly UIManager o;

        public void Enter()
        {
            o.lobby.Open();
            o.lobbyCreationWindow.Close();
            GameManager.Instance.GameState = GameState.InLobby;
        }
        public void Execute() { o.state.ChangeState(new InBrowsingLobbies(o)); }
        public void Exit() { o.lobby.Close(); }
    }

    class InLobbyCreation : IState
    {
        public InLobbyCreation(UIManager o) => this.o = o; readonly UIManager o;

        public void Enter()
        {
            o.lobbyCreationWindow.Open();
            o.lobbyList.Close();
            GameManager.Instance.GameState = GameState.CreatingLobby;
        }
        public void Execute() { o.state.ChangeState(new InBrowsingLobbies(o)); }
        public void Exit() { o.lobbyCreationWindow.Close(); }
    }
}
