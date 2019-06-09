using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Game.Utility;
using Game.Managers;
using Game.Enums;

namespace Game.UI
{
    public class BackButtonUI : SingletonDDOL<BackButtonUI>
    {
        public event Action ReturnToMenu;
        public event Action Clicked;

        public GameObject ThisWindow;
        public GameObject PreviousWindow;
        public string PreviousSceneName;

        TextMeshProUGUI buttonText;
        StateMachine state = new StateMachine();

        protected override void Awake()
        {
            base.Awake();

            buttonText = GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = "Back";

            GetComponentInChildren<Button>().onClick.AddListener(OnClicked);

            void OnClicked() => state.Update();
        }

        void Start()
        {
            UIManager.Instance.StateChanged += OnStateChanged;
            state.ChangeState(new MainMenu(this));

            void OnStateChanged(UIState uiState)
            {
                Debug.Log(uiState);

                if (uiState == UIState.MainMenu)
                {
                    state.ChangeState(new MainMenu(this));
                }

                if (uiState == UIState.InLobby)
                {
                    state.ChangeState(new Lobby(this));
                }

                if (uiState == UIState.SelectingMage)
                {
                    state.ChangeState(new MageSelection(this));
                }

                if (uiState == UIState.BrowsingLobbies)
                {
                    state.ChangeState(new BrowsingLobbies(this));
                }
            }
        }

        class MainMenu : IState
        {
            readonly BackButtonUI o;
            public MainMenu(BackButtonUI o) => this.o = o;

            public void Enter() { UIManager.Instance.Menu.Open(); }
            public void Execute()
            {
                if (UIManager.Instance.State == UIState.SelectingMage)
                {
                    o.state.ChangeState(new MageSelection(o));
                }
                else if (UIManager.Instance.State == UIState.BrowsingLobbies)
                {
                    o.state.ChangeState(new BrowsingLobbies(o));
                }
                else
                {
                    Application.Quit();
                }
            }
            public void Exit() { UIManager.Instance.Menu.Close(UIWindow.Move.Up); }
        }

        class MageSelection : IState
        {
            readonly BackButtonUI o;
            public MageSelection(BackButtonUI o) => this.o = o;

            public void Enter() { UIManager.Instance.MageSelectionUI.Open(); }
            public void Execute()
            {
                if (UIManager.Instance.State == UIState.InLobby)
                {
                    o.state.ChangeState(new Lobby(o));
                }
                else
                {
                    o.state.ChangeState(new MainMenu(o));
                }
            }
            public void Exit()
            {
                if (UIManager.Instance.MageSelectionUI != null)
                {
                    UIManager.Instance.MageSelectionUI.Close(UIWindow.Move.Up);
                }
            }
        }

        class BrowsingLobbies : IState
        {
            readonly BackButtonUI o;
            public BrowsingLobbies(BackButtonUI o) => this.o = o;

            public void Enter() { UIManager.Instance.LobbyListUI.Open(); }
            public void Execute() { o.state.ChangeState(new MainMenu(o)); }
            public void Exit() { UIManager.Instance.LobbyListUI.Close(UIWindow.Move.Down); }
        }

        class Lobby : IState
        {
            readonly BackButtonUI o;
            public Lobby(BackButtonUI o) => this.o = o;

            public void Enter() { UIManager.Instance.LobbyUI.Open(); }
            public void Execute() { o.state.ChangeState(new BrowsingLobbies(o)); }
            public void Exit()
            {
                if (UIManager.Instance.LobbyUI != null)
                {
                    UIManager.Instance.LobbyUI.Close(UIWindow.Move.Up);
                }
            }
        }

        class InGame : IState
        {
            readonly BackButtonUI o;
            public InGame(BackButtonUI o) => this.o = o;

            public void Enter() { }
            public void Execute()
            {
                DialogWindowManager.Instance.Show("Back to menu?", () =>
                {
                    o.ReturnToMenu?.Invoke();
                });
            }
            public void Exit() { o.state.ChangeState(new MainMenu(o)); }
        }
    }
}