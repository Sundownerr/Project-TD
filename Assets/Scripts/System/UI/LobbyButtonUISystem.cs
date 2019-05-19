using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facepunch.Steamworks;
using TMPro;

namespace Game.UI.Lobbies
{
    public class LobbyButtonUISystem : MonoBehaviour
    {
        public event Action<LobbyList.Lobby> Clicked;

        public LobbyList.Lobby Lobby { get; set; }
        public TextMeshProUGUI Label { get; set; }

        void Awake()
        {
            Label = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            GetComponent<Button>().onClick.AddListener(() => Clicked?.Invoke(Lobby));
        }

    }
}