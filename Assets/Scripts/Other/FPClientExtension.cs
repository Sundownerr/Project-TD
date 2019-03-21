using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FPClient = Facepunch.Steamworks.Client;
using Facepunch.Steamworks;
using UnityEngine.UI;

public struct LobbyCallbacks
{
    public Action<Lobby.MemberStateChange, ulong, ulong> StateChanged;
    public Action<ulong> MemberDataUpdated;
    public Action DataUpdated;
    public Action<ulong, string> ChatStringReceived;

    /// <param name="stateChanged">
    /// Called when the state of the Lobby is somehow shifted. Usually when someone joins
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

public static class LobbyExtension
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
    public static void SendChatMessage(InputField chatInputField)
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

    public static void UpdateData(Text UIText, string key)
    {
        var value = GetData(key);

        if (string.IsNullOrWhiteSpace(value))
            return;

        if (UIText.text == value)
            return;

        UIText.text = value;
    }
}
