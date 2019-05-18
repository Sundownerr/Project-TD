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

    public static List<NumeralAttribute> CreateAttributeList_N()
    {
        var enums = Enum.GetValues(typeof(Numeral));
        var attributeList = new List<NumeralAttribute>();

        for (int i = 0; i < enums.Length; i++)
            attributeList.Add(new NumeralAttribute() { Type = (Numeral)enums.GetValue(i), Value = 0, ValuePerLevel = 0, AppliedValue = 0 });
        return attributeList;
    }

    public static List<EnemyAttribute> CreateAttributeList_E()
    {
        var enums = Enum.GetValues(typeof(Enemy));
        var attributeList = new List<EnemyAttribute>();

        for (int i = 0; i < enums.Length; i++)
            attributeList.Add(new EnemyAttribute() { Type = (Enemy)enums.GetValue(i), Value = 0, AppliedValue = 0 });
        return attributeList;
    }

    public static List<SpiritAttribute> CreateAttributeList_S()
    {
        var enums = Enum.GetValues(typeof(Spirit));
        var attributeList = new List<SpiritAttribute>();

        for (int i = 0; i < enums.Length; i++)
            attributeList.Add(new SpiritAttribute() { Type = (Spirit)enums.GetValue(i), Value = 0, ValuePerLevel = 0, AppliedValue = 0 });
        return attributeList;
    }

    public static List<SpiritFlagAttribute> CreateAttributeList_SF()
    {
        var enums = Enum.GetValues(typeof(SpiritFlag));
        var attributeList = new List<SpiritFlagAttribute>();

        for (int i = 0; i < enums.Length; i++)
            attributeList.Add(new SpiritFlagAttribute() { Type = (SpiritFlag)enums.GetValue(i) });
        return attributeList;
    }

    public static EntityAttribute_A_L<Numeral, double> Get(this ISpiritAttributes entity, Numeral type) =>
        entity.NumeralAttributes.Find(x => x.Type == type);

    public static EntityAttribute_A_L<Numeral, double> Get(this IEnemyAttributes entity, Numeral type) =>
       entity.NumeralAttributes.Find(x => x.Type == type);

    public static EntityAttribute_A_L<Game.Enums.Spirit, double> Get(this ISpiritAttributes entity, Spirit type) =>
       entity.SpiritAttributes.Find(x => x.Type == type);

    public static EntityAttribute<Game.Enums.SpiritFlag, bool> Get(this ISpiritAttributes entity, SpiritFlag type) =>
        entity.FlagAttributes.Find(x => x.Type == type);

    public static EntityAttribute_A<Game.Enums.Enemy, double> Get(this IEnemyAttributes entity, Enemy type) =>
        entity.EnemyAttributes.Find(x => x.Type == type);

    public static List<NumeralAttribute> CopyFrom(this List<NumeralAttribute> that, List<NumeralAttribute> other)
    {
        that = new List<NumeralAttribute>();
        that.AddRange(other);
        return that;
    }

    ///<summary>
    /// Return list with ids of entities in list.
    ///</summary>  
    public static List<int> GetIndexes<T>(this List<T> list) where T : IIndexComponent
    {
        var indexes = new List<int>();

        list.ForEach(entity => indexes.Add(entity.Index));

        return indexes;
    }

    ///<summary>
    /// Return prefab of IEntitySystem with IPrefabComponent.
    /// If null then search in owner IEntitySystem
    ///</summary>   
    public static GameObject GetPrefab(this IEntitySystem entitySystem) =>
        entitySystem is IPrefabComponent entityWithPrefab ? entityWithPrefab.Prefab :
        entitySystem.Owner != null ? GetPrefab(entitySystem.Owner) : null;


    ///<summary>
    /// Return owner IEntitySystem of type T
    ///</summary>   
    public static T GetOwnerOfType<T>(this IEntitySystem entitySystem) where T : IEntitySystem
    {
        if (entitySystem.Owner is T system)
        {
            return system;
        }

        return entitySystem.Owner == null ? default(T) : entitySystem.Owner.GetOwnerOfType<T>();
    }

    public static Vector3 ToVector3(this Coordinates3D position) => new Vector3(position.X, position.Y, position.Z);
    public static Coordinates3D ToCoordinates3D(this Vector3 position) => new Coordinates3D(position.x, position.y, position.z);
}


