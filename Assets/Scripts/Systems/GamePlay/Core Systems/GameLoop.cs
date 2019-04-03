using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Systems;
using System;

public class GameLoop : MonoBehaviour
{
    public event EventHandler<PlayerSystem> PlayerCreated = delegate { };

    private static GameLoop instance;
    public static GameLoop Instance
    {
        get => instance;
        private set
        {
            if (instance == null)
                instance = value;
        }
    }

    private PlayerSystem player;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
    }

    private void Start()
    {
        ReferenceHolder.Get.MapAssigned += OnMapAssigned;
        GameManager.Instance.StateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(object sender, GameState e)
    {
        var inGame = e == GameState.MultiplayerInGame || e == GameState.SingleplayerInGame;

        if (!inGame)
            player = null;
    }

    private void OnMapAssigned(object _, PlayerMap e)
    {
        player = new PlayerSystem(e);
        PlayerCreated?.Invoke(null, player);
    }

    private void FixedUpdate()
    {
        player?.UpdateSystem();
    }
}
