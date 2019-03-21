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
    public interface IPrefabComponent
    {
        GameObject Prefab { get; set; }      
    }

    
    public interface IEntitySystem
    {        
        List<int> InstanceId { get; }      
        IEntitySystem OwnerSystem { get; set; }
    }

    public interface IAbilitiySystem : IEntitySystem, ICombatComponent
    {
        AbilityControlSystem AbilityControlSystem { get; set; }
        List<AbilitySystem> AbilitySystems { get; set; }       
    }

    public interface ITraitSystem : IEntitySystem, ICombatComponent
    {
        TraitControlSystem TraitControlSystem { get; set; }
        List<ITraitHandler> TraitSystems { get; set; }
    }

    public interface ICombatComponent : IPrefabComponent
    {
        List<IHealthComponent> Targets { get; set; }
    }

    public interface IHealthComponent : IPrefabComponent
    {
        bool IsOn { get; set; }
        HealthSystem HealthSystem { get; set; }       
    }

    public interface IAttributeComponent 
    {
        List<NumeralAttribute> BaseAttributes { get; set; }
        List<NumeralAttribute> AppliedAttributes { get; set; }            
    }

    public interface IAbilityComponent 
    {
        List<Ability> Abilities { get; set; }
    }

    public interface ITraitComponent 
    {
        List<Trait> Traits { get; set; }
    }

    public interface IDamageDealerChild : IDamageDealer
    {
        IDamageDealer Owner { get; set; }
    }

    public interface IDamageDealer : IEntitySystem
    { }


    public interface ITraitHandler
    {
        ITraitSystem Owner { get; set; }
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