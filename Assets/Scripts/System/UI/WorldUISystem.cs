using Game.Enums;
using Game.Systems;
using Game.Systems.Spirit;
using Game.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class WorldUISystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public GameObject DamageNumber;
        public GameObject LevelUpText;

        ObjectPool damageNumbersPool, levelUpTextPool;
        List<TextMeshProUGUI> damageNumbers = new List<TextMeshProUGUI>(), levelUpTexts = new List<TextMeshProUGUI>();
        WaitForSeconds levelUpTextFadeDelay = new WaitForSeconds(0.7f);
        Color defaultFontColor = new Color(1, 1, 1, 1);
        Color critFontColor = new Color(2f, 0.5f, 0.3f, 1);

        protected override void Awake()
        {
            base.Awake();

            DamageSystem.DamageDealt += OnDamageDealt;

            damageNumbersPool = new ObjectPool(DamageNumber, transform, 15);
            levelUpTextPool = new ObjectPool(LevelUpText, transform, 5);
        }

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;
            Owner.SpiritPlaceSystem.SpiritPlaced += OnSpiritPlaced;
        }

        void FixedUpdate()
        {
            for (int i = 0; i < damageNumbers.Count; i++)
                if (damageNumbers[i].fontSize > 11)
                    damageNumbers[i].fontSize -= 0.3f;
                else
                {
                    damageNumbers[i].gameObject.SetActive(false);
                    damageNumbers.RemoveAt(i);
                }
        }

        void OnDamageDealt(DamageEventArgs e)
        {
            if (e.Target == null || e.Target.Prefab == null)
                return;

            var damageNumber = damageNumbersPool.PopObject();
            var textComponent = damageNumber.GetComponent<TextMeshProUGUI>();
            var damageText = StaticMethods.KiloFormat((int)e.Damage);
            var fontColor = defaultFontColor;
            var random = UnityEngine.Random.Range(-20, 20);
            var textPositionOffset = new Vector3(random, 90 + random, random);

            if (e.CritCount > 0)
            {
                var sb = new StringBuilder();

                for (int i = 1; i < e.CritCount; i++)
                    sb.Append("!");

                fontColor = critFontColor;
                damageText = $"{damageText} {sb.ToString()}";
            }

            damageNumbers.Add(textComponent);
            damageNumber.SetActive(true);

            textComponent.text = damageText;
            textComponent.fontSize = 18 + e.CritCount * 2;
            textComponent.color = fontColor;

            damageNumber.transform.position = e.Target.Prefab.transform.position + textPositionOffset;
        }

        void OnSpiritPlaced(SpiritSystem e) => e.LeveledUp += OnSpiritLevelUp;

        void OnSpiritLevelUp(SpiritSystem spirit)
        {
            var text = levelUpTextPool.PopObject();
            var random = UnityEngine.Random.Range(-20, 20);
            var textPositionOffset = new Vector3(random, Math.Abs(random + 10), random);


            text.SetActive(true);
            text.transform.position = spirit.Prefab.transform.position + textPositionOffset;

            var textComponent = text.GetComponent<TextMeshProUGUI>();
            levelUpTexts.Add(textComponent);
            textComponent.text = $"level up!\n{(int)spirit.Data.Get(Numeral.Level).Value}";
            StartCoroutine(DeactivateLevelUpText());

            #region Helper functions

            IEnumerator DeactivateLevelUpText()
            {
                yield return levelUpTextFadeDelay;
                text.SetActive(false);
            }

            #endregion
        }
    }
}
