using Game.Spirit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Systems
{
    public class WorldUISystem : ExtendedMonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public GameObject DamageNumber;
        public GameObject LevelUpText;

        private ObjectPool damageNumbersPool, levelUpTextPool;
        private List<TextMeshProUGUI> damageNumbers, levelUpTexts;

        protected override void Awake()
        {
            base.Awake();
            
            DamageSystem.DamageDealt += OnDamageDealt;
            damageNumbers = new List<TextMeshProUGUI>();
            levelUpTexts = new List<TextMeshProUGUI>();

            damageNumbersPool = new ObjectPool
            {
                PoolObject = DamageNumber,
                PoolLenght = 5,
                Parent = transform
            };

            levelUpTextPool = new ObjectPool
            {
                PoolObject = LevelUpText,
                PoolLenght = 5,
                Parent = transform
            };
        }

        private void OnDamageDealt(object _, DamageEventArgs e)
        {
            if (e.Target == null || e.Target.Prefab == null)
                return;

            var damageNumber = damageNumbersPool.GetObject();
            var textComponent = damageNumber.GetComponent<TextMeshProUGUI>();
            var damageText = StaticMethods.KiloFormat(e.Damage);
            var fontColor = new Color(1, 1, 1, 1);
            var random = UnityEngine.Random.Range(-20, 20);
            var pos = e.Target.Prefab.transform.position + new Vector3(random, 90 + random, random);

            if (e.CritCount > 0)
            {
                var sb = new StringBuilder();
                if (e.CritCount > 1)
                    for (int i = 0; i < e.CritCount; i++)
                        sb.Append("!");

                fontColor = new Color(2f, 0.5f, 0.3f, 1);
                damageText = $"{damageText} {sb.ToString()}";
            }

            damageNumbers.Add(textComponent);
            damageNumber.SetActive(true);

            textComponent.text = damageText;
            textComponent.fontSize = 18 + e.CritCount * 2;
            textComponent.color = fontColor;

            damageNumber.transform.position = pos;
        }

        private void FixedUpdate()
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

        public void SetSystem(PlayerSystem player)
        {
            Owner = player;
            Owner.SpiritPlaceSystem.SpiritPlaced += OnSpiritPlaced;
        }

        private void OnSpiritPlaced(object _, SpiritSystem e) =>
            e.DataSystem.LeveledUp += OnSpiritLevelUp;
        

        private void OnSpiritLevelUp(object _, SpiritSystem spirit)
        {
            var text = levelUpTextPool.GetObject();
            var random = UnityEngine.Random.Range(-20, 20);
            text.SetActive(true);           
            text.transform.position = 
                spirit.Prefab.transform.position + new Vector3(random, Math.Abs(random + 10), random);

            var textComponent = text.GetComponent<TextMeshProUGUI>();
            levelUpTexts.Add(textComponent);
            textComponent.text = $"level up!\n{spirit.Data.Get(Numeral.Level, From.Base).Value}";
            StartCoroutine(DeactivateLevelUpText());

            #region Helper functions

            IEnumerator DeactivateLevelUpText()
            {
                yield return new WaitForSeconds(0.7f);
                text.SetActive(false);
            }

            #endregion
        } 
    }
}
