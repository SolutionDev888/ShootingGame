//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

/// <summary>
/// Editor component used to display a list of keys from cInput cKeys class.
/// </summary>

public class KeySelector : ScriptableWizard
{
	public delegate void Callback(string key);
	public delegate void CallbackKeyCode(KeyCode key);

	string mKey;
	KeyCode mKeyCode;
	Vector2 mPos = Vector2.zero;
	Callback mCallback;
	CallbackKeyCode mCallbackKeyCode;
	float mClickTime = 0f;
	string mSearch = "";
	string[] mods;
	KeyCode[] keyCodeMods;
	string[] mKeys;
	string mTitle = "";

	string[] GetKeys(string match)
	{
		if (string.IsNullOrEmpty(match)) return GetKeys();
		List<string> ks = new List<string>();

		KeyCode[] keys = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToArray();

		foreach (KeyCode k in keys)
		{
			string s = k.ToString();
			if(string.Equals(match, s, StringComparison.OrdinalIgnoreCase)){
				ks.Add(s);
			}
		}

		// No exact match found? Split up the search into space-separated components.
		string[] keywords = match.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < keywords.Length; ++i) keywords[i] = keywords[i].ToLower();

		// Try to find all keys where all keywords are present
		for (int i = 0, imax = keys.Length; i < imax; ++i)
		{
			string s = keys[i].ToString();

			if (s != null && !string.IsNullOrEmpty(s))
			{
				string tl = s.ToLower();
				int matches = 0;

				for (int b = 0; b < keywords.Length; ++b)
				{
					if (tl.Contains(keywords[b])) ++matches;
				}
				if (matches == keywords.Length) if (!ks.Contains(s)) ks.Add(s);
			}
		}

