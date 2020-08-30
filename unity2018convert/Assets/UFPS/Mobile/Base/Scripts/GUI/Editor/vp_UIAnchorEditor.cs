/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIAnchorEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_UIAnchor class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(vp_UIAnchor))]
public class vp_UIAnchorEditor : Editor
{

	// target component
	public vp_UIAnchor m_Component;
	public vp_UIAnchor.vp_UIAnchorSide m_OldSide;
	
	
	/// <summary>
	/// hooks up the component object as the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (vp_UIAnchor)target;
		
		if(m_Component.Manager == null)
			m_Component.Manager = m_Component.transform.root.GetComponent<vp_UIManager>();
			
		m_Component.Manager.ForceUIRefresh();
		
		m_OldSide = m_Component.Side;

	}
	

	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{
	
		Undo.RecordObject(target, "HUD Anchor Snapshot");

		GUI.color = Color.white;
		
		GUILayout.Space(10);

		m_Component.Side = (vp_UIAnchor.vp_UIAnchorSide)EditorGUILayout.EnumPopup("Side", m_Component.Side);
		if(m_Component.Side != m_OldSide)
		{
			m_Component.RefreshUI();
			m_OldSide = m_Component.Side;
		}
		
		GUILayout.Space(10);

		// update
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);

		}

	}
	

}

