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
                StateChanged?.Invoke(value);
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

      
        BuildUISystem buildUISystem;
        public BuildUISystem BuildUISystem
        {
            get => buildUISystem;
            set
            {
                if (buildUISystem != null)
                {
                    return;
                }

                buildUISystem = value;
            }
        }

        LobbyListUISystem lobbyList;
        public LobbyListUISystem LobbyListUI { get => lobbyList; set => lobbyList = value; }

        MageSelectionUISystem mageSelection;
        public MageSelectionUISystem MageSelectionUI { get => mageSelection; set => mageSelection = value; }

        LobbyUISystem lobby;
        public LobbyUISystem LobbyUI { get => lobby; set => lobby = value; }

        void SetSystem()
        {
            LobbyListUI = Menu.LobbyList;
            LobbyUI = LobbyListUI.LobbyUI;
            MageSelectionUI = Menu.MageSelection;

            Menu.StartMultiplayer += OnStartMultiplayer;
            Menu.StartSingleplayer += OnStartSingleplayer;
            LobbyListUI.CreatedLobby += OnLobbyCreated;
            LobbyListUI.JoinedLobby += OnLobbyJoined;
            LobbyUI.GameStarted += OnGameStarted;
            LobbyUI.ChangeMageClicked += OnLobbyMageChange;
            MageSelectionUI.MageSelected += OnMageSelected;

            State = UIState.MainMenu;

            void OnLobbyCreated()
            {
                State = UIState.InLobby;
            }

            void OnLobbyJoined()
            {
                State = UIState.InLobby;
            }

            void OnLobbyMageChange()
            {
                State = UIState.SelectingMage;
            }

            void OnStartSingleplayer()
            {
                State = UIState.SelectingMage;
            }

            void OnStartMultiplayer()
            {
                State = UIState.BrowsingLobbies;
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
            }
        }
    }
}