		return ks.ToArray();		
	}

	string[] GetKeys()
	{
		if (mKeys == null)
		{
			KeyCode[] keys = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToArray();
			mKeys = new string[keys.Length];
			for (int i = 0, imax = keys.Length; i < imax; ++i)			
				mKeys[i] = keys[i].ToString();			
		}
		return mKeys;
	}

	KeyCode[] GetKeyCodes(string match)
	{
		if (string.IsNullOrEmpty(match)) return GetKeyCodes();
		List<KeyCode> ks = new List<KeyCode>();

		foreach (KeyCode k in GetKeyCodes())
		{
			string s = k.ToString();
			if (string.Equals(match, s, StringComparison.OrdinalIgnoreCase))
			{
				ks.Add(k);
			}
		}

		// No exact match found? Split up the search into space-separated components.
		string[] keywords = match.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < keywords.Length; ++i) keywords[i] = keywords[i].ToLower();

		// Try to find all keys where all keywords are present
		for (int i = 0, imax = GetKeyCodes().Length; i < imax; ++i)
		{
			KeyCode k = GetKeyCodes()[i];
			string s = k.ToString();

			if (s != null && !string.IsNullOrEmpty(s))
			{
				string tl = s.ToLower();
				int matches = 0;

				for (int b = 0; b < keywords.Length; ++b)
				{
					if (tl.Contains(keywords[b])) ++matches;
				}
				if (matches == keywords.Length) if(!ks.Contains(k)) ks.Add(k);
			}
		}

		return ks.ToArray();
	}

	KeyCode[] GetKeyCodes()
	{
		if (mKeyCodes == null)		
			mKeyCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().ToArray();
		
		return mKeyCodes;
	}

	KeyCode[] mKeyCodes;

	/// <summary>
	/// Show the selection wizard.
	/// </summary>

	public static void Show(string title, string key, string[] modifiers, Callback callback)
	{
		KeySelector comp = ScriptableWizard.DisplayWizard<KeySelector>("Select a Key");
		comp.keyCodeMods = null;
		comp.mTitle = title;
		comp.mKey = key;
		comp.mods = modifiers;
		comp.mCallback = callback;
	}

	public static void Show(string title, KeyCode key, KeyCode[] modifiers, CallbackKeyCode callback)
	{
		KeySelector comp = ScriptableWizard.DisplayWizard<KeySelector>("Select a Key");
		comp.mKey = null;
		comp.mods = null;
		comp.mTitle = title;
		comp.mKeyCode = key;
		comp.keyCodeMods = modifiers;
		comp.mCallbackKeyCode = callback;
	}

	bool IsInUse(string key)
	{
		if (mods != null)
		{
			for (int i = 0, imax = mods.Length; i < imax; i++)
				if (mods[i] == key) return true;
		}
		
		return false;
	}

	bool IsInUse(KeyCode key)
	{
		if (keyCodeMods != null)
		{
			for (int i = 0, imax = keyCodeMods.Length; i < imax; i++)
				if (keyCodeMods[i] == key) return true;
		}

		return false;
	}

	/// <summary>
	/// Draw the custom wizard.
	/// </summary>

	void OnGUI()
	{
#if UNITY_4_3
		EditorGUIUtility.LookLikeControls(80f);
#else
		EditorGUIUtility.labelWidth = 80f;
#endif

		bool close = false;
		GUILayout.Label(string.IsNullOrEmpty(mTitle) ? "Choose a Key" : mTitle, "LODLevelNotifyText");
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

		GUIStyle centered = GUI.skin.GetStyle("Label");
		centered.alignment = TextAnchor.UpperCenter;

		GUILayout.Label("Keys in use are disabled and marked as red", centered);

		centered.alignment = TextAnchor.UpperLeft;
			
		float w = 120f;
		float wPad = w + 5f;
		float h = 30f;
		float hPad = h + 5f;
		int columns = Mathf.FloorToInt(Screen.width / wPad);
		if (columns < 1) columns = 1;

		int offset = 0;
		Rect rect = new Rect(10f, 0, w, h);

		GUILayout.Space(10f);
		mPos = GUILayout.BeginScrollView(mPos);

		if (keyCodeMods != null)
		{
			KeyCode[] keyCodes = GetKeyCodes(mSearch);

			while (offset < keyCodes.Length)
			{
				GUILayout.BeginHorizontal();
				{
					int col = 0;
					rect.x = 10f;

					for (; offset < keyCodes.Length; ++offset)
					{
						KeyCode key = keyCodes[offset];
						bool isInUse = IsInUse(key);

						// Button comes first
						GUI.enabled = !isInUse && key != KeyCode.None;
						if (key == mKeyCode) GUI.enabled = true;
						if (GUI.Button(rect, key.ToString()))
						{

							float delta = Time.realtimeSinceStartup - mClickTime;
							mClickTime = Time.realtimeSinceStartup;

							if (mKeyCode != key)
							{
								mKeyCode = key;
								if (mCallbackKeyCode != null)
								{
									mCallbackKeyCode(mKeyCode);
								}
							}
							else if (delta < 0.5f) close = true;
						}

						if (Event.current.type == EventType.Repaint)
						{
							// Draw the selection
							if (key == mKeyCode)							
								NJGEditorTools.DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));							
							else if (isInUse && key != KeyCode.None)							
								NJGEditorTools.DrawOutline(rect, Color.red);							
						}

						GUI.enabled = true;

						if (++col >= columns)
						{
							++offset;
							break;
						}
						rect.x += wPad;
					}
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(hPad);
				rect.y += hPad;
			}
		}
		else
		{

			string[] keys = GetKeys(mSearch);

			while (offset < keys.Length)
			{
				GUILayout.BeginHorizontal();
				{
					int col = 0;
					rect.x = 10f;

					for (; offset < keys.Length; ++offset)
					{
						string key = keys[offset];
						bool isInUse = IsInUse(key);

						// Button comes first
						GUI.enabled = !isInUse && key != "None";
						if (GUI.Button(rect, key))
						{

							float delta = Time.realtimeSinceStartup - mClickTime;
							mClickTime = Time.realtimeSinceStartup;

							if (mKey != key)
							{
								mKey = key;
								if (mCallback != null)
								{
									mCallback(mKey);
								}
							}
							else if (delta < 0.5f) close = true;
						}

						if (Event.current.type == EventType.Repaint)
						{
							// Draw the selection
							if (key == mKey)
							{
								NJGEditorTools.DrawOutline(rect, new Color(0.4f, 1f, 0f, 1f));
							}

							if (isInUse && key != "None")
							{
								NJGEditorTools.DrawOutline(rect, Color.red);
							}
						}

						GUI.enabled = true;

						if (++col >= columns)
						{
							++offset;
							break;
						}
						rect.x += wPad;
					}
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(hPad);
				rect.y += hPad;
			}
		}
		GUILayout.EndScrollView();
		if (close) Close();		
	}
}
