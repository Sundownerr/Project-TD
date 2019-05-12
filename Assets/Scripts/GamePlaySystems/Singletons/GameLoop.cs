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
        public event Action<PlayerSystem> PlayerCreated;

        PlayerSystem player;

        void Start()
        {
            ReferenceHolder.Get.PlayerDataSet += OnPlayerDataSet;
            GameManager.Instance.StateChanged += OnGameStateChanged;
        }

        void OnGameStateChanged(GameState e)
        {
            var inGame = e == GameState.InGameMultiplayer || e == GameState.InGameSingleplayer;

            if (!inGame)
                player = null;
        }

        void OnPlayerDataSet(PlayerSystemData e)
        {
            player = new PlayerSystem(e.Map, e.Mage);
            PlayerCreated?.Invoke(player);
        }

        void FixedUpdate()
        {
            player?.UpdateSystem();
        }
    }
}