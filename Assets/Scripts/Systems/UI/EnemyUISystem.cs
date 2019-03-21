using Game.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game.Systems
{
    public class EnemyUISystem : MonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public GameObject EnemyUI;

        private EnemySystem enemy;
        private TextMeshProUGUI hpText;
        private Image hpBar;
        private float maxHealth;

      
        public void SetSystem(PlayerSystem player)
        {
            Owner = player;
            Owner.PlayerInputSystem.ClickedOnEnemy += OnClickedOnEnemy;
            hpText = EnemyUI.GetComponentInChildren<TextMeshProUGUI>();
            hpBar = EnemyUI.transform.GetChild(0).GetComponent<Image>();
            EnemyUI.SetActive(false);
        }

        private void OnClickedOnEnemy(object _, GameObject e)
        {
            for (int i = 0; i < Owner.Enemies.Count; i++)
                if(Owner.Enemies[i].Prefab == e)
                {
                    enemy = Owner.Enemies[i];
                    break;
                }    
            EnemyUI.SetActive(true);           
        }

        private void LateUpdate()
        {
            if (enemy != null && enemy.Prefab != null)
            {
                EnemyUI.transform.position = enemy.Prefab.transform.position + new Vector3(0, 90, 0);
                hpBar.fillAmount = (float)(enemy.Data.GetValue(Numeral.Health) / enemy.Data.GetValue(Numeral.MaxHealth));               
                hpText.text = StaticMethods.KiloFormat(enemy.Data.GetValue(Numeral.Health));
            }
            else
                EnemyUI.SetActive(false);           
        }
    }
}
