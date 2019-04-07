using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facepunch.Steamworks;
using FPClient = Facepunch.Steamworks.Client;
using TMPro;
using Game;

public class LobbyCreationWindowUISystem : MonoBehaviour, IWindow
{
    public event EventHandler Activated;
    public TextMeshProUGUI MaxPlayersText;
    public Slider PlayersSlider;
    public TMP_Dropdown ModeDropdown, VisibilityDropdown, DifficultyDropdown, MapDropdown, WavesDropdown;
    public TMP_InputField LobbyName;
    public Button CreateButton;

    void Start()
    {
        var dropdownEvent = new TMP_Dropdown.DropdownEvent();
        var sliderEvent = new Slider.SliderEvent();      

        dropdownEvent.AddListener(ModeChanged);
        ModeDropdown.onValueChanged = dropdownEvent;
        sliderEvent.AddListener((value) => MaxPlayersText.text = value.ToString());
        PlayersSlider.onValueChanged = sliderEvent;

        CreateButton.onClick.AddListener(CreateLobby);

        ModeChanged(0);

        #region Helper functions

        void ModeChanged(int optionNumber)
        {
            if (optionNumber == 0)
            {
                PlayersSlider.minValue = 2;
                PlayersSlider.maxValue = 2;

                PlayersSlider.value = PlayersSlider.value > 2 ? 2 : PlayersSlider.value;
                return;
            }

            if(optionNumber == 1)
            {
                PlayersSlider.minValue = 2;
                PlayersSlider.maxValue = 8;
            }
        }

        void CreateLobby()
        {
            var lobbyType = VisibilityDropdown.value == 0 ? Lobby.Type.Public : Lobby.Type.FriendsOnly;
            FPClient.Instance.Lobby.Create(lobbyType, (int)PlayersSlider.value);
        }

        #endregion
    }

    void OnEnable()
    {
        FPClient.Instance.Lobby.OnLobbyCreated += LobbyCreated;
        Activated?.Invoke(null, null);
    }

    void OnDisable()
    {
        FPClient.Instance.Lobby.OnLobbyCreated -= LobbyCreated;      
    }

    void LobbyCreated(bool isSuccesful)
    {
        if (isSuccesful)
        {
            var lobbyName = string.IsNullOrWhiteSpace(LobbyName.text) ? $"{FPClient.Instance.Username}'s lobby" : LobbyName.text;

            FPClient.Instance.Lobby.Name = lobbyName;

            LobbyExtension.SetData(LobbyData.Joinable, LobbyData.Yes);            
            LobbyExtension.SetData(LobbyData.GameStarted, LobbyData.No);
            LobbyExtension.SetData(LobbyData.GameStarting, LobbyData.No);
            LobbyExtension.SetData(LobbyData.Mode, ModeDropdown.options[ModeDropdown.value].text);
            LobbyExtension.SetData(LobbyData.Difficulty, DifficultyDropdown.options[DifficultyDropdown.value].text);
            LobbyExtension.SetData(LobbyData.Map, MapDropdown.options[MapDropdown.value].text);
            LobbyExtension.SetData(LobbyData.Waves, WavesDropdown.options[WavesDropdown.value].text);
            LobbyExtension.SetMemberData(LobbyData.Ready, LobbyData.No);
        }
    }
}
