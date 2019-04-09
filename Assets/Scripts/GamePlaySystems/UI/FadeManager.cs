using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FadeManager : MonoBehaviour
{
    Image image;

    static FadeManager instance;
    public static FadeManager Instance
    {
        get => instance;
        private set
        {
            if (instance == null) instance = value;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this);
        Instance = this;
        image = GetComponent<Image>();
    }

    void Start()
    {
        GameManager.Instance.StateChanged += OnGameStateChanged;
        image.DOFade(0, 1);
    }

    void OnGameStateChanged(object sender, GameState e)
    {
        var isFadeOut = 
            e == GameState.MainMenu ||
            e == GameState.InGameSingleplayer ||
            e == GameState.InGameMultiplayer;

        var isFadeIn = 
            e == GameState.LoadingGame ||
            e == GameState.UnloadingGame;
           

        if(isFadeOut)
            image.DOFade(0, 0.05f);

        if(isFadeIn)
            image.DOFade(1, 0.1f);
    }
}
