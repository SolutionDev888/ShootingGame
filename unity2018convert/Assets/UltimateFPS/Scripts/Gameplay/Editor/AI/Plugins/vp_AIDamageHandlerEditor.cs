using UnityEditor;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_AIDamageHandlerEditor : vp_AIEditor
{

	public static void DeathSpawnObjects()
	{
	
		m_AI.DamageHandler.DeathSpawnObjectsFoldout = EditorGUILayout.Foldout(m_AI.DamageHandler.DeathSpawnObjectsFoldout, "Death Spawn Objects");
		if(m_AI.DamageHandler.DeathSpawnObjectsFoldout)
		{
			if(m_AI.DamageHandler.DeathSpawnObjects.Count == 0)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(20);
				EditorGUILayout.HelpBox("Click the \"Add Object\" button below to add a new Death Spawn Object.", MessageType.Info);
				GUILayout.Space(20);
				GUILayout.EndHorizontal();
			}
			EditorGUI.indentLevel++;
			if(m_AI.DamageHandler.DeathSpawnObjects.Count > 1)
				m_AI.DamageHandlerOnlySpawnOneObject = EditorGUILayout.Toggle(new GUIContent("Only Spawn One", "If checked, only 1 object from the list will be spawned upon death"), m_AI.DamageHandlerOnlySpawnOneObject);
			for(int i=0;i<m_AI.DamageHandler.DeathSpawnObjects.Count;i++)
			{
				GUILayout.BeginHorizontal();
				vp_AIDamageHandler.DeathSpawnObject dso = m_AI.DamageHandler.DeathSpawnObjects[i];
				dso.Foldout = EditorGUILayout.Foldout(dso.Foldout, dso.Prefab == null ? "Object "+(i+1).ToString() : dso.Prefab.name);
				if(GUILayout.Button("Remove", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
				{
					m_AI.DamageHandler.DeathSpawnObjects.RemoveAt(i);
					i--;
					return;
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
				if(dso.Foldout)
				{
					EditorGUI.indentLevel++;
					dso.Prefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab", "The prefab that gets spawned upon death. This would most likely be something the player could pick up"), dso.Prefab, typeof(GameObject), false);
					dso.Probability = EditorGUILayout.Slider(new GUIContent("Probability", "The probability that the above prefab will be spawned upon death"), dso.Probability, 0f, 1f);
					EditorGUI.indentLevel--;
				}
			}
			EditorGUI.indentLevel--;
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button(new GUIContent("Add Object", "Add a new object to spawn upon death"), GUILayout.Width(100)))
				m_AI.DamageHandler.DeathSpawnObjects.Add(null);
			GUILayout.EndHorizontal();
		}
	
	}
	
}
