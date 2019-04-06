using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
	[CreateAssetMenu(fileName = "EnemyAbilityDB", menuName = "Data/Data Base/Enemy Ability DataBase")]

	public class EnemyAbilityDataBase : ScriptableObject
	{
		public List<Ability> Abilities;
	}
}
