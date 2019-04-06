using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine;

namespace Game.Systems
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
            GetSpiritButton.onClick.AddListener(GetSpirit);
            UpdateUI();
        }

        private void OnResourcesChanged(object _, EventArgs e) => UpdateUI();

        public void UpdateUI()
        {
            var resources = Owner.Data.Resources;

            Gold.text           = StaticMethods.KiloFormat(resources.Resource);
            MagicCrystals.text  = StaticMethods.KiloFormat(resources.MagicCrystals);
            SpiritLimit.text     = $"{resources.CurrentSpiritLimit}/{resources.MaxSpiritLimit}";
        }

        private void GetSpirit() => Owner.SpiritCreatingSystem.CreateRandomSpirit();                 
    }
}
