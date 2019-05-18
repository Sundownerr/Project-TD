using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Game.Utility;
using Game.Systems;
using Game.Systems.Enemy;

namespace Game.UI
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

        void OnClickedOnEnemy(GameObject e)
        {
            for (int i = 0; i < Owner.EnemyControlSystem.AllEnemies.Count; i++)
                if(Owner.EnemyControlSystem.AllEnemies[i].Prefab == e)
                {
                    enemy = Owner.EnemyControlSystem.AllEnemies[i];
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
                hpText.text = Uty.KiloFormat(enemy.Data.Get(Enums.Enemy.Health).Sum);
            }
            else
                EnemyUI.SetActive(false);           
        }
    }
}
