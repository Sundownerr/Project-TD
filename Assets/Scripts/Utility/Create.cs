using Game.Systems;
using UnityEngine;
using U = UnityEngine.Object;
using Game.Enums;
using Game.Managers;
using Game.Systems.Spirit;
using Game.Systems.Enemy;
using Game.Data.EnemyEntity;
using Game.Data.SpiritEntity;
using Game.Systems.Cells;
using Game.Data.NetworkRequests;
using System;

namespace Game.Utility.Creator
{
    public static class Create
    {
        public static RangeSystem Range(IPrefabComponent owner, double size, CollideWith collideType, Action<IVulnerable> onEntityEnteredRange, Action<IVulnerable> onEntityExitRange)
        {
            var range = UnityEngine.Object.Instantiate(ReferenceHolder.Instance.RangePrefab, owner.Prefab.transform);
            range.transform.localScale = new Vector3((float)size, 0.001f, (float)size);

            var rangeSystem = range.GetComponent<RangeSystem>();
            rangeSystem.Owner = owner;
            rangeSystem.CollideType = collideType;

            rangeSystem.EntityEntered += onEntityEnteredRange;
            rangeSystem.EntityExit += onEntityExitRange;

            return rangeSystem;
        }

        public static void PlaceEffect(ElementType element, Vector3 position)
        {
            var placeEffect = U.Instantiate(
                 ReferenceHolder.Instance.ElementPlaceEffects[(int)element],
                position + Vector3.up * 5,
                Quaternion.identity);

            U.Destroy(placeEffect, placeEffect.GetComponent<ParticleSystem>().main.duration);
        }

        public static SpiritSystem Spirit(SpiritData data, Cell cell, bool isOwnedByPlayer = true)
        {
            var newSpiritPrefab = U.Instantiate(
                data.Prefab,
                cell.transform.position,
                Quaternion.identity,
                 ReferenceHolder.Instance.SpiritParent);

            var newSpirit = new SpiritSystem(newSpiritPrefab, isOwnedByPlayer)
            {
                Data = data,
                UsedCell = cell.gameObject
            };

            newSpirit.SetSystem(GameData.Instance.Player);
            PlaceEffect(newSpirit.Data.Base.Element, newSpiritPrefab.transform.position);
            return newSpirit;
        }

        public static SpiritSystem Spirit(SpiritData data, Vector3 position, bool isOwnedByPlayer = true)
        {
            var newSpiritPrefab = U.Instantiate(data.Prefab, position, Quaternion.identity, ReferenceHolder.Instance.SpiritParent);
            var newSpirit = new SpiritSystem(newSpiritPrefab, isOwnedByPlayer)
            {
                Data = data,
                UsedCell = null
            };

            newSpirit.SetSystem(GameData.Instance.Player);
            PlaceEffect(newSpirit.Data.Base.Element, newSpiritPrefab.transform.position);
            return newSpirit;
        }

        public static void Spirit(SpiritCreationRequest request, bool isLocalPlayer)
        {
            var pos = request.Position.ToVector3();
            var choosedCell = GameData.Instance.Player.CellControlSystem.Cells[request.CellIndex];
            var spirit = ReferenceHolder.Instance.SpiritDB.Data[request.Index];

            var newSpirit = isLocalPlayer ?
                Create.Spirit(spirit, choosedCell) :
                Create.Spirit(spirit, pos, false);

            GameData.Instance.Player.SpiritPlaceSystem.NetworkCreateSpirit(newSpirit);
        }

        public static EnemySystem Enemy(Data.EnemyEntity.Enemy data, Vector3 position, Vector3[] waypoints, bool isOwnedByPlayer = true, GameObject prefab = null)
        {
            var enemy = prefab ?? U.Instantiate(data.Prefab, position, Quaternion.identity, ReferenceHolder.Instance.EnemyParent);
            var enemySystem = new EnemySystem(enemy, waypoints, isOwnedByPlayer) { Data = data };

            enemySystem.SetSystem(GameData.Instance.Player);

            return enemySystem;
        }

        public static void Enemy(EnemyCreationRequest request, bool isLocalPlayer)
        {
            var enemyFromDB = GameData.Instance.Player.WaveSystem.ListWaves[request.WaveNumber + 1].EnemyTypes[request.PositionInWave];
            var spawnPos = request.Position.ToVector3();
            var waypoints = request.Waypoints.ToVector3Array();

            if (enemyFromDB == null)
            {
                Debug.LogError("enemyfromdb is null");
            }
            else
            {
                var newEnemy = Create.Enemy(enemyFromDB, spawnPos, waypoints, isLocalPlayer);

                SetAbilities();
                SetTraits();

                GameData.Instance.Player.WaveSystem.NetworkSpawnEnemy(newEnemy, isLocalPlayer);

                void SetAbilities()
                {
                    request.AbilityIndexes?.ForEach(index =>
                    {
                        var abilityFromDB = ReferenceHolder.Instance.AbilityDB.Data.Find(abilityInDataBase => abilityInDataBase.Index == index);

                        if (abilityFromDB == null)
                        {
                            Debug.LogError($"can't find ability with index {index}");
                        }
                        else
                        {
                            newEnemy.Data.Abilities.Add(abilityFromDB);
                        }
                    });
                }

                void SetTraits()
                {
                    request.TraitIndexes?.ForEach(index =>
                    {
                        var traitFromDB = ReferenceHolder.Instance.TraitDB.Data.Find(traitInDataBase => traitInDataBase.Index == index);

                        if (traitFromDB == null)
                        {
                            Debug.LogError($"can't find trait with index {index}");
                        }
                        else
                        {
                            newEnemy.Data.Traits.Add(traitFromDB);
                        }
                    });
                }
            }
        }
    }
}
