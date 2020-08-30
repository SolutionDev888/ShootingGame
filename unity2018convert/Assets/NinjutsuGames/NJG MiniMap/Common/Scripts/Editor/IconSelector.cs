///----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Editor component used to display a list of sprites.
/// </summary>

public class IconSelector : ScriptableWizard
{
	public delegate void Callback(string spriteName);

	string mSprite;
	string mSearch = "";
	Vector2 mPos = Vector2.zero;
	Callback mCallback;
	float mClickTime = 0f;
	NJGAtlas mAtlas;
	static Texture2D[] icons;

	/// <summary>
	/// Name of the selected sprite.
	/// </summary>

	//public string spriteName { get { return (mSprite != null) ? mSprite.spriteName : mName; } }

	/// <summary>
	/// Show the selection wizard.
	/// </summary>

	public static void Show(NJGAtlas atlas, string selected, Callback callback)
	{
		IconSelector comp = ScriptableWizard.DisplayWizard<IconSelector>("Select a Sprite");
		comp.mAtlas = atlas;
		comp.mSprite = selected;
		comp.mCallback = callback;		
	}

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI ()
	{
#if UNITY_4_3
		EditorGUIUtility.LookLikeControls(80f);
#else
		EditorGUIUtility.labelWidth = 80f;
#endif

		bool close = false;
		GUILayout.Label("Icons", "LODLevelNotifyText");
		NJGEditorTools.DrawSeparator();

		GUILayout.BeginHorizontal();
		GUILayout.Space(84f);

		string before = mSearch;
		string after = EditorGUILayout.TextField("", before, "SearchTextField");
		mSearch = after;

		if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
		{
			mSearch = "";
			GUIUtility.keyboardControl = 0;
		}
		GUILayout.Space(84f);
		GUILayout.EndHorizontal();

		Texture2D tex = mAtlas.texture as Texture2D;

		if (tex == null)
		{
			GUILayout.Label("The atlas doesn't have a texture to work with");
			return;
		}

		List<string> sprites = mAtlas.GetListOfSprites(mSearch);
			
		float size = 80f;
		float padded = size + 10f;
		int columns = Mathf.FloorToInt(Screen.width / padded);
		if (columns < 1) columns = 1;

		int offset = 0;
		Rect rect = new Rect(10f, 0, size, size);

		GUILayout.Space(10f);
		mPos = GUILayout.BeginScrollView(mPos);

		while (offset < sprites.Count)
		{
			GUILayout.BeginHorizontal();
			{
				int col = 0;
				rect.x = 10f;

				for (; offset < sprites.Count; ++offset)
				{
					NJGAtlas.Sprite sprite = mAtlas.GetSprite(sprites[offset]);
					if (sprite == null) continue;

					// Button comes first
					if (GUI.Button(rect, ""))
					{
						float delta = Time.realtimeSinceStartup - mClickTime;
						mClickTime = Time.realtimeSinceStartup;

						if (mSprite != sprite.name)
						{
							mSprite = sprite.name;
							if (mCallback != null)
							{
								mCallback(mSprite);
							}
						}
						else if (delta < 0.5f) close = true;
					}
						
					if (Event.current.type == EventType.Repaint)
					{
						// On top of the button we have a checkboard grid
						NJGEditorTools.DrawTiledTexture(rect, NJGEditorTools.backdropTexture);
	
						// Calculate the texture's scale that's needed to display the sprite in the clipped area
						float scaleX = rect.width / sprite.width;
						float scaleY = rect.height / sprite.height;
	
						// Stretch the sprite so that it will appear proper
						float aspect = (scaleY / scaleX) / ((float)sprite.height / sprite.width);
						Rect clipRect = rect;
	
						if (aspect != 1f)
						{
							if (aspect < 1f)
							{
								// The sprite is taller than it is wider
								float padding = size * (1f - aspect) * 0.5f;
								clipRect.xMin += padding;
								clipRect.xMax -= padding;
							}
							else
							{
								// The sprite is wider than it is taller
								float padding = size * (1f - 1f / aspect) * 0.5f;
								clipRect.yMin += padding;
								clipRect.yMax -= padding;
							}
						}

						GUI.DrawTextureWithTexCoords(clipRect, tex, sprite.uvs);
	
						// Draw the selection
						if (mSprite == sprite.name)
						{
							NJGEditorTools.DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));
						}
					}

					if (++col >= columns)
					{
						++offset;
						break;
					}
					rect.x += padded;
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(padded);
			rect.y += padded;
		}
		GUILayout.EndScrollView();
		if (close) Close();		
	}
}
