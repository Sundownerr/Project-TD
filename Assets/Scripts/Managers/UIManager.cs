using System;
using System.Collections;
using System.Collections.Generic;
using Game.Data.Mage;
using Game.Enums;
using Game.UI;
using Game.UI.Lobbies;
using Game.Utility;
using UnityEngine;

namespace Game.Managers
{
    public class UIManager : SingletonDDOL<UIManager>
    {
        public event Action ReturnToMenu;
        public event Action GameStarted;
        public event Action<UIState> StateChanged;
        public event Action<MageData> MageSelected;

        public UIState PreviousState { get; private set; }

        UIState state;
        public UIState State
        {
            get => state;
            private set
            {
                PreviousState = state;
                state = value;
                StateChanged?.Invoke(state);
            }
        }

        MenuUISystem menu;
        public MenuUISystem Menu
        {
            get => menu;
            set
            {
                menu = value;
                SetSystem();
            }
        }

        BackButtonUI backButton;
        public BackButtonUI BackButton
        {
            get => backButton;
            set
            {
                backButton = value;
                backButton.Clicked += OnBackButtonClicked;

                void OnBackButtonClicked() { backButtonState.Update(); }
            }
        }

        StateMachine backButtonState = new StateMachine();
        LobbyListUISystem lobbyList;
        LobbyUISystem lobby;
        MageSelectionUISystem mageSelection;


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

            State = UIState.MainMenu;
            backButtonState.ChangeState(new MainMenu(this));

            void OnLobbyCreated()
            {
                State = UIState.InLobby;
                backButtonState.ChangeState(new Lobby(this));
            }

            void OnLobbyJoined()
            {
                State = UIState.InLobby;
                backButtonState.ChangeState(new Lobby(this));
            }

            void OnLobbyMageChange()
            {
                State = UIState.SelectingMage;
                backButtonState.ChangeState(new MageSelection(this));
            }

            void OnStartSingleplayer()
            {
                State = UIState.SelectingMage;
                backButtonState.ChangeState(new MageSelection(this));
            }

            void OnStartMultiplayer()
            {
                State = UIState.BrowsingLobbies;
                backButtonState.ChangeState(new BrowsingLobbies(this));
            }

            void OnMageSelected(MageData e)
            {
                if (PreviousState == UIState.MainMenu)
                {
                    OnGameStarted();
                }
                else if (PreviousState == UIState.InLobby)
                {
                    OnLobbyJoined();
                }

                MageSelected?.Invoke(e);
            }

            void OnGameStarted()
            {
                GameStarted?.Invoke();

                backButtonState.ChangeState(new InGame(this));
            }
        }

        class MainMenu : IState
        {
            readonly UIManager o;
            public MainMenu(UIManager o) => this.o = o;

            public void Enter() { o.menu.Open(); }
            public void Execute()
            {
                if (o.State == UIState.SelectingMage)
                {
                    o.backButtonState.ChangeState(new MageSelection(o));
                }
                else if (o.State == UIState.BrowsingLobbies)
                {
                    o.backButtonState.ChangeState(new BrowsingLobbies(o));
                }
                else
                {
                    Application.Quit();
                }
            }
            public void Exit() { o.menu.Close(UIWindow.Move.Up); }
        }

        class MageSelection : IState
        {
            readonly UIManager o;
            public MageSelection(UIManager o) => this.o = o;

            public void Enter() { o.mageSelection.Open(); }
            public void Execute()
            {
                if (o.State == UIState.InLobby)
                {
                    o.backButtonState.ChangeState(new Lobby(o));
                }
                else
                {
                    o.backButtonState.ChangeState(new MainMenu(o));
                }
            }
            public void Exit()
            {
                if (o.mageSelection != null)
                {
                    o.mageSelection.Close(UIWindow.Move.Up);
                }
            }
        }

        class BrowsingLobbies : IState
        {
            readonly UIManager o;
            public BrowsingLobbies(UIManager o) => this.o = o;

            public void Enter() { o.lobbyList.Open(); }
            public void Execute() { o.backButtonState.ChangeState(new MainMenu(o)); }
            public void Exit() { o.lobbyList.Close(UIWindow.Move.Down); }
        }

        class Lobby : IState
        {
            readonly UIManager o;
            public Lobby(UIManager o) => this.o = o;

            public void Enter() { o.lobby.Open(); }
            public void Execute() { o.backButtonState.ChangeState(new BrowsingLobbies(o)); }
            public void Exit()
            {
                if (o.lobby != null)
                {
                    o.lobby.Close(UIWindow.Move.Up);
                }
            }
        }

        class InGame : IState
        {
            readonly UIManager o;
            public InGame(UIManager o) => this.o = o;

            public void Enter() { }
            public void Execute() { DialogWindowManager.Instance.Show("Back to menu?", () => { o.ReturnToMenu?.Invoke(); }); }
            public void Exit() { o.backButtonState.ChangeState(new MainMenu(o));}
        }
    }
}