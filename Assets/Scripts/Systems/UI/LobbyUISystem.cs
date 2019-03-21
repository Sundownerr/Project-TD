using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facepunch.Steamworks;
using FPClient = Facepunch.Steamworks.Client;
using System.Net;
using Mirror;
using System.Text;
using System.Threading;
using Transport.Steamworks;

public class LobbyUISystem : MonoBehaviour
{
    public InputField ChatInputField;
    public GameObject PlayerTextPrefab, ChatTextPrefab, PlayerTextGroup, ChatTextGroup, LobbyList;
    public Button ReadyButton, LeaveButton, StartServerButton;
    public Text LobbyName, ModeText, DifficultyText, MapText, WavesText;

    private Dictionary<ulong, Text> playerTexts;
    private ObjectPool chatTextsPool;
    private WaitForSeconds delay;

    private void Start()
    {
        delay = new WaitForSeconds(0.5f);
        ReadyButton.onClick.AddListener(SetReady);
        LeaveButton.onClick.AddListener(LeaveLobby);
        StartServerButton.onClick.AddListener(StartLobbyServer);

        chatTextsPool = new ObjectPool(ChatTextPrefab, ChatTextGroup.transform, 5);      
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            LobbyExtension.SendChatMessage(ChatInputField);  
    }

    private void OnEnable()
    {
        playerTexts = playerTexts ?? new Dictionary<ulong, Text>();

        GameManager.Instance.GameState = GameState.InLobby;

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
    }

    private void ChatMessageReceived(ulong senderID, string message) => CreateChatMessage(message);
    
    private void CreateChatMessage(string message) => chatTextsPool.GetObject().GetComponent<InputField>().text = message;    

    private void LobbyDataUpdated()
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
                networkManager.StartClient();
        }
    }

    private void StartLobbyServer()
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
                LobbyExtension.SetData(LobbyData.GameStarted, LobbyData.Yes);
            }
        }
    }

    private void SetReady()
    {
        var isReady = LobbyExtension.GetMemberData(FPClient.Instance.SteamId, LobbyData.Ready) == LobbyData.Yes;

        LobbyExtension.SetMemberData(LobbyData.Ready, isReady ? LobbyData.No : LobbyData.Yes);
    }

    private void LeaveLobby()
    {
        foreach (var pair in playerTexts)
            if (playerTexts.TryGetValue(pair.Key, out var text))
                Destroy(text.gameObject);

        chatTextsPool.DeactivateAll();
        playerTexts.Clear();

        LobbyExtension.ClearLobbyCallbacks();
        FPClient.Instance.Lobby.Leave();
              
        LobbyList.SetActive(true);
        gameObject.SetActive(false);
    }

    private void LobbyMemberDataUpdated(ulong steamID)
    {
        if (playerTexts.TryGetValue(steamID, out var name))
        {
            var levelText = name.transform.GetChild(0).GetComponent<Text>();
            var readyText = name.transform.GetChild(1).GetComponent<Text>();

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

    private void LobbyStateChanged(Lobby.MemberStateChange stateChange, ulong initiatorID, ulong affectedID)
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
            if (stateChange == Lobby.MemberStateChange.Left)
                message = $"{initiatorName} has left";

            if (stateChange == Lobby.MemberStateChange.Disconnected)
                message = $"{initiatorName} has disconnected";

            if (stateChange == Lobby.MemberStateChange.Kicked)
                message = $"{initiatorName} has been kicked";

            if (stateChange == Lobby.MemberStateChange.Banned)
                message = $"{initiatorName} has been banned";

                RemovePlayer(initiatorID);
        }

        LobbyExtension.SendChatMessage(message);
    }

    private void AddPlayer(ulong steamID)
    {
        var prefab = Instantiate(PlayerTextPrefab, PlayerTextGroup.transform);

        playerTexts.Add(steamID, prefab.GetComponent<Text>());
        playerTexts[steamID].text = $"{FPClient.Instance.Friends.GetName(steamID)}";

        FPClient.Instance.Friends.GetAvatar(Friends.AvatarSize.Medium, steamID, LoadAvatar);

        var levelText = playerTexts[steamID].transform.GetChild(0).GetComponent<Text>();
        var readyText = playerTexts[steamID].transform.GetChild(1).GetComponent<Text>();

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

    private void RemovePlayer(ulong steamID)
    {
        if(playerTexts.TryGetValue(steamID, out var text))
        {
            Destroy(text.gameObject);
            playerTexts.Remove(steamID);
        }
    }
}
