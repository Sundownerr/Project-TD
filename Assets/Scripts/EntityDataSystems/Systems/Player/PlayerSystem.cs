using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Spirit;
using Game.Enemy;
using Game.Spirit.Data;
using Game.Data;
using Mirror;
using FPClient = Facepunch.Steamworks.Client;
using System;
using U = UnityEngine.Object;

namespace Game.Systems
{
    public struct PlayerData
    {
        public PlayerMap Map;
        public MageData Mage;
    }

    public class PlayerSystem : IEntitySystem
    {
        public ID ID { get; private set; }
        public IEntitySystem Owner { get; set; }
        public PlayerInputSystem PlayerInputSystem { get; set; }
        public CellControlSystem CellControlSystem { get; private set; }
        public EnemyControlSystem EnemyControlSystem { get; private set; }
        public EnemyUISystem EnemyUISystem { get; private set; }
        public SpiritPlaceSystem SpiritPlaceSystem { get; set; }
        public SpiritControlSystem SpiritControlSystem { get; private set; }
        public SpiritCreatingSystem SpiritCreatingSystem { get; private set; }
        public SpiritUISystem SpiritUISystem { get; private set; }
        public ElementSystem ElementSystem { get; private set; }
        public ElementUISystem ElementUISystem { get; private set; }
        public WaveSystem WaveSystem { get; private set; }
        public WaveUISystem WaveUISystem { get; private set; }
        public ResourceSystem ResourceSystem { get; private set; }
        public ResourceUISystem ResourceUISystem { get; private set; }
        public InventorySystem InventorySystem { get; private set; }
        public InventoryUISystem InventoryUISystem { get; private set; }
        public BuildUISystem BuildUISystem { get; private set; }
        public ItemDropSystem ItemDropSystem { get; private set; }
        public WorldUISystem WorldUISystem { get; private set; }
        public MageHeroSystem MageHeroSystem { get; private set; }
        public DescriptionUISystem DescriptionUISystem { get; private set; }
        public List<SpiritSystem> Spirits { get => SpiritControlSystem.Spirits; }
        public List<EnemySystem> Enemies { get => EnemyControlSystem.Enemies; }
        public List<SpiritData> AvailableSpirits;
        public Player Data { get; private set; }
        public Canvas UICanvas { get; private set; }
        public Canvas WorldCanvas { get; private set; }
        public NetworkPlayer NetworkPlayer;
        public int ItemsCount { get; set; }
        public int WaveAmount { get; set; }
        private bool isSet;

        public PlayerMap Map
        {
            get => map;
            private set
            {
                map = value;
                map.Owner = this;
            }
        }
        private PlayerMap map;

        public PlayerSystem(PlayerMap map, MageData mage)
        {
            WaveAmount = 100;
            Map = map;
            ReferenceHolder.Get.Player = this;
            NetworkPlayer = ReferenceHolder.Get.NetworkPlayer;

            AvailableSpirits = new List<SpiritData>();
            Data = ScriptableObject.CreateInstance<Player>();
            Data.System = this;

            MageHeroSystem = new MageHeroSystem(this, mage);
            EnemyControlSystem = new EnemyControlSystem(this);
            SpiritControlSystem = new SpiritControlSystem(this);
            CellControlSystem = new CellControlSystem(this);
            SpiritPlaceSystem = new SpiritPlaceSystem(this);
            SpiritCreatingSystem = new SpiritCreatingSystem(this);
            ResourceSystem = new ResourceSystem(this);
            ItemDropSystem = new ItemDropSystem(this);
            WaveSystem = new WaveSystem(this);
            ElementSystem = new ElementSystem(this);
            InventorySystem = new InventorySystem(this);

            UICanvas = ReferenceHolder.Get.UICanvas;
            WorldCanvas = ReferenceHolder.Get.WorldCanvas;

            PlayerInputSystem = U.Instantiate(ReferenceHolder.Get.PlayerInputSystem.gameObject).GetComponent<PlayerInputSystem>();
            InventoryUISystem = U.Instantiate(ReferenceHolder.Get.InventoryUISystem.gameObject, UICanvas.transform).GetComponent<InventoryUISystem>();
            ResourceUISystem = U.Instantiate(ReferenceHolder.Get.ResourceUISystem.gameObject, UICanvas.transform).GetComponent<ResourceUISystem>();
            
            SpiritUISystem = U.Instantiate(ReferenceHolder.Get.SpiritUISystem.gameObject, UICanvas.transform).GetComponent<SpiritUISystem>();
            BuildUISystem = U.Instantiate(ReferenceHolder.Get.BuildUISystem.gameObject, WorldCanvas.transform).GetComponentInChildren<BuildUISystem>();
            ElementUISystem = BuildUISystem.transform.parent.GetComponentInChildren<ElementUISystem>();
            WaveUISystem = U.Instantiate(ReferenceHolder.Get.WaveUISystem.gameObject, UICanvas.transform).GetComponent<WaveUISystem>();
            EnemyUISystem = U.Instantiate(ReferenceHolder.Get.EnemyUISystem.gameObject, WorldCanvas.transform).GetComponent<EnemyUISystem>();
            WorldUISystem = U.Instantiate(ReferenceHolder.Get.WorldUISystem.gameObject, WorldCanvas.transform).GetComponent<WorldUISystem>();
            DescriptionUISystem = U.Instantiate(ReferenceHolder.Get.DescriptionUISystem.gameObject, UICanvas.transform).GetComponent<DescriptionUISystem>();

            SetSystem();
        }

        private void SetSystem()
        {
            PlayerInputSystem.SetSystem(this);
            SpiritUISystem.SetSystem(this);
            EnemyControlSystem.SetSystem();
            CellControlSystem.SetSystem();
            SpiritPlaceSystem.SetSystem();
            SpiritCreatingSystem.SetSystem();
            ResourceSystem.SetSystem();
            ItemDropSystem.SetSystem();
            WaveSystem.SetSystem();
            WaveUISystem.SetSystem(this);
            InventorySystem.SetSystem();
            WorldUISystem.SetSystem(this);
            ResourceUISystem.SetSystem(this);
            ElementUISystem.SetSystem(this);
            ElementSystem.SetSystem();
            BuildUISystem.SetSystem(this);
            EnemyUISystem.SetSystem(this);
            InventoryUISystem.SetSystem(this);
            DescriptionUISystem.SetSystem(this);
            SpiritControlSystem.SetSystem();
            MageHeroSystem.SetSystem();

            ReferenceHolder.Get.StartCoroutine(SetCameraPos());
            isSet = true;
        }

        public void UpdateSystem()
        {
            if (isSet)
            {
                EnemyControlSystem.UpdateSystem();
                SpiritControlSystem.UpdateSystem();
                CellControlSystem.UpdateSystem();

                WaveSystem.UpdateSystem();
            }
        }

        private IEnumerator SetCameraPos()
        {
            var cameraObject = Camera.main.transform.parent;
            var cameraPos = new Vector3(Map.transform.position.x, cameraObject.position.y, cameraObject.position.z);
            var cinemachineCamera = cameraObject.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>().gameObject;

            cinemachineCamera.SetActive(false);
            cameraObject.position = cameraPos;

            yield return new WaitForSeconds(0.1f);
            cinemachineCamera.SetActive(true);


        }
    }
}
