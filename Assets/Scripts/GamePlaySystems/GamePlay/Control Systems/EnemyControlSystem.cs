using System.Collections.Generic;
using System;
using U = UnityEngine.Object;
using Game.Systems.Enemy;

namespace Game.Systems
{
    public class EnemyControlSystem
    {
        public List<EnemySystem> OwnedEnemies { get; private set; } = new List<EnemySystem>();
        public List<EnemySystem> NotOwnedEnemies { get; private set; } = new List<EnemySystem>();
        public List<EnemySystem> AllEnemies { get; private set; } = new List<EnemySystem>();
        public event Action<EnemySystem> EnemyDied;

        PlayerSystem Owner;

        public EnemyControlSystem(PlayerSystem player) => Owner = player;

        public void SetSystem()
        {
            Owner.WaveSystem.EnemyCreated += OnEnemySpawned;
        }

        public void UpdateSystem()
        {
            for (int i = 0; i < AllEnemies.Count; i++)
                AllEnemies[i].UpdateSystem();
        }

        void OnEnemySpawned(EnemySystem e) => AddEnemy(e);
        void OnEnemyDied(IHealthComponent e) => DestroyEnemy(e as EnemySystem);
        void OnLastWaypointReached(EnemySystem e) => DestroyEnemy(e);

        void AddEnemy(EnemySystem enemy)
        {
            AllEnemies.Add(enemy);
           
            if (enemy.IsOwnedByLocalPlayer)
                OwnedEnemies.Add(enemy);
            else
                NotOwnedEnemies.Add(enemy);

            enemy.IsOn = true;
            enemy.LastWaypointReached += OnLastWaypointReached;
            enemy.Died += OnEnemyDied;
        }

        void DestroyEnemy(EnemySystem enemy)
        {
            EnemyDied?.Invoke(enemy);

            AllEnemies.Remove(enemy);

            if (enemy != null)
            {
                if (enemy.IsOwnedByLocalPlayer)
                    OwnedEnemies.Remove(enemy);
                else
                    NotOwnedEnemies.Remove(enemy);

                U.Destroy(enemy.Prefab);
                enemy = null;
            }
        }
    }
}
