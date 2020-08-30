/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UITouchWeaponSwitcherEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_UITouchWeaponSwitcher class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(vp_UITouchWeaponSwitcher))]
public class vp_UITouchWeaponSwitcherEditor : vp_UIControlEditor
{

	protected vp_UITouchWeaponSwitcher m_Target = null;

	protected override void OnEnable()
	{
	
		base.OnEnable();
		
		m_Target = (vp_UITouchWeaponSwitcher)target;
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected override void DoInspector()
	{
	
		m_Target.WeaponScroller = (Transform)EditorGUILayout.ObjectField("Weapon Scroller", m_Target.WeaponScroller, typeof(Transform), true);
		m_Target.ChangeOnRelease = EditorGUILayout.Toggle("Change On Release", m_Target.ChangeOnRelease);
		m_Target.WieldTouchDelay = EditorGUILayout.Slider("Wield Touch Delay", m_Target.WieldTouchDelay, 0, 5);
		m_Target.ChangeWeaponThreshold = EditorGUILayout.FloatField("Change Weapon Threshold", m_Target.ChangeWeaponThreshold);
		m_Target.ItemWidth = EditorGUILayout.FloatField("Item Width", m_Target.ItemWidth);
		m_Target.Angle = EditorGUILayout.FloatField("Angle", m_Target.Angle);
		m_Target.ItemYOffset = EditorGUILayout.FloatField("Item Y Offset", m_Target.ItemYOffset);
	
	}

}

