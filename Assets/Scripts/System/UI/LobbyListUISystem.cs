using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Mirror;
using TMPro;
using System;
using Game;
using DG.Tweening;
using Game.Utility;
using Game.Consts;
using Game.Managers;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;

namespace Game.UI.Lobbies
{
    public class LobbyListUISystem : UIWindow
    {
        public event Action JoinedLobby, CreatedLobby;
        public GameObject LobbyButtonPrefab, LobbyInfoTextPrefab, NetworkManagerPrefab, LobbyListGroup, LobbyInfoGroup;
        public LobbyUISystem LobbyUI;
        public Button RefreshButton, CreateLobbyButton, JoinLobbyButton;

        Lobby? selectedLobby;
        ObjectPool lobbyButtonsPool, lobbyInfoTextPool;

        void Start()
        {
            defaultYs = new float[] { transform.GetChild(0).localPosition.y, transform.GetChild(1).localPosition.y };

            lobbyButtonsPool = new ObjectPool(LobbyButtonPrefab, LobbyListGroup.transform, 20);
            lobbyInfoTextPool = new ObjectPool(LobbyInfoTextPrefab, LobbyInfoGroup.transform, 10);

            RefreshButton.onClick.AddListener(OnRefreshButtonClick);
            CreateLobbyButton.onClick.AddListener(OnCreateLobbyClick);
            JoinLobbyButton.onClick.AddListener(OnJoinLobbyClick);

            async void OnCreateLobbyClick()
            {
                var lobbyTask = await SteamMatchmaking.CreateLobbyAsync(5);

                if (lobbyTask.HasValue)
                {
                    var lobby = lobbyTask.Value;

                    if (lobby.SetPublic() && lobby.SetJoinable(true))
                    {
                        Debug.Log("Succesfully created lobby");
                        lobby.Owner = new Friend(SteamClient.SteamId);

                        lobby.SetDefaultData();
                        // CreatedLobby?.Invoke();

                        await JoinLobby(lobby);
                    }
                }
            }

            async Task JoinLobby(Lobby? lobby)
            {
                var joinTask = await lobby.Value.Join();

                if (joinTask == RoomEnter.Success)
                {
                    Debug.Log("succesfully joined lobby");

                    lobbyInfoTextPool.DeactivateAll();
                    lobbyButtonsPool.DeactivateAll();


                    selectedLobby = null;

                    GameData.Instance.CurrentLobby = lobby.Value;

                    JoinedLobby?.Invoke();
                }
                else
                {
                    Debug.LogError($"error when joining lobby: {joinTask}");
                }
            }

            async void OnJoinLobbyClick()
            {
                if (selectedLobby.HasValue)
                {
                    await JoinLobby(selectedLobby);
                }
            }

            void OnRefreshButtonClick()
            {
                lobbyButtonsPool.DeactivateAll();
                UpdateLobbyList();
            }
        }

        public override void Open(float timeToComplete = NumberConsts.UIAnimSpeed)
        {
            if (NetworkManager.singleton == null)
            {
                var prefab = Instantiate(NetworkManagerPrefab);
                NetworkManager.singleton = prefab.GetComponent<ExtendedNetworkManager>();
            }

            UpdateLobbyList();

            base.Open(timeToComplete);
        }

        async void UpdateLobbyList()
        {
            var lobbyList = await SteamMatchmaking.LobbyList.RequestAsync();
            var sb = new StringBuilder();
            var index = 1;

            foreach (var lobby in lobbyList)
            {
                var name = lobby.GetData("name");

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                sb.Append($"{index}.   ")
                    .Append($"{name}  ")
                    .Append($"{lobby.MemberCount} / ")
                    .Append(lobby.MaxMembers);

                var lobbyButton = lobbyButtonsPool.PopObject().GetComponent<LobbyButtonUISystem>();

                lobbyButton.Lobby = lobby;
                lobbyButton.Label.text = sb.ToString();
                lobbyButton.Clicked += OnLobbyButtonClicked;
                sb.Clear();
                index++;
            }

            void OnLobbyButtonClicked(Lobby lobby)
            {
                selectedLobby = lobby;
                lobbyInfoTextPool.DeactivateAll();
                var lobbyData = lobby.Data;

                foreach (var data in lobbyData)
                {
                    lobbyInfoTextPool.PopObject().GetComponent<TextMeshProUGUI>().text = $"{data.Key} {data.Value}";
                }
            }
        }
    }
}