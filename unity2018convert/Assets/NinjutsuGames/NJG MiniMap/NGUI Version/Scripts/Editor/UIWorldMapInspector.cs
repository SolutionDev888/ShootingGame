//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections;
using NJG;

[CustomEditor(typeof(UIWorldMap))]
public class UIWorldMapInspector : NJG.UIWorldMapInspectorBase 
{
	protected override void DrawNotFound()
	{
		UIWorldMap mp = (UIWorldMap)m;
		if (mp.planeRenderer == null)
		{
			mp.planeRenderer = mp.GetComponentInChildren<Renderer>();

			if (mp.planeRenderer == null)
			{
				GUI.backgroundColor = Color.red;
				EditorGUILayout.HelpBox("No Renderer found.", MessageType.Error);
				GUI.backgroundColor = Color.white;

				/*if (GUILayout.Button("Create UITexture"))
				{
					NJGEditorTools.CreateUIMapTexture(m);
				}*/
				EditorGUILayout.Separator();
				return;
			}
		}
		else
		{
			if (mp.material == null) mp.material = NJGEditorTools.GetMaterial(m, true);
		}
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		UIWorldMap mp = (UIWorldMap)m;

		Shader s = Shader.Find("NinjutsuGames/Map TextureMask");
		switch (mp.shaderType)
		{
			case NJGMapBase.ShaderType.ColorMask:
				s = Shader.Find("NinjutsuGames/Map ColorMask");
				m.material.SetColor("_MaskColor", NJGMapBase.instance.cameraBackgroundColor);
				break;
			case NJGMapBase.ShaderType.FOW:
				s = Shader.Find("NinjutsuGames/Map FOW");
				break;
		}

		if (mp.material.shader != s)
		{
			mp.material.shader = s;
		}

		if (GUI.changed)
			EditorUtility.SetDirty(mp);
	}
}
