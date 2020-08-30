/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIControlEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_UIControl class.
//					Classes that derive from vp_UIControl should derive from this
//					class for their editor.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(vp_UIControl))]
public class vp_UIControlEditor : Editor
{

	protected Vector3 m_LastPosition = Vector3.zero;
	protected vp_UIControl m_Component = null;
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void OnEnable()
	{

		m_Component = (vp_UIControl)target;
		
		if(m_Component.Manager == null)
			m_Component.Manager = m_Component.transform.root.GetComponent<vp_UIManager>();
		
		if(m_Component.Manager)
			m_Component.Manager.ForceUIRefresh();

	}
	
	
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		Undo.RecordObject(target, "HUD Control Inspector Snapshot");

		GUI.color = Color.white;

		DoInspector();
		
		GUILayout.Space(10);

		// update
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);

		}

	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void DoInspector(){}
	
	
	public virtual void ShowEventBindingInspector( vp_UIControl control )
	{
	
		if(m_Component.Objects.Count == 0)
		{
			m_Component.Objects.Add(null);
			m_Component.MethodIndexes.Add(0);
			m_Component.Methods.Add(null);
		}
	
		for(int i = 0; i<m_Component.Objects.Count; i++)
		{
			m_Component.Objects[i] = EditorGUILayout.ObjectField("Notify Object", m_Component.Objects[i], typeof(MonoBehaviour), true) as MonoBehaviour;
			if(m_Component.Objects[i] != null)
			{
				GUILayout.Space(-20f);
				GUILayout.BeginHorizontal();
				GUILayout.Space(134f);
				if(m_Component.Objects.Count > 1)
				{
#if UNITY_3_5
					if (GUILayout.Button("X", GUILayout.Width(20f)))
#else
					if (GUILayout.Button("", "ToggleMixed", GUILayout.Width(20f)))
#endif
					{
						m_Component.Objects.RemoveAt(i);
						m_Component.MethodIndexes.RemoveAt(i);
						m_Component.Methods.RemoveAt(i);
						return;
					}
					GUILayout.Space(5);
				}
				GUILayout.EndHorizontal();
			
				if(m_Component.Objects.Count == i+1)
				{
					m_Component.Objects.Add(null);
					m_Component.MethodIndexes.Add(0);
					m_Component.Methods.Add(null);
					return;
				}
				
				
				List<MonoBehaviour> components = m_Component.Objects[i].GetComponents<MonoBehaviour>().ToList();
				List<string> sComponents = new List<string>();
				for(int n=0;n<components.Count; n++)
				{
					MonoBehaviour component = components[n];
					MethodInfo[] methodInfos = component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | ~BindingFlags.SetField);
					if(control.GetType() == typeof(vp_UITouchButton))
						methodInfos = component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
					foreach(MethodInfo methodInfo in methodInfos)
					{
						bool valid = control.GetType() == typeof(vp_UITouchButton) ? methodInfo.GetParameters().Length == 0 && methodInfo.ReturnType == typeof(void) : methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters()[0].ParameterType == typeof(vp_UIControl) && methodInfo.ReturnType == typeof(void);
						if(valid)
							if (methodInfo.Name != "StopAllCoroutines" && methodInfo.Name != "CancelInvoke")
								sComponents.Add(component.GetType().Name+"."+methodInfo.Name);
					}
				}
				
				if(sComponents.Count > 0)
				{
					m_Component.MethodIndexes[i] = EditorGUILayout.Popup("Method", m_Component.MethodIndexes[i], sComponents.ToArray());
					m_Component.Methods[i] = sComponents[m_Component.MethodIndexes[i]];
				}
				else
				{
					EditorGUILayout.HelpBox("No public methods were found in any components on "+m_Component.Objects[i].name+". Add a public method in any component on "+m_Component.Objects[i].name+" in order to use event binding for this object", MessageType.Info);
				}
			}
			GUILayout.Space(10);
		}
	
	}
	
}
