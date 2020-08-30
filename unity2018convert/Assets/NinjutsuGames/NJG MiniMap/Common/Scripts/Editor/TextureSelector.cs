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

public class TextureSelector : ScriptableWizard
{
	public delegate void Callback(Texture2D texture);

	Texture2D mTex;
	string mSearch = "";
	Vector2 mPos = Vector2.zero;
	Callback mCallback;
	float mClickTime = 0f;
	static Texture2D[] icons;

	/// <summary>
	/// Name of the selected sprite.
	/// </summary>

	//public string spriteName { get { return (mSprite != null) ? mSprite.spriteName : mName; } }

	/// <summary>
	/// Show the selection wizard.
	/// </summary>

	public static void Show (Texture2D selectedIcon, Callback callback)
	{
		TextureSelector comp = ScriptableWizard.DisplayWizard<TextureSelector>("Select a Sprite");
		comp.mTex = selectedIcon;
		comp.mCallback = callback;

		
	}

	Texture2D[] GetIcons()
	{
		Object[] ics = Resources.LoadAll("Icons");
		icons = new Texture2D[ics.Length];
		int i = 0;
		foreach (UnityEngine.Object o in ics)
		{
			icons[i] = (Texture2D)o;
			i++;
		}
		return icons;
	}

	Texture2D[] GetIcons(string match)
	{
		if (string.IsNullOrEmpty(match)) return GetIcons();
		List<Texture2D> ks = new List<Texture2D>();

		Texture2D[] tex = GetIcons();

		foreach (Texture2D k in tex)
		{
			string s = k.ToString();
			if (string.Equals(match, s, System.StringComparison.OrdinalIgnoreCase))
			{
				ks.Add(k);
			}
		}

		// No exact match found? Split up the search into space-separated components.
		string[] keywords = match.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < keywords.Length; ++i) keywords[i] = keywords[i].ToLower();

		// Try to find all keys where all keywords are present
		for (int i = 0, imax = tex.Length; i < imax; ++i)
		{
			string s = tex[i].ToString();

			if (s != null && !string.IsNullOrEmpty(s))
			{
				string tl = s.ToLower();
				int matches = 0;

				for (int b = 0; b < keywords.Length; ++b)
				{
					if (tl.Contains(keywords[b])) ++matches;
				}
				if (matches == keywords.Length) if (!ks.Contains(tex[i])) ks.Add(tex[i]);
			}
		}

		return ks.ToArray();
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

		Texture2D[] sprites = GetIcons(mSearch);
			
		float size = 80f;
		float padded = size + 10f;
		int columns = Mathf.FloorToInt(Screen.width / padded);
		if (columns < 1) columns = 1;

		int offset = 0;
		Rect rect = new Rect(10f, 0, size, size);

		GUILayout.Space(10f);
		mPos = GUILayout.BeginScrollView(mPos);

		while (offset < sprites.Length)
		{
			GUILayout.BeginHorizontal();
			{
				int col = 0;
				rect.x = 10f;

				for (; offset < sprites.Length; ++offset)
				{
					Texture2D sprite = sprites[offset];

					// Button comes first
					if (GUI.Button(rect, ""))
					{
						float delta = Time.realtimeSinceStartup - mClickTime;
						mClickTime = Time.realtimeSinceStartup;

						if (mTex != sprite)
						{
							mTex = sprite;
							if (mCallback != null)
							{
								mCallback(sprite);
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

						GUI.DrawTexture(clipRect, sprite);
	
						// Draw the selection
						if (mTex == sprite)
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
