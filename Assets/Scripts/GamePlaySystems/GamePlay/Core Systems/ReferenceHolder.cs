using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Data;
using Game.Enums;
using Game.Data.Databases;
using Game.Systems;
using Game.UI;
using Game.Data.Settings;
using Game.Network;
using NetworkPlayer = Game.Network.NetworkPlayer;

namespace Game.Managers
{
    public class ReferenceHolder : MonoBehaviour
    {
        public event EventHandler<PlayerSystemData> PlayerDataSet;
        static ReferenceHolder get;
        public static ReferenceHolder Get
        {
            get => get;
            private set
            {
                if (get == null)
                    get = value;
            }
        }

        public GameObject[] ElementPlaceEffects;
        public GameObject CellPrefab;
        public GameObject RangePrefab;
        public GameObject LevelUpEffect;
        public GameObject ItemPrefab;
        public PlayerInputSystem PlayerInputSystem;
        public WaveSystem WaveSystem;
        public ResourceUISystem ResourceUISystem;
        public SpiritUISystem SpiritUISystem;
        public ElementUISystem ElementUISystem;
        public SpiritPlaceSystem SpiritPlaceSystem;
        public GameObject BuildUISystem;
        public WaveUISystem WaveUISystem;
        public InventoryUISystem InventoryUISystem;
        public EnemyUISystem EnemyUISystem;
        public WorldUISystem WorldUISystem;
        public DescriptionUISystem DescriptionUISystem;
        public SpiritDataBase SpiritDB;
        public AbilityDatabase AbilityDB;
        public TraitDatabase TraitDB;
        public EnemyDataBase EnemyDB;
        public WaveDataBase WaveDB;
        public ItemDataBase ItemDB;
        public EnemySettings EnemySettings;
        public DamageToArmorSettings DamageToArmorSettings;
        public Transform CellParent;
        public Transform SpiritParent;
        public Transform EnemyParent;
        public Canvas UICanvas;
        public Canvas WorldCanvas;
        public PlayerSystem Player;
        public GameObject NetworkEnemy;

        DescriptionUISystem descriptionUISystem;

        public static List<int> ExpToLevelUp { get; } = new List<int>(25)
        {
            12,
            24,
            37,
            51,
            66,
            82,
            99,
            117,
            136,
            156,
            177,
            199,
            223,
            248,
            275,
            303,
            333,
            365,
            399,
            435,
            473,
            513,
            556,
            601,
            649
        };

        NetworkPlayer networkPlayer;
        public NetworkPlayer NetworkPlayer
        {
            get => networkPlayer;
            set
            {
                if (networkPlayer == null)
                {
                    networkPlayer = value;
                    GetReferences();
                }
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            Get = this;
            GC.Collect();
        }

        void Start()
        {
            GameManager.Instance.StateChanged += OnGameStateChanged;
            GameLoop.Instance.PlayerCreated += OnPlayerCreated;
        }


        void OnPlayerCreated(object _, PlayerSystem e)
        {
            Player = e;

            if (GameManager.Instance.GameState == GameState.InGameMultiplayer)
                NetworkPlayer.LocalPlayer = e;

        }

        void OnGameStateChanged(object _, GameState e)
        {
            if (e == GameState.InGameSingleplayer)
            {
                GetReferences();
                return;
            }

            Player = null;
        }

        void GetReferences()
        {
            if (descriptionUISystem == null)
                descriptionUISystem = DescriptionUISystem;

            UICanvas = GameObject.FindWithTag("UICanvas").GetComponent<Canvas>();
            WorldCanvas = GameObject.FindWithTag("WorldCanvas").GetComponent<Canvas>();
            CellParent = GameObject.FindWithTag("CellParent").transform;
            SpiritParent = GameObject.FindWithTag("SpiritParent").transform;
            EnemyParent = GameObject.FindWithTag("EnemyParent").transform;

            DescriptionUISystem = Instantiate(descriptionUISystem, UICanvas.transform);

            PlayerSystemData playerData;

            playerData.Map = GameManager.Instance.GameState == GameState.InGameMultiplayer ?
                NetworkPlayer.LocalMap.GetComponent<PlayerMap>() :
                GameObject.FindGameObjectWithTag("map").GetComponent<PlayerMap>();

            playerData.Mage = GameData.Instance.ChoosedMage;

            PlayerDataSet?.Invoke(null, playerData);
        }
    }
}