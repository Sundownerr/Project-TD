﻿using System.Collections.Generic;
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
        public Transform CellParent;
        public Transform SpiritParent;
        public Transform EnemyParent;
        public Canvas UICanvas;
        public Canvas WorldCanvas;

        public PlayerSystem Player;
        public NetworkPlayer NetworkPlayer;
        public GameObject MpMap;

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


        private void Awake()
        {
            DontDestroyOnLoad(this);
            Get = this;
            Player = null;
            GameManager.Instance.StateChanged += OnGameStateChanged;
        }

        private void OnGameStateChanged(object sender, GameState e)
        {
            Coroutine networkCoroutine = null;

            if (e == GameState.MultiplayerInGame)
            {
                networkCoroutine = StartCoroutine(WaitNetworkStart());
                return;
            }

            if (e == GameState.SingleplayerInGame)
            {
                GetReferences();
                return;
            }

            Player = null;

            #region Helper functions

            IEnumerator WaitNetworkStart()
            {
                var endOfFrame = new WaitForEndOfFrame();

                while (Get.NetworkPlayer == null)
                    yield return endOfFrame;

                GetReferences();
                StopCoroutine(networkCoroutine);
            }

            #endregion
        }

        private void Start()
        {
            Player = null;
            GC.Collect();
        }

        private void GetReferences()
        {

            UICanvas = GameObject.FindWithTag("UICanvas").GetComponent<Canvas>();
            WorldCanvas = GameObject.FindWithTag("WorldCanvas").GetComponent<Canvas>();
            CellParent = GameObject.FindWithTag("CellParent").transform;
            SpiritParent = GameObject.FindWithTag("SpiritParent").transform;
            EnemyParent = GameObject.FindWithTag("EnemyParent").transform;

            DescriptionUISystem = Instantiate(DescriptionUISystem, UICanvas.transform);

            PlayerMap map = null;

            if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
                map = NetworkPlayer.LocalMap.GetComponent<PlayerMap>();
            else
                map = GameObject.FindGameObjectWithTag("map").GetComponent<PlayerMap>();

            Player = new PlayerSystem(map);

            if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
            {
                Debug.Log("set local pls");
                NetworkPlayer.LocalPlayer = Player;
            }
        }

        private void FixedUpdate()
        {
            
            Player?.UpdateSystem();
        }
    }
}