using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks.Data;

namespace Game.UI.Lobbies
{
    public class LobbyButtonUISystem : MonoBehaviour
    {
        public event Action<Lobby> Clicked;

        public Lobby Lobby { get; set; }
        public TextMeshProUGUI Label { get; set; }

        void Awake()
        {
            Label = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            GetComponent<Button>().onClick.AddListener(() => Clicked?.Invoke(Lobby));
        }

    }
}