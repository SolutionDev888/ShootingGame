/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIAreaSpawnerEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	custom inspector for the vp_AIAreaSpawner class
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(vp_AIAreaSpawner))]
[CanEditMultipleObjects]

public class vp_AIAreaSpawnerEditor : Editor
{

	// target component
	public vp_AIAreaSpawner m_Component = null;
	protected static GUIStyle m_FoldoutStyle = null;
	protected static GUIStyle m_SubFoldoutStyle = null;
	
	public static GUIStyle FoldoutStyle
	{
		get
		{
			if (m_FoldoutStyle == null)
			{
				m_FoldoutStyle = new GUIStyle("Foldout");
				m_FoldoutStyle.fontSize = 11;
				m_FoldoutStyle.fontStyle = FontStyle.Bold;
			}
			return m_FoldoutStyle;
		}
	}
	
	public static GUIStyle SubFoldoutStyle
	{
		get
		{
			if (m_SubFoldoutStyle == null)
			{
				m_SubFoldoutStyle = new GUIStyle("Foldout");
				m_SubFoldoutStyle.fontSize = 10;
				m_SubFoldoutStyle.fontStyle = FontStyle.Bold;
			}
			return m_SubFoldoutStyle;
		}
	}
	
	static public Texture2D blankTexture
	{
		get{ return EditorGUIUtility.whiteTexture; }
	}
	
