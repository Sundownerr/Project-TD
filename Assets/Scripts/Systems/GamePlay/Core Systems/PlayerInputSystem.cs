using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Spirit.Data;
using System;
using Game.Spirit;
using Game.Spirit.Data.Stats;
using Game.Cells;
using Game.Enemy;

namespace Game.Systems
{
    public class PlayerInputSystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public SpiritSystem ChoosedSpirit { get; set; }
        public GameObject Selection;
        public EventSystem EventSystem;

        public event EventHandler MouseOnSpirit = delegate {};
        public event EventHandler PlacingSpirit = delegate {};
        public event EventHandler<SpiritSystem> SpiritSold = delegate{};
        public event EventHandler<SpiritSystem> SpiritUpgraded = delegate{};
        public event EventHandler<GameObject> ClickedOnCell = delegate { };
        public event EventHandler<GameObject> ClickedOnSpirit = delegate { };
        public event EventHandler<GameObject> ClickedOnEnemy = delegate { };
        public event EventHandler ClikedOnGround = delegate { };
        public event EventHandler RMBPresed = delegate { };

        private EnemySystem choosedEnemy;
        private GameObject selection;
        private PointerEventData pointerEventData;
       
        private RaycastHit hit;
        private Ray WorldRay;
        private bool isHitUI, isHitWorldUI;
        private int terrainLayer, enemyLayer, spiritLayer, cellLayer, uiLayer, layerMask;
       
        protected override void Awake()
        {
            base.Awake();

            EventSystem = EventSystem.current;         
            pointerEventData = new PointerEventData(EventSystem);
        
            terrainLayer    = 1 << 9;
            enemyLayer      = 1 << 12;
            spiritLayer      = 1 << 14;
            cellLayer       = 1 << 15;
            uiLayer         = 1 << 5;
            layerMask = terrainLayer | enemyLayer | spiritLayer | cellLayer | uiLayer;
        }

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;

            Owner.SpiritUISystem.Selling += OnSelling;
            Owner.SpiritUISystem.Upgrading += OnUpgrading;
            selection = Instantiate(Selection);
            selection.SetActive(false);
        }

        private void Update() 
        {
            pointerEventData.position = Input.mousePosition;
            
            if (Input.GetMouseButtonDown(0))
            {              
                WorldRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                isHitUI = EventSystem.currentSelectedGameObject != null;

                if (Physics.Raycast(WorldRay, out hit, 100000, layerMask))
                {
                    var isMouseOnSpirit = !isHitUI && hit.transform.gameObject.layer == 14;
                    var isMouseOnGround = !isHitUI && hit.transform.gameObject.layer == 9;
                    var isMouseOnCell = !isHitUI && hit.transform.gameObject.layer == 15;
                    var isMouseOnEnemy = !isHitUI && hit.transform.gameObject.layer == 12;

                    if (isMouseOnSpirit)
                    {                      
                        GetChoosedSpirit();
                        ClickedOnSpirit?.Invoke(null, hit.transform.gameObject);                       
                    }

                    if(isMouseOnEnemy)
                    {
                        GetChoosedEnemy();
                        ClickedOnEnemy?.Invoke(null, hit.transform.gameObject);
                    }

                    if (isMouseOnGround)
                    {
                        ClikedOnGround?.Invoke(null, null);
                        ChoosedSpirit = null;
                        choosedEnemy = null;
                        SetSelection(false);
                    }

                    if (isMouseOnCell)
                    {

                        ClickedOnCell?.Invoke(null, hit.transform.gameObject);
                        ChoosedSpirit = null;
                        choosedEnemy = null;
                        SetSelection(false);
                    }
                    
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                RMBPresed?.Invoke(null, null);
                ChoosedSpirit = null;
                choosedEnemy = null;
                SetSelection(false);
            }

         
            #region Helper functions

            void GetChoosedSpirit()
            {
                for (int i = 0; i < Owner.Spirits.Count; i++)               
                    if (Owner.Spirits[i].Prefab == hit.transform.gameObject)
                    {                      
                        ChoosedSpirit = Owner.Spirits[i];
                        Owner.SpiritUISystem.ActivateUpgradeButton(CheckGradeListOk(out _));
                        ActivateSelection(ChoosedSpirit);
                        return;
                    }               
            }

            void GetChoosedEnemy()
            {
                for (int i = 0; i < Owner.Enemies.Count; i++)
                    if (Owner.Enemies[i].Prefab == hit.transform.gameObject)
                    {
                        choosedEnemy = Owner.Enemies[i];
                        ActivateSelection(Owner.Enemies[i]);
                        return;
                    }                             
            }  

            #endregion
        }


        private void ActivateSelection(IPrefabComponent entity)
        {
            if (selection == null)
                selection = Instantiate(Selection);
            selection.SetActive(true);
            selection.transform.position = entity.Prefab.transform.position;
            selection.transform.SetParent(entity.Prefab.transform);
        }

        private void SetSelection(bool activate)
        {
            if (selection != null)
                selection.SetActive(activate);
        }

        public void OnPlacingNewSpirit(object _, SpiritData spiritData)
        {
            for (int i = 0; i < Owner.AvailableSpirits.Count; i++)           
                if (Owner.AvailableSpirits[i] == spiritData)
                {
                    Owner.AvailableSpirits.RemoveAt(i);
                    break;
                }    
   
            PlacingSpirit?.Invoke(null, null);
        }

        private void OnSelling(object _, EventArgs e) => SpiritSold?.Invoke(null, ChoosedSpirit);
            
        private bool CheckGradeListOk(out List<SpiritData> grades)
        {
           
            var allSpirits = ReferenceHolder.Get.SpiritDataBase.Spirits.
                Elements[(int)ChoosedSpirit.Data.Element].
                Rarities[(int)ChoosedSpirit.Data.Rarity].
                Spirits;

            for (int i = 0; i < allSpirits.Count; i++)            
                if(allSpirits[i].ID.Compare(ChoosedSpirit.Data.ID))
                {
                    grades = allSpirits[i].Grades;

                    return 
                        grades.Count > 0 &&
                        ChoosedSpirit.Data.GradeCount < grades.Count - 1;
                }

            grades = null;
            return false;
        }        

        private void OnUpgrading(object _, EventArgs e) 
        {           
            if (CheckGradeListOk(out List<SpiritData> grades))
            {              
                var upgradedSpiritPrefab = Instantiate(
                    grades[ChoosedSpirit.Data.GradeCount + 1].Prefab, 
                    ChoosedSpirit.Prefab.transform.position, 
                    Quaternion.identity, 
                    ReferenceHolder.Get.SpiritParent);
                var upgradedSpirit= new SpiritSystem(upgradedSpiritPrefab); 
                
                upgradedSpirit.DataSystem.Upgrade(ChoosedSpirit, grades[ChoosedSpirit.Data.GradeCount + 1]);                            
                upgradedSpirit.SetSystem(Owner);                                         
                
                SpiritUpgraded?.Invoke(null, upgradedSpirit);      
                SpiritSold?.Invoke(null, ChoosedSpirit);      
                ChoosedSpirit = upgradedSpirit;
            }
            Owner.SpiritUISystem.ActivateUpgradeButton(ChoosedSpirit.Data.GradeCount < grades.Count - 1);
        }
    }
}
