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

    public interface IVulnerable : IPrefabComponent
    {
        [SerializeField] bool IsOn { get; set; }
    }

    public interface ICanApplyEffects : IPrefabComponent, IVulnerable
    {
        AppliedEffectSystem AppliedEffectSystem { get; }
        void AddEffect(Effect Effect);
        void RemoveEffect(Effect Effect);
        int CountOf(Effect Effect);
        event EventHandler<Effect> EffectApplied;
        event EventHandler<Effect> EffectRemoved;
    }

    public interface IHealthComponent : IPrefabComponent, IVulnerable
    {
        void ChangeHealth(IDamageDealer changer, double damage);
        void OnZeroHealth(object _, IHealthComponent entity);
        HealthSystem HealthSystem { get; }
        event EventHandler<IHealthComponent> Died;
    }

    public interface IAttributeComponent
    {
        [SerializeField] List<NumeralAttribute> BaseAttributes { get; }
    }

    public interface IApplyableAttributeComponent : IAttributeComponent
    {
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
        [SerializeField] IDamageDealer OwnerDamageDealer { get; set; }
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