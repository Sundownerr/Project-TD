using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Spirit.Data.Stats;
using Game.Spirit.Data;
using System;
using TMPro;
using Game.Spirit;
using Game.Enums;
using UnityEngine.Events;

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

        private List<GameObject> spiritButtonGOs = new List<GameObject>();
        private List<SpiritButtonSystem> spiritButtons = new List<SpiritButtonSystem>();
        private Transform rarityTransform;
        private Vector2 newSpiritButtonPos = new Vector2(0, 1);
        private Animator animator;

        private void Start()
        {
            ParentGO = transform.parent.gameObject;
        }



        public void SetSystem(PlayerSystem player)
        {
            Owner = player;

            rarityTransform = Rarity.transform;

            // not working syntax highlighting made me do this
            UnityAction showAstral = () => ShowRarity(ElementType.Astral);
            UnityAction showDarkness = () => ShowRarity(ElementType.Darkness);
            UnityAction showIce = () => ShowRarity(ElementType.Ice);
            UnityAction showIron = () => ShowRarity(ElementType.Iron);
            UnityAction showStorm = () => ShowRarity(ElementType.Storm);
            UnityAction showNature = () => ShowRarity(ElementType.Nature);
            UnityAction showFire = () => ShowRarity(ElementType.Fire);

            ElementButtons[(int)ElementType.Astral].onClick.AddListener(() => ShowRarity(ElementType.Astral));
            ElementButtons[(int)ElementType.Darkness].onClick.AddListener(showDarkness);
            ElementButtons[(int)ElementType.Ice].onClick.AddListener(showIce);
            ElementButtons[(int)ElementType.Iron].onClick.AddListener(showIron);
            ElementButtons[(int)ElementType.Storm].onClick.AddListener(showStorm);
            ElementButtons[(int)ElementType.Nature].onClick.AddListener(showNature);
            ElementButtons[(int)ElementType.Fire].onClick.AddListener(showFire);

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

        private void UpdateAvailableElement() => ElementButtons.ForEach(button => button.gameObject.SetActive(true));

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
                spiritButtonGOs[i].gameObject.SetActive(spiritButtons[i].SpiritData.Base.Element == ChoosedElement);
        }

        public void OnAllThisSpiritsUsed(object _, SpiritButtonSystem spiritButton)
        {
            spiritButtonGOs.Remove(spiritButton.gameObject);
            spiritButtons.Remove(spiritButton);
            var buttonRects = new List<RectTransform>();

            spiritButtons.ForEach(button =>
            {
                if (button.Element == spiritButton.Element)
                    if (button.Rarity == spiritButton.Rarity)
                        buttonRects.Add(button.GetComponent<RectTransform>());
            });

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

            spiritButtons.ForEach(button =>
            {
                if (button.SpiritData.ID.Compare(spiritData.ID))
                {
                    isSameSpirit = true;
                    button.Count++;
                    button.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = button.Count.ToString();
                }

                if (button.Element == spiritData.Base.Element)
                    if (button.Rarity == spiritData.Base.Rarity)
                        spiritCount++;
            });

            if (!isSameSpirit)
                CreateSpiritButton();

            UpdateUI(this, null);

            #region  Helper functions

            void CreateSpiritButton()
            {
                spiritButtonGOs.Add(Instantiate(SpiritButtonPrefab, RarityGOs[(int)spiritData.Base.Rarity].transform));
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

        private void OnDestroy()
        {
            if (ElementButtons != null)
                ElementButtons.ForEach(button => button.onClick.RemoveAllListeners());

            if (spiritButtonGOs != null)
                spiritButtonGOs.ForEach(button => button.GetComponent<Button>().onClick.RemoveAllListeners());

        }
    }
}