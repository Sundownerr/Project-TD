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

        public event EventHandler PlacingSpirit;
        public event EventHandler<SpiritSystem> SpiritSold;
        public event EventHandler<SpiritSystem> SpiritUpgraded;
        public event EventHandler<GameObject> ClickedOnCell;
        public event EventHandler<GameObject> ClickedOnSpirit;
        public event EventHandler<GameObject> ClickedOnEnemy;
        public event EventHandler ClikedOnGround;
        public event EventHandler RMBPresed;

        EnemySystem choosedEnemy;
        GameObject selection;
        PointerEventData pointerEventData;

        RaycastHit hit;
        Ray WorldRay;
        bool isHitUI, isHitWorldUI;
        int terrainLayer, enemyLayer, spiritLayer, cellLayer, uiLayer, layerMask;

        protected override void Awake()
        {
            base.Awake();

            EventSystem = EventSystem.current;
            pointerEventData = new PointerEventData(EventSystem);

            terrainLayer = 1 << 9;
            enemyLayer = 1 << 12;
            spiritLayer = 1 << 14;
            cellLayer = 1 << 15;
            uiLayer = 1 << 5;
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

        void Update()
        {
            pointerEventData.position = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                WorldRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                isHitUI = EventSystem.currentSelectedGameObject != null;

                if (!isHitUI)
                    if (Physics.Raycast(WorldRay, out hit, 100000, layerMask))
                    {
                        var isMouseOnSpirit = hit.transform.gameObject.layer == 14;
                        var isMouseOnGround = hit.transform.gameObject.layer == 9;
                        var isMouseOnCell = hit.transform.gameObject.layer == 15;
                        var isMouseOnEnemy = hit.transform.gameObject.layer == 12;

                        if (isMouseOnSpirit)
                        {
                            GetChoosedSpirit();
                            ClickedOnSpirit?.Invoke(null, hit.transform.gameObject);
                        }

                        if (isMouseOnEnemy)
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
                ChoosedSpirit = Owner.SpiritControlSystem.AllSpirits.Find(spirit => spirit.Prefab == hit.transform.gameObject);
                Owner.SpiritUISystem.ActivateUpgradeButton(CheckGradeListOk(out _));
                ActivateSelection(ChoosedSpirit);
            }

            void GetChoosedEnemy()
            {
                choosedEnemy = Owner.EnemyControlSystem.AllEnemies.Find(enemy => enemy.Prefab == hit.transform.gameObject);
                ActivateSelection(choosedEnemy);
            }

            #endregion
        }


        void ActivateSelection(IPrefabComponent entity)
        {
            if (selection == null)
                selection = Instantiate(Selection);
            selection.SetActive(true);
            selection.transform.position = entity.Prefab.transform.position;
            selection.transform.SetParent(entity.Prefab.transform);
        }

        void SetSelection(bool activate)
        {
            if (selection != null)
                selection.SetActive(activate);
        }

        public void OnPlacingNewSpirit(object _, SpiritData spiritData)
        {
            var placedSpirit = Owner.AvailableSpirits.Find(spirit => spirit.ID.Compare(spiritData.ID));
            Owner.AvailableSpirits.Remove(placedSpirit);

            PlacingSpirit?.Invoke(null, null);
        }

        void OnSelling(object _, EventArgs e) => SpiritSold?.Invoke(null, ChoosedSpirit);

        bool CheckGradeListOk(out List<SpiritData> grades)
        {
            var allSpirits = ReferenceHolder.Get.SpiritDataBase.Spirits.
                Elements[(int)ChoosedSpirit.Data.Base.Element].
                Rarities[(int)ChoosedSpirit.Data.Base.Rarity].
                Spirits;

            var spirit = allSpirits.Find(x => x.ID.Compare(ChoosedSpirit.Data.ID));

            if (spirit != null)
            {
                grades = spirit.Grades;

                return
                    spirit.Grades.Count > 0 &&
                    ChoosedSpirit.Data.GradeCount < spirit.Grades.Count - 1;
            }

            grades = null;
            return false;
        }

        void OnUpgrading(object _, EventArgs e)
        {
            if (CheckGradeListOk(out List<SpiritData> grades))
            {
                var upgradedSpiritPrefab = Instantiate(
                    grades[ChoosedSpirit.Data.GradeCount + 1].Prefab,
                    ChoosedSpirit.Prefab.transform.position,
                    Quaternion.identity,
                    ReferenceHolder.Get.SpiritParent);
                var upgradedSpirit = new SpiritSystem(upgradedSpiritPrefab, true);

                upgradedSpirit.Upgrade(ChoosedSpirit, grades[ChoosedSpirit.Data.GradeCount + 1]);
                upgradedSpirit.SetSystem(Owner);

                SpiritUpgraded?.Invoke(null, upgradedSpirit);
                SpiritSold?.Invoke(null, ChoosedSpirit);
                ChoosedSpirit = upgradedSpirit;
            }
            Owner.SpiritUISystem.ActivateUpgradeButton(ChoosedSpirit.Data.GradeCount < grades.Count - 1);
        }
    }
}
