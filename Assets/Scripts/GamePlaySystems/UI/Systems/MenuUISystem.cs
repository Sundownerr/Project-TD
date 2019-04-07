using UnityEngine;
using UnityEngine.UI;
using Transport.Steamworks;
using UnityEngine.SceneManagement;
using Game.Systems;
using System.Collections;
using System;
using Game;

public class MenuUISystem : MonoBehaviour, IWindow
{
    public event EventHandler ClickedOnMultiplayer;
    public event EventHandler ClickedOnSingleplayer;
    public event EventHandler Activated;
    public event EventHandler<MageData> MageSelected;
    public LobbyListUISystem LobbyList;
    public GameObject GameStateManagerPrefab;
    public MageSelectionUISystem MageSelection;
    public Button SingleplayerButton, MultiplayerButton, TestButton, QuitButton;

    void Awake()
    {
        if (GameManager.Instance == null)
            Instantiate(GameStateManagerPrefab);
    }

    void Start()
    {
        SingleplayerButton.onClick.AddListener(StartSingleplayer);
        MultiplayerButton.onClick.AddListener(StartMultiplayer);
        QuitButton.onClick.AddListener(() => Application.Quit());

        Steam.Instance.Connected += OnSteamConnected;
        Steam.Instance.ConnectionLost += OnSteamConnectionLost;
        MageSelection.MageSelected += OnMageSelected;
        
        GameManager.Instance.Menu = this;
        Activated?.Invoke(null, null);
    }

    void OnSteamConnectionLost(object _, EventArgs e) => MultiplayerButton.interactable = false;
    void OnSteamConnected(object _, EventArgs e) => MultiplayerButton.interactable = true;

    void OnMageSelected(object sender, MageData e)
    {
        MageSelected?.Invoke(null, e);
        SceneManager.LoadSceneAsync("SingleplayerMap");
    }

    void StartSingleplayer()
    {
        MageSelection.gameObject.SetActive(true);
    }

    void StartMultiplayer()
    {
        LobbyList.gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        MageSelection.MageSelected -= OnMageSelected;
    }
}
