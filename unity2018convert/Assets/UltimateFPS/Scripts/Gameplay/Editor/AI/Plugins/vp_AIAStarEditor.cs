using UnityEditor;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
public class vp_AIAStarEditor : vp_AIEditor
{

	public static void ShowModifiers()
	{
	
		Seeker seeker = m_AI.GetComponent<Seeker>();
		AlternativePath altPath = m_AI.GetComponent<AlternativePath>();
		FunnelModifier funnel = m_AI.GetComponent<FunnelModifier>();
		SimpleSmoothModifier simpleSmooth = m_AI.GetComponent<SimpleSmoothModifier>();
	
		GUILayout.BeginHorizontal();
		GUILayout.Space(25);
		// seeker
		if(seeker == null)
		{
			if(GUILayout.Button("Add Seeker", GUILayout.Height(25), GUILayout.MinWidth(150)))
				seeker = m_AI.gameObject.AddComponent<Seeker>();
		}
		else
			if(GUILayout.Button("Remove Seeker", GUILayout.Height(25), GUILayout.MinWidth(150)))
				DestroyImmediate(seeker);
				
		// funnel modifier
		if(funnel == null)
		{
			if(GUILayout.Button("Add Funnel", GUILayout.Height(25), GUILayout.MinWidth(150)))
				funnel = m_AI.gameObject.AddComponent<FunnelModifier>();
		}
		else
			if(GUILayout.Button("Remove Funnel", GUILayout.Height(25), GUILayout.MinWidth(150)))
				DestroyImmediate(funnel);
		GUILayout.EndHorizontal();
			
		GUILayout.BeginHorizontal();
		GUILayout.Space(25);	
		// simple smooth
		if(simpleSmooth == null)
		{
			if(GUILayout.Button("Add Simple Smooth", GUILayout.Height(25), GUILayout.MinWidth(150)))
				simpleSmooth = m_AI.gameObject.AddComponent<SimpleSmoothModifier>();
		}
		else
			if(GUILayout.Button("Remove Simple Smooth", GUILayout.Height(25), GUILayout.MinWidth(150)))
				DestroyImmediate(simpleSmooth);
				
		// alternative path
		if(altPath == null)
		{
			if(GUILayout.Button("Add Alternative Path", GUILayout.Height(25), GUILayout.MinWidth(150)))
				altPath = m_AI.gameObject.AddComponent<AlternativePath>();
		}
		else
			if(GUILayout.Button("Remove Alternative Path", GUILayout.Height(25), GUILayout.MinWidth(150)))
				DestroyImmediate(altPath);
		GUILayout.EndHorizontal();
		
		GUILayout.Space(5);
		
		if(m_AI.AStarAutoPrioritize)
		{
			int priority = 1;
			if(simpleSmooth != null)
				simpleSmooth.priority = priority++;
			if(funnel != null)
				funnel.priority = priority++;
			if(altPath != null)
				altPath.priority = priority++;
		}
	
	}
	
}
