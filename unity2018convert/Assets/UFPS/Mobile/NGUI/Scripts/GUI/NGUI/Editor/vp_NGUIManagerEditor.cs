/////////////////////////////////////////////////////////////////////////////////
//
//	vp_NGUIManagerEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_NGUIManager class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(vp_NGUIManager))]
public class vp_NGUIManagerEditor : vp_UIManagerEditor
{

	/// <summary>
	/// hooks up the component object as the inspector target
	/// </summary>
	public override void OnEnable()
	{

		m_Component = (vp_NGUIManager)target;

	}

	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{
	
		Undo.RecordObject(target, "NGUI Manager Snapshot");

		GUI.color = Color.white;
		
		GUILayout.Space(10);
		
		m_Component.Player = (vp_FPPlayerEventHandler)EditorGUILayout.ObjectField("Player", m_Component.Player, typeof(vp_FPPlayerEventHandler), true);
		if(m_Component.Player == null)
			EditorGUILayout.HelpBox("You must provide a vp_FPPlayerEventHandler from your scene in order for the UI to be able to control the Player.", MessageType.Warning);
		
		GUILayout.Space(5);
		
		m_Component.SimulateTouchWithMouse = EditorGUILayout.Toggle(new GUIContent("Simulate Touch w/ Mouse", "If this is checked, keyboard controls in the editor are disabled and the left mouse click will work like a touch. If it's not checked, normal keyboard controls will work."), m_Component.SimulateTouchWithMouse);
		
		GUILayout.Space(5);
		
		m_Component.DoubleTapTimeout = EditorGUILayout.FloatField("Double Tap Timeout", m_Component.DoubleTapTimeout);
		
		GUILayout.Space(5);
				
		GUILayout.Space(10);

		// update
		if (GUI.changed)
			EditorUtility.SetDirty(target);

	}
	

}

