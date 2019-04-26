using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using System;

namespace Game.Data
{
    [Serializable, CreateAssetMenu(fileName = "MageDB", menuName = "Data/Data Base/Mage DataBase")]
    public class MageDataBase : EntityDataBase<MageData> { }
}