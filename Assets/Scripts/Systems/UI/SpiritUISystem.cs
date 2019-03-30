using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Spirit;
using System;
using System.Collections.Generic;
using Game.Data;

namespace Game.Systems
{
    public class SpiritUISystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; private set; }
        public GameObject SlotWithDescriptionPrefab;
        public TextMeshProUGUI Damage, Range, Mana, AttackSpeed, TriggerChance, SpellDamage, SpellCritChance;
        public TextMeshProUGUI SpiritName, CritChance, Level;
        public Image Image;
        public Button SellButton, UpgradeButton;
        public List<SlotWithCooldown> ItemSlots, AbilitySlots, TraitSlots;
        public List<ItemUISystem> AllItemsUIInSpirits = new List<ItemUISystem>();
        public event EventHandler Selling = delegate { };
        public event EventHandler Upgrading = delegate { };
        public event EventHandler<SpiritItemEventArgs> ItemAddedToSpirit = delegate { };
        public event EventHandler<SpiritItemEventArgs> ItemRemovedFromSpirit = delegate { };
        public event EventHandler<ItemUISystem> MoveItemToPlayer = delegate { };

        private List<bool> isSlotEmpty = new List<bool>();
        private SpiritSystem choosedSpirit;
        private Animator animator;

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
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

            if (activate)
            {
                animator.SetBool("isOpen", true);
                choosedSpirit = Owner.PlayerInputSystem.ChoosedSpirit;
                choosedSpirit.DataSystem.StatsChanged += OnStatsApplied;

                if (choosedSpirit.Owner == Owner)
                {
                    SellButton.gameObject.SetActive(true);

                    SellButton.transform.position = choosedSpirit.Prefab.transform.position + new Vector3(40, 30, 0);
                    UpgradeButton.transform.position = choosedSpirit.Prefab.transform.position + new Vector3(40, 60, 30);
                }

                UpdateUI();
            }
            else
            {
                if (choosedSpirit != null)
                    choosedSpirit.DataSystem.StatsChanged -= OnStatsApplied;

                SellButton.gameObject.SetActive(false);
                UpgradeButton.gameObject.SetActive(false);

                animator.SetBool("isOpen", false);
            }
        }

        private void OnClickedOnSpirit(object _, GameObject spirit) => ActivateUI(true);
        private void OnClickedOnCell(object _, GameObject spirit) => ActivateUI(false);
        private void OnClickedOnGround(object _, EventArgs e) => ActivateUI(false);
        public void OnStatsApplied(object _, EventArgs e) => UpdateValues();

        private void OnMoveItemToSpirit(object _, ItemUISystem itemUI)
        {
            var freeSlotIndex = -1;

            for (int i = 0; i < isSlotEmpty.Count; i++)
                if (isSlotEmpty[i])
                {
                    freeSlotIndex = i;
                    break;
                }

            if (freeSlotIndex < 0)
                MoveItemToPlayer?.Invoke(null, itemUI);
            else
            {
                var freeSlot = ItemSlots[freeSlotIndex];

                itemUI.transform.position = freeSlot.transform.position;
                itemUI.transform.SetParent(freeSlot.transform.parent);

                AddItemToSpirit(itemUI, freeSlotIndex);
            }
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

        private void UpdateValues()
        {
            var spirit = choosedSpirit.Data;

            SpiritName.text = spirit.Name;

            Image.sprite = spirit.Image;

            Level.text = StaticMethods.KiloFormat(
                spirit.Get(Numeral.Level, From.Base).Value +
                spirit.Get(Numeral.Level, From.Applied).Value);

            Damage.text = StaticMethods.KiloFormat(
                spirit.Get(Numeral.Damage, From.Base).Value +
                spirit.Get(Numeral.Damage, From.Applied).Value);

            Range.text = StaticMethods.KiloFormat(
                spirit.Get(Numeral.Range, From.Base).Value +
                spirit.Get(Numeral.Range, From.Applied).Value);

            Mana.text = StaticMethods.KiloFormat(
                spirit.Get(Numeral.Mana, From.Base).Value +
                spirit.Get(Numeral.Mana, From.Applied).Value);

            AttackSpeed.text = StaticMethods.KiloFormat(
                spirit.Get(Numeral.AttackSpeed, From.Base).Value +
                spirit.Get(Numeral.AttackSpeed, From.Applied).Value);

            TriggerChance.text = StaticMethods.KiloFormat(
                spirit.Get(Numeral.TriggerChance, From.Base).Value +
                spirit.Get(Numeral.TriggerChance, From.Base).Value) + "%";

            SpellCritChance.text = StaticMethods.KiloFormat(
                spirit.Get(Numeral.SpellCritChance, From.Base).Value +
                spirit.Get(Numeral.SpellCritChance, From.Base).Value) + "%";

            SpellDamage.text = StaticMethods.KiloFormat(
                spirit.Get(Numeral.SpellDamage, From.Base).Value +
                spirit.Get(Numeral.SpellDamage, From.Base).Value) + "%";

            CritChance.text = StaticMethods.KiloFormat(
                spirit.Get(Numeral.CritChance, From.Base).Value +
                spirit.Get(Numeral.CritChance, From.Base).Value) + "%";
        }

        private void UpdateItems()
        {
            var spiritItems = choosedSpirit.Data.Inventory.Items;
            var maxSlots = choosedSpirit.Data.Get(Numeral.MaxInventorySlots, From.Base).Value;

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
                choosedSpirit.AbilitySystems[i].Used += OnAbilityUsed;
            }
        }

        private void OnAbilityUsed(object sender, AbilitySystem e)
        {

            var slot = AbilitySlots.Find(x => x.EntityID == e.ID);
            var delay = new WaitForSeconds(Time.deltaTime);
            slot.CooldownImage.fillAmount = 1f;

            StartCoroutine(Cooldown());

            IEnumerator Cooldown()
            {
                while (slot.CooldownImage.fillAmount > 0)
                {            
                    slot.CooldownImage.fillAmount -= 1 / e.Ability.Cooldown * Time.deltaTime * 2;
                    yield return delay;
                }
            }
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

