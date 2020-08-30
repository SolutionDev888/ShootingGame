/////////////////////////////////////////////////////////////////////////////////
//
//	vp_ShooterEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_FPShooter class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(vp_AIAudioSource))]
public class vp_AIAudioSourceEditor : Editor
{

	// target component
	public vp_AIAudioSource m_Component = null;

	// foldouts
	public static bool m_StateFoldout;
	public static bool m_PresetFoldout = true;
	private static vp_ComponentPersister m_Persister = null;


	/// <summary>
	/// hooks up the object to the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (vp_AIAudioSource)target;

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
	public virtual void OnDestroy()
	{

		m_Persister.IsActive = false;

	}


	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{
	
		Undo.SetSnapshotTarget(m_Component, "Shooter Snapshot");
    	Undo.CreateSnapshot();

		GUI.color = Color.white;

		string objectInfo = m_Component.gameObject.name;

		if (vp_Utility.IsActive(m_Component.gameObject))
			GUI.enabled = true;
		else
		{
			GUI.enabled = false;
			objectInfo += " (INACTIVE)";
		}

		if (!vp_Utility.IsActive(m_Component.gameObject))
		{
			GUI.enabled = true;
			return;
		}

		if (Application.isPlaying || m_Component.DefaultState.TextAsset == null)
		{

			EditorGUI.indentLevel++;
			m_Component.Distance = EditorGUILayout.Slider("Distance", m_Component.Distance, 0, 500);
			m_Component.ListeningLayers = vp_AIEditor.LayerMaskField("Listening Layers", m_Component.ListeningLayers);
			m_Component.GlobalEventName = EditorGUILayout.TextField("Global Event Name", m_Component.GlobalEventName);
			m_Component.SendRate = EditorGUILayout.Slider("Send Event Rate", m_Component.SendRate, 0, 60);
			EditorGUI.indentLevel--;

		}
		else
			vp_PresetEditorGUIUtility.DefaultStateOverrideMessage();

		// state
		m_StateFoldout = vp_PresetEditorGUIUtility.StateFoldout(m_StateFoldout, m_Component, m_Component.States, m_Persister);

		// preset
		m_PresetFoldout = vp_PresetEditorGUIUtility.PresetFoldout(m_PresetFoldout, m_Component);

		// update
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);
			Undo.RegisterSnapshot();

			// update the default state in order not to loose inspector tweaks
			// due to state switches during runtime
			if (Application.isPlaying)
				m_Component.RefreshDefaultState();

			if (m_Component.Persist)
				m_Persister.Persist();

			m_Component.Refresh();

		}

	}

		
}

