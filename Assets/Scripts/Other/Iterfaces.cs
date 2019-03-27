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
        [SerializeField] GameObject Prefab { get; set; }
    }

    public interface IIDComponent
    {
        [SerializeField] ID ID { get; }
    }

    public interface IEntitySystem : IIDComponent
    {      
        [SerializeField] IEntitySystem OwnerSystem { get; set; }
    }

    public interface IAbilitiySystem : IEntitySystem, ICombatComponent
    {
        [SerializeField] AbilityControlSystem AbilityControlSystem { get; set; }
        [SerializeField] List<AbilitySystem> AbilitySystems { get; set; }
    }

    public interface ITraitSystem : IEntitySystem, ICombatComponent
    {
        [SerializeField] TraitControlSystem TraitControlSystem { get; set; }
        [SerializeField] List<ITraitHandler> TraitSystems { get; set; }
    }

    public interface ICombatComponent : IPrefabComponent
    {
        [SerializeField] List<IHealthComponent> Targets { get; set; }
    }

    public interface IHealthComponent : IPrefabComponent
    {
        [SerializeField] bool IsOn { get; set; }
        [SerializeField] HealthSystem HealthSystem { get; set; }
    }


    public interface IAttributeComponent
    {
        [SerializeField] List<NumeralAttribute> BaseAttributes { get; set; }
        [SerializeField] List<NumeralAttribute> AppliedAttributes { get; set; }
    }

    public interface IAbilityComponent
    {
        [SerializeField] List<Ability> Abilities { get; set; }
    }

    public interface ITraitComponent
    {
        [SerializeField] List<Trait> Traits { get; set; }
    }

    public interface IDamageDealerChild : IDamageDealer
    {
        [SerializeField] IDamageDealer Owner { get; set; }
    }

    public interface IDamageDealer : IEntitySystem
    { }


    public interface ITraitHandler
    {
        [SerializeField] ITraitSystem Owner { get; set; }
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