using System.Collections.Generic;
using UnityEngine;
using System;
using Game.Enemy.Data;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "EnemyDB", menuName = "Data/Data Base/Enemy DataBase")]
   	[Serializable]
	public class EnemyDataBase : ScriptableObject
    {
        [SerializeField]
        public List<Race> Races;

        private void Awake()
        {
            if (Races == null)    
            {      
                Races = new List<Race>();             

				var races = Enum.GetValues(typeof(RaceType));  
                
                for (int i = 0; i < races.Length; i++)
                    Races.Add(new Race());               
            }                           
        }  
    }
}

