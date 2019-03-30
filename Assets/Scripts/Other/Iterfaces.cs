using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Data;
using Game.Systems;
using Game.Spirit;
using Game.Spirit.Data;
using Game.Spirit.System;
using UnityEngine;
using System;

namespace Game
{
    public interface IPrefabComponent : IIDComponent
    {
        [SerializeField] GameObject Prefab { get; }
    }

    public interface IIDComponent
    {
        [SerializeField] ID ID { get; }
    }

    public interface IEntitySystem : IIDComponent
    {
        [SerializeField] IEntitySystem Owner { get; }
    }

    public interface IAbilitiySystem : IEntitySystem, ICombatComponent
    {
        [SerializeField] AbilityControlSystem AbilityControlSystem { get; }
        [SerializeField] List<AbilitySystem> AbilitySystems { get; }
    }

    public interface ITraitSystem : IEntitySystem, ICombatComponent
    {
        [SerializeField] TraitControlSystem TraitControlSystem { get; }
        [SerializeField] List<ITraitHandler> TraitSystems { get; }
    }

    public interface ICombatComponent : IPrefabComponent
    {
        [SerializeField] List<IHealthComponent> Targets { get; }
    }

    public interface IHealthComponent : IPrefabComponent
    {
        [SerializeField] bool IsOn { get; set; }
        [SerializeField] HealthSystem HealthSystem { get; }
    }


    public interface IAttributeComponent
    {
        [SerializeField] List<NumeralAttribute> BaseAttributes { get; }
        [SerializeField] List<NumeralAttribute> AppliedAttributes { get; }
    }

    public interface IAbilityComponent
    {
        [SerializeField] List<Ability> Abilities { get; }
    }

    public interface ITraitComponent
    {
        [SerializeField] List<Trait> Traits { get; }
    }

    public interface IDamageDealerChild : IDamageDealer
    {
        [SerializeField] IDamageDealer OwnerDamageDealer { get; set;}
    }

    public interface IDamageDealer : IEntitySystem
    { }


    public interface ITraitHandler
    {
        [SerializeField] ITraitSystem Owner { get; }
        void IncreaseStatsPerLevel();
        void Apply(IPrefabComponent entity);
        void Set();
    }


    public interface ITrait
    {
        ITraitHandler GetTraitSystem(ITraitSystem owner);
    }

    public interface IHaveDescription
    {
        void GetDescription();
    }
}