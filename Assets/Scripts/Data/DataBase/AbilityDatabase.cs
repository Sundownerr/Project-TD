using System;
using Game.Data.Abilities;
using UnityEngine;

namespace Game.Data.Databases
{
    [Serializable, CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Data/Data Base/Ability DataBase")]
    public class AbilityDatabase : EntityDataBase<Ability>
    {
      
    }
}