/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIDropdownListEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_UIDropdownList class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(vp_UIDropdownList))]
public class vp_UIDropdownListEditor : vp_UIControlEditor
{

	protected vp_UIDropdownList m_Target = null;
	
	protected override void OnEnable()
	{
	
		base.OnEnable();
		
		m_Target = (vp_UIDropdownList)target;
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected override void DoInspector()
	{
	
		base.DoInspector();
		
		GUILayout.Space(10);
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Items");
		GUILayout.Space(113f);
		m_Target.Items = GUILayout.TextArea(m_Target.Items, GUILayout.Height(75), GUILayout.MinWidth(100), GUILayout.MaxWidth(2000));
		GUILayout.Space(18);
		GUILayout.EndHorizontal();
		m_Target.Background = (Transform)EditorGUILayout.ObjectField("Background", m_Target.Background, typeof(Transform), true);
		m_Target.Label = (TextMesh)EditorGUILayout.ObjectField("Label", m_Target.Label, typeof(TextMesh), true);
		object tex = (object)EditorGUILayout.ObjectField("Item Background", m_Target.ItemBackground, typeof(object), false);
		if(tex != null)
			if(tex.GetType() == typeof(Texture))
				m_Target.ItemBackground = (Texture)tex;
		m_Target.ItemFont = (Font)EditorGUILayout.ObjectField("Item Font", m_Target.ItemFont, typeof(Font), false);
		m_Target.ItemHeight = EditorGUILayout.FloatField("Item Height", m_Target.ItemHeight);
		m_Target.ItemLabelPadding = EditorGUILayout.Vector2Field("Item Label Padding", m_Target.ItemLabelPadding);
		
		GUILayout.Space(10);
		
		ShowEventBindingInspector(m_Target);
		
	}
	
}
