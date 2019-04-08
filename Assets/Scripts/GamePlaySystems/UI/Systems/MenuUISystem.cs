using UnityEngine;
using UnityEngine.UI;
using Transport.Steamworks;
using UnityEngine.SceneManagement;
using System;
using Game;
using DG.Tweening;

public class MenuUISystem : MonoBehaviour, IUIWindow
{
    public event EventHandler ClickedOnMultiplayer;
    public event EventHandler ClickedOnSingleplayer;
    public event EventHandler<MageData> MageSelected;

    public LobbyListUISystem LobbyList;
    public GameObject GameStateManagerPrefab;
    public MageSelectionUISystem MageSelection;
    public Button SingleplayerButton, MultiplayerButton;

    float defaultY;

    void Awake()
    {
       
        defaultY = transform.GetChild(0).localPosition.y;
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

        Open();
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
        MageSelection.Open();
        Close();
    }

    void StartMultiplayer()
    {
        LobbyList.Open();
        Close();
    }

    void OnDestroy()
    {
        MageSelection.MageSelected -= OnMageSelected;
    }

    public void Open()
    {
        GameManager.Instance.GameState = GameState.MainMenu; 
        transform.GetChild(0).DOLocalMoveY(0, 0.5f);
    }

    public void Close()
    {
        transform.GetChild(0).DOLocalMoveY(defaultY, 0.5f);
    }
}
