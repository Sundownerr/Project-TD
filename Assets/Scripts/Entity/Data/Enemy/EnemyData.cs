using UnityEngine;
using Game.Enemy.Data;
using System.Collections.Generic;
using Game.Data;
using NaughtyAttributes;
using Game.Systems;
using System.Text;

namespace Game.Enemy
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Data/Enemy")]

    public class EnemyData : Entity, IAttributeComponent, IAbilityComponent, ITraitComponent
    {
        public List<Trait> Traits { get; set; }
        public List<Ability> Abilities { get; set; }
        public List<NumeralAttribute> BaseAttributes { get; set; } 
        public List<NumeralAttribute> AppliedAttributes { get; set; } 

        public Armor.ArmorType ArmorType { get; set; }
        public bool IsInstanced { get; set; }

        public int WaveLevel;
        public RaceType Race;
        public EnemyType Type;

        protected int numberInList;

        private void Awake()
        {
            //   AddToDataBase();           
            Abilities = new List<Ability>();
            Traits = new List<Trait>();
            BaseAttributes = BaseAttributes.CreateAttributeList();
            AppliedAttributes = AppliedAttributes.CreateAttributeList();
        }

        public void SetId()
        {
            Id = new List<int>
            {
                (int)Race,
                numberInList
            };
        }

        #region IF UNITY EDITOR

#if UNITY_EDITOR

        [Button]
        public void AddToDataBase()
        {
            if (!IsInstanced)
                if (DataControlSystem.Load<EnemyDataBase>() is EnemyDataBase dataBase)
                {
                    var races = dataBase.Races;
                    for (int i = 0; i < races.Count; i++)
                        if (i == (int)Race)
                        {
                            for (int j = 0; j < races[i].Enemies.Count; j++)
                                if (this.CompareId(races[i].Enemies[j].Id) || Name == races[i].Enemies[j].Name)
                                    return;

                            races[i].Enemies.Add(this);
                            numberInList = races[i].Enemies.Count - 1;

                            SetId();
                            SetName();
                            DataControlSystem.Save(dataBase);
                            return;
                        }
                }
        }

        private void RemoveFromDataBase()
        {
            if (!IsInstanced)
                if (DataControlSystem.Load<EnemyDataBase>() is EnemyDataBase dataBase)
                {
                    dataBase.Races[(int)Race].Enemies.RemoveAt(numberInList);
                    DataControlSystem.Save(dataBase);
                }
        }

        private void SetName()
        {
            var tempName = new StringBuilder();

            tempName.Append(Race.ToString());
            tempName.Append((int)Race);
            tempName.Append(numberInList);

            Name = tempName.ToString();
        }

        // private void OnDestroy() => RemoveFromDataBase();

        public void OnValuesChanged() => AddToDataBase();

#endif 

        #endregion      
    }
}
