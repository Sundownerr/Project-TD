using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Spirit;
using System;
using System.Collections.Generic;
using Game.Data;
using Game.Enums;

namespace Game.Systems
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
        public event EventHandler Selling = delegate { };
        public event EventHandler Upgrading = delegate { };
        public event EventHandler<SpiritItemEventArgs> ItemAddedToSpirit = delegate { };
        public event EventHandler<SpiritItemEventArgs> ItemRemovedFromSpirit = delegate { };
        public event EventHandler<ItemUISystem> MoveItemToPlayer = delegate { };
        private List<NumeralStatValueUI> numeralStatValues;
        private List<SpiritStatValueUI> spiritStatValues;

        private List<bool> isSlotEmpty = new List<bool>();
        private ObjectPool appliedEffectsUIPool;
        private List<SlotWithCooldown> appliedEffectsUI = new List<SlotWithCooldown>();
        private SpiritSystem choosedSpirit;
        private Animator baseAnimator, expandAnimator;
        private Button expandButton;
        private string isOpen = "isOpen", isExpanded = "isExpanded";
        private Numeral[] hidedNumeralStats = new Numeral[]
        {
            Numeral.BuffDuration,
            Numeral.DebuffDuration,
            Numeral.ItemDropRatio,
            Numeral.ItemQuialityRatio
        };

        private Enums.Spirit[] hidedSpiritStats = new Enums.Spirit[]
        {
            Enums.Spirit.AttackSpeedModifier,
            Enums.Spirit.GoldRatio,
            Enums.Spirit.ExpRatio,
            Enums.Spirit.ManaRegen,
            Enums.Spirit.MulticritCount,
            Enums.Spirit.CritMultiplier,
        };

        protected override void Awake()
        {
            base.Awake();
            baseAnimator = GetComponent<Animator>();
            expandButton = SpiritName.transform.parent.GetComponent<Button>();
            expandButton.onClick.AddListener(ExpandStats);
            numeralStatValues = new List<NumeralStatValueUI>(GetComponentsInChildren<NumeralStatValueUI>(true));
            spiritStatValues = new List<SpiritStatValueUI>(GetComponentsInChildren<SpiritStatValueUI>(true));

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

            SellButton.onClick.AddListener(Sell);
            UpgradeButton.onClick.AddListener(Upgrade);
        }

        private void OnItemUICreated(object _, ItemUISystem itemUI)
        {
            itemUI.BeingDragged += OnItemBeingDragged;
            itemUI.DragEnd += OnItemDragEnd;
            itemUI.DoubleClickedInSpiritInventory += OnItemDoubleClicked;
            itemUI.System.StatsApplied += OnStatsApplied;
        }

        private void ActivateUI(bool activate)
        {
            if (Owner.PlayerInputSystem.ChoosedSpirit == null)
                return;

            UnsubscribeFromSpiritEvents();

            if (activate)
            {
                baseAnimator.SetBool(isOpen, true);
                choosedSpirit = Owner.PlayerInputSystem.ChoosedSpirit;
                choosedSpirit.DataSystem.StatsChanged += OnStatsApplied;

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
                if (choosedSpirit != null)
                    choosedSpirit.DataSystem.StatsChanged -= OnStatsApplied;

                SellButton.gameObject.SetActive(false);
                UpgradeButton.gameObject.SetActive(false);

                baseAnimator.SetBool(isOpen, false);
                baseAnimator.SetBool(isExpanded, false);

                UnsubscribeFromSpiritEvents();
            }

            #region Helper functions

            void SubscribeToSpiritEvents()
            {
                if (choosedSpirit == null)
                    return;

                for (int i = 0; i < choosedSpirit.AbilitySystems.Count; i++)
                    choosedSpirit.AbilitySystems[i].Used += OnAbilityUsed;

                choosedSpirit.EffectApplied += OnEffectApplied;
                choosedSpirit.EffectRemoved += OnEffectRemoved;
            }

            void UnsubscribeFromSpiritEvents()
            {
                if (choosedSpirit == null)
                    return;

                for (int i = 0; i < choosedSpirit.AbilitySystems.Count; i++)
                    choosedSpirit.AbilitySystems[i].Used -= OnAbilityUsed;

                choosedSpirit.EffectApplied -= OnEffectApplied;
                choosedSpirit.EffectRemoved -= OnEffectRemoved;
            }

            #endregion
        }

        private void OnEffectRemoved(object sender, Effect e)
        {
            var effectUI = appliedEffectsUI.Find(x => x.EntityID.Compare(e.ID));

            if (effectUI == null)
                return;

            effectUI.gameObject.SetActive(false);
            appliedEffectsUI.Remove(effectUI);
        }

        private void OnEffectApplied(object sender, Effect e)
        {
            var poolObject = appliedEffectsUIPool.PopObject();
            var effectUI = poolObject.GetComponent<SlotWithCooldown>();

            poolObject.GetComponent<Image>().sprite = e.Image;
            appliedEffectsUI.Add(effectUI);
            effectUI.EntityID = e.ID;
            effectUI.Description = e.Description;
        }

        private void OnClickedOnSpirit(object _, GameObject spirit) => ActivateUI(true);
        private void OnClickedOnCell(object _, GameObject spirit) => ActivateUI(false);
        private void OnClickedOnGround(object _, EventArgs e) => ActivateUI(false);
        private void OnStatsApplied(object _, EventArgs e) => UpdateValues();

        private void OnMoveItemToSpirit(object _, ItemUISystem itemUI)
        {
            for (int i = 0; i < isSlotEmpty.Count; i++)
                if (isSlotEmpty[i])
                {
                    itemUI.transform.position = ItemSlots[i].transform.position;
                    itemUI.transform.SetParent(ItemSlots[i].transform.parent);

                    AddItemToSpirit(itemUI, i);
                    return;
                }

            MoveItemToPlayer?.Invoke(null, itemUI);
        }

        public void OnItemDoubleClicked(object _, ItemUISystem itemUI)
        {
            if (itemUI.DraggedFrom == DraggedFrom.SpiritInventory)
            {
                RemoveItemFromSpirit(itemUI);
                MoveItemToPlayer?.Invoke(null, itemUI);
            }
        }

        private void Sell()
        {
            Selling?.Invoke(null, null);
            ActivateUI(false);
        }

        private void Upgrade()
        {
            Upgrading?.Invoke(null, null);
            UpdateUI();
        }

        private void HideExpandedStatValues(bool hide)
        {
            for (int i = 0; i < hidedNumeralStats.Length; i++)
                numeralStatValues.Find(x => x.Numeral == hidedNumeralStats[i]).gameObject.SetActive(!hide);
        }

        private void UpdateValues()
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
                for (int i = 0; i < numeralStatValues.Count; i++)
                    numeralStatValues[i].Value.text = NumeralToString(numeralStatValues[i].Numeral);

                for (int i = 0; i < spiritStatValues.Count; i++)
                    spiritStatValues[i].Value.text = SpiritToString(spiritStatValues[i].Spirit);

                #region Helper functions

                string NumeralToString(Numeral value)
                {
                    var withPercent =
                        value == Numeral.ItemDropRatio ||
                        value == Numeral.ItemQuialityRatio ||
                        value == Numeral.BuffDuration ||
                        value == Numeral.DebuffDuration;

                    if (value == Numeral.Level)
                        return $"{StaticMethods.KiloFormat((int)spirit.Get(value).Sum)}";

                    return $"{StaticMethods.KiloFormat(spirit.Get(value).Sum)}{(withPercent ? "%" : string.Empty)}";
                }

                string SpiritToString(Enums.Spirit value)
                {
                    var withPercent =
                        value == Enums.Spirit.SpellCritChance ||
                        value == Enums.Spirit.SpellDamage ||
                        value == Enums.Spirit.CritChance ||
                        value == Enums.Spirit.ExpRatio ||
                        value == Enums.Spirit.GoldRatio ||
                        value == Enums.Spirit.CritMultiplier;

                    return $"{StaticMethods.KiloFormat(spirit.Get(value).Sum)}{(withPercent ? "%" : string.Empty)}";
                }
                #endregion
            }
            #endregion
        }

        private void UpdateItems()
        {
            var spiritItems = choosedSpirit.Data.Inventory.Items;
            var maxSlots = choosedSpirit.Data.Get(Enums.Spirit.MaxInventorySlots).Value;

            isSlotEmpty.Clear();
            for (int i = 0; i < ItemSlots.Count; i++)
            {
                var isInSlotLimit = i < maxSlots;

                ItemSlots[i].gameObject.SetActive(isInSlotLimit);
                isSlotEmpty.Add(isInSlotLimit);
            }

            for (int i = 0; i < AllItemsUIInSpirits.Count; i++)
                AllItemsUIInSpirits[i].gameObject.SetActive(false);


            for (int i = 0; i < spiritItems.Count; i++)
                for (int j = 0; j < AllItemsUIInSpirits.Count; j++)
                    if (spiritItems[i].ID.Compare(AllItemsUIInSpirits[j].System.ID))
                    {
                        AllItemsUIInSpirits[j].gameObject.SetActive(true);
                        isSlotEmpty[AllItemsUIInSpirits[j].SlotNumber] = false;
                        break;
                    }
        }

        private void UpdateAbilities()
        {
            var spiritAbilities = choosedSpirit.Data.Abilities;

            for (int i = 0; i < AbilitySlots.Count; i++)
                AbilitySlots[i].gameObject.SetActive(false);

            for (int i = 0; i < spiritAbilities.Count; i++)
            {
                AbilitySlots[i].gameObject.SetActive(true);
                AbilitySlots[i].Description = spiritAbilities[i].Description;
                AbilitySlots[i].GetComponent<Image>().sprite = spiritAbilities[i].Image;
                AbilitySlots[i].CooldownImage.fillAmount = 0;
                AbilitySlots[i].EntityID = choosedSpirit.AbilitySystems[i].ID;
            }
        }

        private void OnAbilityUsed(object sender, AbilitySystem e)
        {
            var slot = AbilitySlots.Find(x => x.EntityID == e.ID);
            var delay = new WaitForSeconds(Time.deltaTime);

            slot.CooldownImage.fillAmount = 1f;

            StartCoroutine(Cooldown());

            #region Helper functions

            IEnumerator Cooldown()
            {
                while (slot.CooldownImage.fillAmount > 0)
                {
                    slot.CooldownImage.fillAmount -= 1 / e.Ability.Cooldown * Time.deltaTime * 2;
                    yield return delay;
                }
            }

            #endregion
        }

        private void UpdateTraits()
        {
            var spiritTraits = choosedSpirit.Data.Traits;

            for (int i = 0; i < TraitSlots.Count; i++)
                TraitSlots[i].gameObject.SetActive(false);

            for (int i = 0; i < spiritTraits.Count; i++)
            {
                TraitSlots[i].gameObject.SetActive(true);
                TraitSlots[i].Description = spiritTraits[i].Description;
                TraitSlots[i].GetComponent<Image>().sprite = spiritTraits[i].Image;
                TraitSlots[i].CooldownImage.fillAmount = 0;
            }
        }

        private void UpdateUI()
        {
            choosedSpirit = Owner.PlayerInputSystem.ChoosedSpirit;

            UpdateItems();
            UpdateValues();
            UpdateAbilities();
            UpdateTraits();
        }

        public void OnItemBeingDragged(object _, ItemDragEventArgs e)
        {
            choosedSpirit = Owner.PlayerInputSystem.ChoosedSpirit;
            RemoveItemFromSpirit(e.ItemUI);
        }

        public void OnItemDragEnd(object _, ItemDragEventArgs e)
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

        private void AddItemToSpirit(ItemUISystem itemUI, int slotNumber)
        {
            AllItemsUIInSpirits.Add(itemUI);
            isSlotEmpty[slotNumber] = false;
            ItemSlots[slotNumber].gameObject.SetActive(false);

            itemUI.DraggedFrom = DraggedFrom.SpiritInventory;
            itemUI.SlotNumber = slotNumber;

            ItemAddedToSpirit?.Invoke(null, new SpiritItemEventArgs(choosedSpirit, itemUI));
            UpdateItems();
            UpdateValues();
        }

        private void RemoveItemFromSpirit(ItemUISystem itemUI)
        {
            if (itemUI.DraggedFrom == DraggedFrom.SpiritInventory)
            {
                AllItemsUIInSpirits.Remove(itemUI);
                isSlotEmpty[itemUI.SlotNumber] = true;
                ItemSlots[itemUI.SlotNumber].gameObject.SetActive(true);

                ItemRemovedFromSpirit?.Invoke(null, new SpiritItemEventArgs(choosedSpirit, itemUI));
                UpdateValues();
            }
        }

        public void ActivateUpgradeButton(bool activate) => UpgradeButton.gameObject.SetActive(activate);
    }
}

