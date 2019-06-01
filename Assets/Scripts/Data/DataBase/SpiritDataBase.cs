using UnityEngine;
using System;
using Game.Data.SpiritEntity;

#if UNITY_EDITOR
#endif

namespace Game.Data.Databases
{
    [CreateAssetMenu(fileName = "SpiritDataBase", menuName = "Data/Data Base/Spirit DataBase")]
    [Serializable]
    public class SpiritDataBase : EntityDataBase<SpiritData>
    {

    }
}