using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Game.Data.Databases;

#if UNITY_EDITOR

[CustomEditor(typeof(SpiritDataBase), true)]
[CanEditMultipleObjects]

public class TowerDataBaseEditor : Editor
{
   
    private List<SerializedProperty> elements;

    private void Awake()
    {
        var spirits = serializedObject.FindProperty("Spirits").FindPropertyRelative("Elements");

        elements = new List<SerializedProperty>();
        for (int i = 0; i < 7; i++)    
            elements.Add(spirits.GetArrayElementAtIndex(i));      
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();   
         
         if(elements != null)
            for (int i = 0; i < elements.Count; i++)
            {           
                EditorGUI.DrawRect(EditorGUILayout.GetControlRect(GUILayout.Height(3)), new Color(0.1f, 0.1f, 0.1f, 0.8f));

                EditorGUILayout.Space();
                GUILayout.Box(elements[i].FindPropertyRelative("Name").stringValue, GUILayout.Width(200));
                
                var rarities = elements[i].FindPropertyRelative("Rarities");
                EditorGUI.indentLevel += 1;

                for (int j = 0; j < rarities.arraySize; j++)
                {
                    var spirits = rarities.GetArrayElementAtIndex(j).FindPropertyRelative("Spirits");

                    EditorGUILayout.PropertyField(spirits, new GUIContent(rarities.GetArrayElementAtIndex(j).FindPropertyRelative("Name").stringValue), true);                         
                }

                EditorGUI.indentLevel -= 1;

                EditorGUILayout.Space();
            }
        
            serializedObject.ApplyModifiedProperties();
    }   
}
#endif
