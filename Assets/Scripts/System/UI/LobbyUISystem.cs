using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facepunch.Steamworks;
using FPClient = Facepunch.Steamworks.Client;
using Mirror;
using TMPro;
using System;
using Game;
using DG.Tweening;
using Game.Utility;
using Game.Systems;
using Game.Consts;
using Game.Managers;
using Game.Enums;
using Game.Data.Mage;

namespace Game.UI
{
    public class LobbyUISystem : UIWindow
    {
        public event Action GameStarted, ChangeMageClicked;
        public TextMeshProUGUI MaxPlayersText;
        public Slider PlayersSlider;
        public TMP_Dropdown ModeDropdown, VisibilityDropdown, DifficultyDropdown, MapDropdown, WavesDropdown;
        public TMP_InputField ChatInputField;
        public GameObject PlayerTextPrefab, ChatTextPrefab, PlayerTextGroup, ChatTextGroup, LobbyList;
        public Button StartServerButton;
        public TextMeshProUGUI LobbyName;

        Dictionary<ulong, LobbyPlayerUI> playerTexts = new Dictionary<ulong, LobbyPlayerUI>();
        ObjectPool chatTextsPool;
        LobbyDataChanger lobbyDataChanger;
        bool isChangingMage = false;

        void Start()
        {
            defaultYs = new float[] { transform.GetChild(0).localPosition.y };
            chatTextsPool = new ObjectPool(ChatTextPrefab, ChatTextGroup.transform, 5);

            StartServerButton.onClick.AddListener(LobbyExt.StartLobbyServer);
            UIManager.Instance.MageSelected += OnMageSelected;
        }

        void OnMageSelected(MageData e)
        {
            if (GameManager.Instance.GameState != GameState.InLobby) return;

            LobbyExt.SetMemberData(LobbyData.MageID, $"{e.Index}");
            isChangingMage = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
                LobbyExt.SendChatMessage(ChatInputField);
        }

        void OnDestroy()
        {
            if (lobbyDataChanger != null)
            {
                lobbyDataChanger.Destroy();
                lobbyDataChanger = null;
            }
        }

        public override void Open(float timeToComplete = NumberConsts.UIAnimSpeed)
        {
            if (isChangingMage) return;

            LobbyExt.SetCallbacks(
                new LobbyCallbacks(
                    LobbyStateChanged,
                    LobbyMemberDataUpdated,
                    LobbyDataUpdated,
                    ChatMessageReceived
                ));

            lobbyDataChanger = new LobbyDataChanger(this);
            LobbyName.text = FPClient.Instance.Lobby.Name;

            UpdatePlayers();
            UpdateLobbyData();

            base.Open(timeToComplete);

            #region Helper functions

            void UpdatePlayers()
            {
                var playerIDs = FPClient.Instance.Lobby.GetMemberIDs();
                for (int i = 0; i < playerIDs.Length; i++) AddPlayer(playerIDs[i]);
            }

            void UpdateLobbyData()
            {
                if (!FPClient.Instance.Lobby.IsOwner)
                    StartServerButton.gameObject.SetActive(false);
                else
                {
                    LobbyExt.SetData(LobbyData.GameStarted, LobbyData.No);
                    LobbyExt.SetData(LobbyData.GameStarting, LobbyData.No);
                    StartServerButton.gameObject.SetActive(true);
                }
            }

            #endregion
        }

        public override void Close(Move moveTo, float timeToComplete = NumberConsts.UIAnimSpeed)
        {
            if (isChangingMage) return;

            base.Close(moveTo);

            foreach (var playerText in playerTexts.Values) Destroy(playerText.gameObject);

            chatTextsPool.DeactivateAll();
            playerTexts.Clear();

            LobbyExt.ClearLobbyCallbacks();
            FPClient.Instance.Lobby.Leave();

            lobbyDataChanger.Destroy();
            lobbyDataChanger = null;
        }

        void ChatMessageReceived(ulong senderID, string message) => CreateChatMessage(message);
        void CreateChatMessage(string message) => chatTextsPool.PopObject().GetComponent<TMP_InputField>().text = message;

