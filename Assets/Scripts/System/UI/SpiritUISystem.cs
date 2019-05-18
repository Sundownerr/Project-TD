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
        public PlayerSystem Owner { get; private set; }
        public GameObject SlotWithDescriptionPrefab, SlotWithCooldownPrefab, BuffGroup;
        public TextMeshProUGUI SpiritName;
        public Image Image, ExpBar;
        public Button SellButton, UpgradeButton;
        public List<SlotWithCooldown> ItemSlots, AbilitySlots, TraitSlots;

        public List<ItemUISystem> AllItemsUIInSpirits = new List<ItemUISystem>();
        public event Action Selling;
        public event Action Upgrading;
        public event Action<SpiritItemEventArgs> ItemAddedToSpirit;
        public event Action<SpiritItemEventArgs> ItemRemovedFromSpirit;
        public event Action<ItemUISystem> MoveItemToPlayer;
        List<NumeralStatValueUI> numeralStatValues;
        List<SpiritStatValueUI> spiritStatValues;

        List<bool> isSlotEmpty = new List<bool>();
        List<SlotWithCooldown> appliedEffectsUI = new List<SlotWithCooldown>();
        ObjectPool appliedEffectsUIPool;
        SpiritSystem choosedSpirit;
        WaitForSeconds deltaTimeDelay;
        Animator baseAnimator, expandAnimator;
        Button expandButton;
        string isOpen = "isOpen", isExpanded = "isExpanded";
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
            expandButton = SpiritName.transform.parent.GetComponent<Button>();
            expandButton.onClick.AddListener(ExpandStats);
            numeralStatValues = new List<NumeralStatValueUI>(GetComponentsInChildren<NumeralStatValueUI>(true));
            spiritStatValues = new List<SpiritStatValueUI>(GetComponentsInChildren<SpiritStatValueUI>(true));
            deltaTimeDelay = new WaitForSeconds(Time.deltaTime);
            appliedEffectsUIPool = new ObjectPool(SlotWithCooldownPrefab, BuffGroup.transform, 7);

            #region Helper functions

            void ExpandStats()
            {
                var isExpanded = baseAnimator.GetBool("isExpanded");
                baseAnimator.SetBool("isExpanded", !isExpanded);
                HideExpandedStatValues(isExpanded);
            }

            #endregion
        }

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;

            SellButton = Instantiate(SellButton.gameObject, Owner.WorldCanvas.transform).GetComponent<Button>();
            UpgradeButton = Instantiate(UpgradeButton.gameObject, Owner.WorldCanvas.transform).GetComponent<Button>();

            Owner.InventoryUISystem.MoveItemToSpirit += OnMoveItemToSpirit;
            Owner.PlayerInputSystem.ClickedOnSpirit += OnClickedOnSpirit;
            Owner.PlayerInputSystem.ClickedOnCell += OnClickedOnCell;
            Owner.PlayerInputSystem.ClikedOnGround += OnClickedOnGround;
            Owner.PlayerInputSystem.RMBPresed += OnClickedOnGround;
            Owner.ItemDropSystem.ItemUICreated += OnItemUICreated;

            SellButton.onClick.AddListener(() =>
            {
                Selling?.Invoke();
                ActivateUI(false);
            });

            UpgradeButton.onClick.AddListener(() =>
            {
                Upgrading?.Invoke();
                UpdateUI();
            });
        }

        void OnItemUICreated(ItemUISystem itemUI)
        {
            itemUI.BeingDragged += OnItemBeingDragged;
            itemUI.DragEnd += OnItemDragEnd;
            itemUI.DoubleClickedInSpiritInventory += OnItemDoubleClicked;
            itemUI.System.StatsApplied += OnStatsApplied;
        }

        void ActivateUI(bool activate)
        {
            if (Owner.PlayerInputSystem.ChoosedSpirit == null)
                return;

            UnsubscribeFromSpiritEvents();

            if (activate)
            {
                baseAnimator.SetBool(isOpen, true);
                choosedSpirit = Owner.PlayerInputSystem.ChoosedSpirit;

                if (choosedSpirit.Owner == Owner)
                {
                    SellButton.gameObject.SetActive(true);

                    SellButton.transform.position = choosedSpirit.Prefab.transform.position + new Vector3(40, 30, 0);
                    UpgradeButton.transform.position = choosedSpirit.Prefab.transform.position + new Vector3(40, 60, 30);
                }

                HideExpandedStatValues(true);
                SubscribeToSpiritEvents();
                UpdateUI();
            }
            else
            {
                SellButton.gameObject.SetActive(false);
                UpgradeButton.gameObject.SetActive(false);

                baseAnimator.SetBool(isOpen, false);
                baseAnimator.SetBool(isExpanded, false);

                UnsubscribeFromSpiritEvents();
            }

            #region Helper functions

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

            #endregion
        }

        void OnEffectRemoved(Effect e)
        {
            var appliedEffectUI = appliedEffectsUI.Find(x => x.EntityIndex == e.Index);
            
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

        void OnClickedOnSpirit(GameObject spirit) => ActivateUI(true);
        void OnClickedOnCell(GameObject spirit) => ActivateUI(false);
        void OnClickedOnGround() => ActivateUI(false);
        void OnStatsApplied() => UpdateValues();

        void OnMoveItemToSpirit(ItemUISystem itemUI)
        {
            var emptySlot = isSlotEmpty.IndexOf(true);

            if (emptySlot < 0)
                MoveItemToPlayer?.Invoke(itemUI);
            else
            {
                itemUI.transform.position = ItemSlots[emptySlot].transform.position;
                itemUI.transform.SetParent(ItemSlots[emptySlot].transform.parent);

                AddItemToSpirit(itemUI, emptySlot);
            }
        }

        public void OnItemDoubleClicked(ItemUISystem itemUI)
        {
            if (itemUI.DraggedFrom == DraggedFrom.SpiritInventory)
            {
                RemoveItemFromSpirit(itemUI);
                MoveItemToPlayer?.Invoke(itemUI);
            }
        }

        void HideExpandedStatValues(bool hide)
        {
            for (int i = 0; i < hidedNumeralStats.Length; i++)
                numeralStatValues.Find(x => x.NumeralValue == hidedNumeralStats[i])?.gameObject.SetActive(!hide);

            for (int i = 0; i < hidedSpiritStats.Length; i++)
                spiritStatValues.Find(x => x.SpiritValue == hidedSpiritStats[i])?.gameObject.SetActive(!hide);
        }

        void UpdateValues()
        {
            var spirit = choosedSpirit.Data;

            SpiritName.text = spirit.Name;
            Image.sprite = spirit.Image;

            SetExpBarValue();
            SetStatValues();

            #region Helper functions

            void SetExpBarValue()
            {
                var expBarValue = 1 / (float)(ReferenceHolder.ExpToLevelUp[(int)spirit.Get(Numeral.Level).Value] / spirit.Get(Numeral.Exp).Value);
                ExpBar.fillAmount = expBarValue;
            }

            void SetStatValues()
            {
                numeralStatValues.ForEach(stat => stat.Value.text = NumeralToString(stat.NumeralValue));
                spiritStatValues.ForEach(stat => stat.Value.text = SpiritToString(stat.SpiritValue));

                #region Helper functions

                string NumeralToString(Numeral value)
                {
                    var withPercent =
                        value == Numeral.ItemDropRate ||
                        value == Numeral.ItemQualityRate ||
                        value == Numeral.BuffTime ||
                        value == Numeral.DebuffTime;

                    if (value == Numeral.Level)
                        return $"{Uty.KiloFormat((int)spirit.Get(value).Sum)}";

                    return $"{Uty.KiloFormat(spirit.Get(value).Sum)}{(withPercent ? "%" : string.Empty)}";
                }

                string SpiritToString(Enums.Spirit value)
                {
                    var withPercent =
                        value == Enums.Spirit.SpellCritChance ||
                        value == Enums.Spirit.SpellDamage ||
                        value == Enums.Spirit.CritChance ||
                        value == Enums.Spirit.ExpRate ||
                        value == Enums.Spirit.ResourceRate ||
                        value == Enums.Spirit.CritMultiplier;

                    return $"{Uty.KiloFormat(spirit.Get(value).Sum)}{(withPercent ? "%" : string.Empty)}";
                }
                #endregion
            }
            #endregion
        }

        void UpdateItems()
        {
            var choosedSpiritItems = choosedSpirit.Data.Inventory.Items;
            var maxSlots = choosedSpirit.Data.Get(Enums.Spirit.MaxInventorySlots).Value;

            isSlotEmpty.Clear();
            AllItemsUIInSpirits.ForEach(item => item.gameObject.SetActive(false));

            UpdateSlotsAmount();

            choosedSpiritItems.ForEach(itemInInventory =>
            {
                var itemFromDroppedItems = AllItemsUIInSpirits.Find(droppedItem => itemInInventory.Index == droppedItem.System.Index);

                if (itemFromDroppedItems != null)
                {
                    itemFromDroppedItems.gameObject.SetActive(true);
                    isSlotEmpty[itemFromDroppedItems.SlotNumber] = false;
                }
            });

            void UpdateSlotsAmount()
            {
                for (int i = 0; i < ItemSlots.Count; i++)
                {
                    var isInSlotLimit = i < maxSlots;

                    ItemSlots[i].gameObject.SetActive(isInSlotLimit);
                    isSlotEmpty.Add(isInSlotLimit);
                }
            }
        }

        void UpdateAbilities()
        {
            var spiritAbilities = choosedSpirit.Data.Abilities;

            AbilitySlots.ForEach(abilitySlot => abilitySlot.gameObject.SetActive(false));

            for (int i = 0; i < spiritAbilities.Count; i++)
            {
                AbilitySlots[i].gameObject.SetActive(true);
                AbilitySlots[i].Description = spiritAbilities[i].Description;
                AbilitySlots[i].GetComponent<Image>().sprite = spiritAbilities[i].Image;
                AbilitySlots[i].CooldownImage.fillAmount = 0;
                AbilitySlots[i].EntityIndex = choosedSpirit.AbilitySystems[i].Index;
            }
        }

        void OnAbilityUsed(AbilitySystem e)
        {
            var slot = AbilitySlots.Find(x => x.EntityIndex == e.Index);
            slot.CooldownImage.fillAmount = 1f;

            StartCoroutine(Cooldown());

            #region Helper functions

            IEnumerator Cooldown()
            {
                while (slot.CooldownImage.fillAmount > 0)
                {
                    slot.CooldownImage.fillAmount -= 1 / e.Ability.Cooldown * Time.deltaTime * 2;
                    yield return deltaTimeDelay;
                }
            }

            #endregion
        }

        void UpdateTraits()
        {
            var spiritTraits = choosedSpirit.Data.Traits;

            TraitSlots.ForEach(traitSlot => traitSlot.gameObject.SetActive(false));

            for (int i = 0; i < spiritTraits.Count; i++)
            {
                TraitSlots[i].gameObject.SetActive(true);
                TraitSlots[i].Description = spiritTraits[i].Description;
                TraitSlots[i].GetComponent<Image>().sprite = spiritTraits[i].Image;
                TraitSlots[i].CooldownImage.fillAmount = 0;
            }
        }

        void UpdateUI()
        {
            choosedSpirit = Owner.PlayerInputSystem.ChoosedSpirit;

            UpdateItems();
            UpdateValues();
            UpdateAbilities();
            UpdateTraits();
        }

        void OnItemBeingDragged(ItemDragEventArgs e)
        {
            choosedSpirit = Owner.PlayerInputSystem.ChoosedSpirit;
            RemoveItemFromSpirit(e.ItemUI);
        }

        void OnItemDragEnd(ItemDragEventArgs e)
        {
            choosedSpirit = Owner.PlayerInputSystem.ChoosedSpirit;

            if (e.OverlappedSlot == null && e.ItemUI.DraggedFrom == DraggedFrom.SpiritInventory)
                AddItemToSpirit(e.ItemUI, e.ItemUI.SlotNumber);

            for (int i = 0; i < ItemSlots.Count; i++)
                if (e.OverlappedSlot == ItemSlots[i])
                    if (isSlotEmpty[i])
                    {
                        AddItemToSpirit(e.ItemUI, i);
                        return;
                    }
        }

        void AddItemToSpirit(ItemUISystem itemUI, int slotNumber)
        {
            AllItemsUIInSpirits.Add(itemUI);
            isSlotEmpty[slotNumber] = false;
            ItemSlots[slotNumber].gameObject.SetActive(false);

            itemUI.DraggedFrom = DraggedFrom.SpiritInventory;
            itemUI.SlotNumber = slotNumber;

            ItemAddedToSpirit?.Invoke(new SpiritItemEventArgs(choosedSpirit, itemUI));
            UpdateItems();
            UpdateValues();
        }

        void RemoveItemFromSpirit(ItemUISystem itemUI)
        {
            if (itemUI.DraggedFrom == DraggedFrom.SpiritInventory)
            {
                AllItemsUIInSpirits.Remove(itemUI);
                isSlotEmpty[itemUI.SlotNumber] = true;
                ItemSlots[itemUI.SlotNumber].gameObject.SetActive(true);

                ItemRemovedFromSpirit?.Invoke(new SpiritItemEventArgs(choosedSpirit, itemUI));
                UpdateValues();
            }
        }

        public void ActivateUpgradeButton(bool activate) => UpgradeButton.gameObject.SetActive(activate);
    }
}

