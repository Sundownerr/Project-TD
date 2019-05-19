using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Game.Data;
using Game.Enums;
using Game.Systems;
using Game.Utility;
using Game.Managers;
using Game.Systems.Spirit;
using Game.Data.Effects;
using Game.Systems.Abilities;

namespace Game.UI
{
    public class SpiritUISystem : ExtendedMonoBehaviour
    {
        public event Action Selling;
        public event Action Upgrading;
        public event Action<SpiritItemEventArgs> ItemAddedToSpirit;
        public event Action<SpiritItemEventArgs> ItemRemovedFromSpirit;
        public event Action<ItemUISystem> MoveItemToPlayer;

        public PlayerSystem Owner { get; private set; }

        [SerializeField] GameObject buffGroup;
        [SerializeField] TextMeshProUGUI spiritName;
        [SerializeField] Image image;
        [SerializeField] Image expBar;
        [SerializeField] List<SlotWithCooldown> itemSlots;
        [SerializeField] List<SlotWithCooldown> abilitySlots;
        [SerializeField] List<SlotWithCooldown> traitSlots;
        [SerializeField] List<ItemUISystem> allItemsUIInSpirits = new List<ItemUISystem>();
        [SerializeField] Button sellButton;
        [SerializeField] Button upgradeButton;

        List<NumeralStatValueUI> numeralStatValuesUI;
        List<SpiritStatValueUI> spiritStatValuesUI;
        List<SlotWithCooldown> appliedEffectsUI = new List<SlotWithCooldown>();
        List<bool> isSlotEmpty = new List<bool>();
        ObjectPool appliedEffectsUIPool;
        WaitForSeconds deltaTimeDelay;
        SpiritSystem choosedSpirit;
        Animator baseAnimator;
        Animator expandAnimator;
        Button expandButton;
        string isOpenBool = "isOpen";
        string isExpandedBool = "isExpanded";

        Numeral[] hidedNumeralStats = new Numeral[]
        {
            Numeral.BuffTime,
            Numeral.DebuffTime,
            Numeral.ItemDropRate,
            Numeral.ItemQualityRate
        };

        Enums.Spirit[] hidedSpiritStats = new Enums.Spirit[]
        {
            Enums.Spirit.AttackSpeed,
            Enums.Spirit.ResourceRate,
            Enums.Spirit.ExpRate,
            Enums.Spirit.ManaRegen,
            Enums.Spirit.MulticritCount,
            Enums.Spirit.CritMultiplier
        };

        protected override void Awake()
        {
            base.Awake();

            baseAnimator = GetComponent<Animator>();
            expandButton = spiritName.transform.parent.GetComponent<Button>();
            expandButton.onClick.AddListener(ExpandStats);
            numeralStatValuesUI = new List<NumeralStatValueUI>(GetComponentsInChildren<NumeralStatValueUI>(true));
            spiritStatValuesUI = new List<SpiritStatValueUI>(GetComponentsInChildren<SpiritStatValueUI>(true));
            deltaTimeDelay = new WaitForSeconds(Time.deltaTime);
            appliedEffectsUIPool = new ObjectPool(ReferenceHolder.Instance.SlotWithCooldownPrefab, buffGroup.transform, 7);

            void ExpandStats()
            {
                var isExpanded = baseAnimator.GetBool(isExpandedBool);

                baseAnimator.SetBool(isExpandedBool, !isExpanded);
                HideExpandedStatValues(isExpanded);
            }
        }

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;

            sellButton = Instantiate(sellButton.gameObject, Owner.WorldCanvas.transform).GetComponent<Button>();
            upgradeButton = Instantiate(upgradeButton.gameObject, Owner.WorldCanvas.transform).GetComponent<Button>();

            Owner.InventoryUISystem.MoveItemToSpirit += OnMoveItemToSpirit;
            Owner.PlayerInputSystem.ClickedOnSpirit += OnClickedOnSpirit;
            Owner.PlayerInputSystem.ClickedOnCell += OnClickedOnCell;
            Owner.PlayerInputSystem.ClikedOnGround += OnClickedOnGround;
            Owner.PlayerInputSystem.RMBPresed += OnClickedOnGround;
            Owner.ItemDropSystem.ItemUICreated += OnItemUICreated;

            sellButton.onClick.AddListener(OnSellButtonClick);
            upgradeButton.onClick.AddListener(OnUpgradeButtonClick);

            void OnUpgradeButtonClick()
            {
                Upgrading?.Invoke();
                UpdateUI();
            }

            void OnSellButtonClick()
            {
                Selling?.Invoke();
                ActivateUI(false);
            }

