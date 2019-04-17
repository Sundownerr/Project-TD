using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facepunch.Steamworks;
using FPClient = Facepunch.Steamworks.Client;
using TMPro;
using Game;
using DG.Tweening;

namespace Game.Systems
{
    public class LobbyDataChanger
    {
        TextMeshProUGUI maxPlayersText;
        Slider playersSlider;
        TMP_Dropdown modeDropdown, visibilityDropdown, difficultyDropdown, mapDropdown, wavesDropdown;
        TMP_InputField LobbyName;

        public LobbyDataChanger(LobbyUISystem lobbyUISystem)
        {
            maxPlayersText = lobbyUISystem.MaxPlayersText;
            playersSlider = lobbyUISystem.PlayersSlider;
            modeDropdown = lobbyUISystem.ModeDropdown;
            difficultyDropdown = lobbyUISystem.DifficultyDropdown;
            mapDropdown = lobbyUISystem.MapDropdown;
            wavesDropdown = lobbyUISystem.WavesDropdown;
            visibilityDropdown = lobbyUISystem.VisibilityDropdown;

            if (FPClient.Instance.Lobby.IsOwner)
            {
                ActivateDropdowns(true);
                modeDropdown.onValueChanged.AddListener(ModeChanged);
                visibilityDropdown.onValueChanged.AddListener(VisibilityChanged);
                difficultyDropdown.onValueChanged.AddListener(DifficultyChanged);
                mapDropdown.onValueChanged.AddListener(MapChanged);
                wavesDropdown.onValueChanged.AddListener(WavesChanged);

                playersSlider.onValueChanged.AddListener((maxPlayers) =>
                {
                    maxPlayersText.text = maxPlayers.ToString();
                    FPClient.Instance.Lobby.MaxMembers = (int)maxPlayers;
                    LobbyExt.SetData(LobbyData.MaxPlayers, ((int)maxPlayers).ToString());
                });

                playersSlider.minValue = 2;
                playersSlider.maxValue = 2;
                playersSlider.value = playersSlider.value > 2 ? 2 : playersSlider.value;
            }
            else
                ActivateDropdowns(false);

            LocalizeDropdownItems();

            #region Helper functions

            void ModeChanged(int option)
            {
                playersSlider.maxValue = option == 0 ? 2 : 8;
                playersSlider.value = playersSlider.value > playersSlider.maxValue ? playersSlider.maxValue : playersSlider.value;

                LobbyExt.SetData(LobbyData.Mode, option.ToString());
            }

            void VisibilityChanged(int option)
            {
                FPClient.Instance.Lobby.LobbyType = option == 0 ? Lobby.Type.Public : Lobby.Type.FriendsOnly;
                LobbyExt.SetData(LobbyData.Visibility, option.ToString());
            }

            void DifficultyChanged(int option)
            {
                LobbyExt.SetData(LobbyData.Difficulty, option.ToString());
            }

            void MapChanged(int option)
            {
                LobbyExt.SetData(LobbyData.Map, option.ToString());
            }

            void WavesChanged(int option)
            {
                LobbyExt.SetData(LobbyData.Waves, option.ToString());
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

            #endregion
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