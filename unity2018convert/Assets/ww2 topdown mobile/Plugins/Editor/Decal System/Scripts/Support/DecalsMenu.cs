//
// Author:
//   Andreas Suter (andy@edelweissinteractive.com)
//
// Copyright (C) 2012 Edelweiss Interactive (http://edelweissinteractive.com)
//

using UnityEngine;
using UnityEditor;
using System.Collections;
using Edelweiss.DecalSystemEditor;

public class DecalsMenu {

	[MenuItem ("GameObject/Create Other/Decals")]
	private static void CreateDecals () {
		GameObject l_GameObject = CreateGameObjectInFrontOfSceneViewCamera ("Decals", 5.0f);
		l_GameObject.AddComponent <DS_Decals> ();
		Selection.activeGameObject = l_GameObject;
	}
	
	[MenuItem ("GameObject/Create Other/Skinned Decals")]
	private static void CreateSkinnedDecals () {
		GameObject l_GameObject = CreateGameObjectInFrontOfSceneViewCamera ("SkinnedDecals", 5.0f);
		l_GameObject.AddComponent <DS_SkinnedDecals> ();
		Selection.activeGameObject = l_GameObject;
	}
	
	private static GameObject CreateGameObjectInFrontOfSceneViewCamera (string a_GameObjectName, float a_UnitsInFrontOfCamera) {
		GameObject l_Result = new GameObject (a_GameObjectName);
		Vector3 l_Position = SceneSupport.UnitsInFrontOfSceneCamera (a_UnitsInFrontOfCamera);
		l_Result.transform.position = l_Position;
		return (l_Result);
	}
}
