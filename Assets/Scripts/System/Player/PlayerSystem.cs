using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Game.Data;
using U = UnityEngine.Object;
using NetworkPlayer = Game.Systems.Network.NetworkPlayer;
using Game.UI;
using Game.Managers;
using Game.Systems.Spirit;
using Game.Systems.Enemy;
using Game.Data.Mage;
using Game.Systems.Mage;
using Game.Data.SpiritEntity;
using Game.Data.Player;
using Game.Systems.Waves;

namespace Game.Systems
{
    public struct PlayerSystemData
    {
        public PlayerMap Map;
        public MageData Mage;
    }

    public class PlayerSystem : IEntitySystem
    {
        public IEntitySystem Owner { get; set; }
        public PlayerInputSystem PlayerInputSystem { get; set; }
        public Cells.ControlSystem CellControlSystem { get; private set; }
        public Enemy.ControlSystem EnemyControlSystem { get; private set; }
        public EnemyUISystem EnemyUISystem { get; private set; }
        public Spirit.PlaceSystem SpiritPlaceSystem { get; set; }
        public Spirit.ControlSystem SpiritControlSystem { get; private set; }
        public Spirit.CreatingSystem SpiritCreatingSystem { get; private set; }
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
        public MageSystem MageHeroSystem { get; private set; }
        public DescriptionUISystem DescriptionUISystem { get; private set; }
        public List<SpiritSystem> OwnedSpirits { get => SpiritControlSystem.OwnedSpirits; }
        public List<EnemySystem> OwnedEnemies { get => EnemyControlSystem.OwnedEnemies; }
        public List<SpiritData> AvailableSpirits;
        public Player Data { get; private set; }
        public Canvas UICanvas { get; private set; }
        public Canvas WorldCanvas { get; private set; }
        public NetworkPlayer NetworkPlayer;
        public int ItemsCount { get; set; }
        public int WaveAmount { get; set; }
        bool isSet;

        public PlayerMap Map
        {
            get => map;
            private set
            {
                map = value;
                map.Owner = this;
            }
        }
        PlayerMap map;

        public PlayerSystem(PlayerMap map, MageData mage)
        {
            WaveAmount = 100;
            Map = map;
            NetworkPlayer = GameData.Instance.NetworkPlayer;

            AvailableSpirits = new List<SpiritData>();
            Data = new Player();
            Data.System = this;

            MageHeroSystem = new MageSystem(this, mage);
            EnemyControlSystem = new Enemy.ControlSystem(this);
            SpiritControlSystem = new Spirit.ControlSystem(this);
            CellControlSystem = new Cells.ControlSystem(this);
            SpiritPlaceSystem = new PlaceSystem(this);
            SpiritCreatingSystem = new Spirit.CreatingSystem(this);
            ResourceSystem = new ResourceSystem(this);
            ItemDropSystem = new ItemDropSystem(this);
            WaveSystem = new WaveSystem(this);
            ElementSystem = new ElementSystem(this);
            InventorySystem = new InventorySystem(this);

            UICanvas = ReferenceHolder.Instance.UICanvas;
            WorldCanvas = ReferenceHolder.Instance.WorldCanvas;

            PlayerInputSystem = U.Instantiate(ReferenceHolder.Instance.PlayerInputSystem.gameObject).GetComponent<PlayerInputSystem>();
            InventoryUISystem = U.Instantiate(ReferenceHolder.Instance.InventoryUISystem.gameObject, UICanvas.transform).GetComponent<InventoryUISystem>();
            ResourceUISystem = U.Instantiate(ReferenceHolder.Instance.ResourceUISystem.gameObject, UICanvas.transform).GetComponent<ResourceUISystem>();

            SpiritUISystem = U.Instantiate(ReferenceHolder.Instance.SpiritUISystem.gameObject, UICanvas.transform).GetComponent<SpiritUISystem>();
            BuildUISystem = U.Instantiate(ReferenceHolder.Instance.BuildUISystem.gameObject, WorldCanvas.transform).GetComponentInChildren<BuildUISystem>();
            ElementUISystem = BuildUISystem.transform.parent.GetComponentInChildren<ElementUISystem>();
            WaveUISystem = U.Instantiate(ReferenceHolder.Instance.WaveUISystem.gameObject, UICanvas.transform).GetComponent<WaveUISystem>();
            EnemyUISystem = U.Instantiate(ReferenceHolder.Instance.EnemyUISystem.gameObject, WorldCanvas.transform).GetComponent<EnemyUISystem>();
            WorldUISystem = U.Instantiate(ReferenceHolder.Instance.WorldUISystem.gameObject, WorldCanvas.transform).GetComponent<WorldUISystem>();
            DescriptionUISystem = U.Instantiate(ReferenceHolder.Instance.DescriptionUISystem.gameObject, UICanvas.transform).GetComponent<DescriptionUISystem>();

            SetSystem();
        }

        void SetSystem()
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

            ReferenceHolder.Instance.StartCoroutine(SetCameraPos());
            isSet = true;

            IEnumerator SetCameraPos()
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
    }
}
