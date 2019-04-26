using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Data
{
    [Serializable, CreateAssetMenu(fileName = "EnemyAbilityDB", menuName = "Data/Data Base/Enemy Ability DataBase")]
    public class EnemyAbilityDataBase : EntityDataBase<Ability> { }
}
