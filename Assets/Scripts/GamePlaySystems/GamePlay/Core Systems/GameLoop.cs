using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Systems;
using System;

public class GameLoop : MonoBehaviour
{
    public event EventHandler<PlayerSystem> PlayerCreated = delegate { };

    static GameLoop instance;
    public static GameLoop Instance
    {
        get => instance;
        private set
        {
            if (instance == null)
                instance = value;
        }
    }

    PlayerSystem player;

    void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }

    void Start()
    {
        ReferenceHolder.Get.PlayerDataSet += OnPlayerDataSet;
        GameManager.Instance.StateChanged += OnGameStateChanged;
    }

    void OnGameStateChanged(object sender, GameState e)
    {
        var inGame = e == GameState.MultiplayerInGame || e == GameState.SingleplayerInGame;

        if (!inGame)
            player = null;
    }

    void OnPlayerDataSet(object _, Game.Systems.PlayerData e)
    {
        player = new PlayerSystem(e.Map, e.Mage);
        PlayerCreated?.Invoke(null, player);
    }

    void FixedUpdate()
    {
        player?.UpdateSystem();
    }
}
