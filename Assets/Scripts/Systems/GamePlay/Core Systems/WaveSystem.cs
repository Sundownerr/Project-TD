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
        public Wave CurrentWave { get; set; }
        public event EventHandler WaveEnded = delegate { };
        public event EventHandler WaveStarted = delegate { };
        public event EventHandler AllWaveEnemiesKilled = delegate { };
        public event EventHandler<EnemySystem> EnemyCreated = delegate { };
        public event EventHandler<EnemyCreationRequest> EnemyCreationRequested = delegate { };
        public event EventHandler WavesGenerated = delegate { };


        private List<List<EnemySystem>> wavesEnemySystem;
        private List<Wave> waves;
        private List<int> currentEnemyCount;
        public PlayerSystem Owner { get; set; }
        public List<Wave> Waves { get => waves; private set => waves = value; }
        public Vector3[] GroundWaypoints { get; private set; }
        public Vector3[] FlyingWaypoints { get; private set; }

        public WaveSystem(PlayerSystem player)
        {
            Owner = player;
            wavesEnemySystem = new List<List<EnemySystem>>();
            Waves = new List<Wave>();
            currentEnemyCount = new List<int>();
        }

        public void SetSystem()
        {
            var generatedWaves = new List<Wave>();
            SetWaypoints();

            Owner.WaveUISystem.WaveStarted += OnWaveStarted;

            if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
            {
                Owner.NetworkPlayer.EnemyCreatingRequestDone += NetworkEnemyCreated;
                generatedWaves = WaveCreatingSystem.GenerateWaves(Owner.NetworkPlayer.WaveEnenmyIDs);
            }
            else
                generatedWaves = WaveCreatingSystem.GenerateWaves(Owner.WaveAmount);

            Waves = generatedWaves;
            WaveNumber = 1;
            CurrentWave = generatedWaves[0];

            //   WavesGenerated?.Invoke(null, null);

            #region  Helper functions

            void NetworkEnemyCreated(object _, EnemySystem e)
            {
                wavesEnemySystem[wavesEnemySystem.Count - 1].Add(e);
                EnemyCreated?.Invoke(_, e);
            }

            void SetWaypoints()
            {
                GroundWaypoints = new Vector3[Owner.Map.GroundWaypoints.Length];
                FlyingWaypoints = new Vector3[Owner.Map.FlyingWaypoints.Length];

                for (int i = 0; i < GroundWaypoints.Length; i++)
                    GroundWaypoints[i] = Owner.Map.GroundWaypoints[i].transform.position;

                for (int i = 0; i < FlyingWaypoints.Length; i++)
                    FlyingWaypoints[i] = Owner.Map.FlyingWaypoints[i].transform.position;
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
            currentEnemyCount.Add(Waves[WaveNumber - 1].EnemyTypes.Count);
            wavesEnemySystem.Add(new List<EnemySystem>());

            ReferenceHolder.Get.StartCoroutine(SpawnEnemyWave(0.2f));

            #region  Helper functions

            IEnumerator SpawnEnemyWave(float delay)
            {
                var spawned = 0;
                var spawnDelay = new WaitForSeconds(delay);

                WaveStarted?.Invoke(null, null);

                while (spawned < CurrentWave.EnemyTypes.Count)
                {
                    Vector3 spawnPosition;
                    Vector3[] waypoints;
                    GetSpawnAndWayPoints();

                    if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
                        CreateEnemyRequest();
                    else
                        CreateEnemy();

                    spawned++;
                    yield return spawnDelay;

                    #region  Helper functions

                    void CreateEnemy()
                    {
                        var newEnemy = StaticMethods.CreateEnemy(CurrentWave.EnemyTypes[spawned], spawnPosition, Owner, waypoints);
                        wavesEnemySystem[wavesEnemySystem.Count - 1].Add(newEnemy);
                        EnemyCreated?.Invoke(null, newEnemy);
                    }

                    void CreateEnemyRequest()
                    {
                        var enemy = CurrentWave.EnemyTypes[spawned];
                        var position = spawnPosition.ToCoordinates3D();

                        EnemyCreationRequested?.Invoke(null, new EnemyCreationRequest()
                        {
                            ID = enemy.ID,
                            Race = (int)enemy.Race,
                            WaveNumber = WaveNumber,
                            Position = position,
                            AbilityIDs = enemy.Abilities.GetIDs<Ability>(),
                            TraitIDs = enemy.Traits.GetIDs<Trait>(),
                            Waypoints = new ListCoordinates3D(waypoints)
                        });
                    }

                    void GetSpawnAndWayPoints()
                    {
                        if ((int)CurrentWave.EnemyTypes[spawned].Type == (int)EnemyType.Flying)
                        {
                            spawnPosition = Owner.Map.FlyingSpawnPoint.transform.position;
                            waypoints = FlyingWaypoints;
                        }
                        else
                        {
                            spawnPosition = Owner.Map.GroundSpawnPoint.transform.position;
                            waypoints = GroundWaypoints;
                        }
                    }

                    #endregion
                }

                if (WaveNumber <= Owner.WaveAmount)
                {
                    CurrentWave = Waves[WaveNumber];
                    WaveNumber++;
                    WaveEnded?.Invoke(null, null);
                }
            }

            #endregion
        }
    }
}
