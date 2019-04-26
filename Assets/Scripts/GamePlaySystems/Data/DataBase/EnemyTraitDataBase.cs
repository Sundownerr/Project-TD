using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Data
{
	[Serializable, CreateAssetMenu(fileName = "EnemyTraitDB", menuName = "Data/Data Base/Enemy Trait DataBase")]
	public class EnemyTraitDataBase : EntityDataBase<Trait> {}
}
