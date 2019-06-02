using System.Collections.Generic;
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

    public interface IAbilitiyComponent : IEntitySystem, ICombatComponent
    {
        [SerializeField] Systems.Abilities.ControlSystem AbilityControlSystem { get; }
     
        bool CheckHaveMana(double requiredAmount);
    }

    public interface ITraitComponent : IEntitySystem, ICombatComponent
    {
        [SerializeField] Systems.Traits.ControlSystem TraitControlSystem { get; }
    }

    public interface ICombatComponent : IPrefabComponent
    {
        [SerializeField] List<IHealthComponent> Targets { get; }
    }

    public interface IVulnerable : IPrefabComponent
    {
        [SerializeField] bool IsOn { get; set; }
    }

    public interface IAppliedEffectsComponent : IPrefabComponent, IVulnerable
    {
        AppliedEffectSystem AppliedEffectSystem { get; }
        void AddEffect(Data.Effect Effect);
        void RemoveEffect(Data.Effect Effect);
        int CountOf(Data.Effect Effect);
        event Action<Data.Effect> EffectApplied;
        event Action<Data.Effect> EffectRemoved;
    }

    public interface IHealthComponent : IPrefabComponent, IVulnerable
    {
        void ChangeHealth(IDamageDealer changer, double damage);
        void OnZeroHealth(IHealthComponent entity);
        HealthSystem HealthSystem { get; }
        event Action<IHealthComponent> Died;
    }

    public interface INumeralAttributes
    {
        List<NumeralAttribute> NumeralAttributes { get; }
    }

    public interface ISpiritAttributes : INumeralAttributes
    {
        List<SpiritAttribute> SpiritAttributes { get; }
        List<SpiritFlagAttribute> FlagAttributes { get; }
    }

    public interface IEnemyAttributes : INumeralAttributes
    {
        List<EnemyAttribute> EnemyAttributes { get; }
    }

    public interface IAbilityData : INumeralAttributes
    {
        [SerializeField] List<Ability> Abilities { get; }
    }

    public interface ITraitData
    {
        [SerializeField] List<Trait> Traits { get; }
    }

    public interface IDamageDealerChild : IDamageDealer
    {
        [SerializeField] IDamageDealer OwnerDamageDealer { get; set; }
    }

    public interface IDamageDealer : IEntitySystem
    { }

    public interface ITraitSystem
    {
        [SerializeField] ITraitComponent Owner { get; }
        void LevelUp();
        void Apply(IPrefabComponent entity);
        void Set();
    }

    public interface ITrait
    {
        ITraitSystem GetTraitSystem(ITraitComponent owner);
    }

    public interface IHaveDescription : IPointerEnterHandler, IPointerExitHandler
    {
        string Description { get; }
        string Title { get; }
        Image Image { get; }
        void GetDescription();
    }
}