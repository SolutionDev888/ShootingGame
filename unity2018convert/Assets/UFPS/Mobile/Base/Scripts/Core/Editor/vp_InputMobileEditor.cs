/////////////////////////////////////////////////////////////////////////////////
//
//	vp_InputMobileEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_InputMobile class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(vp_InputMobile))]
public class vp_InputMobileEditor : Editor
{

	// target component
	public vp_InputMobile m_Component;
	
	/// <summary>
	/// hooks up the component object as the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (vp_InputMobile)target;

	}

	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		GUI.color = Color.white;
		
		GUILayout.Space(10);
		GUILayout.BeginHorizontal();
		GUILayout.Space(10);
		if (GUILayout.Button("Open VP Input Manager", GUILayout.MinWidth(150), GUILayout.MinHeight(25)))
			vp_InputWindow.Init();
		GUILayout.Space(10);
		GUILayout.EndHorizontal();
		GUILayout.Space(10);

		// update
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);

		}

	}
	

}

