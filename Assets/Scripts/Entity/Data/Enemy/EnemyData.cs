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
    [CreateAssetMenu(fileName = "Enemy", menuName = "Data/Enemy New")]

    [Serializable]
    public class EnemyData : Entity, IAttributeComponent, IAbilityComponent, ITraitComponent
    {
        [SerializeField] public List<Trait> Traits { get; set; }
        [SerializeField] public List<Ability> Abilities { get; set; }
        [SerializeField] public List<NumeralAttribute> BaseAttributes { get; set; }
        [SerializeField] public List<NumeralAttribute> AppliedAttributes { get; set; }

        [SerializeField] public Armor.ArmorType ArmorType { get; set; }

        [SerializeField] public int WaveLevel;
        [SerializeField] public RaceType Race;
        [SerializeField] public EnemyType Type;
        [SerializeField] public int numberInList;

        private void Awake()
        {
            ID.Add((int)Race);
            ID.Add(numberInList);
        }

#if UNITY_EDITOR

        [Button]
        public void AddToDataBase()
        {
            if (DataControlSystem.Load<EnemyDataBase>() is EnemyDataBase dataBase)
            {
                var thisEnemyRace = dataBase.Races[(int)Race];

                if (!thisEnemyRace.Enemies.Contains(this))
                {
                    numberInList = thisEnemyRace.Enemies.Count;

                    ID = new ID() { (int)Race, numberInList };
                    SetName();

                    thisEnemyRace.Enemies.Add(this);
                    EditorUtility.SetDirty(this);
                    DataControlSystem.Save(dataBase);
                }
            }
        }

        private void RemoveFromDataBase()
        {
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

#endif 
    }
}
