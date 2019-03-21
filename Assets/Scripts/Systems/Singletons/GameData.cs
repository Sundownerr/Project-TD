using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Facepunch.Steamworks;
using FPClient = Facepunch.Steamworks.Client;
using Transport.Steamworks;

public class GameData : MonoBehaviour
{
    public static GameData Instance
    {
        get => instance;
        private set
        {
            if (instance == null) instance = value;
        }
    }

    public PlayerData PlayerData { get; private set; }

    private static GameData instance;
    private WaitForSeconds retryDelay;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        LoadData();
        retryDelay = new WaitForSeconds(0.5f);
    }

    private void Start()
    {
        StartCoroutine(ListenToGameState());

        #region Helper functions

        IEnumerator ListenToGameState()
        {
            while (GameManager.Instance == null)
                yield return retryDelay;

            GameManager.Instance.StateChanged += OnGameStateChanged;
            StopAllCoroutines();
        } 

        #endregion
    }

    private void OnGameStateChanged(object _, GameState e)
    {
        if(e == GameState.InLobby)
        {
            LobbyExtension.SetMemberData(LobbyData.Level, PlayerData.Level.ToString());
        }
    }

    private void LoadData()
    {
        PlayerData data;
        
        if (!File.Exists("playerData.json"))
        {
            data = new PlayerData(0);

            var newFile = JsonUtility.ToJson(data);
            File.WriteAllText("playerData.json", newFile);
        }
        else
        {
            var stringFromFile = File.ReadAllText("playerData.json");
            var playerDataFromFile = JsonUtility.FromJson<PlayerData>(stringFromFile);

            data = new PlayerData(playerDataFromFile.Level);
        }

        PlayerData = new PlayerData(data.Level);
    }

    public void SaveData(PlayerData data)
    {
        PlayerData = data;

        var newFile = JsonUtility.ToJson(data);
        File.WriteAllText("playerData.json", newFile);
    }
}
