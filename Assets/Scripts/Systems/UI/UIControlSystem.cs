using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;
using FPClient = Facepunch.Steamworks.Client;

public class UIControlSystem : MonoBehaviour
{
    public InputField ChatInputField;
    public event EventHandler IncreaseLevelButtonClicked = delegate { };
    public GameObject TextPrefab, ChatTextPrefab, ChatTextGroup;
    public Button IncreaseLevelButton, QuitButton;


    private List<Text> textPrefabs = new List<Text>();
    private List<PlayerData> playerDatas = new List<PlayerData>();
    private List<string> playerNames = new List<string>();
    private List<InputField> chatTexts = new List<InputField>();

    private void Awake()
    {
        FPClient.Instance.Lobby.OnChatStringRecieved = ChatMessageReceived;
        IncreaseLevelButton.onClick.AddListener(() => IncreaseLevelButtonClicked?.Invoke(null, null));
        QuitButton.onClick.AddListener(QuitGame);
    }

    private void QuitGame()
    {
        var manager = NetworkManager.singleton as ExtendedNetworkManager;

        if (FPClient.Instance.Lobby.CurrentLobby == 0)
        {
            manager.StopHost();
            manager.StopClient();
            return;
        }

        if (FPClient.Instance.Lobby.IsOwner)
            manager.StopHost();

        manager.StopClient();
    }

    private void OnDestroy()
    {
        IncreaseLevelButtonClicked = null;
    }

    public void UpdateUI(PlayerData[] datas, string[] names)
    {
        playerDatas.Clear();
        playerNames.Clear();

        for (int i = 0; i < datas.Length; i++)
            if (datas[i].IsNotEmpty)
            {
                playerDatas.Add(datas[i]);
                playerNames.Add(names[i]);
            }

        AddNewLines();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            LobbyExtension.SendChatMessage(ChatInputField);
    }

    private void ChatMessageReceived(ulong senderID, string message)
    {
        var prefab = Instantiate(ChatTextPrefab, ChatTextGroup.transform);

        chatTexts.Add(prefab.GetComponent<InputField>());
        chatTexts[chatTexts.Count - 1].text = message;
    }

    void AddNewLines()
    {
        for (int i = 0; i < textPrefabs.Count; i++)
            Destroy(textPrefabs[i].gameObject);

        textPrefabs.Clear();

        for (int i = textPrefabs.Count; i < playerDatas.Count; i++)
        {
            var prefab = Instantiate(TextPrefab, transform.GetChild(0));
            textPrefabs.Add(prefab.GetComponent<Text>());
        }

        for (int i = 0; i < playerDatas.Count; i++)
            textPrefabs[i].text = $"{playerNames[i]} Lv.{playerDatas[i].Level}";
    }
}
