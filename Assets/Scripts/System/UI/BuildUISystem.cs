using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Game.Enums;
using Game.Utility;
using Game.Systems;
using Game.Systems.Spirit;
using Game.Data.SpiritEntity;
using Game.Managers;

namespace Game.UI
{
    public class BuildUISystem : ExtendedMonoBehaviour
    {
        public event Action<SpiritData> PlaceNewSpiritClicked;

        public PlayerSystem Owner { get; set; }
        public List<Button> ElementButtons;
        public List<GameObject> RarityGOs;
        public GameObject SpiritButtonPrefab;
        public ElementType ChoosedElement;
        public GameObject Rarity, ParentGO;
        public bool IsChoosedNewSpirit;

        // List<GameObject> spiritButtonGOs = new List<GameObject>();
        List<SpiritButtonSystem> spiritButtons = new List<SpiritButtonSystem>();
        Transform rarityTransform;
        Vector2 newSpiritButtonPos = new Vector2(0, 1);
        Animator animator;

        void Start()
        {
            ParentGO = transform.parent.gameObject;
        }

        public void SetSystem(PlayerSystem player)
        {
            UIManager.Instance.BuildUISystem = this;
            Owner = player;

            rarityTransform = Rarity.transform;

            ElementButtons[(int)ElementType.Astral].onClick.AddListener(ShowAstral);
            ElementButtons[(int)ElementType.Darkness].onClick.AddListener(ShowDarkness);
            ElementButtons[(int)ElementType.Ice].onClick.AddListener(ShowIce);
            ElementButtons[(int)ElementType.Iron].onClick.AddListener(ShowIron);
            ElementButtons[(int)ElementType.Storm].onClick.AddListener(ShowStorm);
            ElementButtons[(int)ElementType.Nature].onClick.AddListener(ShowNature);
            ElementButtons[(int)ElementType.Fire].onClick.AddListener(ShowFire);

            UpdateAvailableElement();
            animator = transform.parent.GetComponent<Animator>();

            Rarity.gameObject.SetActive(false);

            Owner.SpiritCreatingSystem.AddedNewAvailableSpirit += UpdateUI;
            Owner.PlayerInputSystem.ClikedOnGround += OnClickedOnGround;
            Owner.PlayerInputSystem.ClickedOnCell += OnClickedOnCell;
            Owner.SpiritPlaceSystem.SpiritPlaced += OnSpiritPlaced;
            Owner.PlayerInputSystem.ClickedOnSpirit += OnClicledOnSpirit;
            Owner.PlayerInputSystem.RMBPresed += OnClickedOnGround;

            void ShowAstral() => ShowRarity(ElementType.Astral);
            void ShowDarkness() => ShowRarity(ElementType.Darkness);
            void ShowIce() => ShowRarity(ElementType.Ice);
            void ShowIron() => ShowRarity(ElementType.Iron);
            void ShowStorm() => ShowRarity(ElementType.Storm);
            void ShowNature() => ShowRarity(ElementType.Nature);
            void ShowFire() => ShowRarity(ElementType.Fire);
        }

        void ActivateUI(bool activate)
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
                spiritButtons.ForEach(button => button.gameObject.SetActive(false));
            }
        }

        void OnClickedOnCell(GameObject go) => ActivateUI(true);
        void OnClickedOnGround() => ActivateUI(false);
        void OnClicledOnSpirit(GameObject go) => ActivateUI(false);
        void OnSpiritPlaced(SpiritSystem spirit) => ActivateUI(false);

        void UpdateUI()
        {
            UpdateAvailableElement();
            UpdateRarity();
        }

        void UpdateAvailableElement() =>
            ElementButtons.ForEach(button => button.gameObject.SetActive(true));

        void ShowRarity(ElementType element)
        {
            ChoosedElement = element;
            Rarity.gameObject.SetActive(true);
            rarityTransform.SetParent(ElementButtons[(int)ChoosedElement].transform);
            rarityTransform.localPosition = new Vector2(15, 0);

            UpdateRarity();
        }

        void UpdateRarity() =>
            spiritButtons.ForEach(button => button.gameObject.SetActive(button.SpiritData.Base.Element == ChoosedElement));
        
        public void AddSpiritButton(SpiritData spiritData)
        {
            var spiritCount = 0;
            var isSameSpirit = false;

            spiritButtons.ForEach(button =>
            {
                if (button.SpiritData.Index == spiritData.Index)
                {
                    isSameSpirit = true;
                    button.Count++;
                    button.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = button.Count.ToString();
                    spiritCount++;
                }
            });

            if (!isSameSpirit)
            {
                spiritButtons.Add(GetSpiritButton());
            }

            UpdateUI();

            SpiritButtonSystem GetSpiritButton()
            {
                var newSpiritButton = Instantiate(SpiritButtonPrefab, RarityGOs[(int)spiritData.Base.Rarity].transform).GetComponent<SpiritButtonSystem>();

                newSpiritButton.Set(spiritData, Owner, newSpiritButtonPos, spiritCount);
                newSpiritButton.PlaceNewSpiritClicked += OnPlaceNewSpiritClicked;
                newSpiritButton.AllThisSpiritsPlaced += OnAllThisSpiritsUsed;

                return newSpiritButton;

                void OnPlaceNewSpiritClicked(SpiritData obj) => PlaceNewSpiritClicked?.Invoke(obj);

                void OnAllThisSpiritsUsed(SpiritButtonSystem spiritButton)
                {
                    spiritButtons.Remove(spiritButton);

                    var buttonRects = new List<RectTransform>();

                    spiritButtons.ForEach(button =>
                    {
                        if (button.Element == spiritButton.Element)
                        {
                            if (button.Rarity == spiritButton.Rarity)
                            {
                                buttonRects.Add(button.GetComponent<RectTransform>());
                            }
                        }
                    });

                    for (int i = 0; i < buttonRects.Count; i++)
                    {
                        var isNewButtonPosBusy = false;
                        var newButtonPos = (Vector2)buttonRects[i].localPosition - newSpiritButtonPos;

                        for (int j = 0; j < buttonRects.Count; j++)
                        {
                            if (newButtonPos.y == buttonRects[j].localPosition.y)
                            {
                                isNewButtonPosBusy = true;
                                break;
                            }
                        }

                        if (isNewButtonPosBusy)
                        {
                            break;
                        }
                        else if (newButtonPos.y >= 0)
                        {
                            buttonRects[i].localPosition = newButtonPos;
                        }
                    }
                }
            }
        }

        void OnDestroy()
        {
            if (ElementButtons != null)
            {
                ElementButtons.ForEach(button => button.onClick.RemoveAllListeners());
            }

            if (spiritButtons != null)
            {
                spiritButtons.ForEach(button => button.GetComponent<Button>().onClick.RemoveAllListeners());
            }
        }
    }
}