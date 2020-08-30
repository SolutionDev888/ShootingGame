//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NJG;

[CustomEditor(typeof(NJGMap))]
public class NJGMapInspector : NJG.NJGMapInspectorBase
{
	static NJGMap m;

	public override void OnInspectorGUI()
	{
		m = target as NJGMap;
		base.OnInspectorGUI();
	}

	protected override void DrawEditButtons()
	{
		if (UIMiniMap.instance != null)
		{
			GUI.backgroundColor = UIMiniMap.instance != null ? Color.cyan : Color.red;
			GUI.enabled = UIMiniMap.instance != null;
			if (GUILayout.Button(new GUIContent(UIMiniMap.instance != null ? "Edit Mini Map" : "Mini Map not found", "Click to edit the Mini Map")))
			{
				if (UIMiniMap.instance != null)
				{
					Selection.activeGameObject = UIMiniMap.instance.gameObject;
				}
				else
				{
					//NJGEditorTools.CreateMap(true);
				}
			}
		}

		if (UIWorldMap.instance != null)
		{
			GUI.backgroundColor = UIWorldMap.instance != null ? Color.green : Color.red;

			if (GUILayout.Button(new GUIContent(UIWorldMap.instance != null ? "Edit World Map" : "World Map not found", "Click to edit the World Map")))
			{
				if (UIWorldMap.instance != null)
				{
					Selection.activeGameObject = UIWorldMap.instance.gameObject;
				}
				else
				{
					//NJGEditorTools.CreateMap(false);
				}
			}
			GUI.enabled = true;
		}
	}

	protected override void DrawMapNotFound()
	{
		if (UIMiniMap.instance == null && UIWorldMap.instance == null)
		{
			GUI.backgroundColor = Color.red;
			EditorGUILayout.HelpBox("Could not found any UIMiniMap or UIWorldMap instance.", MessageType.Error);
			GUI.backgroundColor = Color.white;
		}
	}

	protected override void DrawComponentSelector()
	{
		ComponentSelector.Draw<UIAtlas>(m.atlas, OnSelectAtlas, true);
		if (m.atlas == null)
		{
			EditorGUILayout.HelpBox("You need to select an atlas first", MessageType.Warning);
		}
	}

    /*protected override void ChangeLayer(int layer)
    {
        if (m.uiCam == null) m.uiCam = m.GetComponentInChildren<UICamera>();
        if (m.uiCam != null) if (m.uiCam.eventReceiverMask != 1 << m.gameObject.layer) m.uiCam.eventReceiverMask = 1 << m.gameObject.layer;

    }*/

	protected override void DrawIconSpriteUI(NJGMapBase.MapItemType mapItem)
	{
		if (m.atlas != null)
		{
			if (string.IsNullOrEmpty(mSpriteName)) mSpriteName = m.atlas.spriteList[0].name;
			string spr = string.IsNullOrEmpty(mapItem.sprite) ? mSpriteName : mapItem.sprite;

			SpriteField("Icon Sprite", m.atlas, spr, delegate(string sp)
			{
				mapItem.OnSelectSprite(sp);
				Save(true);
			});

			EditorGUILayout.Separator();

			//if (string.IsNullOrEmpty(mSelSpriteName)) mSelSpriteName = m.atlas.spriteList[0].name;
			string spr2 = mapItem.selectedSprite;

			SpriteField("Selection Sprite", m.atlas, spr2, delegate(string sp)
			{
				mapItem.OnSelectBorderSprite(sp);
				Save(true);
			});

			float extraSpace = 0;

			// Draw sprite preview.					
			Material mat = m.atlas.spriteMaterial;

			if (mat != null)
			{
				Texture2D tex = mat.mainTexture as Texture2D;

				if (tex != null)
				{
					UISpriteData mSprite = m.atlas.GetSprite(spr);
					UISpriteData mSSprite = m.atlas.GetSprite(spr2);

					if (mSprite != null)
					{
						Rect rect = new Rect(mSprite.x, mSprite.y, mSprite.width, mSprite.height);
						rect = NGUIMath.ConvertToTexCoords(rect, tex.width, tex.height);

						int size = mapItem.useCustomSize ? mapItem.size : m.iconSize;
						Rect rect2 = new Rect();

						if (mSSprite != null)
						{
							rect2 = new Rect(mSSprite.x, mSSprite.y, mSSprite.width, mSSprite.height);
							rect2 = NGUIMath.ConvertToTexCoords(rect2, tex.width, tex.height);
						}

						int size2 = mapItem.useCustomBorderSize ? mapItem.borderSize : m.borderSize;

						int s = Mathf.Max(size, size2);

						GUILayout.Space(4f);
						GUILayout.BeginHorizontal();
						{
							GUILayout.Space((Screen.width - 220) - size);
							GUI.color = mapItem.color;
							DrawSprite(tex, rect, null, false, size, new Vector2(0, 18));

							if (mSSprite != null && rect2.width != 0)
							{
								//int sps = size / 4;
								//Debug.Log("Border "+rect2+" / "+size2+" / "+sps);
								DrawSprite(tex, rect2, null, false, size2, new Vector2(0, 18)); //new Vector2(sps, 18 + sps)
							}
							GUI.color = Color.white;
						}
						GUILayout.EndHorizontal();

						extraSpace = s * (float)mSprite.height / mSprite.width;
					}
				}

				extraSpace = Mathf.Max(0f, extraSpace - 10f);
				GUILayout.Space(extraSpace);
			}
		}
	}

