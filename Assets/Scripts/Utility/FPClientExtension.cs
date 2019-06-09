using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Game.Systems;
using Game.Managers;
using Game.Consts;
using Game.UI;
using Game.Enums;
using Game.UI.Lobbies;
using Steamworks.Data;
using Steamworks;
using System.Threading.Tasks;

namespace Game.Utility
{
  
    public static class LobbyExt
    {
        /// <summary>
        /// Send chat message as player
        /// </summary>
        public static void SendChatMessage(TMP_InputField chatInputField)
        {
            var lobby = GameData.Instance.CurrentLobby;

            if (string.IsNullOrWhiteSpace(chatInputField.text))
            {
                chatInputField.DeactivateInputField();
                return;
            }

            var message = $"{SteamClient.Name}: {chatInputField.text}";

            if (lobby.SendChatString(message))
            {
                chatInputField.text = string.Empty;
                chatInputField.ActivateInputField();
            }
        }


        public static Friend GetMeFromLobby(this Lobby lobby) =>
            new List<Friend>(lobby.Members).Find(x => x.IsMe);


        public static void StartLobbyServer()
        {
            var lobby = GameData.Instance.CurrentLobby;

            if (lobby.Owner.IsMe)
            {
                lobby.SetData(LobbyData.GameStarting, LobbyData.Yes);
                GameManager.Instance.StartCoroutine(StartTimer());
            }

            IEnumerator StartTimer()
            {
                var networkManager = NetworkManager.singleton as ExtendedNetworkManager;
                var timeUntilStart = 4d;

                SendChatMessage($"Starting in 5 ...");

                while (timeUntilStart > 0)
                {
                    SendChatMessage($"{timeUntilStart} ...");
                    yield return new WaitForSeconds(0.5f);
                    timeUntilStart -= 1;
                }

                if (lobby.Owner.IsMe)
                {
                    networkManager.StartHost();
                    // GameManager.Instance.GameState = GameState.LoadingGame;
                    lobby.SetData(LobbyData.GameStarted, LobbyData.Yes);
                }

                void SendChatMessage(string message)
                {
                    if (string.IsNullOrWhiteSpace(message))
                        return;

                    lobby.SendChatString(message);
                }
            }
        }

        public static void SetDefaultData(this Lobby lobby)
        {

            var me = lobby.GetMeFromLobby();
            lobby.SetData("name", $"{SteamClient.Name}'s lobby");

            lobby.SetData(LobbyData.Joinable, LobbyData.Yes);
            lobby.SetData(LobbyData.GameStarted, LobbyData.No);
            lobby.SetData(LobbyData.GameStarting, LobbyData.No);
            lobby.SetData(LobbyData.Mode, string.Empty);
            lobby.SetData(LobbyData.Difficulty, string.Empty);
            lobby.SetData(LobbyData.Map, StringConsts.MultiplayerMap1);
            lobby.SetData(LobbyData.Waves, string.Empty);
            lobby.SetMemberData(me, LobbyData.MageID, $"{GameData.Instance.ChoosedMage.Index}");
            lobby.SetMemberData(me, LobbyData.Ready, LobbyData.No);

        }

        public static void UpdateData(TextMeshProUGUI UIText, string key)
        {
            var value = GameData.Instance.CurrentLobby.GetData(key);

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

        public static async Task<LobbyPlayerUI> SetAvatar(this LobbyPlayerUI player, Friend member)
        {
            var avatar = await member.GetSmallAvatarAsync();

            if (avatar.HasValue)
            {
                var image = avatar.Value;
                var texture = new Texture2D((int)image.Width, (int)image.Height);

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        var pixel = image.GetPixel(x, y);

                        texture.SetPixel(
                            x,
                            (int)image.Height - y,
                            new UnityEngine.Color(pixel.r / 255.0f, pixel.g / 255.0f, pixel.b / 255.0f, pixel.a / 255.0f));
                    }
                }
                texture.Apply();
                player.Avatar.texture = texture;
            }
            return player;
        }
    }

}