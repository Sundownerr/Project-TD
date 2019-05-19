using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine;
using Game.Utility;
using Game.Systems;

namespace Game.UI
{
    public class ResourceUISystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public Button GetSpiritButton;
        public bool IsWaveStarted, IsPlayerReady;
        public int WaveTimer;
        public TextMeshProUGUI Gold, MagicCrystals, SpiritLimit;

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;
            GetSpiritButton = Instantiate(GetSpiritButton.gameObject, Owner.WorldCanvas.transform).GetComponent<Button>();
            Owner.ResourceSystem.ResourcesChanged += OnResourcesChanged;
            GetSpiritButton.onClick.AddListener(OnGetSpiritClick);
            
            UpdateUI();

            void OnResourcesChanged() => UpdateUI();
            void OnGetSpiritClick() => Owner.SpiritCreatingSystem.CreateRandomSpirit();
        }

        public void UpdateUI()
        {
            var resources = Owner.Data.Resources;

            Gold.text = Uty.KiloFormat(resources.Resource);
            MagicCrystals.text = Uty.KiloFormat(resources.MagicCrystals);
            SpiritLimit.text = $"{resources.CurrentSpiritLimit}/{resources.MaxSpiritLimit}";
        }
    }
}
