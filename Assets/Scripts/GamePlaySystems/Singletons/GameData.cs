using System.IO;
using UnityEngine;
using Game.Utility;
using Game.Enums;
using Game.Consts;
using Game.Data.Mage;

namespace Game.Managers
{
    public class GameData : SingletonDDOL<GameData>
    {
        public PlayerData PlayerData { get; private set; }
        public MageData ChoosedMage;

        protected override void Awake()
        {
            base.Awake();

            LoadData();
        }

        void Start()
        {
            GameManager.Instance.StateChanged += OnGameStateChanged;
            UIManager.Instance.MageSelected += OnMageSelected;
        }

        void OnMageSelected(object _, MageData e) => ChoosedMage = e;
        void OnGameStateChanged(object _, GameState e)
        {
            if (e == GameState.InLobby)
            {
                LobbyExt.SetMemberData(LobbyData.Level, PlayerData.Level.ToString());
            }
        }

        void LoadData()
        {
            PlayerData data;

            if (!File.Exists("playerData.json"))
            {
                data = new PlayerData(0);

                var newFile = JsonUtility.ToJson(data);
                File.WriteAllText("playerData.json", newFile);
            }
            else
            {
                var stringFromFile = File.ReadAllText("playerData.json");
                var playerDataFromFile = JsonUtility.FromJson<PlayerData>(stringFromFile);

                data = new PlayerData(playerDataFromFile.Level);
            }

            PlayerData = new PlayerData(data.Level);
        }

        public void SaveData(PlayerData data)
        {
            PlayerData = data;

            var newFile = JsonUtility.ToJson(data);
            File.WriteAllText("playerData.json", newFile);
        }
    }
}