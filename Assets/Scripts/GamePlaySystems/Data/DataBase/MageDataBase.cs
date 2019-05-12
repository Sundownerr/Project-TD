using UnityEngine;
using System;
using Game.Data.Mage;

namespace Game.Data.Databases
{
    [Serializable, CreateAssetMenu(fileName = "MageDataBase", menuName = "Data/Data Base/Mage DataBase")]
    public class MageDataBase : EntityDataBase<MageData> { }
}