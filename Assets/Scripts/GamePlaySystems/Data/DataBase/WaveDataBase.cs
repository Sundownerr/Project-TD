using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.Data.Databases
{
    [CreateAssetMenu(fileName = "WaveDataBase", menuName = "Data/Data Base/Wave Data Base")]

	[Serializable]
	public class WaveDataBase : ScriptableObject
	{
		[SerializeField]
		public List<Wave> Waves, RiftbreakersWaves;
	}
}
