using UnityEngine;
using UnityEngine.UI;
using Facepunch.Steamworks;
using System.Text;
using Mirror;
using FPClient = Facepunch.Steamworks.Client;
using TMPro;
using System;
using Game;
using DG.Tweening;

namespace Game.Systems
{
    public class LobbyListUISystem : UIWindow
    {
        public event EventHandler JoinedLobby, CreatedLobby;
        public GameObject LobbyButtonPrefab, LobbyInfoTextPrefab, NetworkManagerPrefab, LobbyListGroup, LobbyInfoGroup;
        public LobbyUISystem LobbyUI;
        public Button RefreshButton, CreateLobbyButton, JoinLobbyButton;

        LobbyList.Lobby selectedLobby;
        ObjectPool lobbyButtonsPool, lobbyInfoTextPool;

        void Start()
        {
            defaultYs = new float[] { transform.GetChild(0).localPosition.y, transform.GetChild(1).localPosition.y };

            lobbyButtonsPool = new ObjectPool(LobbyButtonPrefab, LobbyListGroup.transform, 20);
            lobbyInfoTextPool = new ObjectPool(LobbyInfoTextPrefab, LobbyInfoGroup.transform, 10);

            RefreshButton.onClick.AddListener(UpdateLobbyList);
            CreateLobbyButton.onClick.AddListener(() => FPClient.Instance.Lobby.Create(Lobby.Type.Public, 2));
            JoinLobbyButton.onClick.AddListener(JoinLobby);
        }

        public override void Open(float timeToComplete = NumberConsts.UIAnimSpeed)
        {
            if (NetworkManager.singleton == null)
            {
                var prefab = Instantiate(NetworkManagerPrefab);
                NetworkManager.singleton = prefab.GetComponent<ExtendedNetworkManager>();
            }

            FPClient.Instance.LobbyList.OnLobbiesUpdated = LobbiesUpdated;
            FPClient.Instance.Lobby.OnLobbyJoined = LobbyJoined;
            FPClient.Instance.Lobby.OnLobbyCreated = LobbyCreated;

            base.Open(timeToComplete);
        }

        void LobbyCreated(bool isSuccesful)
        {
            if (isSuccesful)
            {
                LobbyExt.SetLobbyDefaultData();
                CreatedLobby?.Invoke(null, null);
            }
        }

        void LobbyJoined(bool isSuccesful)
        {
            if (isSuccesful)
            {
                lobbyInfoTextPool.DeactivateAll();
                lobbyButtonsPool.DeactivateAll();

                selectedLobby = null;
                JoinedLobby?.Invoke(null, null);
            }
        }

        void JoinLobby() { if (selectedLobby != null) FPClient.Instance.Lobby.Join(selectedLobby.LobbyID); }

        void LobbiesUpdated()
        {
            var lobbies = FPClient.Instance.LobbyList.Lobbies;
            var sb = new StringBuilder();

            for (int i = 0; i < lobbies.Count; i++)
            {
                sb.Append($"{i}.   ")
                    .Append($"{lobbies[i]?.Name}  ")
                    .Append($"{lobbies[i]?.NumMembers} / ")
                    .Append(lobbies[i]?.MemberLimit);

                var lobbyButton = lobbyButtonsPool.PopObject().GetComponent<LobbyButtonUISystem>();

                lobbyButton.Lobby = lobbies[i];
                lobbyButton.Label.text = sb.ToString();
                lobbyButton.Clicked += OnLobbyButtonClicked;
                sb.Clear();
            }
        }

        void OnLobbyButtonClicked(object _, LobbyList.Lobby lobby)
        {
            if (lobby == null) return;

            selectedLobby = lobby;
            lobbyInfoTextPool.DeactivateAll();

            foreach (var pair in lobby.GetAllData())
                lobbyInfoTextPool.PopObject().GetComponent<TextMeshProUGUI>().text = $"{pair.Key} {pair.Value}";
        }

        void UpdateLobbyList()
        {
            var lobbyFilter = new LobbyList.Filter
            {
                DistanceFilter = LobbyList.Filter.Distance.Worldwide
            };

            FPClient.Instance.LobbyList.Refresh(lobbyFilter);
            lobbyButtonsPool.DeactivateAll();
        }
    }
}