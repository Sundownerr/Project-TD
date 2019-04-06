using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Game.Data;
using Game.Spirit.Data.Stats;

#if UNITY_EDITOR	
using UnityEditor;
#endif
using UnityEngine;
using U = UnityEngine.Object;

namespace Game.Systems
{
	public static class DataControlSystem 
	{		
		public static void Save<T>(T data) where T : ScriptableObject
        {		
			//if (data is Player.Player)			
			//{
   //             var settings = new JsonSerializerSettings();

   //             settings.Formatting = Formatting.Indented;
   //             settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

			//	var newData = JsonConvert.SerializeObject(data,  settings);
   //             File.WriteAllText("playerData.json", newData);
			//	return;
			//}
#if UNITY_EDITOR			
			if (data is SpiritDataBase spiritDB)		
			{	
				AssetDatabase.Refresh();	
				EditorUtility.SetDirty(spiritDB); 
				AssetDatabase.SaveAssets();
				return;
			}

			if (data is EnemyDataBase enemyDB)		
			{	
				AssetDatabase.Refresh();	
				EditorUtility.SetDirty(enemyDB); 	
				AssetDatabase.SaveAssets();			
				return;
			}
#endif
		}

		public static ScriptableObject Load<T>() where T : ScriptableObject

        {
			return
#if UNITY_EDITOR
                typeof(T) == typeof(SpiritDataBase) ? LoadSpiritDB() :
                typeof(T) == typeof(EnemyDataBase) ? LoadEnemyDB() :
#endif
 //               typeof(T) == typeof(Player.Player) ? LoadPlayerData():

                null as ScriptableObject;

            #region  Helper functions

   //         Player.Player LoadPlayerData()
			//{
			//	var playerData = new Player.Player();

			//	if (!File.Exists("playerData.json"))
			//	{
			//		playerData.ElementLevels = new List<int>();
			//		var elementAmount = Enum.GetValues(typeof(ElementType)).Length;

			//		for (int i = 0; i < elementAmount; i++)
			//			playerData.ElementLevels.Add(0);      

			//		playerData.Resources.MaxTowerLimit = 500;
			//		playerData.Resources.StartTowerRerollCount = 3;
			//		playerData.Resources.MagicCrystals = 100;   
			//		playerData.Resources.Gold = 100;
   //                 playerData.Inventory = new Player.Inventory();

			//		var newData = JsonConvert.SerializeObject(playerData);

   //                 File.WriteAllText("playerData.json", newData);
			//	} 
			//	else
			//	{
			//		var dataFromFile = File.ReadAllText("playerData.json");				
			//		var loadedData = JsonConvert.DeserializeObject<Player.Player>(dataFromFile);
					
			//		playerData.ElementLevels 	= new List<int>();
			//		playerData.ElementLevels    	= loadedData.ElementLevels;
			//		playerData.Resources.MagicCrystals       	= loadedData.Resources.MagicCrystals;
			//		playerData.Resources.Gold                	= loadedData.Resources.Gold;
			//		playerData.Resources.CurrentTowerLimit   	= loadedData.Resources.CurrentTowerLimit;
			//		playerData.Resources.MaxTowerLimit       	= loadedData.Resources.MaxTowerLimit;
   //                 playerData.Inventory            = loadedData.Inventory;
			//	}        

			//	return playerData;
			//}		
#if UNITY_EDITOR
			SpiritDataBase LoadSpiritDB() =>					
				(SpiritDataBase)AssetDatabase.LoadAssetAtPath("Assets/DataBase/SpiritDB.asset", typeof(SpiritDataBase));

			EnemyDataBase LoadEnemyDB() =>					
				(EnemyDataBase)AssetDatabase.LoadAssetAtPath("Assets/DataBase/EnemyDB.asset", typeof(EnemyDataBase));
#endif
			#endregion
		}
	}
}
