using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Systems
{
    public class UIManager : MonoBehaviour
    {
        public event EventHandler ReturnToMenu, GameStarted;
        public event EventHandler<MageData> MageSelected;

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
                SetSystem();
            }
        }

        private BackButtonUI backButton;
        public BackButtonUI BackButton
        {
            get => backButton;
            set
            {
                backButton = value;
                backButton.Clicked += OnBackButtonClicked;
            }
        }


        protected StateMachine state = new StateMachine();
        protected LobbyListUISystem lobbyList;
        protected LobbyUISystem lobby;
        protected MageSelectionUISystem mageSelection;

        void Awake()
        {
            if (UIManager.Instance != null) { Destroy(gameObject); return; }

            DontDestroyOnLoad(this);
            Instance = this;
        }

        void SetSystem()
        {
            lobbyList = Menu.LobbyList;
            lobby = lobbyList.LobbyUI;
            mageSelection = Menu.MageSelection;

            Menu.StartMultiplayer += OnStartMultiplayer;
            Menu.StartSingleplayer += OnStartSingleplayer;
            lobbyList.CreatedLobby += OnLobbyCreated;
            lobbyList.JoinedLobby += OnLobbyJoined;
            lobby.GameStarted += OnGameStarted;
            lobby.ChangeMageClicked += OnLobbyMageChange;
            mageSelection.MageSelected += OnMageSelected;

            state.ChangeState(new InMainMenu(this));
        }

        void OnLobbyCreated(object _, EventArgs e) { state.ChangeState(new InLobby(this)); Debug.Log(1);}
        void OnLobbyJoined(object _, EventArgs e) { state.ChangeState(new InLobby(this)); Debug.Log(2);}
        void OnLobbyMageChange(object _, EventArgs e) { state.ChangeState(new InMageSelection(this)); }
        void OnStartSingleplayer(object _, EventArgs e) { state.ChangeState(new InMageSelection(this)); }
        void OnStartMultiplayer(object _, EventArgs e) { state.ChangeState(new InBrowsingLobbies(this)); }
        void OnBackButtonClicked(object _, EventArgs e) { state.Update(); }
        void OnMageSelected(object _, MageData e)
        {
            if (GameManager.Instance.PreviousGameState == GameState.MainMenu) state.ChangeState(new InGame(this));
            else
            if (GameManager.Instance.PreviousGameState == GameState.InLobby) state.ChangeState(new InLobby(this));

            MageSelected?.Invoke(null, e);
        }

        void OnGameStarted(object _, EventArgs e)
        {
            GameStarted?.Invoke(null, null);
            state.ChangeState(new InGame(this));
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
                    Application.Quit();
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
                if (GameManager.Instance.PreviousGameState == GameState.MainMenu) o.state.ChangeState(new InGame(o));
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
                Debug.Log("enter");
                o.lobby.Open();
                GameManager.Instance.GameState = GameState.InLobby;
            }
            public void Execute() { o.state.ChangeState(new InBrowsingLobbies(o)); }
            public void Exit() { if (o.lobby != null) o.lobby.Close(UIWindow.Move.Up); }
        }

        class InGame : IState
        {
            public InGame(UIManager o) => this.o = o; readonly UIManager o;

            public void Enter() { GameManager.Instance.GameState = GameState.LoadingGame; o.GameStarted?.Invoke(null, null); }
            public void Execute() { DialogWindowManager.Instance.Show("Back to menu?", () => { o.ReturnToMenu?.Invoke(null, null); }); }
            public void Exit() { }
        }
    }
}