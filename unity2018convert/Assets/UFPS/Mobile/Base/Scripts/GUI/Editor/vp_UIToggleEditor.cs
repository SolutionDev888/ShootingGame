/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIToggleEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_UIToggle class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(vp_UIToggle))]
public class vp_UIToggleEditor : vp_UIControlEditor
{

	protected vp_UIToggle m_Target = null;
	
	protected override void OnEnable()
	{
	
		base.OnEnable();
		
		m_Target = (vp_UIToggle)target;
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected override void DoInspector()
	{
	
		base.DoInspector();
		
		GUILayout.Space(10);
		
		m_Target.Background = (GameObject)EditorGUILayout.ObjectField("Background", m_Target.Background, typeof(GameObject), true);
		m_Target.Checkmark = (GameObject)EditorGUILayout.ObjectField("Checkmark", m_Target.Checkmark, typeof(GameObject), true);
		bool state = m_Target.State;
		m_Target.State = EditorGUILayout.Toggle("State", m_Target.State);
		if(m_Target.Checkmark != null && m_Target.State != state)
			vp_Utility.Activate(m_Target.Checkmark, m_Target.State);	
		
		GUILayout.Space(10);
		
		ShowEventBindingInspector(m_Target);
		
	}
	
}
