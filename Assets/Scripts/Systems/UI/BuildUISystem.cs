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

            ElementButtons[(int)ElementType.Astral].onClick.AddListener(() => ShowRarity(ElementType.Astral));
            ElementButtons[(int)ElementType.Darkness].onClick.AddListener(() => ShowRarity(ElementType.Darkness));
            ElementButtons[(int)ElementType.Ice].onClick.AddListener(() => ShowRarity(ElementType.Ice));
            ElementButtons[(int)ElementType.Iron].onClick.AddListener(() => ShowRarity(ElementType.Iron));
            ElementButtons[(int)ElementType.Storm].onClick.AddListener(() => ShowRarity(ElementType.Storm));
            ElementButtons[(int)ElementType.Nature].onClick.AddListener(() => ShowRarity(ElementType.Nature));
            ElementButtons[(int)ElementType.Fire].onClick.AddListener(() => ShowRarity(ElementType.Fire));

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

        private void ActivateButtonList(List<Button> list, bool active)
        {
            for (int i = 0; i < list.Count; i++)
                list[i].gameObject.SetActive(active);
        }

        private void UpdateAvailableElement()
        {
            ActivateButtonList(ElementButtons, false);

            for (int i = 0; i < ElementButtons.Count; i++)
                ElementButtons[i].gameObject.SetActive(true);
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

        private void OnDestroy()
        {
            if (ElementButtons != null)
                for (int i = 0; i < ElementButtons.Count; i++)
                    ElementButtons[i].onClick.RemoveAllListeners();

            if (spiritButtonGOs != null)
                for (int i = 0; i < spiritButtonGOs.Count; i++)
                    spiritButtonGOs[i].GetComponent<Button>().onClick.RemoveAllListeners();
        }
    }
}