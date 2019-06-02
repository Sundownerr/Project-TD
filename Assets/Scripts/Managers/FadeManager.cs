using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Game.Utility;
using Game.Enums;

namespace Game.Managers
{
    public class FadeManager : SingletonDDOL<FadeManager>
    {
        Image image;

        protected override void Awake()
        {
            base.Awake();

            image = GetComponent<Image>();
        }

        void Start()
        {
            GameManager.Instance.StateChanged += OnGameStateChanged;
            image.DOFade(0, 1);

            void OnGameStateChanged(GameState e)
            {
                var isFadeOut =
                    e == GameState.InMenu ||
                    e == GameState.InGameSingleplayer ||
                    e == GameState.InGameMultiplayer;

                var isFadeIn =
                    e == GameState.LoadingGame ||
                    e == GameState.UnloadingGame;


                if (isFadeOut)
                    image.DOFade(0, 0.05f);

                if (isFadeIn)
                    image.DOFade(1, 0.1f);
            }
        }
    }
}