	/// <summary>
	/// creates a big 2-button toggle
	/// </summary>
	public static bool SmallButtonToggle(string labelOn, string labelOff, bool state)
	{

		GUIStyle onStyle = new GUIStyle("Button");
		GUIStyle offStyle = new GUIStyle("Button");

		onStyle.fontSize = 8;
		onStyle.alignment = TextAnchor.MiddleCenter;
		onStyle.margin.left = 1;
		onStyle.margin.right = 10;
		onStyle.normal = onStyle.active;
		
		offStyle.fontSize = 8;
		offStyle.alignment = TextAnchor.MiddleCenter;
		offStyle.margin.left = 1;
		offStyle.margin.right = 10;

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button(state ? labelOn : labelOff, state ? onStyle : offStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
			if(state)
				state = false;
			else
				state = true;
		EditorGUILayout.EndHorizontal();

		return state;

	}


	/// <summary>
	/// hooks up the object to the inspector target
	/// </summary>
	public virtual void OnEnable()
	{

		m_Component = (vp_AIAreaSpawner)target;

	}


	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI()
	{

		Undo.SetSnapshotTarget(m_Component, "AIAreaSpawnerEditor Snapshot");
    	Undo.CreateSnapshot();

		GUI.color = Color.white;

		Main();

		// update
		if (GUI.changed)
		{

			EditorUtility.SetDirty(target);
			Undo.RegisterSnapshot();

		}

	}
	
	
	public virtual void Main()
	{
		GUILayout.Space(5);
		
		if(m_Component.AISpawnerObjects.Count > 0)
		{
			Toggle("Spawn At Start", "Yes", "No", ref m_Component.SpawnAtStart, 5);
			GUILayout.Space(5);
		}
	
		for(int i=0;i<m_Component.AISpawnerObjects.Count;i++)
		{
			vp_AIAreaSpawner.AISpawnerObject p = m_Component.AISpawnerObjects[i];
			
			GUILayout.BeginHorizontal();
			string count = p.Prefab == null || p.AmountToSpawn <= 1 ? "" : " ("+p.AmountToSpawn+")";
			p.Foldout = EditorGUILayout.Foldout(p.Foldout, " "+(i+1).ToString() + ". "+p.SpawnObjectName + count, FoldoutStyle);
			GUILayout.FlexibleSpace();
			if(p.Prefab == null)
			{
				p.Enabled = false;
				GUI.enabled = false;
			}
			p.Enabled = SmallButtonToggle("Enabled", "Disabled", p.Enabled);
			GUI.enabled = true;
			
			if(p.Prefab != null)
				if (GUILayout.Button("Reset", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
					if(EditorUtility.DisplayDialog("Are You Sure?", "Are you sure you want to reset all values to the Prefab's defaults?", "Yes", "No"))
						p.ShouldUpdate = true;
			
			if (GUILayout.Button("Duplicate", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
			{
				m_Component.AISpawnerObjects.Add(p.Copy());
				return;
			}
			if (GUILayout.Button("Remove", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
			{
				bool remove = true;
				if(p.Prefab != null)
					remove = EditorUtility.DisplayDialog("Are You Sure?", "Are you sure you want to delete \""+p.SpawnObjectName+"\"?", "Yes", "No");
					
				if(remove)
				{
					m_Component.AISpawnerObjects.RemoveAt(i);
					--i;
					return;
				}
			}
			GUILayout.Space(5);
			GUILayout.EndHorizontal();
			
			if(p.Foldout)
			{
			
				GUILayout.Space(5);
				
				GUILayout.BeginHorizontal();
				GUILayout.Space(15);
				p.SpawnObjectName = EditorGUILayout.TextField("Name", p.SpawnObjectName);
				GUILayout.EndHorizontal();
			
				// Prefab
				GUILayout.BeginHorizontal();
				GUILayout.Space(15);
				GameObject oldPrefab = p.Prefab;
				p.Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", p.Prefab, typeof(GameObject), false);
				if(p.Prefab != null && p.Prefab.GetComponent<vp_AI>() == null)
				{
					EditorUtility.DisplayDialog("Not an AI Prefab", "Prefab's must contain a vp_AI component on them", "OK");
					p.Prefab = oldPrefab;
					return;
				}
				GUILayout.Space(5);
				GUILayout.EndHorizontal();
				
				if(p.Prefab != oldPrefab && p.Prefab != null)
				{
					p.SpawnObjectName = p.Prefab.name;
					p.Enabled = true;
				}
				
				if(p.Prefab == null || p.Prefab != oldPrefab)
					p.ShouldUpdate = true;
				
				if(p.Prefab != null)
				{
					p.UpdateCachedComponents();
					p.UpdateComponents();
				
					GUILayout.BeginHorizontal();
					GUILayout.Space(15);
					p.AmountToSpawn = EditorGUILayout.IntSlider("Amount To Spawn", p.AmountToSpawn, 1, 100);
					GUILayout.EndHorizontal();
					
					if(m_Component.SpawnAtStart)
						Toggle("Spawn At Start", "Yes", "No", ref p.SpawnAtStart, 15);
					
					GUILayout.BeginHorizontal();
					GUILayout.Space(15);
					p.Scale = EditorGUILayout.Vector3Field("Scale", p.Scale);
					GUILayout.EndHorizontal();
					GUILayout.Space(10);
					
					//////////////////
					///
					/// Damage Handler Foldout
					///
					//////////////////	
					if(p.CachedDamageHandlerComponent != null)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space(15);
						p.DamageHandlerFoldout = EditorGUILayout.Foldout(p.DamageHandlerFoldout, "Vitals, Spawning and Death", SubFoldoutStyle);
						if (GUILayout.Button("Reset", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
						{
							if(EditorUtility.DisplayDialog("Are You Sure?", "Are you sure you want to reset these values to the Prefab's defaults?", "Yes", "No"))
							{
								p.UpdateValuesFromComponentNonMonoBehavior( ref p.CachedDamageHandlerComponent, "DamageHandler" );
								return;
							}
						}
						GUILayout.Space(5);
						GUILayout.EndHorizontal();
						GUILayout.Space(5);
						
						if(p.DamageHandlerFoldout)
						{
						
							RangeSlider("Min/Max Random Starting Health", ref p.DamageHandlerStartingHealth, 0, p.CachedDamageHandlerComponent.MaxAllowedHealth);
							if(p.DamageHandlerStartingHealth.y > p.CachedDamageHandlerComponent.MaxAllowedHealth)
								p.CachedDamageHandlerComponent.MaxAllowedHealth = p.DamageHandlerStartingHealth.y > 1000 ? p.DamageHandlerStartingHealth.y+1 : 1000;
							
							GUILayout.BeginHorizontal();
							GUILayout.Space(30);
							GUILayout.Label("Ragdoll", GUILayout.MaxWidth(145));
							p.DamageHandlerRagdoll = (GameObject)EditorGUILayout.ObjectField(p.DamageHandlerRagdoll, typeof(GameObject), true);
							GUILayout.EndHorizontal();
							if(p.DamageHandlerRagdoll)
							{
								Toggle(new GUIContent("Destroy Ragdoll", "Whether ragdoll should be destroyed on respawn or not"), "YES", "NO", ref p.DamageHandlerDestroyRagdollOnRespawn);
								Slider("Disable Ragdoll Time", ref p.DamageHandlerDisableRagdollTime, 0, p.DamageHandlerRespawnTime.y);
								Slider("Ragdoll Force", ref p.DamageHandlerRagdollForce, 0, 1000);
							}
						
							GUILayout.BeginHorizontal();
							GUILayout.Space(30);
							p.DamageHandlerDeathSpawnObjectsFoldout = EditorGUILayout.Foldout(p.DamageHandlerDeathSpawnObjectsFoldout, "Death Spawn Objects");
							GUILayout.EndHorizontal();
							if(p.DamageHandlerDeathSpawnObjectsFoldout)
							{
								if(p.DeathSpawnObjects.Count == 0)
								{
									GUILayout.BeginHorizontal();
									GUILayout.Space(20);
									EditorGUILayout.HelpBox("Click the \"Add Object\" button below to add a new Death Spawn Object.", MessageType.Info);
									GUILayout.Space(20);
									GUILayout.EndHorizontal();
								}
								EditorGUI.indentLevel += 4;
								p.DamageHandlerOnlySpawnOneObject = EditorGUILayout.Toggle("Spawn One Object", p.DamageHandlerOnlySpawnOneObject);
								for(int n=0;n<p.DeathSpawnObjects.Count;n++)
								{
									GUILayout.BeginHorizontal();
									vp_AIDamageHandler.DeathSpawnObject dso = p.DeathSpawnObjects[n];
									dso.Foldout = EditorGUILayout.Foldout(dso.Foldout, dso.Prefab == null ? "Object "+(n+1).ToString() : dso.Prefab.name);
									if(GUILayout.Button("Remove", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
									{
										p.DeathSpawnObjects.RemoveAt(n);
										n--;
										return;
									}
									GUILayout.EndHorizontal();
									GUILayout.Space(5);
									if(dso.Foldout)
									{
										EditorGUI.indentLevel++;
										dso.Prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", dso.Prefab, typeof(GameObject), false);
										dso.Probability = EditorGUILayout.Slider("Probability", dso.Probability, 0f, 1f);
										EditorGUI.indentLevel--;
									}
								}
								EditorGUI.indentLevel -= 4;
								GUILayout.BeginHorizontal();
								GUILayout.FlexibleSpace();
								if(GUILayout.Button("Add Object", GUILayout.Width(100)))
									p.DeathSpawnObjects.Add(null);
								GUILayout.EndHorizontal();
							}
						
							Toggle("Respawns", "YES", "NO", ref p.DamageHandlerRespawns);
							
							/// Min/Max Respawn Time
							RangeSlider("Min/Max Respawn Time (secs)", ref p.DamageHandlerRespawnTime, 0, 600);
							
							Toggle("Rand Respawn Position", "YES", "NO", ref p.RandomRespawnPosition);
							
							Slider("Spawn Check Radius", ref p.DamageHandlerRespawnCheckRadius, 1, 100);
							
							Slider("Dmg State Probability", ref p.DamageHandlerTakeDamageAnimationProbability, 0, 1);
							Slider("Damaged Again Delay", ref p.DamageHandlerTakeDamageAgainDelay, 0, 60);
							GUILayout.BeginHorizontal();
							GUILayout.Space(30);
							p.DamageHandlerTakeDamageEffect = (GameObject)EditorGUILayout.ObjectField("Damaged Effect", p.DamageHandlerTakeDamageEffect, typeof(GameObject), false);
							GUILayout.EndHorizontal();
							if(p.DamageHandlerTakeDamageEffect != null)
								Slider("Effect Probability", ref p.DamageHandlerTakeDamageEffectProbability, 0, 1);
							
							GUILayout.BeginHorizontal();
							GUILayout.Space(15);
							vp_EditorGUIUtility.Separator();
							GUILayout.Space(15);
							GUILayout.EndHorizontal();
						}
						
						GUILayout.Space(5);
					}
						
					
					//////////////////
					///
					/// Movement Foldout
					///
					//////////////////	
					if(p.CachedMovementComponent != null)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space(15);
						p.MovementFoldout = EditorGUILayout.Foldout(p.MovementFoldout, "Movement", SubFoldoutStyle);
						if (GUILayout.Button("Reset", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
						{
							if(EditorUtility.DisplayDialog("Are You Sure?", "Are you sure you want to reset these values to the Prefab's defaults?", "Yes", "No"))
							{
								p.UpdateValuesFromComponentNonMonoBehavior( ref p.CachedMovementComponent, "Movement" );
								return;
							}
						}
						GUILayout.Space(5);
						GUILayout.EndHorizontal();
						GUILayout.Space(5);

						if(p.MovementFoldout)
						{
							Slider("Walk Speed", ref p.MovementWalkSpeed, 0, 50);
							Slider("Walk Turning Speed", ref p.MovementWalkTurningSpeed, 0, 100);
							Slider("Run Speed", ref p.MovementRunSpeed, 0, 50);
							Slider("Run Turning Speed", ref p.MovementRunTurningSpeed, 0, 100);
							GUILayout.Space(10);
						
							Toggle("Allow Roaming", "YES", "NO", ref p.MovementAllowRoaming);
							
							if(p.MovementAllowRoaming)
							{
								EditorGUI.indentLevel++;
								// Waypoints
								GUILayout.BeginHorizontal();
								GUILayout.Space(30);
								p.MovementWaypointsParent = (Transform)EditorGUILayout.ObjectField("Waypoints", p.MovementWaypointsParent, typeof(Transform), true);
								GUILayout.EndHorizontal();
								
								if(p.MovementWaypointsParent == null)
									RangeSlider("Min/Max Roaming Distance", ref p.MovementRoamingDistance, 5, 100);
									
								RangeSlider("Min/Max Roaming Wait Time (secs)", ref p.MovementRoamingWaitTime, 0, 120);
								EditorGUI.indentLevel--;
							}
							
							GUILayout.Space(10);
							Toggle("Allow Retreat", "YES", "NO", ref p.MovementAllowRetreat);
							if(p.MovementAllowRetreat)
							{
								EditorGUI.indentLevel++;
								RangeSlider("Min/Max Retreat Distance From Target", ref p.MovementRetreatDistFromTarget, 1, 500);
								Slider("Retreat Health Threshold", ref p.MovementRetreatHealthThreshold, 0, 1);
								Slider("Low Health Retreat Dist", ref p.MovementRetreatLowHealthDistance, 1, 100);
								Slider("Delay Retreat Can Start", ref p.MovementRetreatCanStartDelay, 0, 10);
								EditorGUI.indentLevel--;
							}
							
							GUILayout.BeginHorizontal();
							GUILayout.Space(15);
							vp_EditorGUIUtility.Separator();
							GUILayout.Space(15);
							GUILayout.EndHorizontal();
						}
						
						GUILayout.Space(5);
					}
					
					
					//////////////////
					///
					/// Senses Foldout
					///
					//////////////////	
					if(p.CachedSensesComponent != null)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space(15);
						p.SensesFoldout = EditorGUILayout.Foldout(p.SensesFoldout, "Senses", SubFoldoutStyle);
						if (GUILayout.Button("Reset", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
						{
							if(EditorUtility.DisplayDialog("Are You Sure?", "Are you sure you want to reset these values to the Prefab's defaults?", "Yes", "No"))
							{
								p.UpdateValuesFromComponentNonMonoBehavior( ref p.CachedSensesComponent, "Senses" );
								return;
							}
						}
						GUILayout.Space(5);
						GUILayout.EndHorizontal();
						GUILayout.Space(5);
						
						if(p.SensesFoldout)
						{
							string checkRateAdd = p.SensesViewCheckRate == 0 ? " (never)" : "";
							Slider("Viewing Check Rate"+checkRateAdd, ref p.SensesViewCheckRate, .1f, 60);
							Slider("Viewing Distance", ref p.SensesViewDistance, 0, 1000);
							Slider("Viewing Angle", ref p.SensesViewingAngle, 0, 360);
							checkRateAdd = p.SensesHearingCheckRate == 0 ? " (never)" : "";
							Slider("Hearing Check Rate"+checkRateAdd, ref p.SensesHearingCheckRate, .1f, 60);
							Slider("Hearing Distance", ref p.SensesHearingDistance, 0, 1000);
						
							GUILayout.BeginHorizontal();
							GUILayout.Space(15);
							vp_EditorGUIUtility.Separator();
							GUILayout.Space(15);
							GUILayout.EndHorizontal();
						}
						
						GUILayout.Space(5);
					}
					
					
					//////////////////
					///
					/// Combat Foldout
					///
					//////////////////	
					if(p.CachedCombatComponent != null)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Space(15);
						p.CombatFoldout = EditorGUILayout.Foldout(p.CombatFoldout, "Combat", SubFoldoutStyle);
						if (GUILayout.Button("Reset", vp_EditorGUIUtility.SmallButtonStyle, GUILayout.MinWidth(50), GUILayout.MaxWidth(50), GUILayout.MinHeight(15)))
						{
							if(EditorUtility.DisplayDialog("Are You Sure?", "Are you sure you want to reset these values to the Prefab's defaults?", "Yes", "No"))
							{
								p.UpdateValuesFromComponentNonMonoBehavior( ref p.CachedCombatComponent, "Combat" );
								return;
							}
						}
						GUILayout.Space(5);
						GUILayout.EndHorizontal();
						GUILayout.Space(5);
						
						if(p.CombatFoldout)
						{
							GUILayout.BeginHorizontal();
							GUILayout.Space(30);
							GUILayout.Label("Hostile Layers/Tags");
							p.CombatHostileLayers = vp_AIEditor.LayerMaskField("", p.CombatHostileLayers);
							p.CombatHostileTags = vp_AIEditor.TagMaskField("", p.CombatHostileTags, ref p.CombatHostileTagsList);
							GUILayout.EndHorizontal();
							
							Slider("Look For Hostile Dist", ref p.CombatHostileCheckDistance, 0, 1000);
							Slider("Alert Friendly Dist", ref p.CombatAlertFriendlyDistance, 0, 100);
						
						
							Toggle("Allow Melee Attacks", "YES", "NO", ref p.CombatAllowMeleeAttacks);
							
							if(p.CombatAllowMeleeAttacks)
							{
								if(p.CombatAllowRangedAttacks)
								{
									int probability = 100 - p.CombatRangedProbability;
									IntSlider("Melee Probability (%)", ref probability, 0, 100);
									p.CombatMeleeProbability = probability;
									Slider("Override Ranged Distance", ref p.CombatOverrideRangedDistance, 0, 50);
								}
								
								RangeSlider( "Min/Max Melee Damage", ref p.CombatMeleeAttackDamage, 0, 1 );
								RangeSlider( "Min/Max Distance to Melee Attack", ref p.CombatMeleeAttackDistance, 1, 100 );
								
								Slider("Attack Again Time (secs)", ref p.CombatMeleeAttackAgainTime, 0, 50);
								
							}
							
							Toggle("Allow Ranged Attacks", "YES", "NO", ref p.CombatAllowRangedAttacks);
							
							if(p.CombatAllowRangedAttacks)
							{
								if(p.CombatAllowMeleeAttacks)
								{
									int probability = 100 - p.CombatMeleeProbability;
									IntSlider("Ranged Probability (%)", ref probability, 0, 100);
									p.CombatRangedProbability = probability;
								}
								
								RangeSlider( "Min/Max Ranged Damage", ref p.CombatRangedAttackDamage, 0, 1 );
								RangeSlider( "Min/Max Distance to Ranged Attack", ref p.CombatRangedAttackDistance, 1, 100 );
								
								IntSlider("Bullets Per Clip", ref p.CombatBulletsPerClip, 0, 50);
								Slider("Reload Time (secs)", ref p.CombatReloadTime, 0, 50);
								
							}
						}
					}
					
					DrawSeparator();
					GUILayout.Space(5);
				}
			}
			
			GUILayout.Space(5);
		}
		
		if(m_Component.AISpawnerObjects.Count == 0)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.HelpBox("Click the \"Add AI\" button below to add a new unit to spawn from this Area Spawner.", MessageType.Info);
			GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}
		
		GUILayout.Space(5);
		GUILayout.BeginHorizontal();
		GUILayout.Space(15);
		if (GUILayout.Button("Add AI", GUILayout.MinWidth(150), GUILayout.MinHeight(25)))
			m_Component.AISpawnerObjects.Add(new vp_AIAreaSpawner.AISpawnerObject());
		GUILayout.Space(15);
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
	
	}
	
	
	protected virtual void Toggle( string label, string onName, string offName, ref bool property, float leftMargin = 30)
	{
	
		GUILayout.BeginHorizontal();
		GUILayout.Space(leftMargin);
		GUILayout.Label(label, GUILayout.MaxWidth(145));
		property = SmallButtonToggle(onName, offName, property);
		GUILayout.EndHorizontal();
	
	}
	
	
	protected virtual void Toggle( GUIContent content, string onName, string offName, ref bool property, float leftMargin = 30)
	{
	
		GUILayout.BeginHorizontal();
		GUILayout.Space(leftMargin);
		GUILayout.Label(content, GUILayout.MaxWidth(145));
		property = SmallButtonToggle(onName, offName, property);
		GUILayout.EndHorizontal();
	
	}
	
	
	protected virtual void FloatField( string name, ref float property, float leftMargin = 30)
	{
	
		GUILayout.BeginHorizontal();
		GUILayout.Space(leftMargin);
		property = EditorGUILayout.FloatField(name, property);
		GUILayout.EndHorizontal();
	
	}
	
	
	protected virtual void Slider( string name, ref float property, float min, float max, float leftMargin = 30 )
	{
	
		GUILayout.BeginHorizontal();
		GUILayout.Space(leftMargin);
		property = EditorGUILayout.Slider(name, property, min, max);
		GUILayout.EndHorizontal();
	
	}
	
	
	protected virtual void IntSlider( string name, ref int property, int min, int max, float leftMargin = 30 )
	{
	
		GUILayout.BeginHorizontal();
		GUILayout.Space(leftMargin);
		property = EditorGUILayout.IntSlider(name, property, min, max);
		GUILayout.EndHorizontal();
	
	}
	
	protected virtual void RangeSlider( string name, ref Vector2 minmax, float minVal, float maxVal, float leftMargin = 30 )
	{
	
		GUILayout.BeginHorizontal();
		GUILayout.Space(leftMargin);
		EditorGUILayout.LabelField(name);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Space(leftMargin);
		float min = minmax.x;
		float max = minmax.y;
		
		min = EditorGUILayout.FloatField(min,GUILayout.MaxWidth(50));
		EditorGUILayout.MinMaxSlider(ref min, ref max, minVal, maxVal);
		max = EditorGUILayout.FloatField(max,GUILayout.MaxWidth(50));
		
		minmax.x = (float)System.Math.Round(min, 2);
		minmax.y = (float)System.Math.Round(max, 2);
		GUILayout.EndHorizontal();
	
	}
	
	static public void DrawSeparator ()
	{
		
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 10f, Screen.width, 4f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 10f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 13f, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
		
	}
		
}

