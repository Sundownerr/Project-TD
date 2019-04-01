using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System;
using Game.Spirit;

namespace Game.Systems
{
    public class ElementUISystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public Button Astral, Darkness, Ice, Iron, Storm, Nature, Fire;
        public Button[] Buttons { get; private set; }

        private TextMeshProUGUI astralLevel, darknessLevel, iceLevel, ironLevel, stormLevel, natureLevel, fireLevel;

        protected override void Awake()
        {
            base.Awake();

            Astral.onClick.AddListener(() => Owner.ElementSystem.LearnElement((int)ElementType.Astral));
            Darkness.onClick.AddListener(() => Owner.ElementSystem.LearnElement((int)ElementType.Darkness));
            Ice.onClick.AddListener(() => Owner.ElementSystem.LearnElement((int)ElementType.Ice));
            Iron.onClick.AddListener(() => Owner.ElementSystem.LearnElement((int)ElementType.Iron));
            Storm.onClick.AddListener(() => Owner.ElementSystem.LearnElement((int)ElementType.Storm));
            Nature.onClick.AddListener(() => Owner.ElementSystem.LearnElement((int)ElementType.Nature));
            Fire.onClick.AddListener(() => Owner.ElementSystem.LearnElement((int)ElementType.Fire));

            astralLevel = Astral.GetComponentInChildren<TextMeshProUGUI>();
            darknessLevel = Darkness.GetComponentInChildren<TextMeshProUGUI>();
            iceLevel = Ice.GetComponentInChildren<TextMeshProUGUI>();
            ironLevel = Iron.GetComponentInChildren<TextMeshProUGUI>();
            stormLevel = Storm.GetComponentInChildren<TextMeshProUGUI>();
            natureLevel = Nature.GetComponentInChildren<TextMeshProUGUI>();
            fireLevel = Fire.GetComponentInChildren<TextMeshProUGUI>();

            Buttons = new Button[] { Astral, Darkness, Ice, Iron, Storm, Nature, Fire };
        }

        private void Start()
        {
            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            Astral.onClick.RemoveAllListeners();
            Darkness.onClick.RemoveAllListeners();
            Ice.onClick.RemoveAllListeners();
            Iron.onClick.RemoveAllListeners();
            Storm.onClick.RemoveAllListeners();
            Nature.onClick.RemoveAllListeners();
            Fire.onClick.RemoveAllListeners();
        }

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;
            Owner.ElementSystem.LearnedElement += OnElementLearned;
            Owner.PlayerInputSystem.ClikedOnGround += OnClickedOnGround;
            Owner.PlayerInputSystem.ClickedOnCell += OnClickedOnCell;
            Owner.SpiritPlaceSystem.SpiritPlaced += OnSpiritPlaced;
            Owner.PlayerInputSystem.ClickedOnSpirit += OnClicledOnSpirit;
            Owner.PlayerInputSystem.RMBPresed += OnClickedOnGround;
        }

        private void ActivateUI(bool activate)
        {
            if (activate)
            {
                UpdateUI();

                Owner.ResourceUISystem.GetSpiritButton.gameObject.SetActive(true);
                Owner.ResourceUISystem.GetSpiritButton.gameObject.transform.position =
                    Owner.CellControlSystem.ChoosedCell.transform.position + new Vector3(-10, 10, -40);
            }
            else
                Owner.ResourceUISystem.GetSpiritButton.gameObject.SetActive(false);
        }

        private void OnClickedOnCell(object _, GameObject go) => ActivateUI(true);
        private void OnClickedOnGround(object _, EventArgs e) => ActivateUI(false);
        private void OnClicledOnSpirit(object _, GameObject go) => ActivateUI(false);
        private void OnSpiritPlaced(object _, SpiritSystem spirit) => ActivateUI(false);
        private void OnElementLearned(object _, int learnCost) => UpdateUI();

        private void UpdateUI()
        {
            astralLevel.text = Owner.Data.ElementLevels[0].ToString();
            darknessLevel.text = Owner.Data.ElementLevels[1].ToString();
            iceLevel.text = Owner.Data.ElementLevels[2].ToString();
            ironLevel.text = Owner.Data.ElementLevels[3].ToString();
            stormLevel.text = Owner.Data.ElementLevels[4].ToString();
            natureLevel.text = Owner.Data.ElementLevels[5].ToString();
            fireLevel.text = Owner.Data.ElementLevels[6].ToString();
        }
    }
}
