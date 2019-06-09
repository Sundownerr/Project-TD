using System.Collections.Generic;
using UnityEngine.UI;
using Steamworks;
using TMPro;
using Game.Consts;
using Game.Utility;
using Game.Utility.Localization;
using Steamworks.Data;

namespace Game.UI.Lobbies
{
    public class LobbyDataChanger
    {
        TextMeshProUGUI maxPlayersText;
        Slider playersSlider;
        TMP_Dropdown modeDropdown;
        TMP_Dropdown visibilityDropdown;
        TMP_Dropdown difficultyDropdown;
        TMP_Dropdown mapDropdown;
        TMP_Dropdown wavesDropdown;
        TMP_InputField LobbyName;
        Lobby lobby;

        public LobbyDataChanger(LobbyUISystem lobbyUISystem, Lobby lobby)
        {
            maxPlayersText = lobbyUISystem.MaxPlayersText;
            playersSlider = lobbyUISystem.PlayersSlider;
            modeDropdown = lobbyUISystem.ModeDropdown;
            difficultyDropdown = lobbyUISystem.DifficultyDropdown;
            mapDropdown = lobbyUISystem.MapDropdown;
            wavesDropdown = lobbyUISystem.WavesDropdown;
            visibilityDropdown = lobbyUISystem.VisibilityDropdown;

            this.lobby = lobby;

            LocalizeDropdownItems();

            if (!lobby.Owner.IsMe)
            {
                ActivateDropdowns(false);
            }
            else
            {
                ActivateDropdowns(true);

                modeDropdown.onValueChanged.AddListener((option) =>
                {
                    playersSlider.maxValue = option == 0 ? 2 : 8;
                    playersSlider.value = playersSlider.value > playersSlider.maxValue ? playersSlider.maxValue : playersSlider.value;

                    lobby.SetData(LobbyData.Mode, option.ToString());
                });

                visibilityDropdown.onValueChanged.AddListener((option) =>
                {
                    if (option == 0)
                    {
                        lobby.SetPublic();
                    }
                    else
                    {
                        lobby.SetPrivate();
                    }

                    //  lobby.SetData(LobbyData.Visibility, option.ToString());
                });

                difficultyDropdown.onValueChanged.AddListener((option) =>
                {
                    lobby.SetData(LobbyData.Difficulty, option.ToString());
                });

                mapDropdown.onValueChanged.AddListener((option) =>
                {
                    lobby.SetData(LobbyData.Map, mapDropdown.options[option].text);
                });

                wavesDropdown.onValueChanged.AddListener((option) =>
                {
                    lobby.SetData(LobbyData.Waves, option.ToString());
                });

                playersSlider.onValueChanged.AddListener((sliderValue) =>
                {
                    maxPlayersText.text = sliderValue.ToString();
                    lobby.MaxMembers = (int)sliderValue;
                    lobby.SetData(LobbyData.MaxPlayers, ((int)sliderValue).ToString());
                });

                playersSlider.minValue = 2;
                playersSlider.maxValue = 2;
                playersSlider.value = playersSlider.value > 2 ? 2 : playersSlider.value;
            }

            void ActivateDropdowns(bool activate)
            {
                modeDropdown.interactable = activate;
                visibilityDropdown.interactable = activate;
                difficultyDropdown.interactable = activate;
                mapDropdown.interactable = activate;
                wavesDropdown.interactable = activate;
            }

            void LocalizeDropdownItems()
            {
                visibilityDropdown.AddOptions(new List<string>()
                {
                    LocaleKeys.VisibilityPublic.GetLocalized(),
                    LocaleKeys.VisibilityFriendsOnly.GetLocalized()
                });

                modeDropdown.AddOptions(new List<string>()
                {
                    LocaleKeys.ModePvp.GetLocalized(),
                    LocaleKeys.ModeCoop.GetLocalized()
                });

                difficultyDropdown.AddOptions(new List<string>()
                {
                    LocaleKeys.DifficultyEasy.GetLocalized(),
                    LocaleKeys.DifficultyNormal.GetLocalized(),
                    LocaleKeys.DifficultyHard.GetLocalized(),
                    LocaleKeys.DifficultyExtreme.GetLocalized()
                });
            }
        }

        public void UpdateData()
        {
            wavesDropdown.value = int.Parse(lobby.GetData(LobbyData.Waves));
            modeDropdown.value = int.Parse(lobby.GetData(LobbyData.Mode));
            mapDropdown.value = int.Parse(lobby.GetData(LobbyData.Map));
            difficultyDropdown.value = int.Parse(lobby.GetData(LobbyData.Difficulty));
            playersSlider.value = int.Parse(lobby.GetData(LobbyData.MaxPlayers));
            visibilityDropdown.value = int.Parse(lobby.GetData(LobbyData.Visibility));
        }

        public void Destroy()
        {
            visibilityDropdown.onValueChanged.RemoveAllListeners();
            difficultyDropdown.onValueChanged.RemoveAllListeners();
            mapDropdown.onValueChanged.RemoveAllListeners();
            wavesDropdown.onValueChanged.RemoveAllListeners();
            modeDropdown.onValueChanged.RemoveAllListeners();
            playersSlider.onValueChanged.RemoveAllListeners();
        }
    }
}