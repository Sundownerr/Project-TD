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

public static class ExtensionMethods
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
    /// Return sum of base and applied attribute of type 
    ///</summary>
    public static double GetValue(this IAttributeComponent data, Numeral type) =>
        data.Get(type, From.Base).Value + data.Get(type, From.Applied).Value;

    ///<summary>
    /// Return random element from given probabilites
    ///</summary>
    public static int RollDice(this List<double> probabilities)
    {
        var total = 0d;

        for (int i = 0; i < probabilities.Count; i++)
            total += probabilities[i];

        var randomProbability = (double)StaticRandom.Instance.Next(0, (int)total);

        for (int i = 0; i < probabilities.Count; i++)
            if (randomProbability < probabilities[i])
                return i;
            else
                randomProbability -= probabilities[i];
        return -1;
    }

    ///<summary>
    /// Fill list with attributes
    ///</summary>   
    public static List<NumeralAttribute> CreateAttributeList()
    {
        var attributeList = new List<NumeralAttribute>();
        var numerals = Enum.GetValues(typeof(Numeral));

        for (int i = 0; i < numerals.Length; i++)
            attributeList.Add(new NumeralAttribute((Numeral)numerals.GetValue(i), 0));

        return attributeList;
    }

    ///<summary>
    /// Return attribute from base or applied attribute list
    ///</summary>   
    public static NumeralAttribute Get(this IAttributeComponent attributeHandler, Numeral type, From baseOrApplied)
    {
        var attributes = baseOrApplied == From.Base ? attributeHandler.BaseAttributes : attributeHandler.AppliedAttributes;

        for (int i = 0; i < attributes.Count; i++)
            if (attributes[i].Type == type)
                return attributes[i];
        return null;
    }

    public static List<NumeralAttribute> CopyFrom(this List<NumeralAttribute> that, List<NumeralAttribute> other)
    {
        that = new List<NumeralAttribute>();
        that.AddRange(other);
        return that;
    }

    public static NumeralAttributeList Wrap(this List<NumeralAttribute> list)
    {
        var wrappedList = new NumeralAttributeList();
        wrappedList.AddRange(list);
        return wrappedList;
    }

    public static List<NumeralAttribute> Unwrap(this NumeralAttributeList wrappedList) => new List<NumeralAttribute>(wrappedList);

    public static List<NumeralAttribute> SetFrom(this List<NumeralAttribute> that, List<NumeralAttribute> other)
    {
        for (int i = 0; i < other.Count; i++)
        {
            var attribute = that.Find(x => x.Type == other[i].Type);
            if (attribute != null)
                attribute.Value = other[i].Value;
        }
        return that;
    }

    public static ListID GetIDs<T>(this List<T> entityList) where T : Entity
    {
        var ids = new ListID();

        for (int i = 0; i < entityList.Count; i++)
            ids.Add(entityList[i].ID);

        return ids;
    }

    ///<summary>
    /// Set effect system owner, id
    ///</summary>
    public static void Set(this EffectSystem effectSystem, AbilitySystem ownerAbility)
    {
        effectSystem.OwnerSystem = ownerAbility;
        effectSystem.ID = new ID();
        effectSystem.ID.AddRange(ownerAbility.ID);
        effectSystem.ID.Add(ownerAbility.EffectSystems.IndexOf(effectSystem));
    }

    ///<summary>
    /// Set spirit system owner, id
    ///</summary>
    public static void SetId(this SpiritSystem spiritSystem)
    {
        var player = spiritSystem.GetOwnerOfType<PlayerSystem>();

        spiritSystem.OwnerSystem = player;
        spiritSystem.ID = new ID() { player.SpiritControlSystem.Spirits.Count };
    }

    ///<summary>
    /// Set enemy system owner, id
    ///</summary>   
    public static void SetId(this EnemySystem enemySystem)
    {
        var player = enemySystem.GetOwnerOfType<PlayerSystem>();

        enemySystem.OwnerSystem = player;
        enemySystem.ID = new ID() { player.EnemyControlSystem.Enemies.Count };
    }

    ///<summary>
    /// Set ability system owner, id and effect systems
    ///</summary>   
    public static void Set(this AbilitySystem abilitySystem, IAbilitiySystem owner)
    {
        abilitySystem.OwnerSystem = owner;
        abilitySystem.ID = new ID();
        abilitySystem.ID.AddRange(owner.ID);
        abilitySystem.ID.Add(owner.AbilitySystems.IndexOf(abilitySystem));

        for (int i = 0; i < abilitySystem.EffectSystems.Count; i++)
        {
            abilitySystem.EffectSystems[i].Set(abilitySystem);

            if (abilitySystem.EffectSystems[i] is IDamageDealerChild child)
                child.Owner = abilitySystem.GetOwnerOfType<IDamageDealer>();
        }

        abilitySystem.Ability.Effects[abilitySystem.Ability.Effects.Count - 1].NextInterval = 0.01f;
    }

    ///<summary>
    /// Return count of effect in IHealthComponent
    ///</summary>   
    public static int CountOf(this IHealthComponent vulnerable, Effect effect)
    {
        var count = 0;
        var appliedEffects = vulnerable.HealthSystem.AppliedEffects;

        for (int i = 0; i < appliedEffects.Count; i++)
            if (effect.ID.Compare(appliedEffects[i].ID))
                count++;
        return count;
    }

    ///<summary>
    /// Add effect to IHealthComponent
    ///</summary>   
    public static void AddEffect(this IHealthComponent vulnerable, Effect effect) =>
        vulnerable.HealthSystem.AppliedEffects.Add(effect);

    ///<summary>
    /// Remove effect from IHealthComponent
    ///</summary>   
    public static void RemoveEffect(this IHealthComponent vulnerable, Effect effect)
    {
        var appliedEffects = vulnerable.HealthSystem.AppliedEffects;

        for (int i = 0; i < appliedEffects.Count; i++)
            if (effect.ID.Compare(appliedEffects[i].ID))
            {
                appliedEffects.RemoveAt(i);
                return;
            }
    }

    ///<summary>
    /// Return prefab of IEntitySystem with IPrefabComponent.
    /// If null then search in owner IEntitySystem
    ///</summary>   
    public static GameObject GetPrefab(this IEntitySystem entitySystem)
    {
        if (entitySystem == null)
            return null;
        if (entitySystem is IPrefabComponent prefabComponent)
            return prefabComponent.Prefab;
        if (entitySystem.OwnerSystem == null)
            return null;
        return GetPrefab(entitySystem.OwnerSystem);
    }

    ///<summary>
    /// Return owner IEntitySystem of type T
    ///</summary>   
    public static T GetOwnerOfType<T>(this IEntitySystem entitySystem) where T : IEntitySystem
    {
        if (entitySystem.OwnerSystem is T system)
            return system;

        return entitySystem.OwnerSystem == null ? default(T) : entitySystem.OwnerSystem.GetOwnerOfType<T>();
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


