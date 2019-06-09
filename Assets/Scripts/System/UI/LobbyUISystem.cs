using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
using Game.Utility.Localization;
using Steamworks.Data;
using Steamworks;
using System.Threading.Tasks;

namespace Game.UI.Lobbies
{
    public class LobbyUISystem : UIWindow
    {
        public event Action GameStarted;
        public event Action ChangeMageClicked;
        public TextMeshProUGUI MaxPlayersText;
        public Slider PlayersSlider;
        public TMP_Dropdown ModeDropdown;
        public TMP_Dropdown VisibilityDropdown;
        public TMP_Dropdown DifficultyDropdown;
        public TMP_Dropdown MapDropdown;
        public TMP_Dropdown WavesDropdown;
        public TMP_InputField ChatInputField;
        public GameObject PlayerTextPrefab;
        public GameObject ChatTextPrefab;
        public GameObject PlayerTextGroup;
        public GameObject ChatTextGroup;
        public GameObject LobbyList;
        public Button StartServerButton;
        public TextMeshProUGUI LobbyName;

        public Lobby Lobby => GameData.Instance.CurrentLobby;

        Dictionary<Friend, LobbyPlayerUI> playerTexts = new Dictionary<Friend, LobbyPlayerUI>();
        ObjectPool chatTextsPool;
        LobbyDataChanger lobbyDataChanger;
        bool isChangingMage = false;

        void Start()
        {
            defaultYs = new float[] { transform.GetChild(0).localPosition.y };
            chatTextsPool = new ObjectPool(ChatTextPrefab, ChatTextGroup.transform, 5);

            StartServerButton.onClick.AddListener(LobbyExt.StartLobbyServer);
            UIManager.Instance.MageSelected += OnMageSelected;

            void OnMageSelected(MageData e)
            {
                if (UIManager.Instance.State != UIState.InLobby)
                {
                    return;
                }

                Lobby.SetMemberData(Lobby.GetMeFromLobby(), LobbyData.MageID, $"{e.Index}");
                isChangingMage = false;
            }
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
            if (isChangingMage)
            {
                return;
            }

            SteamMatchmaking.OnChatMessage += (lobby, member, message) =>
               chatTextsPool.PopObject().GetComponent<TMP_InputField>().text = message;

            SteamMatchmaking.OnLobbyDataChanged += (lobby) =>
            {
                lobbyDataChanger.UpdateData();

                var networkManager = NetworkManager.singleton as ExtendedNetworkManager;

                if (Lobby.GetData(LobbyData.GameStarting) == LobbyData.Yes)
                {
                    if (!NetworkClient.isConnected && !NetworkServer.active)
                    {
                        networkManager.networkAddress = Lobby.Owner.Id.Value.ToString();
                        networkManager.onlineScene = Lobby.GetData(LobbyData.Map);
                    }
                }

                if (Lobby.GetData(LobbyData.GameStarted) == LobbyData.Yes)
                {
                    if (!Lobby.Owner.IsMe)
                    {
                        GameStarted?.Invoke();
                        networkManager.StartClient();
                    }
                }
            };

            SteamMatchmaking.OnLobbyMemberJoined += (lobby, member) =>
           {
               lobby.SendChatString($"{member.Name} has joined.");
               AddPlayer(member);
           };

            SteamMatchmaking.OnLobbyMemberLeave += (lobby, member) =>
            {
                lobby.SendChatString($"{member.Name} has left");

                Destroy(playerTexts[member].gameObject);
                playerTexts.Remove(member);
            };

           
            lobbyDataChanger = new LobbyDataChanger(this, Lobby);
            LobbyName.text = Lobby.GetData("name");

            UpdatePlayers();
            UpdateLobbyData();

            base.Open(timeToComplete);

            void UpdatePlayers()
            {
                var playerIDs = Lobby.Members;

                foreach (var id in playerIDs)
                {
                    AddPlayer(id);
                }
            }

            void UpdateLobbyData()
            {
                if (!Lobby.Owner.IsMe)
                {
                    StartServerButton.gameObject.SetActive(false);
                }
                else
                {
                    Lobby.SetData(LobbyData.GameStarted, LobbyData.No);
                    Lobby.SetData(LobbyData.GameStarting, LobbyData.No);
                    StartServerButton.gameObject.SetActive(true);
                }
            }
        }

        void AddPlayer(Friend member)
        {
            if (playerTexts.ContainsKey(member))
                return;

            playerTexts.Add(member, Instantiate(PlayerTextPrefab, PlayerTextGroup.transform).GetComponent<LobbyPlayerUI>());

            SetPlayerData(member);

            if (member.IsMe)
            {
                playerTexts[member].ReadyClicked += OnReadyClicked;
                playerTexts[member].ChangeMageClicked += OnChangeMageClicked;
                playerTexts[member].Set();
            }

            LobbyMemberDataUpdated(member);

            void OnChangeMageClicked()
            {
                isChangingMage = true;
                ChangeMageClicked?.Invoke();
            }

            void OnReadyClicked()
            {
                var myData = Lobby.GetMeFromLobby();

                Lobby.SetMemberData(myData, LobbyData.Ready,
                    Lobby.GetMemberData(myData, LobbyData.Ready) == LobbyData.Yes ? LobbyData.No : LobbyData.Yes);
            }
        }

        async void SetPlayerData(Friend member)
        {
            playerTexts[member]
                .SetName(member.Name)
                .SetLevel($"Lv.{Lobby.GetMemberData(member, LobbyData.Level)}")
                .SetReady(Lobby.GetMemberData(member, LobbyData.Ready) == LobbyData.Yes ?
                    $"<color=green>{LocaleKeys.ReadyYes.GetLocalized()}</color>" :
                    $"<color=red>{LocaleKeys.ReadyNo.GetLocalized()}</color>");

            await playerTexts[member].SetAvatar(member);
        }

        public override void Close(Move moveTo, float timeToComplete = NumberConsts.UIAnimSpeed)
        {
            if (isChangingMage)
            {
                return;
            }

            base.Close(moveTo);

            foreach (var playerText in playerTexts.Values)
            {
                Destroy(playerText.gameObject);
            }

            chatTextsPool.DeactivateAll();
            playerTexts.Clear();

            Lobby.Leave();

            lobbyDataChanger.Destroy();
            lobbyDataChanger = null;
        }

        void LobbyMemberDataUpdated(Friend member)
        {
            SetPlayerData(member);

            if (Lobby.Owner.IsMe)
            {
                var members = Lobby.Members;

                foreach (var mem in members)
                {
                    if (Lobby.GetMemberData(mem, LobbyData.Ready) == LobbyData.No)
                    {
                        StartServerButton.interactable = false;
                        return;
                    }
                }

                StartServerButton.interactable = true;
            }
        }
    }
}