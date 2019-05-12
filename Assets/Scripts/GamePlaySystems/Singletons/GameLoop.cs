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
        public event EventHandler<PlayerSystem> PlayerCreated;

        PlayerSystem player;

        void Start()
        {
            ReferenceHolder.Get.PlayerDataSet += OnPlayerDataSet;
            GameManager.Instance.StateChanged += OnGameStateChanged;
        }

        void OnGameStateChanged(object sender, GameState e)
        {
            var inGame = e == GameState.InGameMultiplayer || e == GameState.InGameSingleplayer;

            if (!inGame)
                player = null;
        }

        void OnPlayerDataSet(object _, PlayerSystemData e)
        {
            player = new PlayerSystem(e.Map, e.Mage);
            PlayerCreated?.Invoke(null, player);
        }

        void FixedUpdate()
        {
            player?.UpdateSystem();
        }
    }
}