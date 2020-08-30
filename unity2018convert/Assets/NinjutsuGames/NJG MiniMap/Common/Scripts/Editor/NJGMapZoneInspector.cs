//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using NJG;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit NJGMapZone.
/// </summary>

[CustomEditor(typeof(NJGMapZone))]
public class NJGMapZoneInspector : Editor
{
	NJGMapZone m;

	/// <summary>
	/// Draw the inspector.
	/// </summary>

	public override void OnInspectorGUI()
	{
#if UNITY_4_3
		EditorGUIUtility.LookLikeControls(130f);
#else
		EditorGUIUtility.labelWidth = 130f;
#endif
		m = target as NJGMapZone;

		NJGEditorTools.DrawEditMap();

		GUILayout.BeginHorizontal("AppToolbar");
		EditorGUILayout.LabelField(new GUIContent("Zone Name Preview", ""), GUILayout.Width(130f));
		GUI.contentColor = m.color;
		EditorGUILayout.LabelField(new GUIContent(m.zone, ""), EditorStyles.boldLabel);
		GUI.contentColor = Color.white;
		GUILayout.EndHorizontal();		

		string level = NJGEditorTools.DrawList("Level", NJGMapBase.instance.GetLevels(), m.level);
		string zone = NJGEditorTools.DrawList("Zone", NJGMapBase.instance.GetZones(m.level), m.zone);
		string triggerTag = EditorGUILayout.TagField("Trigger Tag", m.triggerTag);
		int colliderRadius = (int)EditorGUILayout.Slider("Collider Radius", m.colliderRadius, 1, 1000);
		bool generateOnTrigger = EditorGUILayout.Toggle("Generate On Trigger", m.generateOnTrigger);

		string name = "Zone - [" + NJGMapZone.list.IndexOf(m) + "] " + m.zone;

		if (m.name != name ||
			m.level != level ||
			m.zone != zone ||
			m.triggerTag != triggerTag ||
			m.colliderRadius != colliderRadius ||
			m.generateOnTrigger != generateOnTrigger)
		{			
			m.name = name;
			m.level = level;
			m.zone = zone;
			m.triggerTag = triggerTag;
			m.colliderRadius = colliderRadius;
			m.zoneCollider.radius = colliderRadius;
			m.generateOnTrigger = generateOnTrigger;
			
			NJGEditorTools.RegisterUndo("NJG Zone Update", m);
		}

		/*if (NJGMapBase.instance != null)
		{
			if (NJGMapBase.instance.atlas != null)
			{				
				extraSpace = Mathf.Max(0f, extraSpace - 30f);
			}
		}*/

		NJGEditorTools.DrawSeparator();
		
		GUILayout.BeginHorizontal();

		GUI.backgroundColor = Color.green;
		if (GUILayout.Button("Add New Zone"))
		{
			NJGMenu.AddMapZone();
		}
		GUI.backgroundColor = Color.white;

		GUI.backgroundColor = Color.red;
		if (GUILayout.Button("Delete Zone"))
		{
			Delete();
		}
		GUI.backgroundColor = Color.white;

		GUILayout.EndHorizontal();		
		
		EditorGUILayout.Separator();
	}

	void Delete()
	{		
		NJGTools.DestroyImmediate(m.gameObject);
		
		if (NJGMapZone.list.Count > 0)
			Selection.activeGameObject = NJGMapZone.list[NJGMapZone.list.Count - 1].gameObject;
		else
			Selection.activeGameObject = NJGMapBase.instance.gameObject;
	}

	#region Draw Sprite Preview

	/// <summary>
	/// Draw an enlarged sprite within the specified texture atlas.
	/// </summary>

	public Rect DrawSprite(Texture2D tex, Rect sprite, Material mat) { return DrawSprite(tex, sprite, mat, true, 0); }

	/// <summary>
	/// Draw an enlarged sprite within the specified texture atlas.
	/// </summary>

	public Rect DrawSprite(Texture2D tex, Rect sprite, Material mat, bool addPadding)
	{
		return DrawSprite(tex, sprite, mat, addPadding, 0);
	}

	/// <summary>
	/// Draw an enlarged sprite within the specified texture atlas.
	/// </summary>

	public Rect DrawSprite(Texture2D tex, Rect sprite, Material mat, bool addPadding, int maxSize)
	{
		float paddingX = addPadding ? 4f / tex.width : 0f;
		float paddingY = addPadding ? 4f / tex.height : 0f;
		float ratio = (sprite.height + paddingY) / (sprite.width + paddingX);

		ratio *= (float)tex.height / tex.width;

		// Draw the checkered background
		Color c = GUI.color;

		Rect rect = GUILayoutUtility.GetRect(0f, 0f);
		rect.width = Screen.width - rect.xMin;
		rect.height = rect.width * ratio;

		rect = new Rect(85, rect.yMin + 0, NJG.NJGMapBase.instance.iconSize, NJG.NJGMapBase.instance.iconSize);

		GUI.color = c;

		if (maxSize > 0)
		{
			float dim = maxSize / Mathf.Max(rect.width, rect.height);
			rect.width *= dim;
			rect.height *= dim;
		}

		// We only want to draw into this rectangle
		if (Event.current.type == EventType.Repaint)
		{
			if (mat == null)
			{
				GUI.DrawTextureWithTexCoords(rect, tex, sprite);
			}
			else
			{
				// NOTE: DrawPreviewTexture doesn't seem to support BeginGroup-based clipping
				// when a custom material is specified. It seems to be a bug in Unity.
				// Passing 'null' for the material or omitting the parameter clips as expected.
				UnityEditor.EditorGUI.DrawPreviewTexture(sprite, tex, mat);
				//UnityEditor.EditorGUI.DrawPreviewTexture(drawRect, tex);
				//GUI.DrawTexture(drawRect, tex);
			}
			rect = new Rect(sprite.x + rect.x, sprite.y + rect.y, sprite.width, sprite.height);
		}
		return rect;
	}
	#endregion
}
