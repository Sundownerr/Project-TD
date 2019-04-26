using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    [Serializable, CreateAssetMenu(fileName = "ItemDB", menuName = "Data/Data Base/Item DataBase")]
    public class ItemDataBase : EntityDataBase<Item> { }
}