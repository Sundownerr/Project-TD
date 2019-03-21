﻿using System;
using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Data;
using Game.Spirit;
using UnityEngine;
namespace Game.Systems
{
    public class AbilitySystem : IEntitySystem
    {
        public IHealthComponent Target { get; set; }
        public bool IsStacked { get; set; }
        public bool IsNeedStack { get; set; }
        public List<EffectSystem> EffectSystems { get; set; }
        public Ability Ability { get; set; }
        public List<int> InstanceId { get; set; }
        public IEntitySystem OwnerSystem { get; set; }

        private int effectCount;
        private float cooldownTimer, nextEffectTimer;

        public AbilitySystem(Ability ability, IAbilitiySystem owner)
        {
            Ability = ability;

            EffectSystems = new List<EffectSystem>();

            for (int i = 0; i < Ability.Effects.Count; i++)
                EffectSystems.Add(Ability.Effects[i].EffectSystem);
        }

        public void Init()
        {
            if (!IsStacked)
                if (cooldownTimer < Ability.Cooldown)
                    cooldownTimer += Time.deltaTime;
                else
                {
                    cooldownTimer = 0;
                    IsNeedStack = CheckNeedStack();
                    CooldownReset();
                }

            nextEffectTimer = nextEffectTimer > Ability.Effects[effectCount].NextInterval ? 0 : nextEffectTimer + Time.deltaTime;

            for (int i = 0; i <= effectCount; i++)
                EffectSystems[i].Init();

            if (!(effectCount >= Ability.Effects.Count - 1))
                if (nextEffectTimer > Ability.Effects[effectCount].NextInterval)
                    effectCount++;

            #region  Helper functions

            bool CheckNeedStack()
            {
                for (int i = 0; i < EffectSystems.Count; i++)
                    if (Ability.Effects[i].IsStackable)
                    {
                        if (!EffectSystems[i].IsEnded)
                            return true;
                    }
                    else
                    if (EffectSystems[i].Target != Target)
                        return true;
                return false;
            }

            #endregion
        }

        public void SetTarget(IHealthComponent target)
        {
            Target = target;

            for (int i = 0; i < EffectSystems.Count; i++)
                EffectSystems[i].SetTarget(target);
        }

        public void StackReset(IAbilitiySystem owner)
        {
            IsStacked = true;
            this.Set(owner);
        }

        public void CooldownReset()
        {
            effectCount = 0;
            nextEffectTimer = 0;

            for (int i = 0; i < EffectSystems.Count; i++)
                EffectSystems[i].ApplyRestart();
        }

        public bool CheckAllEffectsEnded()
        {
            for (int i = 0; i < EffectSystems.Count; i++)
                if (!EffectSystems[i].IsEnded)
                    return false;
            return true;
        }
    }
}
