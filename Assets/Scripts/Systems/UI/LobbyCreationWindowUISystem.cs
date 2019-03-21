using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facepunch.Steamworks;
using FPClient = Facepunch.Steamworks.Client;

public class LobbyCreationWindowUISystem : MonoBehaviour
{
    public Text MaxPlayersText;
    public Slider PlayersSlider;
    public Dropdown ModeDropdown, VisibilityDropdown, DifficultyDropdown, MapDropdown, WavesDropdown;
    public InputField LobbyName;
    public Button CloseButton, CreateButton;

    private void Start()
    {
        var dropdownEvent = new Dropdown.DropdownEvent();
        var sliderEvent = new Slider.SliderEvent();      

        dropdownEvent.AddListener(ModeChanged);
        ModeDropdown.onValueChanged = dropdownEvent;
        sliderEvent.AddListener((value) => MaxPlayersText.text = value.ToString());
        PlayersSlider.onValueChanged = sliderEvent;

        CloseButton.onClick.AddListener(CloseWindow);
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

    private void OnEnable()
    {
        GameManager.Instance.GameState = GameState.CreatingLobby;
        FPClient.Instance.Lobby.OnLobbyCreated += LobbyCreated;
    }

    private void CloseWindow()
    {
       
        FPClient.Instance.Lobby.OnLobbyCreated -= LobbyCreated;

        gameObject.SetActive(false);
    }

    private void LobbyCreated(bool isSuccesful)
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
            CloseWindow();
        }
    }
}
