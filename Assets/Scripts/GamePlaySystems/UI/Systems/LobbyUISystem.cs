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
    public TextMeshProUGUI MaxPlayersText;
    public Slider PlayersSlider;
    public TMP_Dropdown ModeDropdown, VisibilityDropdown, DifficultyDropdown, MapDropdown, WavesDropdown;
    public TMP_InputField ChatInputField;
    public GameObject PlayerTextPrefab, ChatTextPrefab, PlayerTextGroup, ChatTextGroup, LobbyList;
    public Button ReadyButton, StartServerButton;
    public TextMeshProUGUI LobbyName;

    Dictionary<ulong, TextMeshProUGUI> playerTexts = new Dictionary<ulong, TextMeshProUGUI>();
    ObjectPool chatTextsPool;
    LobbyDataChanger lobbyDataChanger;

    void Start()
    {
        defaultYs = new float[] { transform.GetChild(0).localPosition.y };
        chatTextsPool = new ObjectPool(ChatTextPrefab, ChatTextGroup.transform, 5);

        ReadyButton.onClick.AddListener(() => LobbyExt.SetMemberData(LobbyData.Ready,
           LobbyExt.GetMemberData(FPClient.Instance.SteamId, LobbyData.Ready) == LobbyData.Yes ? LobbyData.No : LobbyData.Yes));

        StartServerButton.onClick.AddListener(LobbyExt.StartLobbyServer);
       
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) LobbyExt.SendChatMessage(ChatInputField);
    }

    void OnDestroy()
    {
        lobbyDataChanger.Destroy();
        lobbyDataChanger = null;
    }

    public override void Open(float timeToComplete = NumberConsts.UIAnimSpeed)
    {
        lobbyDataChanger = new LobbyDataChanger(this);
        LobbyName.text = FPClient.Instance.Lobby.Name;

        LobbyExt.SetCallbacks(
            new LobbyCallbacks(
               LobbyStateChanged,
               LobbyMemberDataUpdated,
               LobbyDataUpdated,
               ChatMessageReceived
            ));

        UpdatePlayers();
        UpdateLobbyData();

        
        base.Open(timeToComplete);

        #region Helper functions

        void UpdatePlayers()
        {
            var playerIDs = FPClient.Instance.Lobby.GetMemberIDs();
            for (int i = 0; i < playerIDs.Length; i++) AddPlayer(playerIDs[i]);
        }

        void UpdateLobbyData()
        {
            if (!FPClient.Instance.Lobby.IsOwner)
                StartServerButton.gameObject.SetActive(false);
            else
            {
                LobbyExt.SetData(LobbyData.GameStarted, LobbyData.No);
                LobbyExt.SetData(LobbyData.GameStarting, LobbyData.No);
                StartServerButton.gameObject.SetActive(true);
            }
        }

        #endregion
    }

    public override void Close(Move moveTo, float timeToComplete = NumberConsts.UIAnimSpeed)
    {
        base.Close(moveTo);

        foreach (var playerText in playerTexts.Values) Destroy(playerText.gameObject);

        chatTextsPool.DeactivateAll();
        playerTexts.Clear();

        LobbyExt.ClearLobbyCallbacks();
        FPClient.Instance.Lobby.Leave();

        lobbyDataChanger.Destroy();
        lobbyDataChanger = null;
    }

    void ChatMessageReceived(ulong senderID, string message) => CreateChatMessage(message);
    void CreateChatMessage(string message) => chatTextsPool.PopObject().GetComponent<TMP_InputField>().text = message;

    void LobbyDataUpdated()
    {
        if (!FPClient.Instance.Lobby.IsOwner)
        {
            WavesDropdown.value = int.Parse(LobbyExt.GetData(LobbyData.Waves));
            ModeDropdown.value = int.Parse(LobbyExt.GetData(LobbyData.Mode));
            MapDropdown.value = int.Parse(LobbyExt.GetData(LobbyData.Map));
            DifficultyDropdown.value = int.Parse(LobbyExt.GetData(LobbyData.Difficulty));
            PlayersSlider.value = int.Parse(LobbyExt.GetData(LobbyData.MaxPlayers));
            VisibilityDropdown.value = int.Parse(LobbyExt.GetData(LobbyData.Visibility));
        }

        var networkManager = NetworkManager.singleton as ExtendedNetworkManager;

        if (LobbyExt.GetData(LobbyData.GameStarting) == LobbyData.Yes)
            if (!networkManager.IsClientConnected() && !NetworkServer.active)
            {
                networkManager.networkAddress = FPClient.Instance.Lobby.Owner.ToString();
                networkManager.onlineScene = LobbyExt.GetData(LobbyData.Map);
            }

        if (LobbyExt.GetData(LobbyData.GameStarted) == LobbyData.Yes)
        {
            if (!FPClient.Instance.Lobby.IsOwner)
            {
                GameManager.Instance.GameState = GameState.LoadingGame;
                networkManager.StartClient();
            }

            LobbyExt.ClearLobbyCallbacks();
        }
    }

    void LobbyMemberDataUpdated(ulong steamID)
    {
        var levelText = playerTexts[steamID].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        var readyText = playerTexts[steamID].transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        playerTexts[steamID].text = $"{FPClient.Instance.Friends.GetName(steamID)}";
        levelText.text = $"Lv.{LobbyExt.GetMemberData(steamID, LobbyData.Level)}";
        readyText.text = LobbyExt.GetMemberData(steamID, LobbyData.Ready) == LobbyData.Yes ? "<color=green>Ready</color>" : "<color=red>Not Ready</color>";

        if (FPClient.Instance.Lobby.IsOwner)
        {
            var playerIDs = FPClient.Instance.Lobby.GetMemberIDs();

            for (int i = 0; i < playerIDs.Length; i++)
                if (LobbyExt.GetMemberData(playerIDs[i], LobbyData.Ready) == LobbyData.No)
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

        if (stateChange == Lobby.MemberStateChange.Entered)
        {
            LobbyExt.SendChatMessage($"{initiatorName} has joined.");
            AddPlayer(initiatorID);
        }
        else
        {
            if (stateChange == Lobby.MemberStateChange.Left) LobbyExt.SendChatMessage($"{initiatorName} has left");
            else
            if (stateChange == Lobby.MemberStateChange.Disconnected) LobbyExt.SendChatMessage($"{initiatorName} has disconnected");
            else
            if (stateChange == Lobby.MemberStateChange.Kicked) LobbyExt.SendChatMessage($"{initiatorName} has been kicked");
            else
            if (stateChange == Lobby.MemberStateChange.Banned) LobbyExt.SendChatMessage($"{initiatorName} has been banned");

            Destroy(playerTexts[initiatorID].gameObject);
            playerTexts.Remove(initiatorID);
        }
    }

    void AddPlayer(ulong steamID)
    {

        if (playerTexts.ContainsKey(steamID))
            return;
        var prefab = Instantiate(PlayerTextPrefab, PlayerTextGroup.transform);


        playerTexts.Add(steamID, prefab.GetComponent<TextMeshProUGUI>());
        playerTexts[steamID].text = $"{FPClient.Instance.Friends.GetName(steamID)}";

        FPClient.Instance.Friends.GetAvatar(Friends.AvatarSize.Medium, steamID, LoadAvatar);

        var levelText = playerTexts[steamID].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        var readyText = playerTexts[steamID].transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        levelText.text = $"Lv.{LobbyExt.GetMemberData(steamID, LobbyData.Level)}";
        readyText.text = "Not Ready";

        LobbyMemberDataUpdated(steamID);

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
}
