using UnityEngine;
using System;
using Game.Data.EnemyEntity;

namespace Game.Data.Databases
{
    [CreateAssetMenu(fileName = "EnemyDataBase", menuName = "Data/Data Base/Enemy DataBase")]
    [Serializable]
    public class EnemyDataBase : EntityDataBase<EnemyEntity.Enemy>
    {

    }
}