            void OnItemUICreated(ItemUISystem itemUI)
            {
                itemUI.DoubleClickedInSpiritInventory += OnItemDoubleClicked;
                itemUI.System.StatsApplied += OnStatsApplied;

                void OnItemDoubleClicked(ItemUISystem _)
                {
                    if (itemUI.DraggedFrom == DraggedFrom.SpiritInventory)
                    {
                        RemoveItemFromSpirit();
                        MoveItemToPlayer?.Invoke(itemUI);
                    }

                    void RemoveItemFromSpirit()
                    {
                        if (itemUI.DraggedFrom == DraggedFrom.SpiritInventory)
                        {
                            allItemsUIInSpirits.Remove(itemUI);
                            isSlotEmpty[itemUI.SlotNumber] = true;
                            itemSlots[itemUI.SlotNumber].gameObject.SetActive(true);

                            ItemRemovedFromSpirit?.Invoke(new SpiritItemEventArgs(choosedSpirit, itemUI));
                            UpdateValues();
                        }
                    }
                }
            }

            void OnClickedOnSpirit(GameObject spirit) => ActivateUI(true);
            void OnClickedOnCell(GameObject spirit) => ActivateUI(false);
            void OnClickedOnGround() => ActivateUI(false);

            void OnMoveItemToSpirit(ItemUISystem itemUI)
            {
                var emptySlot = isSlotEmpty.IndexOf(true);

                if (emptySlot < 0)
                {
                    MoveItemToPlayer?.Invoke(itemUI);
                }
                else
                {
                    itemUI.transform.position = itemSlots[emptySlot].transform.position;
                    itemUI.transform.SetParent(itemSlots[emptySlot].transform.parent);

                    AddItemToSpirit(emptySlot);
                }

                void AddItemToSpirit(int slotNumber)
                {
                    allItemsUIInSpirits.Add(itemUI);
                    isSlotEmpty[slotNumber] = false;
                    itemSlots[slotNumber].gameObject.SetActive(false);

                    itemUI.DraggedFrom = DraggedFrom.SpiritInventory;
                    itemUI.SlotNumber = slotNumber;

                    ItemAddedToSpirit?.Invoke(new SpiritItemEventArgs(choosedSpirit, itemUI));
                    UpdateItems();
                    UpdateValues();
                }
            }

            void ActivateUI(bool activate)
            {
                if (Owner.PlayerInputSystem.ChoosedSpirit == null)
                    return;

                UnsubscribeFromSpiritEvents();

                if (activate)
                {
                    baseAnimator.SetBool(isOpenBool, true);
                    choosedSpirit = Owner.PlayerInputSystem.ChoosedSpirit;

                    if (choosedSpirit.Owner == Owner)
                    {
                        sellButton.gameObject.SetActive(true);

                        sellButton.transform.position = choosedSpirit.Prefab.transform.position + new Vector3(40, 30, 0);
                        upgradeButton.transform.position = choosedSpirit.Prefab.transform.position + new Vector3(40, 60, 30);
                    }

                    HideExpandedStatValues(true);
                    SubscribeToSpiritEvents();
                    UpdateUI();
                }
                else
                {
                    sellButton.gameObject.SetActive(false);
                    upgradeButton.gameObject.SetActive(false);

                    baseAnimator.SetBool(isOpenBool, false);
                    baseAnimator.SetBool(isExpandedBool, false);

                    UnsubscribeFromSpiritEvents();
                }

                void SubscribeToSpiritEvents()
                {
                    if (choosedSpirit == null) return;

                    choosedSpirit.AbilitySystems.ForEach(ability => ability.Used += OnAbilityUsed);
                    choosedSpirit.EffectApplied += OnEffectApplied;
                    choosedSpirit.EffectRemoved += OnEffectRemoved;
                    choosedSpirit.StatsChanged += OnStatsApplied;
                }

                void UnsubscribeFromSpiritEvents()
                {
                    if (choosedSpirit == null) return;

                    choosedSpirit.AbilitySystems.ForEach(ability => ability.Used -= OnAbilityUsed);
                    choosedSpirit.EffectApplied -= OnEffectApplied;
                    choosedSpirit.EffectRemoved -= OnEffectRemoved;
                    choosedSpirit.StatsChanged -= OnStatsApplied;
                }

                void OnEffectRemoved(Effect e)
                {
                    var appliedEffectUI = appliedEffectsUI.Find(effectUI => effectUI.EntityIndex == e.Index);

                    if (appliedEffectUI == null) return;

                    appliedEffectUI.gameObject.SetActive(false);
                    appliedEffectsUI.Remove(appliedEffectUI);
                }

                void OnEffectApplied(Effect e)
                {
                    var poolObject = appliedEffectsUIPool.PopObject();
                    var appliedEffectUI = poolObject.GetComponent<SlotWithCooldown>();

                    poolObject.GetComponent<Image>().sprite = e.Image;
                    appliedEffectsUI.Add(appliedEffectUI);
                    appliedEffectUI.EntityIndex = e.Index;
                    appliedEffectUI.Description = e.Description;
                }

                void OnAbilityUsed(AbilitySystem e)
                {
                    var slot = abilitySlots.Find(x => x.EntityIndex == e.Index);
                    slot.CooldownImage.fillAmount = 1f;

                    StartCoroutine(Cooldown());

                    IEnumerator Cooldown()
                    {
                        while (slot.CooldownImage.fillAmount > 0)
                        {
                            slot.CooldownImage.fillAmount -= 1 / e.Ability.Cooldown * Time.deltaTime * 2;
                            yield return deltaTimeDelay;
                        }
                    }
                }
            }

