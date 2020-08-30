/////////////////////////////////////////////////////////////////////////////////
//
//	vp_FPInputMobileEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_FPInputMobile class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(vp_FPInputMobile))]

public class vp_FPInputMobileEditor : Editor
{

	// target component
	public vp_FPInputMobile m_Component = null;

	// input foldouts
	// NOTE: these are static so they remain open when toggling
	// between different components. this simplifies copying
	// content (prefabs / sounds) between components
	public static bool m_TouchLookFoldout;
	public static bool m_StateFoldout;
	public static bool m_PresetFoldout = true;

	private static vp_ComponentPersister m_Persister = null;

	
	/// <summary>
	/// hooks up the FPSCamera object to the inspector target
	/// </summary>
	public void OnEnable()
	{

		m_Component = (vp_FPInputMobile)target;

		if (m_Persister == null)
			m_Persister = new vp_ComponentPersister();
		m_Persister.Component = m_Component;
		m_Persister.IsActive = true;

		if (m_Component.DefaultState == null)
			m_Component.RefreshDefaultState();
		
	}


	/// <summary>
	/// disables the persister and removes its reference
	/// </summary>
	void OnDestroy()
	{

		m_Persister.IsActive = false;

	}

	
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		m_Component.MouseCursorBlocksMouseLook = false;

		if (Application.isPlaying || m_Component.DefaultState.TextAsset == null)
		{

			DoTouchLookFoldout();

		}
		else
			vp_PresetEditorGUIUtility.DefaultStateOverrideMessage();

		// state foldout
		m_StateFoldout = vp_PresetEditorGUIUtility.StateFoldout(m_StateFoldout, m_Component, m_Component.States, m_Persister);

		// preset foldout
		m_PresetFoldout = vp_PresetEditorGUIUtility.PresetFoldout(m_PresetFoldout, m_Component);

		// update default state and persist in order not to loose inspector tweaks
		// due to state switches during runtime - UNLESS a runtime state button has
		// been pressed (in which case user wants to toggle states as opposed to
		// reset / alter them)
		if (GUI.changed &&
			(!vp_PresetEditorGUIUtility.RunTimeStateButtonTarget == m_Component))
		{

			EditorUtility.SetDirty(target);

			if (Application.isPlaying)
				m_Component.RefreshDefaultState();

			if (m_Component.Persist)
				m_Persister.Persist();
	
			m_Component.Refresh();

		}
		
	}


	/// <summary>
	/// 
	/// </summary>
	public virtual void DoTouchLookFoldout()
	{

		m_TouchLookFoldout = EditorGUILayout.Foldout(m_TouchLookFoldout, "Touch Look");
		if (m_TouchLookFoldout)
		{

			m_Component.MouseLookSensitivity = EditorGUILayout.Vector2Field("Sensitivity", m_Component.MouseLookSensitivity);
			m_Component.MouseLookSmoothSteps = EditorGUILayout.IntSlider("Smooth Steps", m_Component.MouseLookSmoothSteps, 1, 20);
			m_Component.MouseLookSmoothWeight = EditorGUILayout.Slider("Smooth Weight", m_Component.MouseLookSmoothWeight, 0, 1);
			m_Component.MouseLookAcceleration = EditorGUILayout.Toggle("Acceleration", m_Component.MouseLookAcceleration);
			if (!m_Component.MouseLookAcceleration)
				GUI.enabled = false;
			m_Component.MouseLookAccelerationThreshold = EditorGUILayout.Slider("Acc. Threshold", m_Component.MouseLookAccelerationThreshold, 0, 5);
			GUI.enabled = true;
			m_Component.MouseLookInvert = EditorGUILayout.Toggle("Invert Y", m_Component.MouseLookInvert);

			vp_EditorGUIUtility.Separator();
		}

	}



}

