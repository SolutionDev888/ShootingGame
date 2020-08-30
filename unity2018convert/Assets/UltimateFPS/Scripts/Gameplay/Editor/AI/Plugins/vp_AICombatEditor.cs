using UnityEditor;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_AICombatEditor : vp_AIEditor
{

	public static void HostileLayersTags()
	{
	
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(new GUIContent("Hostile Layers/Tags", "Layers and tags that this AI considers hostile. Checked items in these dropdowns will be attacked."), GUILayout.MaxWidth(150));
		m_AI.CombatHostileLayers = vp_AIEditor.LayerMaskField("", m_AI.CombatHostileLayers);
		m_AI.CombatHostileTags = TagMaskField("", m_AI.CombatHostileTags, ref m_AI.CombatHostileTagsList);
		GUILayout.EndHorizontal();
	
	}
	
	
	public static void MeleeIfRanged()
	{
	
		if(m_AI.CombatAllowRangedAttacks)
		{
			m_AI.CombatMeleeProbability = EditorGUILayout.IntSlider(new GUIContent("Melee Probability", "The probability for the AI to choose to do a melee attack over a ranged attack"), 100 - m_AI.CombatRangedProbability, 0, 100);
			m_AI.CombatOverrideRangedDistance = EditorGUILayout.Slider(new GUIContent("Override Ranged Distance", "The distance from the target where only melee attacks will be used"), m_AI.CombatOverrideRangedDistance, 0, 50);
		}
	
	}
	
	
	public static void RangedIfMelee()
	{
	
		if(m_AI.CombatAllowMeleeAttacks)
			m_AI.CombatRangedProbability = EditorGUILayout.IntSlider(new GUIContent("Ranged Probability", "The probability for the AI to choose to do a ranged attack over a melee attack"), 100 - m_AI.CombatMeleeProbability, 0, 100);
	
	}
	
}
