using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facepunch.Steamworks;

public class LobbyButtonUISystem : MonoBehaviour
{
    public event EventHandler<LobbyList.Lobby> Clicked = delegate { };

    public LobbyList.Lobby Lobby { get; set; }
    public Text Label { get; set; }

    private void Awake()
    {
        var button = GetComponent<Button>();
        Label = transform.GetChild(0).GetComponent<Text>();

        button.onClick.AddListener(() => Clicked?.Invoke(this, Lobby));
    }
   
}
