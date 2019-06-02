using System.IO;
using UnityEngine;
using Game.Utility;
using Game.Enums;
using Game.Consts;
using Game.Data.Mage;
using Game.Systems;
using System;
using Game.Data;
using NetworkPlayer = Game.Systems.Network.NetworkPlayer;

namespace Game.Managers
{
    public class GameData : SingletonDDOL<GameData>
    {
        public event Action<PlayerSystem> PlayerDataSet;

        NetworkPlayer networkPlayer;
        public NetworkPlayer NetworkPlayer
        {
            get => networkPlayer;
            set
            {
                networkPlayer = value;
                SetPlayerData();
            }
        }

        public UserData UserData { get; private set; }
        public MageData ChoosedMage { get;  set; }
        public PlayerSystem Player { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            LoadData();

            void LoadData()
            {
                if (!File.Exists("playerData.json"))
                {
                    var newFile = JsonUtility.ToJson(UserData);

                    UserData = new UserData(0);
                    File.WriteAllText("playerData.json", newFile);
                }
                else
                {
                    var stringFromFile = File.ReadAllText("playerData.json");
                    var playerDataFromFile = JsonUtility.FromJson<UserData>(stringFromFile);

                    UserData = new UserData(playerDataFromFile.Level);
                }
            }
        }

        void Start()
        {
            UIManager.Instance.StateChanged += OnUIStateChanged;
            UIManager.Instance.MageSelected += OnMageSelected;
            ReferenceHolder.Instance.ReferencesSet += OnReferencesSet;

            void OnMageSelected(MageData e) => ChoosedMage = e;

            void OnReferencesSet() => SetPlayerData();

            void OnUIStateChanged(UIState e)
            {
                if (e == UIState.InLobby)
                {
                    LobbyExt.SetMemberData(LobbyData.Level, UserData.Level.ToString());
                    return;
                }
            }
        }

        void SetPlayerData()
        {
            var playerData = new PlayerSystemData();

            playerData.Map = GameManager.Instance.GameState == GameState.InGameMultiplayer ?
                NetworkPlayer.LocalMap.GetComponent<PlayerMap>() :
                GameObject.FindGameObjectWithTag("map").GetComponent<PlayerMap>();

            playerData.Mage = ChoosedMage;

            Player = new PlayerSystem(playerData.Map, playerData.Mage);

            if (NetworkPlayer != null)
            {
                NetworkPlayer.LocalPlayer = Player;
            }

            PlayerDataSet?.Invoke(Player);
        }

        public void SaveData(UserData data)
        {
            UserData = data;

            var newFile = JsonUtility.ToJson(data);
            File.WriteAllText("playerData.json", newFile);
        }
    }
}