using Game;
using Game.Cells;
using Game.Enemy;
using Game.Data;
using Game.Systems;
using Game.Spirit;
using Game.Spirit.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Game.Enums;
using RotaryHeart.Lib.SerializableDictionary;
using Game.Wrappers;

public static class Ext
{
    ///<summary>
    /// Return distance between vectors
    ///</summary>
    public static float GetDistanceTo(this Vector3 pos1, Vector3 pos2)
    {
        Vector3 heading;
        float distanceSquared;
        float distance;

        heading.x = pos1.x - pos2.x;
        heading.y = pos1.y - pos2.y;
        heading.z = pos1.z - pos2.z;

        distanceSquared = heading.x * heading.x + heading.y * heading.y + heading.z * heading.z;
        distance = Mathf.Sqrt(distanceSquared);

        return distance;
    }

    ///<summary>
    /// Return percernt of value
    ///</summary>
    public static double GetPercent(this double value, double desiredPercent) => value / 100 * desiredPercent;

    ///<summary>
    /// Return random element from given probabilites
    ///</summary>
    public static int RollDice(this double[] probabilities)
    {
        var total = 0d;

        for (int i = 0; i < probabilities.Length; i++)
            total += probabilities[i];

        var randomProbability = (double)StaticRandom.Instance.Next(0, (int)total);

        for (int i = 0; i < probabilities.Length; i++)
            if (randomProbability < probabilities[i])
                return i;
            else
                randomProbability -= probabilities[i];
        return -1;
    }

    public static List<NumeralAttribute> CreateAttributeList(this List<NumeralAttribute> emptyList)
    {
        var enums = Enum.GetValues(typeof(Numeral));
        emptyList = new List<NumeralAttribute>();

        for (int i = 0; i < enums.Length; i++)
            emptyList.Add(new NumeralAttribute() { Type = (Numeral)enums.GetValue(i) });
        return emptyList;
    }

    public static List<EnemyAttribute> CreateAttributeList(this List<EnemyAttribute> emptyList)
    {
        var enums = Enum.GetValues(typeof(Enemy));
        emptyList = new List<EnemyAttribute>();

        for (int i = 0; i < enums.Length; i++)
            emptyList.Add(new EnemyAttribute() { Type = (Enemy)enums.GetValue(i) });
        return emptyList;
    }

    public static List<SpiritAttribute> CreateAttributeList(this List<SpiritAttribute> emptyList)
    {
        var enums = Enum.GetValues(typeof(Spirit));
        emptyList = new List<SpiritAttribute>();

        for (int i = 0; i < enums.Length; i++)
            emptyList.Add(new SpiritAttribute() { Type = (Spirit)enums.GetValue(i) });
        return emptyList;
    }

    public static List<SpiritFlagAttribute> CreateAttributeList(this List<SpiritFlagAttribute> emptyList)
    {
        var enums = Enum.GetValues(typeof(SpiritFlag));
        emptyList = new List<SpiritFlagAttribute>();

        for (int i = 0; i < enums.Length; i++)
            emptyList.Add(new SpiritFlagAttribute() { Type = (SpiritFlag)enums.GetValue(i) });
        return emptyList;
    }

    public static NumeralAttribute Get(this ISpiritAttributes attributeHandler, Numeral type) =>
        attributeHandler.NumeralAttributes.Find(x => x.Type == type);

    public static NumeralAttribute Get(this IEnemyAttributes attributeHandler, Numeral type) =>
       attributeHandler.NumeralAttributes.Find(x => x.Type == type);

    public static SpiritAttribute Get(this ISpiritAttributes attributeHandler, Spirit type) =>
       attributeHandler.SpiritAttributes.Find(x => x.Type == type);

    public static SpiritFlagAttribute Get(this ISpiritAttributes attributeHandler, SpiritFlag type) =>
        attributeHandler.FlagAttributes.Find(x => x.Type == type);

    public static EnemyAttribute Get(this IEnemyAttributes attributeHandler, Enemy type) =>
        attributeHandler.EnemyAttributes.Find(x => x.Type == type);

    public static List<NumeralAttribute> CopyFrom(this List<NumeralAttribute> that, List<NumeralAttribute> other)
    {
        that = new List<NumeralAttribute>();
        that.AddRange(other);
        return that;
    }

    public static ListID GetIDs<T>(this List<T> list) where T : IIDComponent
    {
        var ids = new ListID();

        for (int i = 0; i < list.Count; i++)
            ids.Add(list[i].ID);

        return ids;
    }



