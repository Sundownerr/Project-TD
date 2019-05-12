using System;
using Game.Data.Items;
using UnityEngine;

namespace Game.Data.Databases
{
    [Serializable, CreateAssetMenu(fileName = "ItemDataBase", menuName = "Data/Data Base/Item DataBase")]
    public class ItemDataBase : EntityDataBase<Item> { }
}