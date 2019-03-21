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
    
    public GameObject GameDataPrefab, SteamInstancePrefab, ReferenceHolderPrefab;
    public event EventHandler<GameState> StateChanged = delegate { };

    public static GameManager Instance
    {
        get => instance;
        private set
        {
            if (instance == null) instance = value;
        }
    }

    public GameState GameState {
        get => gameState;
        set
        {           
            gameState = value;
            StateChanged?.Invoke(null, gameState);
            Debug.Log(value);
        }
    }

    private static GameManager instance;
    private GameState gameState;

    private void Awake()
    {
        Application.targetFrameRate = 70;
        QualitySettings.vSyncCount = 0;
        Cursor.lockState = CursorLockMode.Confined;

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void Start()
    {
        if (GameData.Instance == null)
            Instantiate(GameDataPrefab);

        if (Steam.Instance == null)
            Instantiate(SteamInstancePrefab);

        if (ReferenceHolder.Get == null)
           Instantiate(ReferenceHolderPrefab);
    }

    public void GoToMainMenu()
    {
        Destroy(Steam.Instance.gameObject);
        Destroy(NetworkManager.singleton.gameObject);
        NetworkManager.Shutdown();
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
