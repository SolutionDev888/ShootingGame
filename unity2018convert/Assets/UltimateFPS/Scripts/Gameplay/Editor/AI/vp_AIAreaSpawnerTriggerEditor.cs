/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIAreaSpawnerTriggerEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_AIAreaSpawnerTriggerEditor class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(vp_AIAreaSpawnerTrigger))]
[CanEditMultipleObjects]

public class vp_AIAreaSpawnerTriggerEditor : Editor
{

	// target component
	public vp_AIAreaSpawnerTrigger m_Component = null;
	public static long lastUpdateTick;
	
	static public Texture2D blankTexture
	{
		get{ return EditorGUIUtility.whiteTexture; }
	}


	/// <summary>
	/// hooks up the object to the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (vp_AIAreaSpawnerTrigger)target;

	}


	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		Undo.SetSnapshotTarget(m_Component, "AIAreaSpawnerTriggerEditor Snapshot");
    	Undo.CreateSnapshot();

		GUI.color = Color.white;

		Main();

		// update
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);
			Undo.RegisterSnapshot();

		}

	}
	
	
	public virtual void Main()
	{
		
		GUILayout.Space(5);
		
		m_Component.Spawner = (vp_AIAreaSpawner)EditorGUILayout.ObjectField("AI Spawner", m_Component.Spawner, typeof(vp_AIAreaSpawner), true);
		if(m_Component.Spawner == null)
			m_Component.SpawnObjectsOnTrigger = 0;
		
		if(m_Component.Spawner != null)
		{
			m_Component.TriggerMask = vp_AIEditor.LayerMaskField("Triggerable Layers", m_Component.TriggerMask);
		
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("AI Affected on Trigger");
		
			string[] options = new string[m_Component.Spawner.AISpawnerObjects.Count];
			for(int i=0; i<options.Length; i++)
				options[i] = m_Component.Spawner.AISpawnerObjects[i].SpawnObjectName;
			m_Component.SpawnObjectsOnTrigger = EditorGUILayout.MaskField(m_Component.SpawnObjectsOnTrigger, options);
			if(System.DateTime.Now.Ticks - lastUpdateTick > 10000000L && Event.current.type == EventType.Layout)
			{
				m_Component.SpawnObjectsList.Clear();
				for(int i=0; i<options.Length; i++)
					if((m_Component.SpawnObjectsOnTrigger & 1<<i) != 0)
						m_Component.SpawnObjectsList.Add(m_Component.Spawner.AISpawnerObjects[i]);
			}
			
			EditorGUILayout.EndHorizontal();
		}
		
	}
	
	static public void DrawSeparator ()
	{
		
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 10f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 10f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 13f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
		
	}
		
}

