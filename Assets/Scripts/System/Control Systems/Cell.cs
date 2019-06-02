using System;
using System.Collections.Generic;
using Game.Managers;
using Game.Systems.Cells;
using Game.Systems.Spirit;
using UnityEngine;
using U = UnityEngine.Object;

namespace Game.Systems.Cells
{
    public class ControlSystem
    {
        public bool IsGridBuilded { get; set; }
        public List<Cell> Cells { get; set; } = new List<Cell>();
        public Cell ChoosedCell { get; set; }
        public PlayerSystem Owner { get; set; }

        Color blue;
        Color green;
        Color red;
        Color choosedColor;
        List<Color> cellColors = new List<Color>();

        Camera mainCam = Camera.main;
        RaycastHit hit;

        public ControlSystem(PlayerSystem player)
        {
            Owner = player;

            red = new Color(0.3f, 0.1f, 0.1f, 0.6f);
            green = new Color(0.1f, 2f, 0.1f, 0.9f);
            blue = new Color(0.1f, 0.1f, 0.3f, 0.7f);
            choosedColor = green + new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        public void SetSystem()
        {
            CreateGrid();

            Owner.PlayerInputSystem.ClickedOnCell += OnClickedOnCell;
            Owner.PlayerInputSystem.ClikedOnGround += OnClickedOnGround;
            Owner.PlayerInputSystem.ClickedOnSpirit += OnClickedOnSpirit;
            Owner.PlayerInputSystem.RMBPresed += OnRMBPressed;
            Owner.SpiritPlaceSystem.SpiritPlaced += OnSpiritPlaced;

            void CreateGrid()
            {
                var cellAreas = Owner.Map.CellAreas;

                for (var i = 0; i < cellAreas.Count; i++)
                {
                    var ray = new Ray(cellAreas[i].transform.position, Vector3.up);
                    var layerMask = 1 << 15;

                    if (!Physics.Raycast(ray, 100, layerMask))
                    {
                        var spawnPos =
                            cellAreas[i].transform.position +
                            new Vector3(0, cellAreas[i].transform.localScale.y / 1.9f, 0);

                        var newCell = U.Instantiate(
                            ReferenceHolder.Instance.CellPrefab,
                            spawnPos,
                            Quaternion.identity,
                            ReferenceHolder.Instance.CellParent).GetComponent<Cell>();

                        newCell.Owner = Owner;
                        Cells.Add(newCell);
                        cellColors.Add(green);
                    }
                }

                for (int i = 0; i < Cells.Count; i++)
                {
                    if (!Cells[i].IsExpanded)
                    {
                        Cells[i].Expand();
                    }
                }

                IsGridBuilded = true;
            }
        }

        void OnSpiritPlaced(SpiritSystem spirit) => ChoosedCell = null;
        void OnRMBPressed() => ChoosedCell = null;
        void OnClickedOnGround() => ChoosedCell = null;
        void OnClickedOnSpirit(GameObject spirit) => ChoosedCell = null;
        void OnClickedOnCell(GameObject cellGO) => ChoosedCell = ChoosedCell ?? cellGO.GetComponent<Cell>();

        public void UpdateSystem()
        {
            if (IsGridBuilded)
            {
                if (ChoosedCell == null)
                {
                    var mousePosRay = mainCam.ScreenPointToRay(Input.mousePosition);
                    var terrainLayer = 1 << 9;
                    var cellLayer = 1 << 15;
                    var layerMask = terrainLayer | cellLayer;

                    if (Physics.Raycast(mousePosRay, out hit, 99999, layerMask))
                    {
                        for (int i = 0; i < Cells.Count; i++)
                        {
                            var notChoosedColor = Cells[i].Renderer.material.color;
                            var distance = hit.point.GetDistanceTo(Cells[i].gameObject);
                            var alpha = Mathf.Clamp((100 - distance) * 0.01f, 0, 0.2f);
                            notChoosedColor.a = Mathf.Lerp(notChoosedColor.a, alpha, Time.deltaTime * 6);

                            Cells[i].gameObject.SetActive(notChoosedColor.a > 0.01);
                            Cells[i].Renderer.material.color = distance < 20 ? choosedColor : notChoosedColor;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Cells.Count; i++)
                    {
                        if (Cells[i] == ChoosedCell)
                        {
                            Cells[i].Renderer.material.color = choosedColor;
                        }
                        else
                        {
                            var color = Cells[i].Renderer.material.color;
                            color.a = Mathf.Lerp(color.a, 0, 0.1f);

                            Cells[i].Renderer.material.color = color;
                        }
                    }
                }
            }
        }
    }
}
