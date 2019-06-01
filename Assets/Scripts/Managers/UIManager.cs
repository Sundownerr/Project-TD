﻿using System;
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
        public event Action ReturnToMenu, GameStarted;
        public event Action<MageData> MageSelected;

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

                void OnBackButtonClicked() { state.Update(); }
            }
        }

        protected StateMachine state = new StateMachine();
        protected LobbyListUISystem lobbyList;
        protected LobbyUISystem lobby;
        protected MageSelectionUISystem mageSelection;

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

            GameManager.Instance.GameState = GameState.MainMenu;
            state.ChangeState(new InMainMenu(this));

            void OnLobbyCreated()
            {
                GameManager.Instance.GameState = GameState.InLobby;
                state.ChangeState(new InLobby(this));
            }

            void OnLobbyJoined()
            {
                GameManager.Instance.GameState = GameState.InLobby;
                state.ChangeState(new InLobby(this));
            }

            void OnLobbyMageChange()
            {
                GameManager.Instance.GameState = GameState.SelectingMage;
                state.ChangeState(new InMageSelection(this));
            }

            void OnStartSingleplayer()
            {
                GameManager.Instance.GameState = GameState.SelectingMage;
                state.ChangeState(new InMageSelection(this));
            }

            void OnStartMultiplayer()
            {
                GameManager.Instance.GameState = GameState.BrowsingLobbies;
                state.ChangeState(new InBrowsingLobbies(this));
            }

            void OnMageSelected(MageData e)
            {
                if (GameManager.Instance.PreviousGameState == GameState.MainMenu)
                {
                    OnGameStarted();
                }
                else if (GameManager.Instance.PreviousGameState == GameState.InLobby)
                {
                    OnLobbyJoined();
                }

                MageSelected?.Invoke(e);
            }

            void OnGameStarted()
            {
                GameManager.Instance.GameState = GameState.LoadingGame;
                GameStarted?.Invoke();

                state.ChangeState(new InGame(this));
            }
        }

        class InMainMenu : IState
        {
            readonly UIManager o;
            public InMainMenu(UIManager o) => this.o = o;

            public void Enter() { o.menu.Open(); }
            public void Execute()
            {
                if (GameManager.Instance.GameState == GameState.SelectingMage)
                    o.state.ChangeState(new InMageSelection(o));
                else
                if (GameManager.Instance.GameState == GameState.BrowsingLobbies)
                    o.state.ChangeState(new InBrowsingLobbies(o));
                else
                    Application.Quit();
            }
            public void Exit() { o.menu.Close(UIWindow.Move.Up); }
        }

        class InMageSelection : IState
        {
            readonly UIManager o;
            public InMageSelection(UIManager o) => this.o = o;

            public void Enter() { o.mageSelection.Open(); }
            public void Execute()
            {
                if (GameManager.Instance.PreviousGameState == GameState.MainMenu)
                    o.state.ChangeState(new InMainMenu(o));
                else
                if (GameManager.Instance.PreviousGameState == GameState.InLobby)
                    o.state.ChangeState(new InLobby(o));
            }
            public void Exit() { if (o.mageSelection != null) o.mageSelection.Close(UIWindow.Move.Up); }
        }

        class InBrowsingLobbies : IState
        {
            readonly UIManager o;
            public InBrowsingLobbies(UIManager o) => this.o = o;

            public void Enter() { o.lobbyList.Open(); }
            public void Execute() { o.state.ChangeState(new InMainMenu(o)); }
            public void Exit() { o.lobbyList.Close(UIWindow.Move.Down); }
        }

        class InLobby : IState
        {
            readonly UIManager o;
            public InLobby(UIManager o) => this.o = o;

            public void Enter() { o.lobby.Open(); }
            public void Execute() { o.state.ChangeState(new InBrowsingLobbies(o)); }
            public void Exit() { if (o.lobby != null) o.lobby.Close(UIWindow.Move.Up); }
        }

        class InGame : IState
        {
            readonly UIManager o;
            public InGame(UIManager o) => this.o = o;

            public void Enter() { }
            public void Execute() { DialogWindowManager.Instance.Show("Back to menu?", () => { o.ReturnToMenu?.Invoke(); }); }
            public void Exit() { }
        }
    }
}