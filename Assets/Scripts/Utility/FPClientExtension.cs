using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPClient = Facepunch.Steamworks.Client;
using Facepunch.Steamworks;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Game.Systems;
using Game.Managers;
using Game.Consts;
using Game.UI;
using Game.Enums;

namespace Game.Utility
{
    public struct LobbyCallbacks
    {
        public Action<Lobby.MemberStateChange, ulong, ulong> StateChanged;
        public Action<ulong> MemberDataUpdated;
        public Action DataUpdated;
        public Action<ulong, string> ChatStringReceived;

        /// <param name="stateChanged"> Called when the state of the Lobby is somehow shifted. Usually when someone joins
        /// or leaves the lobby. The first ulong is the SteamID of the user that initiated
        /// the change. The second ulong is the person that was affected</param>
        /// <param name="chatStringReceived">Callback when chat string received</param>
        /// <param name="dataUpdated"> Called when the lobby data itself has been updated. 
        /// Called when someone has joined/left, Owner has updated data, etc.</param>
        /// <param name="memberDataUpdated">Called when a member of the lobby has updated either 
        /// their personal Lobby metadata or someone's global steam state has changed (like a display name). 
        /// Parameter is the user steamID who changed</param>
        public LobbyCallbacks(Action<Lobby.MemberStateChange, ulong, ulong> stateChanged, Action<ulong> memberDataUpdated, Action dataUpdated, Action<ulong, string> chatStringReceived)
        {
            StateChanged = stateChanged;
            MemberDataUpdated = memberDataUpdated;
            DataUpdated = dataUpdated;
            ChatStringReceived = chatStringReceived;
        }
    }

    public static class FPClientExt
    {
        public static void LeaveGame()
        {
            if (FPClient.Instance == null) return;
            
            FPClient.Instance.Lobby.OnLobbyStateChanged = null;
            FPClient.Instance.Lobby.OnLobbyMemberDataUpdated = null;
            FPClient.Instance.LobbyList.OnLobbiesUpdated = null;
            FPClient.Instance.Lobby.OnLobbyCreated = null;
            FPClient.Instance.Lobby.OnLobbyJoined = null;
            FPClient.Instance.Lobby.OnChatStringRecieved = null;

            FPClient.Instance.Lobby.Leave();
        }
    }

    public static class LobbyExt
    {
        public static void SetCallbacks(LobbyCallbacks lobbyCallbacks)
        {
            FPClient.Instance.Lobby.OnLobbyStateChanged = lobbyCallbacks.StateChanged;
            FPClient.Instance.Lobby.OnLobbyMemberDataUpdated = lobbyCallbacks.MemberDataUpdated;
            FPClient.Instance.Lobby.OnLobbyDataUpdated = lobbyCallbacks.DataUpdated;
            FPClient.Instance.Lobby.OnChatStringRecieved = lobbyCallbacks.ChatStringReceived;
        }

        public static void ClearLobbyCallbacks()
        {
            FPClient.Instance.Lobby.OnLobbyStateChanged = null;
            FPClient.Instance.Lobby.OnLobbyMemberDataUpdated = null;
            FPClient.Instance.Lobby.OnLobbyDataUpdated = null;
            FPClient.Instance.Lobby.OnChatStringRecieved = null;
        }

        /// <summary>
        /// Send chat message as player
        /// </summary>
        public static void SendChatMessage(TMP_InputField chatInputField)
        {

            if (string.IsNullOrWhiteSpace(chatInputField.text))
            {
                chatInputField.DeactivateInputField();
                return;
            }

            var message = $"{FPClient.Instance.Username}: {chatInputField.text}";

            if (FPClient.Instance.Lobby.SendChatMessage(message))
            {
                chatInputField.text = string.Empty;
                chatInputField.ActivateInputField();
            }
        }

        /// <summary>
        /// Send chat message as server
        /// </summary>
        public static void SendChatMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            FPClient.Instance.Lobby.SendChatMessage(message);
        }

