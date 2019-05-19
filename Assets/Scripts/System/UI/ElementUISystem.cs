using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System;
using Game.Enums;
using Game.Utility;
using Game.Systems;
using Game.Systems.Spirit;

namespace Game.UI
{
    public class ElementUISystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public Button Astral, Darkness, Ice, Iron, Storm, Nature, Fire;
        public Button[] Buttons { get; private set; }

        TextMeshProUGUI astralLevel, darknessLevel, iceLevel, ironLevel, stormLevel, natureLevel, fireLevel;

        protected override void Awake()
        {
            base.Awake();

            Astral.onClick.AddListener(OnAstralClick);
            Darkness.onClick.AddListener(OnDarknessClick);
            Ice.onClick.AddListener(OnIceClick);
            Iron.onClick.AddListener(OnIronClick);
            Storm.onClick.AddListener(OnStormClick);
            Nature.onClick.AddListener(OnNatureClick);
            Fire.onClick.AddListener(OnFireClick);

            astralLevel = Astral.GetComponentInChildren<TextMeshProUGUI>();
            darknessLevel = Darkness.GetComponentInChildren<TextMeshProUGUI>();
            iceLevel = Ice.GetComponentInChildren<TextMeshProUGUI>();
            ironLevel = Iron.GetComponentInChildren<TextMeshProUGUI>();
            stormLevel = Storm.GetComponentInChildren<TextMeshProUGUI>();
            natureLevel = Nature.GetComponentInChildren<TextMeshProUGUI>();
            fireLevel = Fire.GetComponentInChildren<TextMeshProUGUI>();

            void OnAstralClick() => Owner.ElementSystem.LearnElement((int)ElementType.Astral);
            void OnDarknessClick() => Owner.ElementSystem.LearnElement((int)ElementType.Darkness);
            void OnIceClick() => Owner.ElementSystem.LearnElement((int)ElementType.Ice);
            void OnIronClick() => Owner.ElementSystem.LearnElement((int)ElementType.Iron);
            void OnStormClick() => Owner.ElementSystem.LearnElement((int)ElementType.Storm);
            void OnNatureClick() => Owner.ElementSystem.LearnElement((int)ElementType.Nature);
            void OnFireClick() => Owner.ElementSystem.LearnElement((int)ElementType.Fire);

            Buttons = new Button[] { Astral, Darkness, Ice, Iron, Storm, Nature, Fire };
        }

        void Start()
        {
            gameObject.SetActive(true);
        }

        void OnDestroy()
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

            void OnClickedOnCell(GameObject go) => ActivateUI(true);
            void OnClickedOnGround() => ActivateUI(false);
            void OnClicledOnSpirit(GameObject go) => ActivateUI(false);
            void OnSpiritPlaced(SpiritSystem spirit) => ActivateUI(false);
            void OnElementLearned(int learnCost) => UpdateUI();

            void ActivateUI(bool activate)
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

            void UpdateUI()
            {
                astralLevel.text = Owner.Data.ElementLevels[(int)ElementType.Astral].ToString();
                darknessLevel.text = Owner.Data.ElementLevels[(int)ElementType.Darkness].ToString();
                iceLevel.text = Owner.Data.ElementLevels[(int)ElementType.Ice].ToString();
                ironLevel.text = Owner.Data.ElementLevels[(int)ElementType.Iron].ToString();
                stormLevel.text = Owner.Data.ElementLevels[(int)ElementType.Storm].ToString();
                natureLevel.text = Owner.Data.ElementLevels[(int)ElementType.Nature].ToString();
                fireLevel.text = Owner.Data.ElementLevels[(int)ElementType.Fire].ToString();
            }
        }
    }
}
