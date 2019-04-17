using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;
using FPClient = Facepunch.Steamworks.Client;
using TMPro;

public class UIControlSystem : MonoBehaviour
{
    public TMP_InputField ChatInputField;
    public event EventHandler IncreaseLevelButtonClicked = delegate { };
    public GameObject TextPrefab, ChatTextPrefab, ChatTextGroup;
    public Button IncreaseLevelButton, QuitButton;


    List<TextMeshProUGUI> textPrefabs = new List<TextMeshProUGUI>();
    List<PlayerData> playerDatas = new List<PlayerData>();
    List<string> playerNames = new List<string>();
    List<TMP_InputField> chatTexts = new List<TMP_InputField>();

    void Awake()
    {
        if (FPClient.Instance != null)
            FPClient.Instance.Lobby.OnChatStringRecieved = ChatMessageReceived;
        IncreaseLevelButton.onClick.AddListener(() => IncreaseLevelButtonClicked?.Invoke(null, null));
        QuitButton.onClick.AddListener(QuitGame);
    }

    void QuitGame()
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

    void OnDestroy()
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            LobbyExt.SendChatMessage(ChatInputField);
    }

    void ChatMessageReceived(ulong senderID, string message)
    {
        var prefab = Instantiate(ChatTextPrefab, ChatTextGroup.transform);

        chatTexts.Add(prefab.GetComponent<TMP_InputField>());
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
            textPrefabs.Add(prefab.GetComponent<TextMeshProUGUI>());
        }

        for (int i = 0; i < playerDatas.Count; i++)
            textPrefabs[i].text = $"{playerNames[i]} Lv.{playerDatas[i].Level}";
    }
}
