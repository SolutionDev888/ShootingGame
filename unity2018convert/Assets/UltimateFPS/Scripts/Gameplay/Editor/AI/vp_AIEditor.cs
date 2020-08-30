/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIEditor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This is the main editor class for the AI and displays all
//					the plugins using vp_AIAttributes
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[CustomEditor(typeof(vp_AI))]
public class vp_AIEditor : Editor
{
	
	public static long lastUpdateTick;
	
	protected static vp_AI m_AI = null;							// Main AI Component
	protected List<FieldInfo> Plugins = new List<FieldInfo>();	// List of plugins from m_Component
	
			
	protected static GUIStyle m_FoldoutStyle = null;
	public static GUIStyle FoldoutStyle
	{
		get
		{
			if (m_FoldoutStyle == null)
			{
				m_FoldoutStyle = new GUIStyle("Foldout");
				m_FoldoutStyle.fontSize = 14;
			}
			return m_FoldoutStyle;
		}
	}
	
	
	protected static GUIStyle m_SubFoldoutStyle = null;
	public static GUIStyle SubFoldoutStyle
	{
		get
		{
			if (m_SubFoldoutStyle == null)
			{
				m_SubFoldoutStyle = new GUIStyle("Foldout");
				m_SubFoldoutStyle.fontSize = 11;
				m_SubFoldoutStyle.fontStyle = FontStyle.Bold;
			}
			return m_SubFoldoutStyle;
		}
	}
	
	
	protected virtual void OnEnable()
	{
	
		m_AI = (vp_AI)target;
		
		if(m_AI != null)
			if(m_AI.DefaultState == null)
				m_AI.RefreshDefaultState();
				
		Plugins = m_AI.GetType().GetFields().Where(info => info.FieldType.BaseType == typeof(vp_AIPlugin)).OrderBy(info => ((vp_AIPlugin)info.GetValue(m_AI)).SortOrder).ToList();
		
	}
	
	
	public override void OnInspectorGUI()
	{
	
		Undo.SetSnapshotTarget(m_AI, "vp_AI Snapshot");
    	Undo.CreateSnapshot();
		
		DrawSeparator(2);
		
		StatesAndPresets();
			
		foreach(FieldInfo plugin in Plugins)
			DisplayPlugin(plugin);
		
		if (GUI.changed)
		{
		
			EditorUtility.SetDirty(target);
			Undo.RegisterSnapshot();

		}
	
	}
	
	
	/// <summary>
	/// Displays a plugin from it's FieldInfo
	/// which is automatically obtained from vp_AI
	/// </summary>
	public void DisplayPlugin( FieldInfo plugin )
	{
	
		int foldoutCount = 0;
		int foldoutsOpen = 0;
		if(!BeginPlugin(ref ((vp_AIPlugin)plugin.GetValue(m_AI)).MainFoldout, ref ((vp_AIPlugin)plugin.GetValue(m_AI)).Enabled, ((vp_AIPlugin)plugin.GetValue(m_AI)).Title, plugin.Name, this.GetType()))
			return;
			
		List<FieldInfo> fields = m_AI.GetType().GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).Where(fi => fi.Name.IndexOf(plugin.Name) != -1).ToList();
			
