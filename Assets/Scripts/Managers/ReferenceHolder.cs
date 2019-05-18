using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Data;
using Game.Enums;
using Game.Data.Databases;
using Game.Systems;
using Game.UI;
using Game.Data.Settings;
using NetworkPlayer = Game.Systems.Network.NetworkPlayer;
using Game.Utility;
using Game.Systems.Network;

namespace Game.Managers
{
    public class ReferenceHolder : SingletonDDOL<ReferenceHolder>
    {
        public event Action ReferencesSet;

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

        protected override void Awake()
        {
            base.Awake();
            GC.Collect();
        }

        void Start()
        {
            GameManager.Instance.StateChanged += OnGameStateChanged;

            void OnGameStateChanged(GameState e)
            {
                var inGame =
                    e == GameState.InGameMultiplayer ||
                    e == GameState.InGameSingleplayer;

                if (inGame)
                {
                    GetReferences();
                    return;
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


                    if (GameManager.Instance.GameState == GameState.InGameSingleplayer)
                    {
                        ReferencesSet?.Invoke();
                    }

                }
            }
        }
    }
}