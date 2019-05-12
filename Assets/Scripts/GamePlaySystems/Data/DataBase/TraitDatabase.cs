using System;
using Game.Data.Traits;
using UnityEngine;

namespace Game.Data.Databases
{
    [Serializable, CreateAssetMenu(fileName = "TraitDatabase", menuName = "Data/Data Base/Trait DataBase")]
    public class TraitDatabase : EntityDataBase<Trait> { }
}