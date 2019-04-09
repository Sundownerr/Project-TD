using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facepunch.Steamworks;
using FPClient = Facepunch.Steamworks.Client;
using Mirror;
using TMPro;
using System;
using Game;
using DG.Tweening;

public class LobbyUISystem : UIWindow
{

    public TMP_InputField ChatInputField;
    public GameObject PlayerTextPrefab, ChatTextPrefab, PlayerTextGroup, ChatTextGroup, LobbyList;
    public Button ReadyButton, StartServerButton;
    public TextMeshProUGUI LobbyName, ModeText, DifficultyText, MapText, WavesText;

    Dictionary<ulong, TextMeshProUGUI> playerTexts;
    ObjectPool chatTextsPool;
    WaitForSeconds delay;

    void Start()
    {
        defaultYs = new float[] { transform.GetChild(0).localPosition.y };

        delay = new WaitForSeconds(0.5f);
        ReadyButton.onClick.AddListener(SetReady);
        StartServerButton.onClick.AddListener(StartLobbyServer);

        chatTextsPool = new ObjectPool(ChatTextPrefab, ChatTextGroup.transform, 5);
        playerTexts = new Dictionary<ulong, TextMeshProUGUI>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            LobbyExtension.SendChatMessage(ChatInputField);
    }

    public override void Open(float timeToComplete = NumberConsts.UIAnimSpeed)
    {
        
        LobbyExtension.SetCallbacks(
           new LobbyCallbacks(
               LobbyStateChanged,
               LobbyMemberDataUpdated,
               LobbyDataUpdated,
               ChatMessageReceived
               ));

        if (!FPClient.Instance.Lobby.IsOwner)
            StartServerButton.gameObject.SetActive(false);
        else
        {
            LobbyExtension.SetData(LobbyData.GameStarted, LobbyData.No);
            LobbyExtension.SetData(LobbyData.GameStarting, LobbyData.No);
            StartServerButton.gameObject.SetActive(true);
        }

        LobbyName.text = FPClient.Instance.Lobby.Name;

        var playerIDs = FPClient.Instance.Lobby.GetMemberIDs();

        for (int i = 0; i < playerIDs.Length; i++)
        {
            AddPlayer(playerIDs[i]);
            LobbyMemberDataUpdated(playerIDs[i]);
        }

        GameManager.Instance.GameState = GameState.InLobby;
        base.Open(timeToComplete);
    }

    public override void Close(Move moveTo, float timeToComplete = NumberConsts.UIAnimSpeed)
    {
        base.Close(moveTo);
        LeaveLobby();
    }

    void ChatMessageReceived(ulong senderID, string message) => CreateChatMessage(message);

    void CreateChatMessage(string message) => chatTextsPool.PopObject().GetComponent<TMP_InputField>().text = message;

    void LobbyDataUpdated()
    {
        var networkManager = NetworkManager.singleton as ExtendedNetworkManager;

        LobbyExtension.UpdateData(ModeText, LobbyData.Mode);
        LobbyExtension.UpdateData(DifficultyText, LobbyData.Difficulty);
        LobbyExtension.UpdateData(MapText, LobbyData.Map);
        LobbyExtension.UpdateData(WavesText, LobbyData.Waves);

        if (LobbyExtension.GetData(LobbyData.GameStarting) == LobbyData.Yes)
        {
            if (!networkManager.IsClientConnected() && !NetworkServer.active)
            {
                networkManager.networkAddress = FPClient.Instance.Lobby.Owner.ToString();
                networkManager.onlineScene = LobbyExtension.GetData(LobbyData.Map);
            }
        }

        if (LobbyExtension.GetData(LobbyData.GameStarted) == LobbyData.Yes)
        {
            LobbyExtension.ClearLobbyCallbacks();
            if (!FPClient.Instance.Lobby.IsOwner)
            {
                GameManager.Instance.GameState = GameState.LoadingGame;
                networkManager.StartClient();
            }
        }
    }

    void StartLobbyServer()
    {
        if (FPClient.Instance.Lobby.IsOwner)
        {
            LobbyExtension.SetData(LobbyData.GameStarting, LobbyData.Yes);
            StartCoroutine(StartTimer());
        }

        IEnumerator StartTimer()
        {
            var networkManager = NetworkManager.singleton as ExtendedNetworkManager;
            var timeUntilStart = 4d;

            LobbyExtension.SendChatMessage($"Starting in 5 ...");

            while (timeUntilStart > 0)
            {
                LobbyExtension.SendChatMessage($"{timeUntilStart} ...");
                yield return delay;
                timeUntilStart -= 1;
            }

            if (FPClient.Instance.Lobby.IsOwner)
            {
                networkManager.StartHost();
                GameManager.Instance.GameState = GameState.LoadingGame;
                LobbyExtension.SetData(LobbyData.GameStarted, LobbyData.Yes);
            }
        }
    }

