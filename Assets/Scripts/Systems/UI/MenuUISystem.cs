using UnityEngine;
using UnityEngine.UI;
using Transport.Steamworks;
using UnityEngine.SceneManagement;
using Game.Systems;
using System.Collections;
using System;

public class MenuUISystem : MonoBehaviour
{
    public GameObject LobbyList, GameStateManagerPrefab;
    public Button SingleplayerButton, MultiplayerButton, TestButton, QuitButton;

    private void Awake()
    {
        if (GameManager.Instance == null)
            Instantiate(GameStateManagerPrefab);
    }

    private void Start()
    {
        SingleplayerButton.onClick.AddListener(StartSingleplayer);
        MultiplayerButton.onClick.AddListener(StartMultiplayer);
        QuitButton.onClick.AddListener(() => Application.Quit());

        GameManager.Instance.GameState = GameState.MainMenu;
        Steam.Instance.Connected += OnSteamConnected;
        Steam.Instance.ConnectionLost += OnSteamConnectionLost;
    }

    private void OnSteamConnectionLost(object _, EventArgs e) => MultiplayerButton.interactable = false;
    private void OnSteamConnected(object _, EventArgs e) => MultiplayerButton.interactable = true;

    private void StartSingleplayer()
    {
        SceneManager.LoadSceneAsync("SingleplayerMap");
    }

    private void StartMultiplayer()
    {
        gameObject.SetActive(false);
        LobbyList.SetActive(true);
    }
}
