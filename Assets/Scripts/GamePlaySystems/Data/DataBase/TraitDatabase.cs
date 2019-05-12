using System;
using System.Collections;
using System.Collections.Generic;
using Game.Data.Traits;
using UnityEngine;

namespace Game.Data.Databases
{
    [Serializable, CreateAssetMenu(fileName = "TraitDatabase", menuName = "Data/Data Base/Trait DataBase")]
    public class TraitDatabase : EntityDataBase<Trait> { }
}