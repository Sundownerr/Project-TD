using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Data;
using Game.Enums;
using Game.Data.NetworkRequests;
using Game.Managers;
using Game.Utility;
using Game.Systems.Enemy;
using Game.Utility.Creator;

namespace Game.Systems.Waves
{
    public class WaveSystem
    {
        public event Action WaveEnded, WaveStarted, AllWaveEnemiesKilled;
        public event Action<EnemySystem> EnemyCreated;
        public event Action<EnemyCreationRequest> EnemyCreationRequested;
        public int WaveNumber { get; set; }
        public PlayerSystem Owner { get; private set; }
        public Queue<Wave> Waves { get; private set; } = new Queue<Wave>();
        public List<Wave> ListWaves { get; private set; }
        public List<Vector3> GroundWaypoints { get; private set; }
        public List<Vector3> FlyingWaypoints { get; private set; }

        List<List<EnemySystem>> wavesEnemySystem = new List<List<EnemySystem>>();
        List<int> currentEnemyCount = new List<int>();
        WaitForSeconds spawnDelay = new WaitForSeconds(0.2f);
        int spawned;
        Coroutine spawnCoroutine;

        public WaveSystem(PlayerSystem player) => Owner = player;

        public void NetworkSpawnEnemy(EnemySystem e, bool isEnenmyOwned)
        {
            if (isEnenmyOwned)
            {
                wavesEnemySystem[wavesEnemySystem.Count - 1].Add(e);
            }

            EnemyCreated?.Invoke(e);

            if (spawned == Waves.Peek().EnemyTypes.Count)
            {
                if (WaveNumber <= Owner.WaveAmount)
                {
                    if (spawnCoroutine != null)
                    {
                        GameLoop.Instance.StopCoroutine(spawnCoroutine);
                    }

                    SetNextWave();
                }
            }
        }

        public void SetSystem()
        {
            GroundWaypoints = GetWaypoints(Owner.Map.GroundWaypoints);
            FlyingWaypoints = GetWaypoints(Owner.Map.FlyingWaypoints);

            Owner.WaveUISystem.WaveStarted += OnWaveStarted;

            if (GameManager.Instance.GameState != GameState.InGameMultiplayer)
            {
                Waves = Systems.Waves.CreatingSystem.GenerateWaves(Owner.WaveAmount);
            }
            else
            {
                Waves = Systems.Waves.CreatingSystem.GenerateWaves(Owner.NetworkPlayer.NetworkWaveDatas);
            }

            ListWaves = new List<Wave>(Waves);
            Waves.Dequeue();

            List<Vector3> GetWaypoints(List<GameObject> waypointObjects)
            {
                var waypoints = new List<Vector3>(waypointObjects.Count);
                waypointObjects.ForEach(waypoint => waypoints.Add(waypoint.transform.position));

                return waypoints;
            }
        }

        public void UpdateSystem()
        {
            AddMagicCrystalAfterWaveEnd();

            void AddMagicCrystalAfterWaveEnd()
            {
                for (int waveId = 0; waveId < wavesEnemySystem.Count; waveId++)
                {
                    var killedEnemyCount = 0;

                    for (int enemyId = 0; enemyId < wavesEnemySystem[waveId].Count; enemyId++)
                    {
                        if (wavesEnemySystem[waveId][enemyId].Prefab == null)
                        {
                            killedEnemyCount++;
                            if (killedEnemyCount == currentEnemyCount[waveId])
                            {
                                AllWaveEnemiesKilled?.Invoke();
                                wavesEnemySystem.RemoveAt(waveId);
                                currentEnemyCount.RemoveAt(waveId);
                                break;
                            }
                        }
                    }
                }
            }
        }

        void SetNextWave()
        {
            Waves.Dequeue();
            WaveNumber++;

            WaveEnded?.Invoke();
        }

        public void OnWaveStarted()
        {
            currentEnemyCount.Add(Waves.Peek().EnemyTypes.Count);
            wavesEnemySystem.Add(new List<EnemySystem>());

            spawnCoroutine = GameLoop.Instance.StartCoroutine(SpawnEnemyWave());

            IEnumerator SpawnEnemyWave()
            {
                WaveStarted?.Invoke();

                spawned = 0;

                while (spawned < Waves.Peek().EnemyTypes.Count)
                {
                    var isEnemyFlying = Waves.Peek().EnemyTypes[spawned].Type == EnemyType.Flying;
                    var enemyPositions = GetEnemyPositions(isEnemyFlying);
                    var spawnPosition = enemyPositions.spawnPosition;
                    var waypoints = enemyPositions.waypoints;

                    if (GameManager.Instance.GameState == GameState.InGameMultiplayer)
                    {
                        SendEnemyRequest();
                    }
                    else
                    {
                        CreateEnemy();
                    }

                    spawned++;
                    
                    yield return spawnDelay;

                    void CreateEnemy()
                    {
                        var newEnemy = Create.Enemy(Waves.Peek().EnemyTypes[spawned], spawnPosition, waypoints);

                        wavesEnemySystem[wavesEnemySystem.Count - 1].Add(newEnemy);
                        EnemyCreated?.Invoke(newEnemy);
                    }

                    void SendEnemyRequest()
                    {
                        var enemy = Waves.Peek().EnemyTypes[spawned];
                        var position = spawnPosition.ToCoordinates3D();

                        EnemyCreationRequested?.Invoke(new EnemyCreationRequest()
                        {
                            Index = enemy.Index,
                            Race = (int)enemy.Race,
                            WaveNumber = WaveNumber,
                            Position = position,
                            PositionInWave = spawned,
                            AbilityIndexes = enemy.Abilities?.GetIndexes(),
                            TraitIndexes = enemy.Traits?.GetIndexes(),
                            Waypoints = new ListCoordinates3D(waypoints)
                        });
                    }

                    (Vector3 spawnPosition, List<Vector3> waypoints) GetEnemyPositions(bool isEnenmyFlying) =>
                         isEnenmyFlying ?
                            (Owner.Map.FlyingSpawnPoint.transform.position, FlyingWaypoints) :
                            (Owner.Map.GroundSpawnPoint.transform.position, GroundWaypoints);
                }

                if (GameManager.Instance.GameState == GameState.InGameSingleplayer)
                {
                    if (WaveNumber <= Owner.WaveAmount)
                    {
                        SetNextWave();
                    }
                }
            }
        }
    }
}
