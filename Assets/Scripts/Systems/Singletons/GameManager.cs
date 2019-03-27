using Game.Systems;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using Transport.Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    MainMenu,
    SingleplayerInGame,
    MultiplayerInGame,
    BrowsingLobbies,
    CreatingLobby,
    InLobby
}

public class GameManager : MonoBehaviour
{

    public event EventHandler<GameState> StateChanged = delegate { };
    public GameObject GameDataPrefab, SteamInstancePrefab, ReferenceHolderPrefab, GameLoopPrefab;

    private static GameManager instance;
    public static GameManager Instance
    {
        get => instance;
        private set
        {
            if (instance == null) instance = value;
        }
    }

    private GameState gameState;
    public GameState GameState
    {
        get => gameState;
        set
        {
            gameState = value;
            StateChanged?.Invoke(null, gameState);
            Debug.Log(value);
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;

        Application.targetFrameRate = 70;
        QualitySettings.vSyncCount = 0;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Start()
    {
        if (GameData.Instance == null) Instantiate(GameDataPrefab);
        if (Steam.Instance == null) Instantiate(SteamInstancePrefab);
        if (ReferenceHolder.Get == null) Instantiate(ReferenceHolderPrefab);
        if (GameLoop.Instance == null) Instantiate(GameLoopPrefab);
    }

    public void GoToMainMenu()
    {
        Destroy(Steam.Instance.gameObject);
        Destroy(NetworkManager.singleton.gameObject);
        NetworkManager.Shutdown();
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
