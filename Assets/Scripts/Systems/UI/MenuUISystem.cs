using UnityEngine;
using UnityEngine.UI;
using Transport.Steamworks;
using UnityEngine.SceneManagement;
using Game.Systems;
using System.Collections;

public class MenuUISystem : MonoBehaviour
{
    public GameObject LobbyList, GameStateManagerPrefab;
    public Button SingleplayerButton, MultiplayerButton, TestButton, QuitButton;

    

    private void Awake()
    {
        if (GameManager.Instance == null)
            Instantiate(GameStateManagerPrefab);

        SceneManager.sceneLoaded += (scene, loadMode) =>
        {
            if (scene.name == "SingleplayerMap")
            {
                GameManager.Instance.GameState = GameState.SingleplayerInGame;
                return;
            }

            if (scene.name == "MultiplayerMap1")
            {
                GameManager.Instance.GameState = GameState.MultiplayerInGame;
            }
        };
    }

   
    private void Start()
    {
        SingleplayerButton.onClick.AddListener(StartSingleplayer);
        MultiplayerButton.onClick.AddListener(StartMultiplayer);
        QuitButton.onClick.AddListener(() => Application.Quit());
       
 
        GameManager.Instance.GameState = GameState.MainMenu;
    }

    private void StartSingleplayer()
    {
        SceneManager.LoadSceneAsync("SingleplayerMap");       
    }

    private void StartMultiplayer()
    {
        gameObject.SetActive(false);
        LobbyList.SetActive(true);
    }

    private void Update()
    {
        MultiplayerButton.interactable = Steam.Instance.ClientOnline;
    }
}
