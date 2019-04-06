using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
	[CreateAssetMenu(fileName = "EnemyTraitDB", menuName = "Data/Data Base/Enemy Trait DataBase")]

	public class EnemyTraitDataBase : ScriptableObject
	{
		public List<Trait> Traits;
	}
}
