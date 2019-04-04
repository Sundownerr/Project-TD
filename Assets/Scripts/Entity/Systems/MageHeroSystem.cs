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
        public MageHeroSystem(PlayerSystem owner, MageHero data)
        {
            Owner = owner;
            Data = data;
        }

        public void SetSystem()
        {
            (Owner as PlayerSystem).WaveSystem.EnemyCreated += OnEnemyCreated;
            (Owner as PlayerSystem).SpiritPlaceSystem.SpiritPlaced += OnSpiritPlaced;
        }

        private void OnSpiritPlaced(object sender, SpiritSystem e) { Modify(e.Data); e.LeveledUp += OnSpiritLeveledUp; }

        private void OnSpiritLeveledUp(object sender, SpiritSystem e)
        {
            for (int i = 0; i < Data.SpiritAttributes.Count; i++)
            {
                var mageHeroModificator = Data.SpiritAttributes[i];
                var attribute = e.Data.SpiritAttributes.Find(x => x.Type == mageHeroModificator.Type);

                attribute.AppliedValue += mageHeroModificator.ValuePerLevel;
            }

            for (int i = 0; i < Data.SpiritFlagAttributes.Count; i++)
            {
                var mageHeroModificator = Data.SpiritFlagAttributes[i];
                var attribute = e.Data.FlagAttributes.Find(x => x.Type == mageHeroModificator.Type);

                attribute.Value = mageHeroModificator.Value;
            }
        }

        private void ModifyAttribute<EnumType>(EntityAttributeApplyableLevelUpable<EnumType, double> entityAttribute, EntityAttributeApplyableLevelUpable<EnumType, double> mageAttribute)
        {
            if (mageAttribute.IncreacePerLevel == Increase.ByPercent)
                entityAttribute.AppliedValue += Ext.GetPercent(entityAttribute.AppliedValue, mageAttribute.Value);
            else
                entityAttribute.AppliedValue += mageAttribute.Value;
        }

        private void OnEnemyCreated(object sender, EnemySystem e) => Modify(e.Data);

        private void ModifyNumeralAttributes<EntityData>(EntityData data)
        {
            var attribute = new NumeralAttribute();
            for (int i = 0; i < Data.NumeralAttributes.Count; i++)
            {
                var mageHeroModificator = Data.NumeralAttributes[i];

                if (data is EnemyData enemy)
                    attribute = enemy.NumeralAttributes.Find(x => x.Type == mageHeroModificator.Type);

                if (data is SpiritData spirit)
                    attribute = spirit.NumeralAttributes.Find(x => x.Type == mageHeroModificator.Type);

                ModifyAttribute(attribute, mageHeroModificator);
            }
        }

        void Modify<EntityData>(EntityData data) where EntityData : Entity
        {
            if (data is EnemyData enemy)
                ModifyEnemyAttributes();

            if (data is SpiritData spirit)
                ModifySpiritAttributes();

            #region Helper functions

            void ModifyEnemyAttributes()
            {
                ModifyNumeralAttributes(enemy);

                for (int i = 0; i < Data.EnemyAttributes.Count; i++)
                {
                    var mageHeroModificator = Data.EnemyAttributes[i];
                    var attribute = enemy.EnemyAttributes.Find(x => x.Type == mageHeroModificator.Type);

                    attribute.AppliedValue += mageHeroModificator.Value;
                }
            }

            void ModifySpiritAttributes()
            {
                ModifyNumeralAttributes(spirit);

                for (int i = 0; i < Data.SpiritAttributes.Count; i++)
                {
                    var mageHeroModificator = Data.SpiritAttributes[i];
                    var attribute = spirit.SpiritAttributes.Find(x => x.Type == mageHeroModificator.Type);

                    ModifyAttribute(attribute, mageHeroModificator);
                }

                for (int i = 0; i < Data.SpiritFlagAttributes.Count; i++)
                {
                    var mageHeroModificator = Data.SpiritFlagAttributes[i];
                    var attribute = spirit.FlagAttributes.Find(x => x.Type == mageHeroModificator.Type);

                    attribute.Value = mageHeroModificator.Value;
                }
            }

            #endregion
        }

        public IEntitySystem Owner { get; private set; }
        public ID ID { get; private set; }
        public MageHero Data { get; private set; }
    }
}
