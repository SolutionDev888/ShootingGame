/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UITouchButtonEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_UITouchButton class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(vp_UITouchButton))]
public class vp_UITouchButtonEditor : vp_UIControlEditor
{

	protected vp_UITouchButton m_Target = null;

	protected override void OnEnable()
	{
	
		base.OnEnable();
		
		m_Target = (vp_UITouchButton)target;
	
	}

	/// <summary>
	/// 
	/// </summary>
	protected override void DoInspector()
	{
	
		base.DoInspector();
	
		vp_Input inputManager = null;
		
		vp_Input[] inputs = Resources.FindObjectsOfTypeAll(typeof(vp_Input)) as vp_Input[];
		if(inputs.Length > 0)
			inputManager = inputs[0];
		
		// if no instance was found, try and load it from resources
		if(inputManager == null)
		{
			GameObject go = Resources.Load("Input/vp_Input") as GameObject;
			if(go != null)
				inputManager = go.GetComponent<vp_Input>();
		}

		if(inputManager == null)
		{
			EditorGUILayout.HelpBox("Could not find a vp_InputManager component in the hierarchy.", MessageType.Info);
			return;
		}
		
		List<string> buttons = inputManager.ButtonKeys;
		if(buttons == null)
		{
			EditorGUILayout.HelpBox("No Buttons have been added to the Input Manager.", MessageType.Info);
			return;
		}
		
		GUILayout.Space(10);
		
		m_Target.OverrideTouches = m_Target.Manager != null ? EditorGUILayout.Toggle("Override Touches", m_Target.OverrideTouches) : false;
		
		string[] buttonStrings = new string[ buttons.Count+2 ];
		buttonStrings[0] = "Event Binding \u21C6";
		buttonStrings[1] = "";
		int id = 0;
		for (int i = 0; i < buttons.Count; ++i)
		{
			if(buttons[i] == m_Target.Action)
				id = i+2;
				
			buttonStrings[i+2] = buttons[i];
		}
		
		m_Target.Event = (vp_UITouchButton.vp_UIButtonState)EditorGUILayout.EnumPopup("Event", m_Target.Event);
		if(m_Target.Event == vp_UITouchButton.vp_UIButtonState.OnDoubleRelease || m_Target.Event == vp_UITouchButton.vp_UIButtonState.OnRelease)
			m_Target.RequireStayInBounds = EditorGUILayout.Toggle(new GUIContent("Require Stay in Bounds", "When this is on, the event will only trigger if the touch stays within the button's bounds from the time the touch begins to when it ends."), m_Target.RequireStayInBounds);
		
		int newId = EditorGUILayout.Popup("Perform Action", id, buttonStrings);
		m_Target.Action = buttonStrings[newId];
		
		if(m_Target.Action != "Event Binding \u21C6")
			return;
			
		GUILayout.Space(10);
		
		ShowEventBindingInspector(m_Target);
		
	}
	

}

