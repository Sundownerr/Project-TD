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
            SingleplayerButton.onClick.AddListener(() => StartSingleplayer?.Invoke());
            MultiplayerButton.onClick.AddListener(() => StartMultiplayer?.Invoke());

            GameManager.Instance.SteamConnected += OnSteamConnected;
            GameManager.Instance.SteamLostConnection += OnSteamLostConnection;
            GameManager.Instance.Menu = this;  
        }

        public override void Open(float timeToComplete = NumberConsts.UIAnimSpeed)
        {
            Active?.Invoke();
            base.Open();
        }

        void OnSteamLostConnection() => MultiplayerButton.interactable = false;
        void OnSteamConnected() => MultiplayerButton.interactable = true;

        void OnDestroy()
        {
            SingleplayerButton.onClick.RemoveAllListeners();
            MultiplayerButton.onClick.RemoveAllListeners();
        }
    }
}