        public static string GetData(string key) => FPClient.Instance.Lobby.CurrentLobbyData.GetData(key);
        public static bool SetData(string key, string value) => FPClient.Instance.Lobby.CurrentLobbyData.SetData(key, value);

        public static string GetMemberData(ulong steamID, string key) => FPClient.Instance.Lobby.GetMemberData(steamID, key);
        public static void SetMemberData(string key, string value) => FPClient.Instance.Lobby.SetMemberData(key, value);

        public static void StartLobbyServer()
        {
            if (FPClient.Instance.Lobby.IsOwner)
            {
                LobbyExt.SetData(LobbyData.GameStarting, LobbyData.Yes);
                GameManager.Instance.StartCoroutine(StartTimer());
            }

            IEnumerator StartTimer()
            {
                var networkManager = NetworkManager.singleton as ExtendedNetworkManager;
                var timeUntilStart = 4d;

                LobbyExt.SendChatMessage($"Starting in 5 ...");

                while (timeUntilStart > 0)
                {
                    LobbyExt.SendChatMessage($"{timeUntilStart} ...");
                    yield return new WaitForSeconds(0.5f);
                    timeUntilStart -= 1;
                }

                if (FPClient.Instance.Lobby.IsOwner)
                {
                    networkManager.StartHost();
                    GameManager.Instance.GameState = GameState.LoadingGame;
                    LobbyExt.SetData(LobbyData.GameStarted, LobbyData.Yes);
                }
            }
        }

        public static void SetLobbyDefaultData()
        {
            FPClient.Instance.Lobby.Name = $"{FPClient.Instance.Username}'s lobby";

            LobbyExt.SetData(LobbyData.Joinable, LobbyData.Yes);
            LobbyExt.SetData(LobbyData.GameStarted, LobbyData.No);
            LobbyExt.SetData(LobbyData.GameStarting, LobbyData.No);
            LobbyExt.SetData(LobbyData.Mode, string.Empty);
            LobbyExt.SetData(LobbyData.Difficulty, string.Empty);
            LobbyExt.SetData(LobbyData.Map, StringConsts.MultiplayerMap1);
            LobbyExt.SetData(LobbyData.Waves, string.Empty);
            LobbyExt.SetMemberData(LobbyData.MageID, $"{GameData.Instance.ChoosedMage.Index}");
            LobbyExt.SetMemberData(LobbyData.Ready, LobbyData.No);

        }

        public static void UpdateData(TextMeshProUGUI UIText, string key)
        {
            var value = GetData(key);

            if (string.IsNullOrWhiteSpace(value))
                return;

            if (UIText.text == value)
                return;

            UIText.text = value;
        }

        public static LobbyPlayerUI SetName(this LobbyPlayerUI player, string name)
        {
            player.PlayerNameText.text = name;
            return player;
        }

        public static LobbyPlayerUI SetLevel(this LobbyPlayerUI player, string level)
        {
            player.LevelText.text = level;
            return player;
        }

        public static LobbyPlayerUI SetReady(this LobbyPlayerUI player, string ready)
        {
            player.ReadyText.text = ready;
            return player;
        }

        public static LobbyPlayerUI SetMage(this LobbyPlayerUI player, string mageName)
        {
            player.MageNameText.text = mageName;
            return player;
        }

        public static LobbyPlayerUI SetAvatar(this LobbyPlayerUI player, ulong steamID)
        {
            FPClient.Instance.Friends.GetAvatar(Friends.AvatarSize.Medium, steamID, LoadAvatar);

            void LoadAvatar(Facepunch.Steamworks.Image image)
            {
                if (image == null)
                    return;

                var texture = new Texture2D(image.Width, image.Height);

                for (int x = 0; x < image.Width; x++)
                    for (int y = 0; y < image.Height; y++)
                    {
                        var pixel = image.GetPixel(x, y);

                        texture.SetPixel(
                            x,
                            image.Height - y,
                            new UnityEngine.Color(pixel.r / 255.0f, pixel.g / 255.0f, pixel.b / 255.0f, pixel.a / 255.0f));
                    }

                texture.Apply();
                player.Avatar.texture = texture;
            }
            return player;
        }
    }

}