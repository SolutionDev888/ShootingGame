//
// Author:
//   Andreas Suter (andy@edelweissinteractive.com)
//
// Copyright (C) 2012 Edelweiss Interactive (http://edelweissinteractive.com)
//

using UnityEngine;
using UnityEditor;
using System.Collections;
using Edelweiss.DecalSystem;
using Edelweiss.DecalSystemEditor;

[CustomEditor (typeof (DS_DecalProjectorGroup))]
public class DS_DecalProjectorGroupEditor : DecalProjectorGroupEditor {
	
	public override void OpenUVRectangleWindow () {
		EditorWindow.GetWindow (typeof (DS_UVRectangleEditorWindow));
	}
	
	public override void AddProjectorToGameObject (GameObject a_GameObject) {
		a_GameObject.AddComponent <DS_DecalProjector> ();
	}
	
	public override void AddProjectorGroupToGameObject (GameObject a_GameObject) {
		a_GameObject.AddComponent <DS_DecalProjectorGroup> ();
	}
}
