//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

/// <summary>
/// This script adds the NJGMenu menu options to the Unity Editor.
/// </summary>

static public class NJGMenu
{
	//[MenuItem("NJG MiniMap/Add NJG MiniMap %&m")]
	static public void AddNJGMiniMap()
	{		
		GameObject parent = NJGEditorTools.SelectedRoot();
		GameObject go = CreateLocalGameObject(true);
		go.AddComponent<NJG.NJGMapBase>();
		go.name = "NJG MiniMap";
		go.transform.parent = parent.transform;
		go.transform.localPosition = Vector3.zero;
	}

	//[MenuItem("NJG MiniMap/Add a World Zone #&z")]
	static public void AddWorldZone()
	{
		if (NJG.NJGMapBase.zonesRoot == null)
		{
			NJG.NJGMapBase.zonesRoot = CreateLocalGameObject(false);
			NJG.NJGMapBase.zonesRoot.transform.parent = NJG.NJGMapBase.zonesRoot.transform.parent.transform.parent;
			NJG.NJGMapBase.zonesRoot.transform.localPosition = Vector3.zero;
			NJG.NJGMapBase.zonesRoot.name = "_MapZones";
		}

		GameObject go = CreateLocalGameObject(false);
		go.AddComponent<NJGMapZone>();		
		go.transform.parent = NJG.NJGMapBase.zonesRoot.transform;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		go.transform.position = ray.GetPoint(1);
	}

	static public void AddMapZone()
	{
		if (NJG.NJGMapBase.zonesRoot == null)
		{
			NJG.NJGMapBase.zonesRoot = CreateLocalGameObject(false);
			NJG.NJGMapBase.zonesRoot.transform.parent = NJG.NJGMapBase.zonesRoot.transform.parent.transform.parent;
			NJG.NJGMapBase.zonesRoot.transform.localPosition = Vector3.zero;
			NJG.NJGMapBase.zonesRoot.name = "_MapZones";
		}

		GameObject go = CreateLocalGameObject(false);
		go.AddComponent<NJGMapZone>();
		go.transform.parent = NJG.NJGMapBase.zonesRoot.transform;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		go.transform.position = ray.GetPoint(1);
	}

	static GameObject CreateLocalGameObject(bool usePrefabCheck)
	{
		if (PrefabCheck() || !usePrefabCheck)
		{
			// Create our new GameObject
			GameObject newGameObject = new GameObject();
			newGameObject.name = "GameObject";

			// If there is a selected object in the scene then make the new object its child.
			
			newGameObject.transform.parent = Selection.activeTransform;
			newGameObject.name = "Child";

			// Place the new GameObject at the same position as the parent.
			//newGameObject.transform.localPosition = Vector3.zero;
			newGameObject.transform.localRotation = Quaternion.identity;
			newGameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			if(Selection.activeGameObject != null) newGameObject.layer = Selection.activeGameObject.layer;			

			// Select our newly created GameObject
			Selection.activeGameObject = newGameObject;

			return newGameObject;
		}

		return null;
	}

	/// <summary>
	/// Helper function that checks to see if there are objects selected.
	/// </summary>

	static bool HasValidSelection()
	{
		if (Selection.objects == null || Selection.objects.Length == 0)
		{
			Debug.LogWarning("You must select an object first");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Helper function that checks to see if there is an object with a Transform component selected.
	/// </summary>

	static bool HasValidTransform()
	{
		if (Selection.activeTransform == null)
		{
			Debug.LogWarning("You must select an object first");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Helper function that checks to see if a prefab is currently selected.
	/// </summary>

	static bool PrefabCheck()
	{
		if (Selection.activeTransform != null)
		{
			// Check if the selected object is a prefab instance and display a warning
#if UNITY_3_4
			PrefabType type = EditorUtility.GetPrefabType(Selection.activeGameObject);
#else
			PrefabType type = PrefabUtility.GetPrefabType(Selection.activeGameObject);
#endif

			if (type == PrefabType.PrefabInstance)
			{
				return EditorUtility.DisplayDialog("Losing prefab",
					"This action will lose the prefab connection. Are you sure you wish to continue?",
					"Continue", "Cancel");
			}
		}
		return true;
	}
}