            void UpdateUI()
            {
                choosedSpirit = Owner.PlayerInputSystem.ChoosedSpirit;

                UpdateItems();
                UpdateValues();
                UpdateAbilities();
                UpdateTraits();

                void UpdateAbilities()
                {
                    var spiritAbilities = choosedSpirit.Data.Abilities;

                    abilitySlots.ForEach(abilitySlot => abilitySlot.gameObject.SetActive(false));

                    for (int i = 0; i < spiritAbilities.Count; i++)
                    {
                        abilitySlots[i].gameObject.SetActive(true);
                        abilitySlots[i].Description = spiritAbilities[i].Description;
                        abilitySlots[i].GetComponent<Image>().sprite = spiritAbilities[i].Image;
                        abilitySlots[i].CooldownImage.fillAmount = 0;
                        abilitySlots[i].EntityIndex = choosedSpirit.AbilitySystems[i].Index;
                    }
                }

                void UpdateTraits()
                {
                    var spiritTraits = choosedSpirit.Data.Traits;

                    traitSlots.ForEach(traitSlot => traitSlot.gameObject.SetActive(false));

                    for (int i = 0; i < spiritTraits.Count; i++)
                    {
                        traitSlots[i].gameObject.SetActive(true);
                        traitSlots[i].Description = spiritTraits[i].Description;
                        traitSlots[i].GetComponent<Image>().sprite = spiritTraits[i].Image;
                        traitSlots[i].CooldownImage.fillAmount = 0;
                    }
                }
            }

            void UpdateItems()
            {
                var choosedSpiritItems = choosedSpirit.Data.Inventory.Items;
                var maxSlots = choosedSpirit.Data.Get(Enums.Spirit.MaxInventorySlots).Value;

                isSlotEmpty.Clear();
                allItemsUIInSpirits.ForEach(item => item.gameObject.SetActive(false));

                UpdateSlotsAmount();

                choosedSpiritItems.ForEach(itemInInventory =>
                {
                    var itemFromDroppedItems = allItemsUIInSpirits.Find(droppedItem => itemInInventory.Index == droppedItem.System.Index);

                    if (itemFromDroppedItems != null)
                    {
                        itemFromDroppedItems.gameObject.SetActive(true);
                        isSlotEmpty[itemFromDroppedItems.SlotNumber] = false;
                    }
                });

                void UpdateSlotsAmount()
                {
                    for (int i = 0; i < itemSlots.Count; i++)
                    {
                        var isInSlotLimit = i < maxSlots;

                        itemSlots[i].gameObject.SetActive(isInSlotLimit);
                        isSlotEmpty.Add(isInSlotLimit);
                    }
                }
            }

            void OnStatsApplied() => UpdateValues();

            void UpdateValues()
            {
                var spirit = choosedSpirit.Data;

                spiritName.text = spirit.Name;
                image.sprite = spirit.Image;

                SetExpBarValue();
                SetStatValues();

                void SetExpBarValue()
                {
                    var neededExp = (float)ReferenceHolder.ExpToLevelUp[(int)spirit.Get(Numeral.Level).Value];
                    var currentExp = (float)spirit.Get(Numeral.Exp).Value;
                    var expBarValue = 1 / (neededExp / currentExp);

                    expBar.fillAmount = expBarValue;
                }

                void SetStatValues()
                {
                    numeralStatValuesUI.ForEach(statUI => statUI.Value.text = statUI.NumeralValue.AttributeToString());
                    spiritStatValuesUI.ForEach(statUI => statUI.Value.text = statUI.SpiritValue.AttributeToString());
                }
            }
        }

        void HideExpandedStatValues(bool hide)
        {
            for (int i = 0; i < hidedNumeralStats.Length; i++)
            {
                numeralStatValuesUI.Find(attributeUI => attributeUI.NumeralValue == hidedNumeralStats[i])?.gameObject.SetActive(!hide);
            }

            for (int i = 0; i < hidedSpiritStats.Length; i++)
            {
                spiritStatValuesUI.Find(attributeUI => attributeUI.SpiritValue == hidedSpiritStats[i])?.gameObject.SetActive(!hide);
            }
        }

        public void ActivateUpgradeButton(bool activate) => upgradeButton.gameObject.SetActive(activate);
    }
}

