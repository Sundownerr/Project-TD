using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Facepunch.Steamworks;
using FPClient = Facepunch.Steamworks.Client;
using Transport.Steamworks;
using Game;
using System;

public class GameData : MonoBehaviour
{
    static GameData instance;
    public static GameData Instance
    {
        get => instance;
        private set
        {
            if (instance == null) instance = value;
        }
    }

    public PlayerData PlayerData { get; private set; }
    public MageData ChoosedMage { get; private set; }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        LoadData();       
    }

    void Start()
    {
        GameManager.Instance.StateChanged += OnGameStateChanged;
        GameManager.Instance.Menu.MageSelected += OnMageSelected;
    }

    void OnMageSelected(object _, MageData e) => ChoosedMage = e;
    void OnGameStateChanged(object _, GameState e)
    {
        if(e == GameState.InLobby)
        {
            LobbyExt.SetMemberData(LobbyData.Level, PlayerData.Level.ToString());
        }
    }

    void LoadData()
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
