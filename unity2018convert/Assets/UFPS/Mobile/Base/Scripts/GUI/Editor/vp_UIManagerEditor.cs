/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIManagerEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_UIManager class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(vp_UIManager))]
public class vp_UIManagerEditor : Editor
{

	// target component
	public vp_UIManager m_Component;
	
	private static GUIStyle m_HeaderStyle = null;
	public static GUIStyle HeaderStyle
	{
		get
		{
			if (m_HeaderStyle == null)
			{
				m_HeaderStyle = new GUIStyle("Label");
				m_HeaderStyle.fontSize = 12;
				m_HeaderStyle.fontStyle = FontStyle.Bold;
			}
			return m_HeaderStyle;
		}
	}

	/// <summary>
	/// hooks up the component object as the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (vp_UIManager)target;
		
		if(m_Component.UICamera == null)
			m_Component.UICamera = vp_UIManager.GetUICamera(m_Component.transform);
			
		if(!Application.isPlaying)
			m_Component.ForceUIRefresh();

	}

	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		Undo.RecordObject(target, "UI Manager Snapshot");

		GUI.color = Color.white;
		
		GUILayout.Space(10);
		
		m_Component.UILayer = EditorGUILayout.LayerField("UI Layer", m_Component.UILayer);
		
		GUILayout.Space(5);
		
		m_Component.Player = (vp_FPPlayerEventHandler)EditorGUILayout.ObjectField("Player", m_Component.Player, typeof(vp_FPPlayerEventHandler), true);
		if(m_Component.Player == null)
			EditorGUILayout.HelpBox("You must provide a vp_FPPlayerEventHandler from your scene in order for the UI to be able to control the Player.", MessageType.Warning);
		
		GUILayout.Space(5);
		
		m_Component.UICamera = (Camera)EditorGUILayout.ObjectField("UI Camera", m_Component.UICamera, typeof(Camera), true);
		if(m_Component.UICamera == null)
			EditorGUILayout.HelpBox("A vp_UICamera component could not be found in this GameObject's hierarchy, therefore a camera object could not automatically be found.", MessageType.Warning);
		
		GUILayout.Space(5);
		
		GUILayout.Label("Resolution", HeaderStyle);
		EditorGUI.indentLevel++;
		m_Component.Resolution = new Vector2(EditorGUILayout.FloatField("Width", m_Component.Resolution.x), EditorGUILayout.FloatField("Height", m_Component.Resolution.y));
		EditorGUI.indentLevel--;
		
		GUILayout.Space(5);
		
		m_Component.SimulateTouchWithMouse = EditorGUILayout.Toggle(new GUIContent("Simulate Touch w/ Mouse", "If this is checked, keyboard controls in the editor are disabled and the left mouse click will work like a touch. If it's not checked, normal keyboard controls will work."), m_Component.SimulateTouchWithMouse);
		
		GUILayout.Space(5);
		
		m_Component.DoubleTapTimeout = EditorGUILayout.FloatField("Double Tap Timeout", m_Component.DoubleTapTimeout);
		
		float x = (m_Component.Resolution.x / m_Component.Resolution.y) * 2;
		Bounds newBounds = new Bounds(m_Component.UICamera.transform.position, new Vector3(x, 2, 0));
		if(newBounds != m_Component.ScreenArea)
		{
			m_Component.ScreenArea = newBounds;
			m_Component.ForceUIRefresh();
		}
		
		GUILayout.Space(10);
		
		if(GUILayout.Button("Force UI Refresh"))
		{
			m_Component.ForceUIRefresh();
		}
				
		GUILayout.Space(10);

		// update
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);

		}

	}
	

}

