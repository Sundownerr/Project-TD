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
        public List<EnemySystem> Enemies { get; private set; } = new List<EnemySystem>();
        public event EventHandler<EnemySystem> EnemyDied = delegate { };

        PlayerSystem Owner;

        public EnemyControlSystem(PlayerSystem player) => Owner = player;
        
        public void SetSystem()
        {
            Owner.WaveSystem.EnemyCreated += OnEnemySpawned;
        }

        public void UpdateSystem()
        {
            for (int i = 0; i < Enemies.Count; i++)
                Enemies[i].UpdateSystem();
        }

        void OnEnemySpawned(object _, EnemySystem e) => AddEnemy(e);
        void OnEnemyDied(object _, IHealthComponent e) => DestroyEnemy(e as EnemySystem);
        void OnLastWaypointReached(object _, EnemySystem e) => DestroyEnemy(e);

        void AddEnemy(EnemySystem enemy)
        {
            Enemies.Add(enemy);
          
            enemy.IsOn = true;
            enemy.LastWaypointReached += OnLastWaypointReached;
            enemy.Died += OnEnemyDied;
        }

        void DestroyEnemy(EnemySystem enemy)
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
