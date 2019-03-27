using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;
using Game.Systems;
using Game.Spirit;
using Game.Data;
using Game.Enemy.Data;
using System;
using U = UnityEngine.Object;

namespace Game.Systems
{
    public class EnemyControlSystem
    {
        public List<EnemySystem> Enemies { get; private set; }
        public event EventHandler<EnemySystem> EnemyDied = delegate { };

        private PlayerSystem Owner;

        public EnemyControlSystem(PlayerSystem player)
        {
            Owner = player;
            Enemies = new List<EnemySystem>();
        }

        public void SetSystem()
        {
            Owner.WaveSystem.EnemyCreated += OnEnemySpawned;
        }

        public void UpdateSystem()
        {
            for (int i = 0; i < Enemies.Count; i++)
                Enemies[i].UpdateSystem();
        }

        private void OnEnemySpawned(object _, EnemySystem e) => AddEnemy(e);
        private void OnEnemyDied(object _, IHealthComponent e) => DestroyEnemy(e as EnemySystem);
        private void OnLastWaypointReached(object _, EnemySystem e) => DestroyEnemy(e);

        private void AddEnemy(EnemySystem enemy)
        {
           
            Enemies.Add(enemy);

            enemy.IsOn = true;
           // enemy.OwnerSystem = Owner;
            enemy.LastWaypointReached += OnLastWaypointReached;
            enemy.HealthSystem.Died += OnEnemyDied;
        }

        private void DestroyEnemy(EnemySystem enemy)
        {
            EnemyDied?.Invoke(null, enemy);

            if (enemy != null)
            {
                Enemies.Remove(enemy);
                
                U.Destroy(enemy.Prefab);
            }
        }
    }
}
