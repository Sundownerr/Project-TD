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
        public TextMeshProUGUI AstralLevel, DarknessLevel, IceLevel, IronLevel, StormLevel, NatureLevel, FireLevel;

        private Animator animator;

        protected override void Awake()
        {
            base.Awake();

            Astral.onClick.AddListener(LearnAstral);
            Darkness.onClick.AddListener(LearnDarkness);
            Ice.onClick.AddListener(LearnIce);
            Iron.onClick.AddListener(LearnIron);
            Storm.onClick.AddListener(LearnStorm);
            Nature.onClick.AddListener(LearnNature);
            Fire.onClick.AddListener(LearnFire);
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            gameObject.SetActive(true);
        }

        private void ActivateUI(bool activate)
        {
            if(activate)
            {
              
                var choosedCell = Owner.CellControlSystem.ChoosedCell;
                UpdateUI();
                transform.position = choosedCell.transform.position + new Vector3(-30, 20, 20);
                animator.SetBool("isOpen", true);
                
                Owner.ResourceUISystem.GetSpiritButton.gameObject.SetActive(true);
                Owner.ResourceUISystem.GetSpiritButton.gameObject.transform.position = choosedCell.transform.position + new Vector3(-10, 10, -100);
            }
            else
            {
                animator.SetBool("isOpen", false);
                Owner.ResourceUISystem.GetSpiritButton.gameObject.SetActive(false);
            }
        }

        private void OnClickedOnCell(object _, GameObject go)      => ActivateUI(true);
        private void OnClickedOnGround(object _, EventArgs e)      => ActivateUI(false);        
        private void OnClicledOnSpirit(object _, GameObject go)     => ActivateUI(false);
        private void OnSpiritPlaced(object _, SpiritSystem spirit)    => ActivateUI(false);

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

        private void UpdateUI()
        {
            AstralLevel.text    = Owner.Data.ElementLevels[0].ToString();
            DarknessLevel.text  = Owner.Data.ElementLevels[1].ToString();
            IceLevel.text       = Owner.Data.ElementLevels[2].ToString();
            IronLevel.text      = Owner.Data.ElementLevels[3].ToString();
            StormLevel.text     = Owner.Data.ElementLevels[4].ToString();
            NatureLevel.text    = Owner.Data.ElementLevels[5].ToString();
            FireLevel.text      = Owner.Data.ElementLevels[6].ToString();
        }

        private void OnElementLearned(object _, int learnCost) => UpdateUI();

        private void LearnAstral()  => Owner.ElementSystem.LearnElement(0);
        private void LearnDarkness() => Owner.ElementSystem.LearnElement(1);
        private void LearnIce()     => Owner.ElementSystem.LearnElement(2);
        private void LearnIron()    => Owner.ElementSystem.LearnElement(3);
        private void LearnStorm()   => Owner.ElementSystem.LearnElement(4);
        private void LearnNature()  => Owner.ElementSystem.LearnElement(5);
        private void LearnFire()    => Owner.ElementSystem.LearnElement(6);      
    }
}
