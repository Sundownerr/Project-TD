using UnityEngine;
using UnityEngine.UI;
using Transport.Steamworks;
using UnityEngine.SceneManagement;
using System;
using Game;
using DG.Tweening;

public class MenuUISystem : UIWindow
{
    public event EventHandler<MageData> MageSelected;

    public LobbyListUISystem LobbyList;
    public GameObject GameStateManagerPrefab;
    public MageSelectionUISystem MageSelection;
    public Button SingleplayerButton, MultiplayerButton;

    void Awake()
    {
        defaultYs = new float[] { transform.GetChild(0).localPosition.y };

        if (GameManager.Instance == null)
            Instantiate(GameStateManagerPrefab);
    }

    void Start()
    {
        SingleplayerButton.onClick.AddListener(StartSingleplayer);
        MultiplayerButton.onClick.AddListener(StartMultiplayer);

        Steam.Instance.Connected += OnSteamConnected;
        Steam.Instance.ConnectionLost += OnSteamConnectionLost;
        MageSelection.MageSelected += OnMageSelected;

        GameManager.Instance.Menu = this;
        GameManager.Instance.GameState = GameState.MainMenu;
    }

    void OnSteamConnectionLost(object _, EventArgs e) => MultiplayerButton.interactable = false;
    void OnSteamConnected(object _, EventArgs e) => MultiplayerButton.interactable = true;

    void OnMageSelected(object _, MageData e)
    {
        MageSelected?.Invoke(null, e);
    }

    void StartSingleplayer()
    {
        GameManager.Instance.GameState = GameState.SelectingMage;
    }

    void StartMultiplayer()
    {
        GameManager.Instance.GameState = GameState.BrowsingLobbies;
    }

    void OnDestroy()
    {
        MageSelection.MageSelected -= OnMageSelected;
    }
}
