using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Facepunch.Steamworks;
using System.Text;
using System.Net;
using Transport.Steamworks;
using Mirror;
using FPClient = Facepunch.Steamworks.Client;
using TMPro;

public class LobbyListUISystem : MonoBehaviour
{

    public GameObject LobbyGO, LobbyButtonPrefab, LobbyInfoTextPrefab, NetworkManagerPrefab, LobbyListGroup, LobbyInfoGroup, LobbyCreationWindow;
    public Button RefreshButton, CreateLobbyButton, JoinLobbyButton;

    private LobbyList.Lobby selectedLobby;
    private ObjectPool lobbyButtonsPool, lobbyInfoTextPool;

    private void Start()
    {
        if(NetworkManager.singleton == null)
        {
            var prefab = Instantiate(NetworkManagerPrefab);
            NetworkManager.singleton = prefab.GetComponent<ExtendedNetworkManager>();
        }

        lobbyButtonsPool = new ObjectPool(LobbyButtonPrefab, LobbyListGroup.transform, 20);
        lobbyInfoTextPool = new ObjectPool(LobbyInfoTextPrefab, LobbyInfoGroup.transform, 10);

        RefreshButton.onClick.AddListener(UpdateLobbyList);
        CreateLobbyButton.onClick.AddListener(() => LobbyCreationWindow.SetActive(true));
        JoinLobbyButton.onClick.AddListener(JoinLobby);

        FPClient.Instance.LobbyList.OnLobbiesUpdated = LobbiesUpdated;      
        FPClient.Instance.Lobby.OnLobbyJoined += LobbyJoined;
        FPClient.Instance.Lobby.OnLobbyCreated += LobbyJoined;
    }

    private void OnEnable()
    {
        GameManager.Instance.GameState = GameState.BrowsingLobbies;
        //FPClient.Instance.LobbyList.Refresh();
    }

    private void LobbyJoined(bool isSuccesful)
    {
        if (isSuccesful)
        {
            lobbyInfoTextPool.DeactivateAll();
            lobbyButtonsPool.DeactivateAll();

            selectedLobby = null;
            LobbyGO.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    private void JoinLobby()
    {
        if (selectedLobby != null)
            FPClient.Instance.Lobby.Join(selectedLobby.LobbyID);
    }

    private void LobbiesUpdated()
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

    private void OnLobbyButtonClicked(object _, LobbyList.Lobby lobby)
    {
        if (lobby == null)
            return;

        selectedLobby = lobby;

        lobbyInfoTextPool.DeactivateAll();

        foreach (var pair in lobby.GetAllData())       
            lobbyInfoTextPool.PopObject().GetComponent<TextMeshProUGUI>().text = $"{pair.Key} {pair.Value}";        
    }

    private void UpdateLobbyList()
    {
        var lobbyFilter = new LobbyList.Filter
        {
            DistanceFilter = LobbyList.Filter.Distance.Worldwide,     
        };

        FPClient.Instance.LobbyList.Refresh(lobbyFilter);
        lobbyButtonsPool.DeactivateAll();
    }
}