        void LobbyDataUpdated()
        {
            if (!FPClient.Instance.Lobby.IsOwner)
            {
                WavesDropdown.value = int.Parse(LobbyExt.GetData(LobbyData.Waves));
                ModeDropdown.value = int.Parse(LobbyExt.GetData(LobbyData.Mode));
                MapDropdown.value = int.Parse(LobbyExt.GetData(LobbyData.Map));
                DifficultyDropdown.value = int.Parse(LobbyExt.GetData(LobbyData.Difficulty));
                PlayersSlider.value = int.Parse(LobbyExt.GetData(LobbyData.MaxPlayers));
                VisibilityDropdown.value = int.Parse(LobbyExt.GetData(LobbyData.Visibility));
            }

            var networkManager = NetworkManager.singleton as ExtendedNetworkManager;

            if (LobbyExt.GetData(LobbyData.GameStarting) == LobbyData.Yes)
                if (!NetworkClient.isConnected && !NetworkServer.active)
                {
                    networkManager.networkAddress = FPClient.Instance.Lobby.Owner.ToString();
                    networkManager.onlineScene = LobbyExt.GetData(LobbyData.Map);
                }

            if (LobbyExt.GetData(LobbyData.GameStarted) == LobbyData.Yes)
            {
                if (!FPClient.Instance.Lobby.IsOwner)
                {
                    GameStarted?.Invoke();
                    networkManager.StartClient();
                }

                LobbyExt.ClearLobbyCallbacks();
            }
        }

        void LobbyMemberDataUpdated(ulong steamID)
        {
            playerTexts[steamID]
                .SetName(FPClient.Instance.Friends.GetName(steamID))
                .SetLevel($"Lv.{LobbyExt.GetMemberData(steamID, LobbyData.Level)}")
                .SetReady(LobbyExt.GetMemberData(steamID, LobbyData.Ready) == LobbyData.Yes ?
                    $"<color=green>{LocaleKeys.ReadyYes.GetLocalized()}</color>" :
                    $"<color=red>{LocaleKeys.ReadyNo.GetLocalized()}</color>")
                .SetAvatar(steamID);

            if (FPClient.Instance.Lobby.IsOwner)
            {
                var playerIDs = FPClient.Instance.Lobby.GetMemberIDs();

                for (int i = 0; i < playerIDs.Length; i++)
                    if (LobbyExt.GetMemberData(playerIDs[i], LobbyData.Ready) == LobbyData.No)
                    {
                        StartServerButton.interactable = false;
                        return;
                    }

                StartServerButton.interactable = true;
            }
        }

        void LobbyStateChanged(Lobby.MemberStateChange stateChange, ulong initiatorID, ulong affectedID)
        {
            var initiatorName = FPClient.Instance.Friends.GetName(initiatorID);
            var affectedName = FPClient.Instance.Friends.GetName(affectedID);

            if (stateChange == Lobby.MemberStateChange.Entered)
            {
                LobbyExt.SendChatMessage($"{initiatorName} has joined.");
                AddPlayer(initiatorID);
            }
            else
            {
                if (stateChange == Lobby.MemberStateChange.Left) LobbyExt.SendChatMessage($"{initiatorName} has left");
                else
                if (stateChange == Lobby.MemberStateChange.Disconnected) LobbyExt.SendChatMessage($"{initiatorName} has disconnected");
                else
                if (stateChange == Lobby.MemberStateChange.Kicked) LobbyExt.SendChatMessage($"{initiatorName} has been kicked");
                else
                if (stateChange == Lobby.MemberStateChange.Banned) LobbyExt.SendChatMessage($"{initiatorName} has been banned");

                Destroy(playerTexts[initiatorID].gameObject);
                playerTexts.Remove(initiatorID);
            }
        }

        void AddPlayer(ulong steamID)
        {
            if (playerTexts.ContainsKey(steamID)) return;

            playerTexts.Add(steamID, Instantiate(PlayerTextPrefab, PlayerTextGroup.transform).GetComponent<LobbyPlayerUI>());

            playerTexts[steamID]
                .SetName(FPClient.Instance.Friends.GetName(steamID))
                .SetLevel($"Lv.{LobbyExt.GetMemberData(steamID, LobbyData.Level)}")
                .SetReady(LobbyExt.GetMemberData(steamID, LobbyData.Ready) == LobbyData.Yes ?
                    $"<color=green>{LocaleKeys.ReadyYes.GetLocalized()}</color>" :
                    $"<color=red>{LocaleKeys.ReadyNo.GetLocalized()}</color>")
                .SetAvatar(steamID);

            if (steamID == FPClient.Instance.SteamId)
            {
                playerTexts[steamID].ReadyClicked += OnReadyClicked;
                playerTexts[steamID].ChangeMageClicked += OnChangeMageClicked;
                playerTexts[steamID].Set();
            }

            LobbyMemberDataUpdated(steamID);
        }

        void OnChangeMageClicked()
        {
            isChangingMage = true;
            ChangeMageClicked?.Invoke();
        }

        void OnReadyClicked()
        {
            LobbyExt.SetMemberData(LobbyData.Ready,
                LobbyExt.GetMemberData(FPClient.Instance.SteamId, LobbyData.Ready) == LobbyData.Yes ? LobbyData.No : LobbyData.Yes);
        }
    }
}