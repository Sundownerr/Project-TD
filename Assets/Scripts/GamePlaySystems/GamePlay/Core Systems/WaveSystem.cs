using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using UnityEngine;
using System;
using Game.Enemy.Data;
using U = UnityEngine.Object;
using Game.Data;
using Game.Spirit;
using Game.Enums;

namespace Game.Systems
{

    public class WaveSystem
    {
        public int WaveNumber { get; set; }
        public PlayerSystem Owner { get; private set; }
        public Queue<Wave> Waves { get; private set; }  = new Queue<Wave>();
        public List<Wave> ListWaves { get; private set; }
        public Vector3[] GroundWaypoints { get; private set; }
        public Vector3[] FlyingWaypoints { get; private set; }
        public event EventHandler WaveEnded = delegate { };
        public event EventHandler WaveStarted = delegate { };
        public event EventHandler AllWaveEnemiesKilled = delegate { };
        public event EventHandler<EnemySystem> EnemyCreated = delegate { };
        public event EventHandler<EnemyCreationRequest> EnemyCreationRequested = delegate { };
        public event EventHandler WavesGenerated = delegate { };

        List<List<EnemySystem>> wavesEnemySystem  = new List<List<EnemySystem>>();
        List<int> currentEnemyCount = new List<int>();
        WaitForSeconds spawnDelay = new WaitForSeconds(0.2f);
        int spawned;
        Coroutine spawnCoroutine;

        public WaveSystem(PlayerSystem player) => Owner = player;
        
        public void SetSystem()
        {
            SetWaypoints();

            Owner.WaveUISystem.WaveStarted += OnWaveStarted;

            if (GameManager.Instance.GameState == GameState.MultiplayerInGame)
            {
                Owner.NetworkPlayer.EnemyCreatingRequestDone += NetworkEnemyCreated;
                Waves = WaveCreatingSystem.GenerateWaves(Owner.NetworkPlayer.WaveEnenmyIDs);
            }
            else
                Waves = WaveCreatingSystem.GenerateWaves(Owner.WaveAmount);

            ListWaves = new List<Wave>(Waves);
            Waves.Dequeue();
            
            //   WavesGenerated?.Invoke(null, null);

            #region  Helper functions

            void NetworkEnemyCreated(object _, EnemySystem e)
            {
                wavesEnemySystem[wavesEnemySystem.Count - 1].Add(e);
                EnemyCreated?.Invoke(_, e);

                if (spawned == Waves.Peek().EnemyTypes.Count - 1)
                    if (WaveNumber <= Owner.WaveAmount)
                    {
                        if(spawnCoroutine != null)
                            ReferenceHolder.Get.StopCoroutine(spawnCoroutine);
                        SetNextWave();
                    }     
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

        void SetNextWave()
        {
            Waves.Dequeue();
            WaveNumber++;
            WaveEnded?.Invoke(null, null);
        }

        public void OnWaveStarted(object _, EventArgs e)
        {
            currentEnemyCount.Add(Waves.Peek().EnemyTypes.Count);
            wavesEnemySystem.Add(new List<EnemySystem>());

            spawnCoroutine = ReferenceHolder.Get.StartCoroutine(SpawnEnemyWave());

            #region  Helper functions

            IEnumerator SpawnEnemyWave()
            {
                WaveStarted?.Invoke(null, null);

                spawned = 0;

                while (spawned < Waves.Peek().EnemyTypes.Count)
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
                        var newEnemy = StaticMethods.CreateEnemy(Waves.Peek().EnemyTypes[spawned], spawnPosition, Owner, waypoints);
                        wavesEnemySystem[wavesEnemySystem.Count - 1].Add(newEnemy);
                        EnemyCreated?.Invoke(null, newEnemy);
                    }

                    void CreateEnemyRequest()
                    {
                        var enemy = Waves.Peek().EnemyTypes[spawned];
                        var position = spawnPosition.ToCoordinates3D();

                        EnemyCreationRequested?.Invoke(null, new EnemyCreationRequest()
                        {
                            ID = enemy.ID,
                            Race = (int)enemy.Race,
                            WaveNumber = WaveNumber,
                            Position = position,
                            AbilityIDs = enemy.Abilities.GetIDs(),
                            TraitIDs = enemy.Traits.GetIDs(),
                            Waypoints = new ListCoordinates3D(waypoints)
                        });
                    }

                    void GetSpawnAndWayPoints()
                    {
                        if (Waves.Peek().EnemyTypes[spawned].Type == EnemyType.Flying)
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

                if (GameManager.Instance.GameState == GameState.SingleplayerInGame)
                    if (WaveNumber <= Owner.WaveAmount)
                        SetNextWave();
            }

            #endregion
        }
    }
}
