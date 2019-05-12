using Game;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Game.Enums;
using System.Text.RegularExpressions;
using Lean.Localization;
using Game.Data.Attributes;
using Game.Data.NetworkRequests;
using Game.Utility;
using Game.Managers;
using Game.Data.Enemy;
using Game.Systems.Cells;

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

    ///<summary>
    /// Return random element from given probabilites
    ///</summary>
    public static int RollDice(this List<double> probabilities)
    {
        var total = 0d;

        probabilities.ForEach(probability => total += probability);

        var randomProbability = (double)StaticRandom.Instance.Next(0, (int)total);

        for (int i = 0; i < probabilities.Count; i++)
            if (randomProbability < probabilities[i])
                return i;
            else
                randomProbability -= probabilities[i];
        return -1;
    }

    public static string[] SplitCamelCase(this string source) => Regex.Split(source, @"(?<!^)(?=[A-Z])");

    public static string GetLocalized<EnumType>(this EnumType type) where EnumType : struct, Enum =>
        LeanLocalization.GetTranslationText(type.GetStringKey());

    public static string GetLocalized(this string key) =>
        LeanLocalization.GetTranslationText(key);

    public static string StringEnumToStringKey(this string source, string keyPrefix)
    {
        var splitted = source.SplitCamelCase();
        var sb = new StringBuilder().Append(keyPrefix);

        for (int i = 0; i < splitted.Length; i++)
            sb.Append("-").Append(splitted[i].ToLower());

        return sb.ToString();
    }

    public static List<NumeralAttribute> CreateAttributeList_N()
    {
        var enums = Enum.GetValues(typeof(Numeral));
        var emptyList = new List<NumeralAttribute>();

        for (int i = 0; i < enums.Length; i++)
            emptyList.Add(new NumeralAttribute() { Type = (Numeral)enums.GetValue(i), Value = 0, ValuePerLevel = 0, AppliedValue = 0 });
        return emptyList;
    }

    public static List<EnemyAttribute> CreateAttributeList_E()
    {
        var enums = Enum.GetValues(typeof(Enemy));
        var emptyList = new List<EnemyAttribute>();

        for (int i = 0; i < enums.Length; i++)
            emptyList.Add(new EnemyAttribute() { Type = (Enemy)enums.GetValue(i), Value = 0, AppliedValue = 0 });
        return emptyList;
    }

    public static List<SpiritAttribute> CreateAttributeList_S()
    {
        var enums = Enum.GetValues(typeof(Spirit));
        var emptyList = new List<SpiritAttribute>();

        for (int i = 0; i < enums.Length; i++)
            emptyList.Add(new SpiritAttribute() { Type = (Spirit)enums.GetValue(i), Value = 0, ValuePerLevel = 0, AppliedValue = 0 });
        return emptyList;
    }

    public static List<SpiritFlagAttribute> CreateAttributeList_SF()
    {
        var enums = Enum.GetValues(typeof(SpiritFlag));
        var emptyList = new List<SpiritFlagAttribute>();

        for (int i = 0; i < enums.Length; i++)
            emptyList.Add(new SpiritFlagAttribute() { Type = (SpiritFlag)enums.GetValue(i) });
        return emptyList;
    }

    public static EntityAttribute_A_L<Numeral, double> Get(this ISpiritAttributes attributeHandler, Numeral type) =>
        attributeHandler.NumeralAttributes.Find(x => x.Type == type);

    public static EntityAttribute_A_L<Numeral, double> Get(this IEnemyAttributes attributeHandler, Numeral type) =>
       attributeHandler.NumeralAttributes.Find(x => x.Type == type);

    public static EntityAttribute_A_L<Game.Enums.Spirit, double> Get(this ISpiritAttributes attributeHandler, Spirit type) =>
       attributeHandler.SpiritAttributes.Find(x => x.Type == type);

    public static EntityAttribute<Game.Enums.SpiritFlag, bool> Get(this ISpiritAttributes attributeHandler, SpiritFlag type) =>
        attributeHandler.FlagAttributes.Find(x => x.Type == type);

    public static EntityAttribute_A<Game.Enums.Enemy, double> Get(this IEnemyAttributes attributeHandler, Enemy type) =>
        attributeHandler.EnemyAttributes.Find(x => x.Type == type);

    public static List<NumeralAttribute> CopyFrom(this List<NumeralAttribute> that, List<NumeralAttribute> other)
    {
        that = new List<NumeralAttribute>();
        that.AddRange(other);
        return that;
    }

    ///<summary>
    /// Return list with ids of entities in list.
    ///</summary>  
    public static List<int> GetIDs<T>(this List<T> list) where T : IIndexComponent
    {
        var ids = new List<int>();

        list.ForEach(entity => ids.Add(entity.Index));

        return ids;
    }

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


