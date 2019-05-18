using Game.Systems;
using UnityEngine;
using Mirror;
using Game.Utility;
using Game.Managers;

namespace Game.Systems.Cells
{
    public class Cell : ExtendedMonoBehaviour
    {
        public bool IsBusy { get; set; }
        public bool IsChosen { get; set; }
        public bool IsExpanded { get; private set; }
        public Renderer Renderer { get; set; }
        public PlayerSystem Owner { get; set; }

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
            Owner.CellControlSystem.Cells.Add(this);
            Renderer = GetComponent<Renderer>();
            Renderer.material.color = new Color(0.1f, 0.2f, 0.1f, -1f);
            gameObject.layer = 15;
        }

        public void Expand()
        {
            var forward = new Vector3(0, 0, 1);
            var back = new Vector3(0, 0, -1);
            var left = new Vector3(1, 0, 0);
            var right = new Vector3(-1, 0, 0);
            var down = new Vector3(0, -1, 0);
            var buildingAreaLayer = 1 << 8;
            var terrainLayer = 1 << 9;
            var buildLayerMask = ~terrainLayer | buildingAreaLayer;

            var spacing = transform.localScale.x;
            var rayDistance = transform.localScale.x;

            var results = new RaycastHit[1];

            var forwardRay = new Ray(transform.position + forward * rayDistance, down);
            var backRay = new Ray(transform.position + back * rayDistance, down);
            var rightRay = new Ray(transform.position + right * rayDistance, down);
            var leftRay = new Ray(transform.position + left * rayDistance, down);
            var downRay = new Ray(transform.position, down);

            var isForwardHit = Physics.RaycastNonAlloc(forwardRay, results, 5, buildLayerMask) > 0;
            var isBackHit = Physics.RaycastNonAlloc(backRay, results, 5, buildLayerMask) > 0;
            var isRightHit = Physics.RaycastNonAlloc(rightRay, results, 5, buildLayerMask) > 0;
            var isLeftHit = Physics.RaycastNonAlloc(leftRay, results, 5, buildLayerMask) > 0;
            var isDownHit = Physics.RaycastNonAlloc(downRay, results, 15, buildLayerMask) > 0;

            var isNothingHit = !isForwardHit && !isBackHit && !isLeftHit && !isRightHit;

            if (isNothingHit || !isDownHit)
                return;

            if (isForwardHit)
                Fill(forward);

            if (isBackHit)
                Fill(back);

            if (isLeftHit)
                Fill(left);

            if (isRightHit)
                Fill(right);

            #region  Helper functions

            void Fill(Vector3 spawnDirection)
            {
                if (!Physics.Raycast(transform.position, spawnDirection, rayDistance, buildLayerMask))
                {
                    var prefab = UnityEngine.Object.Instantiate(
                         ReferenceHolder.Instance.CellPrefab,
                        transform.position + spawnDirection * spacing,
                        Quaternion.identity,
                         ReferenceHolder.Instance.CellParent);

                    var cell = prefab.GetComponent<Cell>();

                    cell.Owner = Owner;
                    Owner.CellControlSystem.Cells.Add(cell);
                    IsExpanded = true;
                }
            }

            #endregion
        }
    }
}