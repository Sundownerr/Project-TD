using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Data
{
	[Serializable, CreateAssetMenu(fileName = "SpiritTraitDB", menuName = "Data/Data Base/Spirit Trait DataBase")]
	public class SpiritTraitDataBase : EntityDataBase<Trait> {}
}