    void SetReady()
    {
        var isReady = LobbyExtension.GetMemberData(FPClient.Instance.SteamId, LobbyData.Ready) == LobbyData.Yes;

        LobbyExtension.SetMemberData(LobbyData.Ready, isReady ? LobbyData.No : LobbyData.Yes);
    }

    void LeaveLobby()
    {
        foreach (var pair in playerTexts)
            if (playerTexts.TryGetValue(pair.Key, out var text))
                Destroy(text.gameObject);

        chatTextsPool.DeactivateAll();
        playerTexts.Clear();

        LobbyExtension.ClearLobbyCallbacks();
        FPClient.Instance.Lobby.Leave();
    }

    void LobbyMemberDataUpdated(ulong steamID)
    {
        if (playerTexts.TryGetValue(steamID, out var name))
        {
            var levelText = name.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var readyText = name.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            name.text = $"{FPClient.Instance.Friends.GetName(steamID)}";
            levelText.text = $"Lv.{LobbyExtension.GetMemberData(steamID, LobbyData.Level)}";
            readyText.text = LobbyExtension.GetMemberData(steamID, LobbyData.Ready) == LobbyData.Yes ?
                    "<color=green>Ready</color>" :
                    "<color=red>Not Ready</color>";
        }

        if (FPClient.Instance.Lobby.IsOwner)
        {
            var playerIDs = FPClient.Instance.Lobby.GetMemberIDs();

            for (int i = 0; i < playerIDs.Length; i++)
                if (LobbyExtension.GetMemberData(playerIDs[i], LobbyData.Ready) == LobbyData.No)
                {
                    StartServerButton.interactable = false;
                    return;
                }

            StartServerButton.interactable = true;
        }
    }

    void LobbyStateChanged(Lobby.MemberStateChange stateChange, ulong initiatorID, ulong affectedID)
    {
        var initiatorName = FPClient.Instance.Friends.GetName(initiatorID);
        var affectedName = FPClient.Instance.Friends.GetName(affectedID);
        var message = string.Empty;

        if (stateChange == Lobby.MemberStateChange.Entered)
        {
            message = $"{initiatorName} has joined.";
            AddPlayer(initiatorID);
        }
        else
        {
            if (stateChange == Lobby.MemberStateChange.Left) message = $"{initiatorName} has left";
            if (stateChange == Lobby.MemberStateChange.Disconnected) message = $"{initiatorName} has disconnected";
            if (stateChange == Lobby.MemberStateChange.Kicked) message = $"{initiatorName} has been kicked";
            if (stateChange == Lobby.MemberStateChange.Banned) message = $"{initiatorName} has been banned";

            RemovePlayer(initiatorID);
        }

        LobbyExtension.SendChatMessage(message);
    }

    void AddPlayer(ulong steamID)
    {
        var prefab = Instantiate(PlayerTextPrefab, PlayerTextGroup.transform);

        playerTexts.Add(steamID, prefab.GetComponent<TextMeshProUGUI>());
        playerTexts[steamID].text = $"{FPClient.Instance.Friends.GetName(steamID)}";

        FPClient.Instance.Friends.GetAvatar(Friends.AvatarSize.Medium, steamID, LoadAvatar);

        var levelText = playerTexts[steamID].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        var readyText = playerTexts[steamID].transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        levelText.text = $"Lv.{LobbyExtension.GetMemberData(steamID, LobbyData.Level)}";
        readyText.text = "Not Ready";

        void LoadAvatar(Facepunch.Steamworks.Image image)
        {
            if (image == null)
                return;

            var texture = new Texture2D(image.Width, image.Height);

            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    var pixel = image.GetPixel(x, y);

                    texture.SetPixel(x, image.Height - y, new UnityEngine.Color(pixel.r / 255.0f, pixel.g / 255.0f, pixel.b / 255.0f, pixel.a / 255.0f));
                }

            texture.Apply();
            playerTexts[steamID].transform.GetChild(2).GetComponent<RawImage>().texture = texture;
        }
    }

    void RemovePlayer(ulong steamID)
    {
        if (playerTexts.TryGetValue(steamID, out var text))
        {
            Destroy(text.gameObject);
            playerTexts.Remove(steamID);
        }
    }
}
