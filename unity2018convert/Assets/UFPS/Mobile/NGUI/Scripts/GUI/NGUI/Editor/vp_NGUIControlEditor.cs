/////////////////////////////////////////////////////////////////////////////////
//
//	vp_NGUIControlEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_NGUIControl class.
//					Classes that derive from vp_NGUIControl should derive from this
//					class for their editor.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(vp_NGUIControl))]
public class vp_NGUIControlEditor : Editor
{

	protected vp_UIControl m_Component = null;
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void OnEnable()
	{

		m_Component = (vp_UIControl)target;
		
		if(m_Component.Manager == null)
			m_Component.Manager = m_Component.transform.root.GetComponent<vp_NGUIManager>();
		
		m_Component.Manager.ForceUIRefresh();

	}
	
	
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{
	
		Undo.RecordObject(target, "NGUI Control Editor Snapshot");

		GUI.color = Color.white;

		DoInspector();
		
		GUILayout.Space(10);

		// update
		if (GUI.changed)
			EditorUtility.SetDirty(target);

	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void DoInspector(){}
	
}
