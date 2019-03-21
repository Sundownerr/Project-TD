using UnityEngine;
using Game.Enemy.Data;
using System.Collections.Generic;
using Game.Data;
using NaughtyAttributes;
using Game.Systems;
using System.Text;
using System;
using UnityEditor;

namespace Game.Enemy
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Data/Enemy")]

    [Serializable]
    public class EnemyData : Entity, IAttributeComponent, IAbilityComponent, ITraitComponent
    {
        [SerializeField] public List<Trait> Traits { get; set; }
        [SerializeField] public List<Ability> Abilities { get; set; }
        [SerializeField] public List<NumeralAttribute> BaseAttributes { get; set; }
        [SerializeField] public List<NumeralAttribute> AppliedAttributes { get; set; }

        [SerializeField] public Armor.ArmorType ArmorType { get; set; }
        [SerializeField] public bool IsInstanced { get; set; }

        [SerializeField] public int WaveLevel;
        [SerializeField] public RaceType Race;
        [SerializeField] public EnemyType Type;

        protected int numberInList;

        public void SetId()
        {
            Id = new List<int>
            {
                (int)Race,
                numberInList
            };
        }

#if UNITY_EDITOR

        private void Awake()
        {
            //   AddToDataBase();           
            Abilities = new List<Ability>();
            Traits = new List<Trait>();
            BaseAttributes = BaseAttributes.CreateAttributeList();
            AppliedAttributes = AppliedAttributes.CreateAttributeList();
        }

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
                            EditorUtility.SetDirty(this);
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
    }
}