		foreach(FieldInfo info in fields)
		{
			bool end = false;	
			
			// position any vp_AIEndFoldout at the end of the attributes list
			object[] tempAttr = info.GetCustomAttributes(typeof(vp_AIField), true);
			List<object> attributes = new List<object>();
			object endFoldout = null;
			for (int i = 0; i < tempAttr.Length; i ++)
			{
				if(tempAttr[i].GetType() == typeof(vp_AIEndFoldout))
					endFoldout = tempAttr[i];
				else
					attributes.Add(tempAttr[i]);
			}
			if(endFoldout != null)
				attributes.Add(endFoldout);
				
			// loop through the attributes
			for (int i = 0; i < attributes.Count; i ++)
			{
				vp_AIField attribute = (vp_AIField)attributes[i];
			
				// vp_AIBeginFoldout
				if(attribute.GetType() == typeof(vp_AIBeginFoldout))
				{
					foldoutCount++;
					if(SetFoldoutValue( info, attribute ))
						foldoutsOpen++;
						
					if(foldoutsOpen == foldoutCount)
						EditorGUI.indentLevel++;
				}
				
				// vp_AIEndFoldout
				if(attribute.GetType() == typeof(vp_AIEndFoldout))
					end = true;
					
				if(foldoutsOpen != foldoutCount && !end)
					continue;
						
				// vp_AICustomMethod Attribute
				if(attribute.GetType() == typeof(vp_AICustomMethod))
					CustomMethod( plugin, attribute );
				
				// vp_AIField Attribute
				if(attribute.GetType() == typeof(vp_AIField))
				{
					if(info.FieldType.BaseType == typeof(Component) || info.FieldType.BaseType == typeof(UnityEngine.Object))
						SetObjectValue( info, attribute, true );
					else
						SetStandardValue( info, attribute );
				}
				
				// vp_AIAnimationStateField Attribute
				if(attribute.GetType() == typeof(vp_AIAnimationStateField))
					info.SetValue(m_AI, GetAnimationState(attribute.Title, (vp_AIState)info.GetValue(m_AI), attribute.Tooltip));
				
				// vp_AIAudioStateField Attribute
				if(attribute.GetType() == typeof(vp_AIAudioStateField))
					info.SetValue(m_AI, GetAudioState(attribute.Title, (vp_AIState)info.GetValue(m_AI), attribute.Tooltip));
				
				// vp_AIRangeField
				if(attribute.GetType() == typeof(vp_AIRangeField))
					RangeField( info, attribute );
				
				if(end)
				{
					if(foldoutsOpen == foldoutCount)
					{
						GUILayout.Space(10);
						EditorGUI.indentLevel--;
					}
						
					foldoutCount--;
					if(foldoutCount == 0)
						foldoutsOpen = 0;
					else
						foldoutsOpen--;
				}
				
			}
		}
		
