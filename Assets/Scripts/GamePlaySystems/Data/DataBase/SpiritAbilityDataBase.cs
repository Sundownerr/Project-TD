using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Data
{
	[Serializable, CreateAssetMenu(fileName = "SpiritAbilityDB", menuName = "Data/Data Base/Spirit Ability DataBase")]
	public class SpiritAbilityDataBase : EntityDataBase<Ability> {}
}
