using System;
using System.Collections;
using System.Collections.Generic;
using Game.Enemy;
using Game.Spirit;
using Game.Spirit.Data;
using Game.Systems;
using UnityEngine;
using Game.Enums;

namespace Game
{
    public class MageHeroSystem : IEntitySystem
    {
        public IEntitySystem Owner { get; private set; }
        public ID ID { get; private set; }
        public MageHero Mage { get; private set; }

        private enum From
        {
            StartingAttribute,
            PerLevelAttribute
        }

        public MageHeroSystem(PlayerSystem owner, MageHero data)
        {
            Owner = owner;
            Mage = data;
        }

        public void SetSystem()
        {
            var ownerPlayer = Owner as PlayerSystem;
            
            ownerPlayer.WaveSystem.EnemyCreated += OnEnemyCreated;
            ownerPlayer.SpiritPlaceSystem.SpiritPlaced += OnSpiritPlaced;
        }

        private void OnSpiritPlaced(object _, SpiritSystem e) { ModifyAttributes(e.Data, From.StartingAttribute); e.LeveledUp += OnSpiritLeveledUp; }
        private void OnSpiritLeveledUp(object _, SpiritSystem e) => ModifyAttributes(e.Data, From.PerLevelAttribute);
        private void OnEnemyCreated(object _, EnemySystem e) => ModifyAttributes(e.Data, From.StartingAttribute);

        private void ModifyAttributes(EnemyData enemy, From getFrom)
        {
            ModifyNumeralAttributes(enemy, getFrom);

            Mage.EnemyAttributes.ForEach(mageAttribute =>
                enemy.EnemyAttributes.Find(attribute => mageAttribute.Type == attribute.Type).AppliedValue += mageAttribute.Value);
        }

        private void ModifyAttributes(SpiritData spirit, From getFrom)
        {
            ModifyNumeralAttributes(spirit, getFrom);

            Mage.SpiritAttributes.ForEach(mageAttribute =>
                ModifyEntityAttribute(
                    spirit.SpiritAttributes.Find(attribute => attribute.Type == mageAttribute.Type),
                    mageAttribute,
                    getFrom));

            Mage.SpiritFlagAttributes.ForEach(mageAttribute =>
                spirit.FlagAttributes.Find(attribute => mageAttribute.Type == attribute.Type).Value = mageAttribute.Value);
        }

        private void ModifyNumeralAttributes<EntityData>(EntityData data, From getFrom)
        {
            var entityAttribute = new NumeralAttribute();

            Mage.NumeralAttributes.ForEach(mageAttribute =>
            {
                if (data is EnemyData enemy)
                    entityAttribute = enemy.NumeralAttributes.Find(attribute => attribute.Type == mageAttribute.Type);

                if (data is SpiritData spirit)
                    entityAttribute = spirit.NumeralAttributes.Find(attribute => attribute.Type == mageAttribute.Type);

                ModifyEntityAttribute(entityAttribute, mageAttribute, getFrom);
            });
        }

        private void ModifyEntityAttribute<EnumType>(EntityAttributeApplyableLevelUpable<EnumType, double> entityAttribute, EntityAttributeApplyableLevelUpable<EnumType, double> mageAttribute, From getFrom)
        {
            var value = getFrom == From.StartingAttribute ?
                mageAttribute.Value :
                mageAttribute.ValuePerLevel;

            var applyableValue = mageAttribute.IncreasePerLevel == Increase.ByPercent ?
                Ext.GetPercent(entityAttribute.AppliedValue, value) :
                value;

            entityAttribute.AppliedValue += applyableValue;
        }
    }
}