		EndPlugin();

	}
	
	
	/// <summary>
	/// Displays a range field
	/// </summary>
	public static void RangeField( FieldInfo info, vp_AIField attribute )
	{
	
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		float min = ((Vector2)info.GetValue(m_AI)).x;
		float max = ((Vector2)info.GetValue(m_AI)).y;
		
		min = EditorGUILayout.FloatField(min,GUILayout.MaxWidth(100));
		EditorGUILayout.MinMaxSlider(ref min, ref max, attribute.MinValue, attribute.MaxValue);
		max = EditorGUILayout.FloatField(max,GUILayout.MaxWidth(100));
		
		info.SetValue(m_AI, new Vector2((float)System.Math.Round(min, 2), (float)System.Math.Round(max, 2)));
		GUILayout.EndHorizontal();
	
	}
	
	
	/// <summary>
	/// Handles custom method execution
	/// </summary>
	public virtual void CustomMethod( FieldInfo plugin, vp_AIField attribute )
	{
	
		List<Type> types = (from System.Type t in Assembly.GetExecutingAssembly().GetTypes() where t.IsSubclassOf(typeof(vp_AIEditor)) select t).ToList();
		types.Add(this.GetType());
		Type type = this.GetType();
		string className = ((vp_AICustomMethod)attribute).ClassName != "" ? ((vp_AICustomMethod)attribute).ClassName : "vp_AI"+plugin.Name+"Editor";
		Type tempType = types.FirstOrDefault(t => t.ToString() == className);
		if(tempType != null)
			type = tempType;
		
		MethodInfo methodInfo = type.GetMethod(((vp_AICustomMethod)attribute).MethodName);
		if(methodInfo != null)
			methodInfo.Invoke(this, null);
	
	}
	
	
	/// <summary>
	/// Handles all the standard types
	/// </summary>
	public static void SetStandardValue( FieldInfo info, vp_AIField attribute )
	{
	
		// Integer
		if(info.FieldType == typeof(int))
			if(attribute.Title == "" || attribute.Title == null)
				if(attribute.Slider)
					info.SetValue(m_AI, EditorGUILayout.IntSlider((int)info.GetValue(m_AI), (int)attribute.MinValue, (int)attribute.MaxValue));
				else
					info.SetValue(m_AI, EditorGUILayout.IntField((int)info.GetValue(m_AI)));
			else
				if(attribute.Slider)
					info.SetValue(m_AI, EditorGUILayout.IntSlider(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip), (int)info.GetValue(m_AI), (int)attribute.MinValue, (int)attribute.MaxValue));
				else
					info.SetValue(m_AI, EditorGUILayout.IntField(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip), (int)info.GetValue(m_AI)));
		
		// Float			
		if(info.FieldType == typeof(float))
			if(attribute.Title == "" || attribute.Title == null)
				if(attribute.Slider)
					info.SetValue(m_AI, EditorGUILayout.Slider((float)info.GetValue(m_AI), attribute.MinValue, attribute.MaxValue));
				else
					info.SetValue(m_AI, EditorGUILayout.FloatField((float)info.GetValue(m_AI)));
			else
				if(attribute.Slider)
					info.SetValue(m_AI, EditorGUILayout.Slider(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip), (float)info.GetValue(m_AI), attribute.MinValue, attribute.MaxValue));
				else
					info.SetValue(m_AI, EditorGUILayout.FloatField(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip), (float)info.GetValue(m_AI)));
				
		// Bool
		if(info.FieldType == typeof(bool))
			if(attribute.Title == "" || attribute.Title == null)
				info.SetValue(m_AI, EditorGUILayout.Toggle((bool)info.GetValue(m_AI)));
			else
				info.SetValue(m_AI, EditorGUILayout.Toggle(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip), (bool)info.GetValue(m_AI)));
				
		// String
		if(info.FieldType == typeof(string))
			if(attribute.Title == "" || attribute.Title == null)
				info.SetValue(m_AI, EditorGUILayout.TextField((string)info.GetValue(m_AI)));
			else
				info.SetValue(m_AI, EditorGUILayout.TextField(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip), (string)info.GetValue(m_AI)));
				
		// Vector2
		if(info.FieldType == typeof(Vector2))
			if(attribute.Title == "" || attribute.Title == null)
				info.SetValue(m_AI, EditorGUILayout.Vector2Field(info.Name, (Vector2)info.GetValue(m_AI)));
			else
			{
				EditorGUILayout.LabelField(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip));
				GUILayout.Space(-20);
				info.SetValue(m_AI, EditorGUILayout.Vector2Field("", (Vector2)info.GetValue(m_AI)));
			}
				
		// Vector3
		if(info.FieldType == typeof(Vector3))
			if(attribute.Title == "" || attribute.Title == null)
				info.SetValue(m_AI, EditorGUILayout.Vector3Field(info.Name, (Vector3)info.GetValue(m_AI)));
			else
			{
				EditorGUILayout.LabelField(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip));
				GUILayout.Space(-20);
				info.SetValue(m_AI, EditorGUILayout.Vector3Field("", (Vector3)info.GetValue(m_AI)));
			}
				
		// Vector4
		if(info.FieldType == typeof(Vector4))
			if(attribute.Title == "" || attribute.Title == null)
				info.SetValue(m_AI, EditorGUILayout.Vector4Field(info.Name, (Vector4)info.GetValue(m_AI)));
			else
			{
				EditorGUILayout.LabelField(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip));
				GUILayout.Space(-20);
				info.SetValue(m_AI, EditorGUILayout.Vector4Field("", (Vector4)info.GetValue(m_AI)));
			}
				
		// Enum
		if(info.FieldType.IsEnum)
			if(attribute.Title == "" || attribute.Title == null)
				info.SetValue(m_AI, EditorGUILayout.EnumPopup((Enum)info.GetValue(m_AI)));
			else
				info.SetValue(m_AI, EditorGUILayout.EnumPopup(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip), (Enum)info.GetValue(m_AI)));
				
		// LayerMask
		if(info.FieldType == typeof(LayerMask))
			info.SetValue(m_AI, LayerMaskField(attribute.Title, (LayerMask)info.GetValue(m_AI), attribute.Title+"\n\n"+attribute.Tooltip));
	
	}
	
	
	/// <summary>
	/// Handles UnityEngine.Objects
	/// </summary>
	public static void SetObjectValue( FieldInfo info, vp_AIField attribute, bool allowSceneObjects )
	{
	
		if(attribute.Title == "" || attribute.Title == null)
			info.SetValue(m_AI, EditorGUILayout.ObjectField((UnityEngine.Object)info.GetValue(m_AI), info.FieldType, true) );
		else
			info.SetValue(m_AI, EditorGUILayout.ObjectField(new GUIContent(attribute.Title, attribute.Title+"\n\n"+attribute.Tooltip), (UnityEngine.Object)info.GetValue(m_AI), info.FieldType, true) );
	
	}
	
	
	/// <summary>
	/// Handles foldouts from vp_AIBeginFoldout
	/// </summary>
	public static bool SetFoldoutValue( FieldInfo info, vp_AIField attribute )
	{
		
		EditorGUILayout.Space();
		bool foldout = EditorGUI.Foldout(new Rect(GUILayoutUtility.GetLastRect().xMin+5, GUILayoutUtility.GetLastRect().yMin, 225, 20), (bool)info.GetValue(m_AI), " "+attribute.Title, true, (bool)info.GetValue(m_AI) ? SubFoldoutStyle : new GUIStyle("Foldout"));
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		info.SetValue(m_AI, foldout);
		return foldout;
	
	}
	
	
	/// <summary>
	/// creates a big button toggle
	/// </summary>
	public static bool ButtonToggle(string labelOn, string labelOff, bool state, string tooltip = "")
	{

		GUIStyle onStyle = new GUIStyle("Button");
		GUIStyle offStyle = new GUIStyle("Button");

		onStyle.fontSize = 9;
		onStyle.alignment = TextAnchor.MiddleCenter;
		onStyle.normal = onStyle.active;
		
		offStyle.fontSize = 9;
		offStyle.alignment = TextAnchor.MiddleCenter;

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button(new GUIContent(state ? labelOn : labelOff, tooltip), state ? onStyle : offStyle, GUILayout.MinWidth(60), GUILayout.MaxWidth(60), GUILayout.Height(17)))
			if(state)
				state = false;
			else
				state = true;
		EditorGUILayout.EndHorizontal();

		return state;

	}
	
	
	/// <summary>
	/// Handles the display of a plugins foldout, enabled button and reset button
	/// </summary>
	public static bool BeginPlugin( ref bool foldout, ref bool enabled, string title, string prefix, Type editor = null )
	{
	
		GUIStyle style = new GUIStyle("Button");

		style.fontSize = 9;
		style.alignment = TextAnchor.MiddleCenter;
		
		GUILayout.Space(10);
	
		GUILayout.BeginHorizontal();
		GUILayout.Space(0);
		foldout = EditorGUI.Foldout(new Rect(GUILayoutUtility.GetLastRect().xMin+5, GUILayoutUtility.GetLastRect().yMin, 225, 20), foldout, " "+title, true, FoldoutStyle);
		GUILayout.FlexibleSpace();
		if(foldout)
		{
			enabled = ButtonToggle("Enabled", "Disabled", enabled, "Determines whether this plugin is enabled or not");
			if(GUILayout.Button(new GUIContent("Reset", "Reset all values back to the defaults."), style, GUILayout.Width(60), GUILayout.Height(17)))
			{
				if(EditorUtility.DisplayDialog("Are You Sure?", "This will revert all "+title+" Settings back to the defaults. Are you sure you want to do this", "Yes", "No"))
				{
					vp_AI newAI = m_AI.gameObject.AddComponent<vp_AI>();
					foreach(FieldInfo info in newAI.GetType().GetFields())
					{
						if(info.Name.IndexOf(prefix) != -1)
						{
							m_AI.GetType().GetField(info.Name).SetValue(m_AI, info.GetValue(newAI));
							if(info.Name == prefix)
							{
								if(info.FieldType.BaseType == typeof(vp_AIPlugin))
								{
									vp_AIPlugin comp = (vp_AIPlugin)info.GetValue(m_AI);
									comp.MainFoldout = true;
								}
							}
						}
					}
					
					// optional reset from this class
					MethodInfo method = editor.GetMethod("Reset"+prefix);
					if(method != null)
						method.Invoke(editor, null);
						
					// optional editor reset
					if(editor != null)
					{
						method = editor.GetMethod("Reset");
						if(method != null)
							method.Invoke(editor, null);
					}
					
					// attempt to check for an editor class for this plugin
					List<Type> types = (from System.Type t in Assembly.GetExecutingAssembly().GetTypes() where t.IsSubclassOf(typeof(vp_AIEditor)) select t).ToList();
					Type type = types.FirstOrDefault(t => t.ToString() == "vp_AI"+prefix+"Editor");
					if(type != null)
					{
						method = type.GetMethod("Reset");
						if(method != null)
							method.Invoke(type, null);
					}
						
					DestroyImmediate(newAI);
				}
			}
		}
		else
		{
			GUIStyle label = new GUIStyle("Label");
			label.fontSize = 11;
			label.normal.textColor = new Color(.4f,.4f,.4f);
			EditorGUILayout.LabelField(!enabled ? "Disabled" : "", label, GUILayout.Width(60));
		}
		GUILayout.EndHorizontal();
		
		if(!foldout)
		{
			DrawSeparator(2);
			return false;
		}
			
		GUILayout.Space(10);
			
		GUI.enabled = enabled;
		EditorGUI.indentLevel++;
		
		return true;
	
	}
	
	
	/// <summary>
	/// Ends the plugin.
	/// </summary>
	public static void EndPlugin()
	{
	
		GUILayout.Space(10);
		EditorGUI.indentLevel--;
		GUI.enabled = true;
		DrawSeparator();
	
	}
	
	
	/// <summary>
	/// Returns a list of states enabled for animation
	/// </summary>
	public static string[] AnimationStateNames
	{
	
		get{
			List<vp_AIState> states = m_AI.States.Where(s => s.Name != "Default" && s.IsAnimationState).ToList();
			string[] names = new string[states.Count+1];
			for(int i=0;i<names.Length;i++)
				names[i] = i == 0 ? "None" : states[i-1].Name;
				
			return names;
		}
	
	}
	
	
	/// <summary>
	/// Returns a popup with enabled animation states
	/// </summary>
	public static vp_AIState GetAnimationState( string title, vp_AIState state, string tooltip = "" )
	{
		
		List<vp_AIState> states = m_AI.States.Where(s => s.Name != "Default" && s.IsAnimationState).ToList();
		states.Insert(0, null);
		state = states.FirstOrDefault(s => s != null && s.Name == state.Name);
		int selected = state == null ? 0 : states.IndexOf(state);
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(new GUIContent(title, title+"\n\n"+tooltip), GUILayout.MinWidth(100), GUILayout.MaxWidth(200));
		selected = EditorGUILayout.Popup(selected, AnimationStateNames);
		GUILayout.EndHorizontal();
		return selected == -1 ? null : states[selected];
	
	}
	
	
	/// <summary>
	/// Returns a list of states enabled for audio
	/// </summary>
	public static string[] AudioStateNames
	{
	
		get{
			List<vp_AIState> states = m_AI.States.Where(s => s.Name != "Default" && s.IsAudioState).ToList();
			string[] names = new string[states.Count+1];
			for(int i=0;i<names.Length;i++)
				names[i] = i == 0 ? "None" : states[i-1].Name;
				
			return names;
		}
	
	}
	
	
	/// <summary>
	/// Returns a popup list with enabled audio states
	/// </summary>
	public static vp_AIState GetAudioState( string title, vp_AIState state, string tooltip = "" )
	{
		
		List<vp_AIState> states = m_AI.States.Where(s => s.Name != "Default" && s.IsAudioState).ToList();
		states.Insert(0, null);
		state = states.FirstOrDefault(s => s != null && s.Name == state.Name);
		int selected = state == null ? 0 : states.IndexOf(state);
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(new GUIContent(title, title+"\n\n"+tooltip), GUILayout.MinWidth(100), GUILayout.MaxWidth(200));
		selected = EditorGUILayout.Popup(selected, AudioStateNames);
		GUILayout.EndHorizontal();
		return selected == -1 ? null : states[selected];
	
	}
	
	
	/// <summary>
	/// Handles the display of the States and Presets section
	/// </summary>
	protected static void StatesAndPresets()
	{
	
		GUILayout.Space(10);
		
		m_AI.StatesAndPresetsMainFoldout = EditorGUI.Foldout(new Rect(GUILayoutUtility.GetLastRect().xMin+5, GUILayoutUtility.GetLastRect().yMin+10, 400, 30), m_AI.StatesAndPresetsMainFoldout, " States And Presets", true, FoldoutStyle);
		GUILayout.Space(20);
		
		if(!m_AI.StatesAndPresetsMainFoldout)
		{
			DrawSeparator(2);
			return;
		}
		
		GUILayout.Space(10);
			
		EditorGUI.indentLevel++;
		
		// state
		m_AI.StateFoldout = vp_AIPresetEditorGUIUtility.StateFoldout(m_AI.StateFoldout, m_AI, m_AI.States);

		// preset
		m_AI.PresetFoldout = vp_AIPresetEditorGUIUtility.PresetFoldout(m_AI.PresetFoldout, m_AI);

		// update
		if (GUI.changed)
		{

			// update the default state in order not to loose inspector tweaks
			// due to state switches during runtime
			if (Application.isPlaying)
				m_AI.RefreshDefaultState();

			m_AI.Refresh();

		}
		
		EditorGUI.indentLevel--;
		
		GUILayout.Space(10);
		
		DrawSeparator();
	
	}
	
	
	/// <summary>
	/// Returns a layermask popup
	/// </summary>
	public static LayerMask LayerMaskField (string label, LayerMask selected, string tooltip = "") {
	    return LayerMaskField (label,selected,true,tooltip);
	}
	 
	 
	/// <summary>
	/// Returns a layermask popup
	/// </summary>
	public static LayerMask LayerMaskField (string label, LayerMask selected, bool showSpecial, string tooltip = "")
	{

		List<string> layers = new List<string>();
	    List<int> layerNumbers = new List<int>();
	 
		string selectedLayers = "";
	 
	    for (int i=0;i<32;i++)
	    {
			string layerName = LayerMask.LayerToName (i);
	 
			if(layerName != "")
				if(selected == (selected | (1 << i)))
	 				if(selectedLayers == "")
						selectedLayers = layerName;
					else
	              		selectedLayers = "Mixed ...";
	    }
	 
	    if (Event.current.type != EventType.MouseDown && Event.current.type != EventType.ExecuteCommand)
	    {
			if (selected.value == 0)
				layers.Add ("Nothing");
			else if (selected.value == -1)
				layers.Add ("Everything");
			else
				layers.Add (selectedLayers);
				
	       layerNumbers.Add (-1);
	    }
	 
	    if (showSpecial)
	    {
	       layers.Add ((selected.value == 0 ? "\u2714   " : "      ") + "Nothing");
	       layerNumbers.Add (-2);
	 
	       layers.Add ((selected.value == -1 ? "\u2714   " : "      ") + "Everything");
	       layerNumbers.Add (-3);
	    }
	 
	    for (int i=0;i<32;i++)
	    {
	 
			string layerName = LayerMask.LayerToName (i);
	 
			if (layerName != "")
			{
				if (selected == (selected | (1 << i)))
					layers.Add ("\u2714   "+layerName);
				else
	          		layers.Add ("      "+layerName);
	          		
	         layerNumbers.Add (i);
	       }
	    }
	 
	    bool preChange = GUI.changed;
	 
	    GUI.changed = false;
	 
	    int newSelected = 0;
	 
	    if (Event.current.type == EventType.MouseDown)
			newSelected = -1;
			
		
		string[] strings = layers.ToArray();
		GUIContent[] gcStrings = new GUIContent[strings.Length];
		for(int i=0; i<strings.Length; i++)
			gcStrings[i] = new GUIContent(strings[i]);
	 
	    newSelected = EditorGUILayout.Popup(new GUIContent(label, tooltip), newSelected, gcStrings, EditorStyles.layerMaskField, GUILayout.MinWidth(100));
	 
		if (GUI.changed && newSelected >= 0)
	    {
			if(showSpecial && newSelected == 0)
				selected = 0;
			else if (showSpecial && newSelected == 1)
				selected = -1;
			else
	 
			if (selected == (selected | (1 << layerNumbers[newSelected])))
				selected &= ~(1 << layerNumbers[newSelected]);
			else
				selected = selected | (1 << layerNumbers[newSelected]);
		}else
	       GUI.changed = preChange;
	 
	    return selected;
	    
	}
	
	
	/// <summary>
	/// Displays a Tag mask popup
	/// </summary>
	public static int TagMaskField(string label, int selected, ref List<string> list)
	{
	
		string[] options = UnityEditorInternal.InternalEditorUtility.tags;
		selected = EditorGUILayout.MaskField(selected, options, GUILayout.MinWidth(100));
		lastUpdateTick = System.DateTime.Now.Ticks;
		
		list.Clear();
		for(int i=0; i<options.Length; i++)
			if((selected & 1<<i) != 0)
				list.Add(options[i]);
		
		return selected;
	
	}
	
	
	static public Texture2D blankTexture = EditorGUIUtility.whiteTexture;
	static public void DrawSeparator( float height = 4f )
	{
		
		GUILayout.Space(12f);

		if (Event.current.type == EventType.Repaint)
		{
			Texture2D tex = blankTexture;
			Rect rect = GUILayoutUtility.GetLastRect();
			GUI.color = new Color(0f, 0f, 0f, 0.25f);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 10f, Screen.width, height), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 10f, Screen.width, 1f), tex);
			GUI.DrawTexture(new Rect(0f, rect.yMin + 9f+height, Screen.width, 1f), tex);
			GUI.color = Color.white;
		}
		
	}
		
}

