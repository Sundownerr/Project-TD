using Game.Enums;
using Game.Data.Attributes;
using Game.Systems.Spirit;
using Game.Systems.Enemy;
using Game.Data.Mage;
using Game.Data.Enemy;
using Game.Data.Spirit;

namespace Game.Systems.Mage
{
    public class MageHeroSystem : IEntitySystem
    {
        public IEntitySystem Owner { get; private set; }
        public MageData Mage { get; private set; }

        enum From
        {
            StartingAttribute,
            PerLevelAttribute
        }

        public MageHeroSystem(PlayerSystem owner, MageData data)
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

        void OnSpiritPlaced(object _, SpiritSystem e)
        {
            if (e.IsOwnedByLocalPlayer)
            {
                ModifyAttributes(e.Data, From.StartingAttribute);
                e.LeveledUp += OnSpiritLeveledUp;
            }
        }

        void OnEnemyCreated(object _, EnemySystem e)
        {
            if (e.IsOwnedByLocalPlayer)
                ModifyAttributes(e.Data, From.StartingAttribute);
        }

        void OnSpiritLeveledUp(object _, SpiritSystem e) => ModifyAttributes(e.Data, From.PerLevelAttribute);

        void ModifyAttributes(EnemyData enemy, From getFrom)
        {
            ModifyNumeralAttributes(enemy, getFrom);

            Mage.EnemyAttributes.ForEach(mageAttribute =>
                enemy.EnemyAttributes.Find(attribute => mageAttribute.Type == attribute.Type).AppliedValue += mageAttribute.Value);
        }

        void ModifyAttributes(SpiritData spirit, From getFrom)
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

        void ModifyNumeralAttributes<EntityData>(EntityData data, From getFrom)
        {
            var entityAttribute = new EntityAttribute_A_L<Numeral, double>();

            Mage.NumeralAttributes.ForEach(mageAttribute =>
            {
                if (data is EnemyData enemy)
                    entityAttribute = enemy.NumeralAttributes.Find(attribute => attribute.Type == mageAttribute.Type);

                if (data is SpiritData spirit)
                    entityAttribute = spirit.NumeralAttributes.Find(attribute => attribute.Type == mageAttribute.Type);

                ModifyEntityAttribute(entityAttribute, mageAttribute, getFrom);
            });
        }

        void ModifyEntityAttribute<EnumType>(EntityAttribute_A_L<EnumType, double> entityAttribute, EntityAttribute_A_L<EnumType, double> mageAttribute, From getFrom)
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
