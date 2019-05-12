﻿using System.Collections.Generic;
using Game.Systems;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Data.Attributes;
using Game.Data.Traits;
using Game.Data.Effects;
using Game.Data.Abilities;
using Game.Systems.Abilities;

namespace Game
{
    public interface IPrefabComponent
    {
        [SerializeField] GameObject Prefab { get; }

    }

    public interface IIndexComponent
    {
        int Index { get; }
    }

    public interface IEntitySystem
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

    public interface ICanReceiveEffects : IPrefabComponent, IVulnerable
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

    public interface ISpiritAttributes
    {
        List<NumeralAttribute> NumeralAttributes { get; }
        List<SpiritAttribute> SpiritAttributes { get; }
        List<SpiritFlagAttribute> FlagAttributes { get; }
    }

    public interface IEnemyAttributes
    {
        List<NumeralAttribute> NumeralAttributes { get; }
        List<EnemyAttribute> EnemyAttributes { get; }
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

    public interface IHaveDescription : IPointerEnterHandler, IPointerExitHandler
    {
        string Description { get; }
        string Title { get; }
        Image Image { get; }
        void GetDescription();
    }
}