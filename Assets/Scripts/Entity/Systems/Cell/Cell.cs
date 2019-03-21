using Game.Systems;
using UnityEngine;
using Mirror;

namespace Game.Cells
{
    public class Cell : ExtendedMonoBehaviour
    {
        public bool IsBusy { get; set; }
        public bool IsChosen { get; set; }
        public bool IsExpanded { get; set; }
        public Renderer Renderer { get; set; }
        public PlayerSystem Owner { get; set; }

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            Owner.CellControlSystem.Cells.Add(this);
            Renderer = GetComponent<Renderer>();
            Renderer.material.color = new Color(0.1f, 0.2f, 0.1f, -1f);

            if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
            {
                gameObject.AddComponent<NetworkIdentity>();
                gameObject.AddComponent<NetworkTransform>();
            }
        }
    }
}