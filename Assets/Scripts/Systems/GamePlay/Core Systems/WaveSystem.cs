using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;
using System;
using Game.Enemy.Data;
using U = UnityEngine.Object;
using Game.Data;
using Game.Spirit;

namespace Game.Systems
{

    public class WaveSystem
    {
        public int WaveNumber { get; set; }
        public List<EnemyData> CurrentWaveEnemies { get; set; }
        public event EventHandler WaveEnded = delegate { };
        public event EventHandler WaveStarted = delegate { };
        public event EventHandler AllWaveEnemiesKilled = delegate { };
        public event EventHandler<EnemySystem> EnemyCreated = delegate { };
        public event EventHandler<EnemyCreationRequest> EnemyCreationRequested = delegate { };

        private List<List<EnemySystem>> wavesEnemySystem;
        private List<List<EnemyData>> wavesEnemyData;
        private List<int> currentEnemyCount;
        public PlayerSystem Owner { get; set; }

        public WaveSystem(PlayerSystem player)
        {
            Owner = player;

            CurrentWaveEnemies = new List<EnemyData>();
            wavesEnemySystem = new List<List<EnemySystem>>();
            wavesEnemyData = new List<List<EnemyData>>();
        }

        public void SetSystem()
        {
            if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
                Owner.NetworkPlayer.EnemyCreatingRequestDone += NetworkEnemyCreated;

            Owner.WaveUISystem.WaveStarted += OnWaveStarted;

            currentEnemyCount = new List<int>();
            wavesEnemyData = GenerateWaves(Owner.WaveAmount);
            WaveNumber = 1;
            CurrentWaveEnemies = wavesEnemyData[0];

            #region  Helper functions

            List<List<EnemyData>> GenerateWaves(int waveAmount)
            {
                var randomWaveIds = new List<int>(waveAmount);
                var raceTypes = Enum.GetValues(typeof(RaceType));
                var tempWaves = new List<List<EnemyData>>(waveAmount);
                var waves = ReferenceHolder.Get.WaveDataBase.Waves;

                for (int i = 0; i < waveAmount; i++)
                    randomWaveIds.Add(StaticRandom.Instance.Next(0, waves.Count));

                for (int i = 0; i < waveAmount; i++)
                    tempWaves.Add(WaveCreatingSystem.CreateWave(waves[randomWaveIds[i]], i));

                return tempWaves;
            }

            void NetworkEnemyCreated(object _, EnemySystem e)
            {
                wavesEnemySystem[wavesEnemySystem.Count - 1].Add(e);
                EnemyCreated?.Invoke(_, e);
            }

            #endregion
        }

        public void UpdateSystem()
        {
            AddMagicCrystalAfterWaveEnd();

            #region  Helper functions

            void AddMagicCrystalAfterWaveEnd()
            {
                for (int waveId = 0; waveId < wavesEnemySystem.Count; waveId++)
                {
                    var killedEnemyCount = 0;

                    for (int enemyId = 0; enemyId < wavesEnemySystem[waveId].Count; enemyId++)
                        if (wavesEnemySystem[waveId][enemyId].Prefab == null)
                        {
                            killedEnemyCount++;
                            if (killedEnemyCount == currentEnemyCount[waveId])
                            {
                                AllWaveEnemiesKilled?.Invoke(null, null);
                                wavesEnemySystem.RemoveAt(waveId);
                                currentEnemyCount.RemoveAt(waveId);
                                break;
                            }
                        }
                }
            }

            #endregion
        }

        public void OnWaveStarted(object _, EventArgs e)
        {
            currentEnemyCount.Add(wavesEnemyData[WaveNumber - 1].Count);
            wavesEnemySystem.Add(new List<EnemySystem>());

            ReferenceHolder.Get.StartCoroutine(SpawnEnemyWave(0.2f));

            #region  Helper functions

            IEnumerator SpawnEnemyWave(float delay)
            {
                var spawned = 0;
                var spawnDelay = new WaitForSeconds(delay);

                WaveStarted?.Invoke(null, null);

                while (spawned < CurrentWaveEnemies.Count)
                {
                    var spawnPosition = (int)CurrentWaveEnemies[spawned].Type == (int)EnemyType.Flying ?
                        Owner.Map.FlyingSpawnPoint.transform.position :
                        Owner.Map.GroundSpawnPoint.transform.position;

                    if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
                        EnemyCreationRequested?.Invoke(null, new EnemyCreationRequest(CurrentWaveEnemies[spawned], spawnPosition));
                    else
                        CreateEnemy();

                    spawned++;
                    yield return spawnDelay;

                    #region  Helper functions

                    void CreateEnemy()
                    {
                        var newEnemy = StaticMethods.CreateEnemy(CurrentWaveEnemies[spawned], spawnPosition, Owner);

                        wavesEnemySystem[wavesEnemySystem.Count - 1].Add(newEnemy);

                        EnemyCreated?.Invoke(null, newEnemy);
                    }

                    #endregion
                }

                if (WaveNumber <= Owner.WaveAmount)
                {
                    CurrentWaveEnemies = wavesEnemyData[WaveNumber];
                    WaveNumber++;
                    WaveEnded?.Invoke(null, null);
                }
            }

            #endregion
        }
    }
}
