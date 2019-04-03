using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Data;
using UnityEngine.EventSystems;
using NaughtyAttributes;
using System.Collections;

namespace Game.Systems
{
    public class ReferenceHolder : MonoBehaviour
    {
        public event EventHandler<PlayerMap> MapAssigned = delegate { };
        private static ReferenceHolder get;
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
        public EnemyDataBase EnemyDataBase;
        public SpiritDataBase SpiritDataBase;
        public WaveDataBase WaveDataBase;
        public EnemyAbilityDataBase EnemyAbilityDataBase;
        public EnemyTraitDataBase EnemyTraitDataBase;
        public ItemDataBase ItemDataBase;
        public EnemySettings EnemySettings;
        public DamageToArmorSettings DamageToArmorSettings;
        public Transform CellParent;
        public Transform SpiritParent;
        public Transform EnemyParent;
        public Canvas UICanvas;
        public Canvas WorldCanvas;
        public PlayerSystem Player;
        public GameObject NetworkEnemy;

        private DescriptionUISystem descriptionUISystem;

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

        private NetworkPlayer networkPlayer;
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

        private void Awake()
        {
            DontDestroyOnLoad(this);
            Get = this;
            GC.Collect();
        }

        private void Start()
        {
            GameManager.Instance.StateChanged += OnGameStateChanged;
            GameLoop.Instance.PlayerCreated += OnPlayerCreated;
        }

        private void OnPlayerCreated(object _, PlayerSystem e)
        {
            Player = e;

            if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
                NetworkPlayer.LocalPlayer = e;
        }

        private void OnGameStateChanged(object _, GameState e)
        {
            if (e == GameState.SingleplayerInGame)
            {
                GetReferences();
                return;
            }

            Player = null;
        }

        private void GetReferences()
        {
            if (descriptionUISystem == null)
                descriptionUISystem = DescriptionUISystem;
                
            UICanvas = GameObject.FindWithTag("UICanvas").GetComponent<Canvas>();
            WorldCanvas = GameObject.FindWithTag("WorldCanvas").GetComponent<Canvas>();
            CellParent = GameObject.FindWithTag("CellParent").transform;
            SpiritParent = GameObject.FindWithTag("SpiritParent").transform;
            EnemyParent = GameObject.FindWithTag("EnemyParent").transform;

            DescriptionUISystem = Instantiate(descriptionUISystem, UICanvas.transform);

            MapAssigned?.Invoke(null, GameManager.Instance.GameState == GameState.MultiplayerInGame ?
                NetworkPlayer.LocalMap.GetComponent<PlayerMap>() :
                GameObject.FindGameObjectWithTag("map").GetComponent<PlayerMap>());
        }
    }
}