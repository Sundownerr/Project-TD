using System;
using Game.Data.Effects;
using UnityEngine;

namespace Game.Data.Databases
{
    [Serializable, CreateAssetMenu(fileName = "EffectDataBase", menuName = "Data/Data Base/Effect DataBase")]
    public class EffectDataBase : EntityDataBase<Effect>
    {

    }
}
