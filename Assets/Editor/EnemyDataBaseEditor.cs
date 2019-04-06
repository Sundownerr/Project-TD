using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Game.Data;
using Game.Enemy.Data;
using System;
using Game.Enums;

#if UNITY_EDITOR

[CustomEditor(typeof(EnemyDataBase), true)]
[CanEditMultipleObjects]

public class EnemyDataBaseEditor : Editor 
{
	private SerializedProperty allCreeps;
    private SerializedProperty[] races;

    private void Awake()
    {
        allCreeps = serializedObject.FindProperty("Races");

        races = new SerializedProperty[allCreeps.arraySize];

        for (int i = 0; i < races.Length; i++)
        {
            races[i] = allCreeps.GetArrayElementAtIndex(i).FindPropertyRelative("Enemies");
        }
    
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();   
         
            for (int i = 0; i < races.Length; i++)
            {           
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(3)), new Color(0.1f, 0.1f, 0.1f, 0.8f));

                EditorGUILayout.Space();

				GUILayout.Box(Enum.GetNames(typeof(RaceType))[i], GUILayout.Width(200));	
        	
                EditorGUI.indentLevel += 1;
 				
				EditorGUILayout.PropertyField(races[i], GUIContent.none, true);                         
                				
				EditorGUI.indentLevel -= 1;
                EditorGUILayout.Space();
            }
        
            serializedObject.ApplyModifiedProperties();
    }   
}

#endif