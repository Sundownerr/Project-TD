using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Systems;
using System;
using Game.Utility;
using Game.Enums;

namespace Game.Managers
{
    public class GameLoop : SingletonDDOL<GameLoop>
    {
        PlayerSystem player;
        bool isPlayerCreated;

        void Start()
        {
            GameData.Instance.PlayerDataSet += OnPlayerDataSet;
            GameManager.Instance.StateChanged += OnGameStateChanged;

            void OnGameStateChanged(GameState e)
            {
                var inGame =
                    e == GameState.InGameMultiplayer ||
                    e == GameState.InGameSingleplayer;

                if (!inGame)
                {
                    player = null;
                    isPlayerCreated = false;
                }
            }

            void OnPlayerDataSet(PlayerSystem e)
            {
                player = e;
                isPlayerCreated = true;
            }
        }

        void Update()
        {
            if (!isPlayerCreated)
            {
                return;
            }
            
            player.UpdateSystem();
        }
    }
}