	protected override void DrawArrowSpriteUI(NJGMapBase.MapItemType mapItem)
	{
		if (m.atlas != null && mapItem.haveArrow)
		{
			GUILayout.BeginVertical("Box");

			if (string.IsNullOrEmpty(mSpriteName)) mSpriteName = m.atlas.spriteList[0].name;
			string spr = string.IsNullOrEmpty(mapItem.arrowSprite) ? mSpriteName : mapItem.arrowSprite;

			mapItem.arrowOffset = EditorGUILayout.IntField("Arrow Offset", mapItem.arrowOffset);
			mapItem.arrowRotate = EditorGUILayout.Toggle("Arrow Rotate", mapItem.arrowRotate);

			SpriteField("Arrow Sprite", m.atlas, spr, delegate(string sp)
			{
				mapItem.OnSelectArrowSprite(sp);
				Save(true);
			});

			float extraSpace = 0;

			// Draw sprite preview.					
			Material mat = m.atlas.spriteMaterial;

			if (mat != null)
			{
				Texture2D tex = mat.mainTexture as Texture2D;

				if (tex != null)
				{
					UISpriteData mSprite = m.atlas.GetSprite(spr);

					if (mSprite != null)
					{
						Rect rect = new Rect(mSprite.x, mSprite.y, mSprite.width, mSprite.height);
						rect = NGUIMath.ConvertToTexCoords(rect, tex.width, tex.height);

						GUILayout.Space(4f);
						GUILayout.BeginHorizontal();
						{
							GUILayout.Space((Screen.width - 220) - m.arrowSize);
							GUI.color = mapItem.color;
							DrawSprite(tex, rect, null, false, m.arrowSize, new Vector2(0, 18));
							GUI.color = Color.white;
						}
						GUILayout.EndHorizontal();

						extraSpace = m.arrowSize * (float)mSprite.height / mSprite.width;
					}
				}

				extraSpace = Mathf.Max(0f, extraSpace - 10f);
				GUILayout.Space(extraSpace);
			}
			// Depth

			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Arrow Depth");

				int depth = mapItem.arrowDepth;
				if (GUILayout.Button("Back", GUILayout.Width(60f))) --depth;
				depth = EditorGUILayout.IntField(depth);
				if (GUILayout.Button("Forward", GUILayout.Width(60f))) ++depth;

				if (mapItem.arrowDepth != depth)
				{
					mapItem.arrowDepth = depth;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
	}

	#region Helper Methods
	void OnSelectFont(UnityEngine.Object obj)
	{
		NGUISettings.ambigiousFont = obj as UIFont;
		Repaint();
	}

	/// <summary>
	/// Save selected atlas.
	/// </summary>

	void OnSelectAtlas(UnityEngine.Object obj)
	{
		m.atlas = obj as UIAtlas;

		// Automatically choose the first sprite
		if (string.IsNullOrEmpty(mSpriteName))
		{
			if (m.atlas != null && m.atlas.spriteList.Count > 0)
			{
				SetAtlasSprite(m.atlas.spriteList[0]);
				mSpriteName = m.defaultSprite.name;
			}
		}
	}

	/// <summary>
	/// Set the atlas sprite directly.
	/// </summary>

	protected void SetAtlasSprite(UISpriteData sp)
	{
		if (sp != null)
		{
			m.defaultSprite = sp;
			mSpriteName = sp.name;
		}
		else
		{
			mSpriteName = (m.defaultSprite != null) ? m.defaultSprite.name : "";
			m.defaultSprite = sp;
		}
	}
	#endregion

	#region Sprite Field
	/// <summary>
	/// Draw a sprite selection field.
	/// </summary>

	static public void SpriteField(string fieldName, UIAtlas atlas, string spriteName,
		SpriteSelector.Callback callback, params GUILayoutOption[] options)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(fieldName, GUILayout.Width(76f));

		if (GUILayout.Button(spriteName, "MiniPullDown", options))
		{
			NGUISettings.atlas = atlas;
			NGUISettings.selectedSprite = spriteName;
			SpriteSelector.Show(callback);
		}
		GUILayout.EndHorizontal();
	}

	/// <summary>
	/// Draw a sprite selection field.
	/// </summary>

	static public void SpriteField(string fieldName, UIAtlas atlas, string spriteName, SpriteSelector.Callback callback)
	{
		SpriteField(fieldName, null, atlas, spriteName, callback);
	}

	/// <summary>
	/// Draw a sprite selection field.
	/// </summary>

	static public void SpriteField(string fieldName, string caption, UIAtlas atlas, string spriteName, SpriteSelector.Callback callback)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label(fieldName, GUILayout.Width(116f));

		if (atlas.GetSprite(spriteName) == null)
			spriteName = "";

		if (GUILayout.Button(spriteName, "MiniPullDown", GUILayout.Width(120f)))
		{
			NGUISettings.atlas = atlas;
			NGUISettings.selectedSprite = spriteName;
			SpriteSelector.Show(callback);
		}

		if (!string.IsNullOrEmpty(caption))
		{
			GUILayout.Space(20f);
			GUILayout.Label(caption);
		}

		GUILayout.EndHorizontal();
		GUILayout.Space(-4f);

	}
	#endregion
}