    ///<summary>
    /// Set effect system owner, id
    ///</summary>

    public static bool IsBossOrCommander(this EnemyData enemy) => enemy.Type == EnemyType.Boss || enemy.Type == EnemyType.Commander;

    ///<summary>
    /// Return prefab of IEntitySystem with IPrefabComponent.
    /// If null then search in owner IEntitySystem
    ///</summary>   
    public static GameObject GetPrefab(this IEntitySystem entitySystem)
    {
        return entitySystem == null ? null :
                entitySystem is IPrefabComponent prefabComponent ? prefabComponent.Prefab :
                entitySystem.Owner == null ? null :
                GetPrefab(entitySystem.Owner);
    }

    ///<summary>
    /// Return owner IEntitySystem of type T
    ///</summary>   
    public static T GetOwnerOfType<T>(this IEntitySystem entitySystem) where T : IEntitySystem
    {
        if (entitySystem.Owner is T system)
            return system;

        return entitySystem.Owner == null ? default(T) : entitySystem.Owner.GetOwnerOfType<T>();
    }

    ///<summary>
    /// Return id as string
    ///</summary>   
    public static string GetIdString(this List<int> Id)
    {
        var stringBuilder = new StringBuilder();

        for (int i = 0; i < Id.Count; i++)
            stringBuilder.Append(Id[i].ToString());

        return stringBuilder.ToString();
    }

    public static Vector3 ToVector3(this Coordinates3D position) => new Vector3(position.X, position.Y, position.Z);
    public static Coordinates3D ToCoordinates3D(this Vector3 position) => new Coordinates3D(position.x, position.y, position.z);

    public static void Expand(this Cell ownerCell)
    {
        var forward = new Vector3(0, 0, 1);
        var back = new Vector3(0, 0, -1);
        var left = new Vector3(1, 0, 0);
        var right = new Vector3(-1, 0, 0);
        var down = new Vector3(0, -1, 0);
        var buildingAreaLayer = 1 << 8;
        var terrainLayer = 1 << 9;
        var buildLayerMask = ~terrainLayer | buildingAreaLayer;

        var spacing = ownerCell.gameObject.transform.localScale.x;
        var rayDistance = ownerCell.gameObject.transform.localScale.x;

        var results = new RaycastHit[1];

        var forwardRay = new Ray(ownerCell.gameObject.transform.position + forward * rayDistance, down);
        var backRay = new Ray(ownerCell.gameObject.transform.position + back * rayDistance, down);
        var rightRay = new Ray(ownerCell.gameObject.transform.position + right * rayDistance, down);
        var leftRay = new Ray(ownerCell.gameObject.transform.position + left * rayDistance, down);
        var downRay = new Ray(ownerCell.gameObject.transform.position, down);

        var isForwardHit = Physics.RaycastNonAlloc(forwardRay, results, 5, buildLayerMask) > 0;
        var isBackHit = Physics.RaycastNonAlloc(backRay, results, 5, buildLayerMask) > 0;
        var isRightHit = Physics.RaycastNonAlloc(rightRay, results, 5, buildLayerMask) > 0;
        var isLeftHit = Physics.RaycastNonAlloc(leftRay, results, 5, buildLayerMask) > 0;
        var isDownHit = Physics.RaycastNonAlloc(downRay, results, 15, buildLayerMask) > 0;

        var isNothingHit = !isForwardHit && !isBackHit && !isLeftHit && !isRightHit;

        if (isNothingHit || !isDownHit)
            return;

        if (isForwardHit)
            Fill(forward);

        if (isBackHit)
            Fill(back);

        if (isLeftHit)
            Fill(left);

        if (isRightHit)
            Fill(right);

        #region  Helper functions

        void Fill(Vector3 spawnDirection)
        {
            if (!Physics.Raycast(ownerCell.gameObject.transform.position, spawnDirection, rayDistance, buildLayerMask))
            {
                var prefab = UnityEngine.Object.Instantiate(
                    ReferenceHolder.Get.CellPrefab,
                    ownerCell.gameObject.transform.position + spawnDirection * spacing,
                    Quaternion.identity,
                    ReferenceHolder.Get.CellParent);

                var cell = prefab.GetComponent<Cell>();

                cell.Owner = ownerCell.Owner;
                ownerCell.Owner.CellControlSystem.Cells.Add(cell);
                ownerCell.IsExpanded = true;
            }
        }

        #endregion
    }


}


