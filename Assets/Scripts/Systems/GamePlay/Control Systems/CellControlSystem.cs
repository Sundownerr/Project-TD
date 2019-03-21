using System;
using System.Collections;
using System.Collections.Generic;
using Game.Cells;
using Game.Spirit;
using UnityEngine;
using U = UnityEngine.Object;

namespace Game.Systems
{
    public class CellControlSystem
    {
        public bool IsGridBuilded { get; set; }
        public List<Cell> Cells { get; set; }
        public Cell ChoosedCell { get; set; }

        private Color blue, red, green, choosedColor;
        private Camera mainCam;
        public PlayerSystem Owner { get; set; }

        public CellControlSystem(PlayerSystem player)
        {
            Owner = player;
            Cells = new List<Cell>();

            red = new Color(0.3f, 0.1f, 0.1f, 0.6f);
            green = new Color(0.1f, 2f, 0.1f, 0.9f);
            blue = new Color(0.1f, 0.1f, 0.3f, 0.7f);
            choosedColor = green + new Color(0.5f, 0.5f, 0.5f, 1f);
            mainCam = Camera.main;       
        }       

        public void SetSystem()
        {
            CreateGrid();

            Owner.PlayerInputSystem.ClickedOnCell += OnClickedOnCell;
            Owner.PlayerInputSystem.ClikedOnGround += OnClickedOnGround;
            Owner.PlayerInputSystem.ClickedOnSpirit += OnClickedOnSpirit;
            Owner.PlayerInputSystem.RMBPresed += OnRMBPressed;
            Owner.SpiritPlaceSystem.SpiritPlaced += OnSpiritPlaced;

            #region  Helper functions

            void CreateGrid()
            {
                var cellAreas = Owner.Map.CellAreas;
                

                for (var i = 0; i < cellAreas.Length; i++)
                {
                    var ray = new Ray(cellAreas[i].transform.position, Vector3.up);
                    var layerMask = 1 << 15;

                    if (!Physics.Raycast(ray, 100, layerMask))
                    {
                        var spawnPos =
                            cellAreas[i].transform.position +
                            new Vector3(0, cellAreas[i].transform.localScale.y / 1.9f, 0);

                        var prefab = U.Instantiate(
                            ReferenceHolder.Get.CellPrefab, 
                            spawnPos,
                            Quaternion.identity, 
                            ReferenceHolder.Get.CellParent).GetComponent<Cell>();

                        prefab.Owner = Owner;
                        Cells.Add(prefab);
                    }
                }

                for (int i = 0; i < Cells.Count; i++)
                    if (!Cells[i].IsExpanded)
                        Cells[i].Expand();

                IsGridBuilded = true;
            }

            #endregion
        }

        private void OnSpiritPlaced(object sender, SpiritSystem spirit) => ChoosedCell = null;
        private void OnRMBPressed(object sender, EventArgs e) => ChoosedCell = null;
        private void OnClickedOnGround(object sender, EventArgs e) => ChoosedCell = null;
        private void OnClickedOnSpirit(object sender, GameObject spirit) => ChoosedCell = null;
        private void OnClickedOnCell(object sender, GameObject cellGO)
        {

            ChoosedCell = ChoosedCell ?? cellGO.GetComponent<Cell>();
        }

        public void UpdateSystem()
        {           
            if (IsGridBuilded)            
                if (ChoosedCell == null)
                {
                    var mousePosRay = mainCam.ScreenPointToRay(Input.mousePosition);
                    var terrainLayer = 1 << 9;
                    var cellLayer = 1 << 15;
                    var layerMask = terrainLayer | cellLayer;
                    var hit = new RaycastHit();

                    if (Physics.Raycast(mousePosRay, out hit, 99999, layerMask))                  
                        for (int i = 0; i < Cells.Count; i++)
                        {
                            var color = Cells[i].Renderer.material.color;
                            var distance = hit.point.GetDistanceTo(Cells[i].transform.position);
                            var alpha = Mathf.Clamp((100 - distance) * 0.01f, 0, 0.2f);
                            var notChoosedColor = new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, alpha, Time.deltaTime * 6));

                            Cells[i].gameObject.SetActive(color.a > 0.01);
                            Cells[i].Renderer.material.color = distance < 20 ? choosedColor : notChoosedColor;
                        }                   
                }
                else               
                    for (int i = 0; i < Cells.Count; i++)
                        Cells[i].Renderer.material.color = 
                            Cells[i] == ChoosedCell ? choosedColor : new Color(
                                Cells[i].Renderer.material.color.r,
                                Cells[i].Renderer.material.color.g,
                                Cells[i].Renderer.material.color.b,
                                Mathf.Lerp(Cells[i].Renderer.material.color.a, 0, 0.1f));           
        }
    }
}
