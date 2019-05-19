using UnityEngine;
using UnityEngine.UI;
using Transport.Steamworks;
using UnityEngine.SceneManagement;
using System;
using Game;
using DG.Tweening;
using Game.Systems;
using Game.Consts;
using Game.Managers;
using Game.UI.Lobbies;

namespace Game.UI
{
    public class MenuUISystem : UIWindow
    {
        public event Action Active, StartSingleplayer, StartMultiplayer;

        public LobbyListUISystem LobbyList;
        public MageSelectionUISystem MageSelection;
        public Button SingleplayerButton, MultiplayerButton;

        void Awake()
        {
            defaultYs = new float[] { transform.GetChild(0).localPosition.y };
        }

        void Start()
        {
            SingleplayerButton.onClick.AddListener(OnSingleplayerButtonClick);
            MultiplayerButton.onClick.AddListener(OnMultiplayerButtonClick);

            GameManager.Instance.SteamConnected += OnSteamConnected;
            GameManager.Instance.SteamLostConnection += OnSteamLostConnection;
            GameManager.Instance.Menu = this;

            void OnMultiplayerButtonClick() => StartMultiplayer?.Invoke();
            void OnSingleplayerButtonClick() => StartSingleplayer?.Invoke();
            void OnSteamLostConnection() => MultiplayerButton.interactable = false;
            void OnSteamConnected() => MultiplayerButton.interactable = true;
        }

        public override void Open(float timeToComplete = NumberConsts.UIAnimSpeed)
        {
            Active?.Invoke();
            base.Open();
        }

        void OnDestroy()
        {
            SingleplayerButton.onClick.RemoveAllListeners();
            MultiplayerButton.onClick.RemoveAllListeners();
        }
    }
}