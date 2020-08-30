/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UITouchLookEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_UITouchLook class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(vp_UITouchLook))]
public class vp_UITouchLookEditor : vp_UIControlEditor
{

	protected vp_UITouchLook m_Target = null;

	protected override void OnEnable()
	{
	
		base.OnEnable();
		
		m_Target = (vp_UITouchLook)target;
	
	}

	/// <summary>
	/// 
	/// </summary>
	protected override void DoInspector()
	{
		
		m_Target.RotationSensitivity = EditorGUILayout.Vector2Field("Rotation Sensitivity", m_Target.RotationSensitivity);
		
		m_Target.AutopitchFoldout = EditorGUILayout.Foldout(m_Target.AutopitchFoldout, "Auto Pitch");
		if(m_Target.AutopitchFoldout)
		{
			EditorGUI.indentLevel++;
			m_Target.AutoPitchSpeed = EditorGUILayout.Slider("Speed"+(m_Target.AutoPitchSpeed == 0 ? " (off)" : ""), m_Target.AutoPitchSpeed, 0, 100);
			if(m_Target.AutoPitchSpeed > 0)
			{
				m_Target.AutoPitchDelay = EditorGUILayout.Slider("Delay"+(m_Target.AutoPitchDelay == 0 ? " (none)" : ""), m_Target.AutoPitchDelay, 0, 30);
				m_Target.AutoPitchWhenIdle = EditorGUILayout.Toggle("Auto Pitch When Idle", m_Target.AutoPitchWhenIdle);
			}
			EditorGUI.indentLevel--;
		}
		
	}
	

}

