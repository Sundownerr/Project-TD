using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;
using FPClient = Facepunch.Steamworks.Client;
using TMPro;
using Game.Utility;
using Game.Managers;

namespace Game.Systems
{
    public class UIControlSystem : MonoBehaviour
    {
        public TMP_InputField ChatInputField;
        public event Action IncreaseLevelButtonClicked;
        public GameObject TextPrefab, ChatTextPrefab, ChatTextGroup;
        public Button IncreaseLevelButton, QuitButton;


        List<TextMeshProUGUI> textPrefabs = new List<TextMeshProUGUI>();
        List<UserData> userDatas = new List<UserData>();
        List<string> userNames = new List<string>();
        List<TMP_InputField> chatTexts = new List<TMP_InputField>();

        void Awake()
        {
            if (FPClient.Instance != null)
            {
                FPClient.Instance.Lobby.OnChatStringRecieved = ChatMessageReceived;
            }

            IncreaseLevelButton.onClick.AddListener(OnIncreaseLevelClick);
            QuitButton.onClick.AddListener(QuitGame);

            void OnIncreaseLevelClick() => IncreaseLevelButtonClicked?.Invoke();

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
                {
                    manager.StopHost();
                }

                manager.StopClient();
            }

            void ChatMessageReceived(ulong senderID, string message)
            {
                var prefab = Instantiate(ChatTextPrefab, ChatTextGroup.transform);

                chatTexts.Add(prefab.GetComponent<TMP_InputField>());
                chatTexts[chatTexts.Count - 1].text = message;
            }
        }

        void OnDestroy()
        {
            IncreaseLevelButtonClicked = null;
        }

        public void UpdateUI(UserData[] datas, string[] names)
        {
            userDatas.Clear();
            userNames.Clear();

            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i].IsNotEmpty)
                {
                    userDatas.Add(datas[i]);
                    userNames.Add(names[i]);
                }
            }

            AddNewLines();

            void AddNewLines()
            {
                for (int i = 0; i < textPrefabs.Count; i++)
                {
                    Destroy(textPrefabs[i].gameObject);
                }

                textPrefabs.Clear();

                for (int i = textPrefabs.Count; i < userDatas.Count; i++)
                {
                    var prefab = Instantiate(TextPrefab, transform.GetChild(0));
                    textPrefabs.Add(prefab.GetComponent<TextMeshProUGUI>());
                }

                for (int i = 0; i < userDatas.Count; i++)
                {
                    textPrefabs[i].text = $"{userNames[i]} Lv.{userDatas[i].Level}";
                }
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                LobbyExt.SendChatMessage(ChatInputField);
            }
        }
    }
}