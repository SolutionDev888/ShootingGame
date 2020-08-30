//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections;
using NJG;

[CustomEditor(typeof(UIMiniMap))]
public class UIMiniMapInspector : NJG.UIMiniMapInspectorBase
{
	protected override void DrawNotFound()
	{
		UIMiniMap mp = (UIMiniMap)m;
		if (mp != null)
		{
			if (mp.planeRenderer == null)
			{
				mp.planeRenderer = mp.GetComponentInChildren<Renderer>();

				if (mp.planeRenderer == null)
				{
					GUI.backgroundColor = Color.red;
					EditorGUILayout.HelpBox("No UITexture found.", MessageType.Error);
					GUI.backgroundColor = Color.white;

					EditorGUILayout.Separator();
					return;
				}
			}
			else
			{
				if (mp.material == null) mp.material = NJGEditorTools.GetMaterial(m, true);
			}
		}		
	}

	UISprite mOverlay;
	int mBorderOffset;

	protected override void DrawFrameUI()
	{
		UIMiniMap mp = (UIMiniMap)m;
		mOverlay = (UISprite)EditorGUILayout.ObjectField("Frame Overlay", mp.overlay, typeof(UISprite), true);
		if (mp.overlay != mOverlay)
		{
			mp.overlay = mOverlay;
			NJGEditorTools.RegisterUndo("UIMiniMap Frame changed", mp);
			mp.UpdateAlignment();
		}

		if (mp.overlay)
		{
			mBorderOffset = EditorGUILayout.IntField("Frame Offset", mp.overlayBorderOffset);
			if (mp.overlayBorderOffset != mBorderOffset)
			{
				mp.overlayBorderOffset = mBorderOffset;
				NJGEditorTools.RegisterUndo("UIMiniMap Frame Offset", mp);
				mp.UpdateAlignment();
			}
		}
		//if (northIcon != null)
		//	northIconOffset = EditorGUILayout.IntField(new GUIContent("North Icon Offset", "Adjust the north icon distance from map border"), m.northIconOffset);
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		UIMiniMap mp = (UIMiniMap)m;

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
