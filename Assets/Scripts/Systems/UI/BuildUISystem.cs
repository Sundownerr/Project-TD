using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Spirit.Data.Stats;
using Game.Spirit.Data;
using System;
using TMPro;
using Game.Spirit;

namespace Game.Systems
{
    public class BuildUISystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public List<Button> ElementButtons;
        public List<GameObject> RarityGOs;
        public GameObject SpiritButtonPrefab;
        public ElementType ChoosedElement;
        public GameObject Rarity, ParentGO;
        public bool IsChoosedNewSpirit;    

        private List<SpiritData> availableSpirits;      
        private List<GameObject> spiritButtonGOs;
        private List<SpiritButtonSystem> spiritButtons;
        private Transform rarityTransform;
        private Vector2 newSpiritButtonPos;
        private Animator animator;

        private void Start()
        {
            ParentGO = transform.parent.gameObject;
            spiritButtonGOs = new List<GameObject>();
            spiritButtons = new List<SpiritButtonSystem>();
            availableSpirits = new List<SpiritData>();
            newSpiritButtonPos = new Vector2(0, 1);       
        }

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;
            availableSpirits = Owner.AvailableSpirits;
            rarityTransform = Rarity.transform;

            ElementButtons[0].onClick.AddListener(ShowAstral);
            ElementButtons[1].onClick.AddListener(ShowDarkness);
            ElementButtons[2].onClick.AddListener(ShowIce);
            ElementButtons[3].onClick.AddListener(ShowIron);
            ElementButtons[4].onClick.AddListener(ShowStorm);
            ElementButtons[5].onClick.AddListener(ShowNature);
            ElementButtons[6].onClick.AddListener(ShowFire);

            UpdateAvailableElement();
            animator = transform.parent.GetComponent<Animator>();

            Rarity.gameObject.SetActive(false);

            Owner.SpiritCreatingSystem.AddedNewAvailableSpirit += UpdateUI;
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
                var choosedCell = Owner.CellControlSystem.ChoosedCell;

                UpdateAvailableElement();
                UpdateRarity();

                transform.parent.position = choosedCell.transform.position + new Vector3(0, 60, 0);
                animator.SetBool("isOpen", true);
            }
            else
            {
                animator.SetBool("isOpen", false);
                for (int i = 0; i < spiritButtonGOs.Count; i++)
                    spiritButtonGOs[i].SetActive(false);
            }
        }

        private void OnClickedOnCell(object _, GameObject go) => ActivateUI(true);
        private void OnClickedOnGround(object _, EventArgs e) => ActivateUI(false);
        private void OnClicledOnSpirit(object _, GameObject go) => ActivateUI(false);
        private void OnSpiritPlaced(object _, SpiritSystem spirit) => ActivateUI(false);


        private void UpdateUI(object _, EventArgs e)
        {
            UpdateAvailableElement();
            UpdateRarity();
        }

        private void ActivateButtonList(ref List<Button> list, bool active)
        {
            for (int i = 0; i < list.Count; i++)            
                list[i].gameObject.SetActive(active);                                   
        }

        private void UpdateAvailableElement()
        {           
            ActivateButtonList(ref ElementButtons, false);      

            for (int i = 0; i < Owner.AvailableSpirits.Count; i++)           
                ElementButtons[(int)Owner.AvailableSpirits[i].Element].gameObject.SetActive(true);
        }      

        private void ShowRarity(ElementType element)
        {           
            ChoosedElement = element;
            Rarity.gameObject.SetActive(true);
            rarityTransform.SetParent(ElementButtons[(int)ChoosedElement].transform);
            rarityTransform.localPosition = new Vector2(15, 0);

            UpdateRarity();                       
        }

        private void UpdateRarity()
        {                          
            for (int i = 0; i < spiritButtons.Count; i++)                      
                spiritButtonGOs[i].gameObject.SetActive(spiritButtons[i].SpiritData.Element == ChoosedElement);                                                                                                    
        }

        public void OnAllThisSpiritsUsed(object _, SpiritButtonSystem spiritButton)
        {
            spiritButtonGOs.Remove(spiritButton.gameObject);
            spiritButtons.Remove(spiritButton);
            var buttonRects = new List<RectTransform>();

            for (int i = 0; i < spiritButtons.Count; i++)
                if (spiritButtons[i].SpiritData.Element == spiritButton.SpiritData.Element)
                    if (spiritButtons[i].SpiritData.Rarity == spiritButton.SpiritData.Rarity)
                        buttonRects.Add(spiritButtons[i].GetComponent<RectTransform>());

            for (int i = 0; i < buttonRects.Count; i++)
            {
                var isNewButtonPosBusy = false;
                var newButtonPos = (Vector2)buttonRects[i].localPosition - newSpiritButtonPos;

                for (int j = 0; j < buttonRects.Count; j++)
                    if (newButtonPos.y == buttonRects[j].localPosition.y)
                    {
                        isNewButtonPosBusy = true;
                        break;
                    }

                if (isNewButtonPosBusy)
                    break;
                else
                    if (newButtonPos.y >= 0)
                        buttonRects[i].localPosition = newButtonPos;
            }
        }

        public void AddSpiritButton(SpiritData spiritData)
        {
            var spiritCount = 0;               
            var isSameSpirit = false;
             
            for (int i = 0; i < spiritButtons.Count; i++)
            {
                if (spiritButtons[i].SpiritData == spiritData)
                {          
                    isSameSpirit = true;            
                    AddSpiritAmount(i);                                                                     
                }  
            
                if (spiritButtons[i].SpiritData.Element == spiritData.Element)
                    if (spiritButtons[i].SpiritData.Rarity == spiritData.Rarity)
                        spiritCount++;
            }

            if (!isSameSpirit)
                CreateSpiritButton();

            UpdateUI(this, null);
            
            #region  Helper functions

            void AddSpiritAmount(int index)
            {
                spiritButtons[index].Count++;
                spiritButtons[index].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = spiritButtons[index].Count.ToString();       
            }           

            void CreateSpiritButton()
            {
                spiritButtonGOs.Add(Instantiate(SpiritButtonPrefab, RarityGOs[(int)spiritData.Rarity].transform));                      
                spiritButtons.Add(spiritButtonGOs[spiritButtonGOs.Count - 1].GetComponent<SpiritButtonSystem>());

                var spiritButton = spiritButtons[spiritButtons.Count - 1];     
                var spiritButtonImage = spiritButton.gameObject.transform.GetChild(0).GetComponent<Image>();

                spiritButton.Owner = Owner;
                spiritButton.SpiritData = spiritData;
                spiritButton.PlaceNewSpirit += Owner.PlayerInputSystem.OnPlacingNewSpirit;
                spiritButton.PlaceNewSpirit += Owner.SpiritPlaceSystem.OnPlacingNewSpirit;
                spiritButton.AllThisSpiritsPlaced += OnAllThisSpiritsUsed;
                spiritButton.GetComponent<RectTransform>().localPosition = newSpiritButtonPos * spiritCount;
                spiritButtonImage.sprite = spiritData.Image;
                spiritButton.Count = 1;             
            }        

            #endregion
        }

        private void ShowAstral()   => ShowRarity(ElementType.Astral);     
        private void ShowDarkness() => ShowRarity(ElementType.Darkness);                
        private void ShowIce()      => ShowRarity(ElementType.Ice);               
        private void ShowIron()     => ShowRarity(ElementType.Iron);               
        private void ShowStorm()    => ShowRarity(ElementType.Storm);             
        private void ShowNature()   => ShowRarity(ElementType.Nature);        
        private void ShowFire()     => ShowRarity(ElementType.Fire);                 
    }
}