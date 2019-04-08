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

public class LobbyListUISystem : UIWindow
{

    public GameObject LobbyButtonPrefab, LobbyInfoTextPrefab, NetworkManagerPrefab, LobbyListGroup, LobbyInfoGroup;
    public LobbyUISystem LobbyUI;
    public LobbyCreationWindowUISystem LobbyCreationWindow;
    public Button RefreshButton, CreateLobbyButton, JoinLobbyButton;

    LobbyList.Lobby selectedLobby;
    ObjectPool lobbyButtonsPool, lobbyInfoTextPool;

    void Start()
    {
        defaultYs[0] = transform.GetChild(0).localPosition.y;
        defaultYs[1] = transform.GetChild(1).localPosition.y;
       
        lobbyButtonsPool = new ObjectPool(LobbyButtonPrefab, LobbyListGroup.transform, 20);
        lobbyInfoTextPool = new ObjectPool(LobbyInfoTextPrefab, LobbyInfoGroup.transform, 10);

        RefreshButton.onClick.AddListener(UpdateLobbyList);
        CreateLobbyButton.onClick.AddListener(() => LobbyCreationWindow.Open());
        JoinLobbyButton.onClick.AddListener(JoinLobby);
    }

    public override void Open()
    {
        if (NetworkManager.singleton == null)
        {
            var prefab = Instantiate(NetworkManagerPrefab);
            NetworkManager.singleton = prefab.GetComponent<ExtendedNetworkManager>();
        }

        FPClient.Instance.LobbyList.OnLobbiesUpdated = LobbiesUpdated;
        FPClient.Instance.Lobby.OnLobbyJoined += LobbyJoined;
        FPClient.Instance.Lobby.OnLobbyCreated += LobbyJoined;

        GameManager.Instance.GameState = GameState.BrowsingLobbies;
        transform.GetChild(0).DOLocalMoveY(0, 0.5f).SetEase(Ease.InOutQuint);
        transform.GetChild(1).DOLocalMoveY(0, 0.5f).SetEase(Ease.InOutQuint);
    }

    public override void Close(Move moveTo)
    {
        transform.GetChild(0).DOLocalMoveY(moveTo == Move.Up ? defaultYs[0] : -defaultYs[0], 0.5f).SetEase(Ease.InOutQuint);
        transform.GetChild(1).DOLocalMoveY(moveTo == Move.Up ? defaultYs[0] : -defaultYs[0], 0.5f).SetEase(Ease.InOutQuint);
    }

    void LobbyJoined(bool isSuccesful)
    {
        if (isSuccesful)
        {
            lobbyInfoTextPool.DeactivateAll();
            lobbyButtonsPool.DeactivateAll();

            selectedLobby = null;
            LobbyUI.gameObject.SetActive(true);
            GameManager.Instance.GameState = GameState.InLobby;
        }
    }

    void JoinLobby()
    {
        if (selectedLobby != null)
            FPClient.Instance.Lobby.Join(selectedLobby.LobbyID);
    }

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
