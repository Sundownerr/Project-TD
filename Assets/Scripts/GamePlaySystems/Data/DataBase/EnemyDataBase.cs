using UnityEngine;
using System;
using Game.Data.Enemy;

namespace Game.Data.Databases
{
    [CreateAssetMenu(fileName = "EnemyDataBase", menuName = "Data/Data Base/Enemy DataBase")]
    [Serializable]
    public class EnemyDataBase : EntityDataBase<EnemyData>
    {

    }
}

