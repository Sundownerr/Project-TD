using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "MageDB", menuName = "Data/Data Base/Mage DataBase")]
    public class MageDataBase : ScriptableObject
    {
        public List<MageData> Data;
    }
}