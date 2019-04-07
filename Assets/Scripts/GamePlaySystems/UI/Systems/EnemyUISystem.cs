using Game.Enemy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.Enums;

namespace Game.Systems
{
    public class EnemyUISystem : MonoBehaviour
    {
        public PlayerSystem Owner { get; set; }
        public GameObject EnemyUI;

        EnemySystem enemy;
        TextMeshProUGUI hpText;
        Image hpBar;
        float maxHealth;

      
        public void SetSystem(PlayerSystem player)
        {
            Owner = player;
            Owner.PlayerInputSystem.ClickedOnEnemy += OnClickedOnEnemy;
            hpText = EnemyUI.GetComponentInChildren<TextMeshProUGUI>();
            hpBar = EnemyUI.transform.GetChild(0).GetComponent<Image>();
            EnemyUI.SetActive(false);
        }

        void OnClickedOnEnemy(object _, GameObject e)
        {
            for (int i = 0; i < Owner.Enemies.Count; i++)
                if(Owner.Enemies[i].Prefab == e)
                {
                    enemy = Owner.Enemies[i];
                    break;
                }    
            EnemyUI.SetActive(true);           
        }

        void LateUpdate()
        {
            if (enemy != null && enemy.Prefab != null)
            {
                EnemyUI.transform.position = enemy.Prefab.transform.position + new Vector3(0, 90, 0);
                hpBar.fillAmount = (float)(enemy.Data.Get(Enums.Enemy.Health).Sum / enemy.Data.Get(Enums.Enemy.MaxHealth).Sum);               
                hpText.text = StaticMethods.KiloFormat(enemy.Data.Get(Enums.Enemy.Health).Sum);
            }
            else
                EnemyUI.SetActive(false);           
        }
    }
}
