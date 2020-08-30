//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright � 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace NJG
{
	public class NJGMapInspectorBase : Editor
	{
		static NJGMapBase m;
		protected string mSpriteName;
		protected string mSelSpriteName;
		Texture2D mTex;
		Texture2D mUserTex;
		int mLayer;
		bool saveTexture;
		bool mSet;
		bool mSetBoundsManually;
		Color mColor;
		string mWorldName;
		int mRenderOffset;
		Vector3 mManualBounds;

		SerializedProperty renderLayers;
		SerializedProperty boundLayers;

		static public Texture2D njgLogo { get { if (mLogo == null) mLogo = AssetDatabase.LoadAssetAtPath(iconPath + "NJG.png", typeof(Texture2D)) as Texture2D; return mLogo; } }
		static public Texture2D iconList { get { if (mListIcon == null) mListIcon = AssetDatabase.LoadAssetAtPath(iconPath + "IconList.png", typeof(Texture2D)) as Texture2D; return mListIcon; } }
		static public Texture2D iconThumb { get { if (mThumbIcon == null) mThumbIcon = AssetDatabase.LoadAssetAtPath(iconPath + "IconThumbnails.png", typeof(Texture2D)) as Texture2D; return mThumbIcon; } }

		static public string iconPath = "Assets/NinjutsuGames/NJG MiniMap/Icons/" + (EditorGUIUtility.isProSkin ? "Dark" : "Light") + "/";

		#region GUI Content

		GUIContent helpLink = new GUIContent("Help", "Opens the ninjutsu games forum in your browser.");
		GUIContent documentationLink = new GUIContent("Documentation", "Opens the NJG MiniMap documentation in your browser.");
		GUIContent checkForUpdates = new GUIContent("Updates", "Checks if you have the newest version of NJG MiniMap.");

		#endregion

		static Texture2D mLogo;
		static Texture2D mListIcon;
		static Texture2D mThumbIcon;
		//static NJGMapRenderer mRenderer;

		void OnEnable()
		{
			renderLayers = serializedObject.FindProperty("renderLayers");
			boundLayers = serializedObject.FindProperty("boundLayers");
			EditorApplication.hierarchyWindowChanged += OnSceneChanged;
			//if (mRenderer == null) mRenderer = NJGMapRenderer.instance;
		}

		void OnDestroy()
		{
			EditorApplication.hierarchyWindowChanged -= OnSceneChanged;
		}

		static void OnSceneChanged()
		{
			if(NJGMapBase.instance != null) NJGMapBase.instance.UpdateBounds();
		}

		Vector3 v3FrontTopLeft;
		Vector3 v3FrontTopRight;
		Vector3 v3FrontBottomLeft;
		Vector3 v3FrontBottomRight;
		Vector3 v3BackTopLeft;
		Vector3 v3BackTopRight;
		Vector3 v3BackBottomLeft;
		Vector3 v3BackBottomRight; 

		void DrawBounds()
		{
			Vector3 v3Center = m.bounds.center;
			Vector3 v3Extents = m.bounds.extents;

			v3FrontTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
			v3FrontTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner
			v3FrontBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
			v3FrontBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner
			v3BackTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
			v3BackTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner
			v3BackBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
			v3BackBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner

			v3FrontTopLeft = m.transform.TransformPoint(v3FrontTopLeft);
			v3FrontTopRight = m.transform.TransformPoint(v3FrontTopRight);
			v3FrontBottomLeft = m.transform.TransformPoint(v3FrontBottomLeft);
			v3FrontBottomRight = m.transform.TransformPoint(v3FrontBottomRight);
			v3BackTopLeft = m.transform.TransformPoint(v3BackTopLeft);
			v3BackTopRight = m.transform.TransformPoint(v3BackTopRight);
			v3BackBottomLeft = m.transform.TransformPoint(v3BackBottomLeft);
			v3BackBottomRight = m.transform.TransformPoint(v3BackBottomRight);

			Debug.DrawLine(v3FrontTopLeft, v3FrontTopRight, Color.red);
			Debug.DrawLine(v3FrontTopRight, v3FrontBottomRight, Color.red);
			Debug.DrawLine(v3FrontBottomRight, v3FrontBottomLeft, Color.red);
			Debug.DrawLine(v3FrontBottomLeft, v3FrontTopLeft, Color.red);

			Debug.DrawLine(v3BackTopLeft, v3BackTopRight, Color.red);
			Debug.DrawLine(v3BackTopRight, v3BackBottomRight, Color.red);
			Debug.DrawLine(v3BackBottomRight, v3BackBottomLeft, Color.red);
			Debug.DrawLine(v3BackBottomLeft, v3BackTopLeft, Color.red);

			Debug.DrawLine(v3FrontTopLeft, v3BackTopLeft, Color.red);
			Debug.DrawLine(v3FrontTopRight, v3BackTopRight, Color.red);
			Debug.DrawLine(v3FrontBottomRight, v3BackBottomRight, Color.red);
			Debug.DrawLine(v3FrontBottomLeft, v3BackBottomLeft, Color.red);

			//Debug.DrawLine(new Vector3(v0.x, v1.y, c.z), new Vector3(v1.x, v1.y, c.z), Color.red);
		}

		/// <summary>
		/// Draw the inspector.
		/// </summary>

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

#if UNITY_4_3
			EditorGUIUtility.LookLikeControls(130f);
#else
			EditorGUIUtility.labelWidth = 130f;
#endif
			m = target as NJGMapBase;

			GUI.SetNextControlName("empty");
			GUI.Button(new Rect(0, 0, 0, 0), "", GUIStyle.none);

			EditorGUILayout.Space();

			GUILayout.BeginVertical(GUILayout.Width(njgLogo == null ? 0 : njgLogo.height));
			GUILayout.BeginHorizontal();
			if (njgLogo != null) GUILayout.Label(njgLogo);
			GUIStyle title = EditorStyles.boldLabel;
			title.alignment = TextAnchor.MiddleLeft;

			GUILayout.BeginVertical();
			GUI.contentColor = Color.cyan;
			GUILayout.Label("NJG Minimap " + NJGMapBase.VERSION, title);
			GUI.contentColor = Color.white;

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(documentationLink, GUILayout.MaxWidth(105)))
			{
				Application.OpenURL("https://docs.google.com/document/d/11CtGGOQjnT58W7whT5cj1HqrDwlU1ZRNAvS-Y2AJchY");
			}
			if (GUILayout.Button(helpLink, GUILayout.MaxWidth(65)))
			{
				Application.OpenURL("http://www.ninjutsugames.com/forum/");
			}
			if (GUILayout.Button(checkForUpdates, GUILayout.MaxWidth(65)))
			{
				Application.OpenURL("https://www.assetstore.unity3d.com/#/content/8339");
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();	
			
			/*if (string.IsNullOrEmpty(LayerMask.LayerToName(m.gameObject.layer)))
			{
				GUI.backgroundColor = Color.red;
				EditorGUILayout.HelpBox("You need to create a special layer for this game object\nPlease create or assign a layer for this gameObject.\nYou can use the same layer of your NGUI UI", MessageType.Error);
				GUI.backgroundColor = Color.white;
			}*/

			DrawMapNotFound();

			EditorGUILayout.Separator();			
			GUI.backgroundColor = Color.white;

			GUIStyle activeTabStyle = new GUIStyle("ButtonMid");
			GUIStyle activeTabStyleLeft = new GUIStyle("ButtonLeft");
			GUIStyle activeTabStyleRight = new GUIStyle("ButtonRight");

			GUIStyle inactiveTabStyle = new GUIStyle(activeTabStyle);
			GUIStyle inactiveTabStyleLeft = new GUIStyle(activeTabStyleLeft);
			GUIStyle inactiveTabStyleRight = new GUIStyle(activeTabStyleRight);

			activeTabStyle.normal = activeTabStyle.active;
			activeTabStyleLeft.normal = activeTabStyleLeft.active;
			activeTabStyleRight.normal = activeTabStyleRight.active;

			//if (screen == NJGMapBase.SettingsScreen.Login) screen = NJGMapBase.SettingsScreen.Items;
			GUILayout.BeginHorizontal();

			for (int i = 0, imax = (int)NJGMapBase.SettingsScreen._LastDoNotUse; i < imax; i++)
			{
				//if ((NJGMapBase.SettingsScreen)i == NJGMapBase.SettingsScreen.Login) continue;
				GUIStyle active = activeTabStyleLeft;
				if (i > 0) active = activeTabStyle;
				if (i == (int)NJGMapBase.SettingsScreen._LastDoNotUse - 1) active = activeTabStyleRight;

				GUIStyle inactive = inactiveTabStyleLeft;
				if (i > 0) inactive = inactiveTabStyle;
				if (i == (int)NJGMapBase.SettingsScreen._LastDoNotUse - 1) inactive = inactiveTabStyleRight;

				GUI.backgroundColor = m.screen == (NJGMapBase.SettingsScreen)i ? Color.cyan : Color.white;
				if (GUILayout.Button(((NJGMapBase.SettingsScreen)i).ToString(), m.screen == (NJGMapBase.SettingsScreen)i ? active : inactive))
				{
					GUI.FocusControl("empty");
					m.screen = (NJGMapBase.SettingsScreen)i;
				}
			}
			GUI.backgroundColor = Color.white;

			GUILayout.EndHorizontal();

			switch (m.screen)
			{
				case NJGMapBase.SettingsScreen.General:
					DrawGeneralSettingsUI();
					break;
				case NJGMapBase.SettingsScreen.Icons:
					DrawIconsUI();
					break;
				case NJGMapBase.SettingsScreen.Zones:
					DrawLevelsUI();
					break;
				case NJGMapBase.SettingsScreen.FOW:
					DrawFOWUI();
					break;
			}
			EditorGUILayout.Separator();

			NJGEditorTools.DrawSeparator();
			EditorGUILayout.BeginHorizontal();			
			DrawEditButtons();

			if (!mSet)
			{
				if (!Application.isPlaying) m.SetTexture(m.generateMapTexture ? m.mapTexture : m.userMapTexture);
				mSet = true;
			}

			if (m.miniMap == null) m.miniMap = GameObject.FindObjectOfType(typeof(UIMiniMapBase)) as UIMiniMapBase;
			if (m.worldMap == null) m.worldMap = GameObject.FindObjectOfType(typeof(UIWorldMapBase)) as UIWorldMapBase;

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Separator();

			Save(false);
			serializedObject.ApplyModifiedProperties();			
		}

		void DrawFOWUI()
		{
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Fog of War Settings", EditorStyles.boldLabel);
			EditorGUILayout.Separator();

			bool fowEnabled = EditorGUILayout.Toggle("Enabled", m.fow.enabled);

			if (m.fow.enabled != fowEnabled)
			{
				m.fow.enabled = fowEnabled;
				if (m.fow.enabled)
				{
					if (UIMiniMapBase.inst != null)
					{
						UIMiniMapBase.inst.shaderType = NJGMapBase.ShaderType.FOW;
						UIMiniMapBase.inst.material.shader = Shader.Find("NinjutsuGames/Map FOW");
					}
					if (UIWorldMapBase.inst != null)
					{
						UIWorldMapBase.inst.shaderType = NJGMapBase.ShaderType.FOW;
						UIWorldMapBase.inst.material.shader = Shader.Find("NinjutsuGames/Map FOW");
					}
					if(Application.isPlaying) NJGFOW.instance.ResetFOW();
				}
				else
				{
					if (UIMiniMapBase.inst != null)
					{
						UIMiniMapBase.inst.shaderType = NJGMapBase.ShaderType.TextureMask;
						UIMiniMapBase.inst.material.shader = Shader.Find("NinjutsuGames/Map TextureMask");
					}
					if (UIWorldMapBase.inst != null)
					{
						UIWorldMapBase.inst.shaderType = NJGMapBase.ShaderType.TextureMask;
						UIWorldMapBase.inst.material.shader = Shader.Find("NinjutsuGames/Map TextureMask");
					}
				}
				NJGEditorTools.RegisterUndo("NJGMap Settings", m);
				//EditorUtility.SetDirty(m);
				
			}

			GUI.enabled = m.fow.enabled;
			//m.fow.fowSystem = (NJGMapBase.FOW.FOWSystem)EditorGUILayout.EnumPopup("FOW System Type", m.fow.fowSystem);

			/*if (m.fow.fowSystem == NJGMapBase.FOW.FOWSystem.BuiltInFOW)
			{*/
				m.fow.trailEffect = EditorGUILayout.Toggle("Trail Effect", m.fow.trailEffect);
				m.fow.textureBlendTime = EditorGUILayout.Slider(new GUIContent("Texture Blend Time", "How long it takes to reveal the texture"), m.fow.textureBlendTime, 0f, 1f);
				m.fow.updateFrequency = EditorGUILayout.Slider(new GUIContent("Update Frequency", "How often FOW will be updated"), m.fow.updateFrequency, 0.01f, 1f);
				//m.fow.hideIcons = EditorGUILayout.Toggle("Hide Icons", m.fow.hideIcons);
				//m.fow.textureSize = EditorGUILayout.IntField("Texture Size", m.fow.textureSize);
				//m.fow.worldSize = EditorGUILayout.Vector2Field("World Size", m.fow.worldSize);
				m.fow.revealDistance = (int)EditorGUILayout.Slider(new GUIContent("Reveal Distance", "How much in distance should be revealed around the map target."), m.fow.revealDistance, 0, 100);
				m.fow.blurIterations = (int)EditorGUILayout.Slider(
					new GUIContent("Blur Iterations",
						"How many blur iterations will be performed. More iterations result in smoother edges. \nBlurring happens on a separate thread and does not affect performance."),
						m.fow.blurIterations, 0, 100);
				m.fow.fogColor = EditorGUILayout.ColorField("Color", m.fow.fogColor);
			/*}
			else
			{
				EditorGUILayout.HelpBox("You need to uncomment the #define TasharenFOW on NJGFOW.cs class in order to make it work!", MessageType.Warning);
			}*/

			GUI.enabled = m.fow.enabled && Application.isPlaying;
			if (GUILayout.Button(new GUIContent("Reset FOW", "Reset Fog of War clearing all explored areas.")))
			{
				NJGFOW.instance.ResetFOW();
			}
			GUI.enabled = true;
		}

		#region General Settings

        //protected virtual void ChangeLayer(int layer) { }

		void DrawGeneralSettingsUI()
		{
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
			EditorGUILayout.Separator();

			mWorldName = EditorGUILayout.TextField(new GUIContent("World Name", "The name of the world that is going to be displayed on World Map and Mini Map titles."), m.worldName);

			if (m.worldName != mWorldName)
			{
				m.worldName = mWorldName;
				NJGEditorTools.RegisterUndo("World name", m);
			}

			bool generateAtStart = EditorGUILayout.Toggle("Generate at start", m.generateAtStart);
			bool dontDestroy = EditorGUILayout.Toggle("Dont Destroy", m.dontDestroy);

			GUI.enabled = m.dontDestroy;

			bool renderOnLevelLoad = EditorGUILayout.Toggle(new GUIContent("Render OnLevelLoad",
				"NJGMap.instance.GenerateMap() will be called automatically when a new level is loaded")
				, m.renderOnLevelLoad);

			GUI.enabled = true;
			/*mLayer = EditorGUILayout.LayerField(new GUIContent("Layer", "The layer of the whole map system"), m.layer);

			if (m.layer != mLayer)
			{
				m.layer = mLayer;
				SetLayerRecursively(m.transform, m.layer);
                if (m.mCam == null) m.mCam = m.GetComponentInChildren<Camera>();
                if (m.mCam != null) if (m.mCam.cullingMask != 1 << m.gameObject.layer) m.mCam.cullingMask = 1 << m.gameObject.layer;
				m.boundLayers = LayerMaskExtensions.RemoveFromMask(m.boundLayers, new string[] { LayerMask.LayerToName(m.layer) });
                ChangeLayer(m.layer);
				NJGEditorTools.RegisterUndo("Map layer", m);
			}*/
			//NJG.NJGMapBase.Resolution mapResolution = (NJG.NJGMapBase.Resolution)EditorGUILayout.EnumPopup(new GUIContent("Resolution", "Map resolution"), m.mapResolution);
			NJG.NJGMapBase.Orientation orientation = (NJG.NJGMapBase.Orientation)EditorGUILayout.EnumPopup(new GUIContent("Orientation", "Wheter if its a normal 3d game or side scroller"), m.orientation);
			NJG.NJGMapBase.RenderMode renderMode = (NJG.NJGMapBase.RenderMode)EditorGUILayout.EnumPopup(new GUIContent("Render Mode", "Map render mode"), m.renderMode);

			if (m.orientation != orientation ||
				//m.mapResolution != mapResolution||
				m.renderMode != renderMode ||
				m.generateAtStart != generateAtStart ||
				m.dontDestroy != dontDestroy ||
				m.renderOnLevelLoad != renderOnLevelLoad)
			{

				m.orientation = orientation;
				m.dontDestroy = dontDestroy;
				m.renderOnLevelLoad = renderOnLevelLoad;
				//m.mapResolution = mapResolution;
				m.mapResolution = NJG.NJGMapBase.Resolution.Normal;
				m.renderMode = renderMode;
				m.generateAtStart = generateAtStart;
				NJGEditorTools.RegisterUndo("Map properties", m);
			}

			string mRMode = "";

			switch (m.renderMode)
			{
				case NJGMapBase.RenderMode.Once:
					mRMode = "Render the map only once";
					break;
				case NJGMapBase.RenderMode.ScreenChange:
					mRMode = "Render the map once or re-render ir again if the screen size changes";
					break;
				case NJGMapBase.RenderMode.Dynamic:
					mRMode = "Render the map every each: " + m.dynamicRenderTime + " times + by calling the method NJG.NJGMapBase.instance.GenerateMap()";
					break;
			}

			EditorGUILayout.HelpBox(mRMode, MessageType.Info);

			float updateFrequency = EditorGUILayout.Slider(new GUIContent("Update Frequency", "How often the map will be updated"), m.updateFrequency, 0.01f, 1f);
			if (m.updateFrequency != updateFrequency)
			{
				m.updateFrequency = updateFrequency;
				NJGEditorTools.RegisterUndo("Map update frequency", m);
			}

			/*bool shouldBeOn = NJGEditorTools.DrawHeader("Camera Frustum Settings");
			if (shouldBeOn)
			{
				Color c = Color.grey;
				c.a = 0.5f;
				GUI.backgroundColor = c;
				NJGEditorTools.BeginContents();
				GUI.backgroundColor = Color.white;

				NJGEditorTools.DrawProperty("Camera", serializedObject.FindProperty("cameraFrustum"));
				NJGEditorTools.DrawProperty("Color", serializedObject.FindProperty("cameraFrustumColor"));
				NJGEditorTools.EndContents();
			}*/

			bool shouldBeOn = NJGEditorTools.DrawHeader("Bounds Settings");
			if (shouldBeOn)
			{
				Color c = Color.grey;
				c.a = 0.5f;
				GUI.backgroundColor = c;
				NJGEditorTools.BeginContents();
				GUI.backgroundColor = Color.white;

				bool showBounds = EditorGUILayout.Toggle("Display Bounds", m.showBounds);
				mSetBoundsManually = EditorGUILayout.Toggle("Custom Bounds", m.setBoundsManually);
				if (m.setBoundsManually != mSetBoundsManually ||
					m.showBounds != showBounds)
				{
					m.showBounds = showBounds;
					m.setBoundsManually = mSetBoundsManually;
					m.UpdateBounds();
					NJGEditorTools.RegisterUndo("Map bounds", m);
				}

				EditorGUILayout.BeginHorizontal();

				if (!m.setBoundsManually)
				{
					EditorGUILayout.PropertyField(boundLayers, new GUIContent("Boundary Layers", "Which layers are going to be used for bounds calculation."));

					if ((m.boundLayers.value & 1 << m.layer) != 0)
					{
						m.boundLayers = LayerMaskExtensions.RemoveFromMask(m.boundLayers, new string[] { LayerMask.LayerToName(m.layer) });
					}

					//if (m.boundLayers != mBoundLayers) m.boundLayers = mBoundLayers;					
				}
				else
				{
					EditorGUILayout.BeginVertical();
					mManualBounds = EditorGUILayout.Vector3Field("Bounds Size", m.customBounds);
					Vector3 mBoundsCenter = EditorGUILayout.Vector3Field("Bounds Center", m.customBoundsCenter);
					EditorGUILayout.EndVertical();
					if (m.customBounds != mManualBounds || m.customBoundsCenter != mBoundsCenter)
					{
						m.customBounds = mManualBounds;
						m.customBoundsCenter = mBoundsCenter;
						m.UpdateBounds();
						NJGEditorTools.RegisterUndo("Map bounds", m);
					}
				}

				EditorGUILayout.EndHorizontal();

				mRenderOffset = EditorGUILayout.IntField(new GUIContent("Bounds Offset", "Adds extra space to the render bounds"), m.boundsOffset);

				if (m.boundsOffset != mRenderOffset)
				{
					m.boundsOffset = mRenderOffset;
					m.UpdateBounds();
					NJGEditorTools.RegisterUndo("Bounds Offset");
				}

				EditorGUILayout.LabelField(new GUIContent("Scene Bounds", "Scene bounds calculated automatically"));

				string boundsText = "Center: X:" + System.Math.Round(m.bounds.center.x, 2) + ", Y:" + System.Math.Round(m.bounds.center.y, 2) + ", Z:" + System.Math.Round(m.bounds.center.z, 2);
				boundsText += "\nExtents: X:" + System.Math.Round(m.bounds.extents.x, 2) + ", Y:" + System.Math.Round(m.bounds.extents.y, 2) + ", Z:" + System.Math.Round(m.bounds.extents.z, 2);
				boundsText += "\nSize: X:" + System.Math.Round(m.bounds.size.x, 2) + ", Y:" + System.Math.Round(m.bounds.size.y, 2) + ", Z:" + System.Math.Round(m.bounds.size.z, 2);
				boundsText += "\nMin: X:" + System.Math.Round(m.bounds.min.x, 2) + ", Y:" + System.Math.Round(m.bounds.min.y, 2) + ", Z:" + System.Math.Round(m.bounds.min.z, 2);
				boundsText += "\nMax: X:" + System.Math.Round(m.bounds.max.x, 2) + ", Y:" + System.Math.Round(m.bounds.max.y, 2) + ", Z:" + System.Math.Round(m.bounds.max.z, 2);

				EditorGUILayout.HelpBox(boundsText, MessageType.Info);

				if (m.boundLayers.value == 0)
				{
					m.UpdateBounds();
					GUI.backgroundColor = Color.red;
					EditorGUILayout.HelpBox("Need a layer in order to get objects for bounds calculation.", MessageType.Error);
					GUI.backgroundColor = Color.white;
				}

				NJGEditorTools.EndContents();
			}
			
			//m.mapSize = EditorGUILayout.Vector2Field("Map Size", m.mapSize);			
			shouldBeOn = NJGEditorTools.DrawHeader("Render Settings");
			if (shouldBeOn)
			{
				Color c = Color.grey;
				c.a = 0.5f;
				GUI.backgroundColor = c;
				NJGEditorTools.BeginContents();
				GUI.backgroundColor = Color.white;				

				EditorGUILayout.PropertyField(renderLayers, new GUIContent("Render Layers", "Which layers are going to be used for rendering."));

				if ((m.renderLayers.value & 1 << m.layer) != 0)
					m.renderLayers = LayerMaskExtensions.RemoveFromMask(m.renderLayers, new string[] { LayerMask.LayerToName(m.layer) });

#if UNITY_4_3
				EditorGUIUtility.LookLikeControls(130f);
#else
				EditorGUIUtility.labelWidth = 130f;
#endif
				GUI.enabled = !Application.isPlaying;
				bool generateMapTexture = EditorGUILayout.Toggle(new GUIContent("Automatic Generation", "If true the map will be generated automatically at runtime."), m.generateMapTexture);
				GUI.enabled = true;				

				if (m.generateMapTexture)
				{
					if (m.useTextureGenerated)
					{
						mTex = m.mapTexture = m.userMapTexture = null;
						m.useTextureGenerated = false;
						m.SetTexture(null);
						NJGEditorTools.RegisterUndo("Map texture change");
					}
				}
				else
				{
					if (!m.useTextureGenerated)
					{
						if (mUserTex != null) m.userMapTexture = mUserTex;
						if (m.userMapTexture == null && mUserTex == null) m.userMapTexture = mUserTex = NJGEditorTools.GetTexture();
						m.useTextureGenerated = true;
						m.SetTexture(m.userMapTexture);
						NJGEditorTools.RegisterUndo("Map texture change");
					}
				}

				EditorGUILayout.BeginVertical("AppToolbar");
				EditorGUILayout.Separator();
				EditorGUILayout.BeginHorizontal();				
				GUI.enabled = !m.generateMapTexture && !Application.isPlaying;
				if (m.generateMapTexture)
				{
					mTex = (Texture2D)EditorGUILayout.ObjectField(m.mapTexture, typeof(Texture2D), false, GUILayout.Width(64f), GUILayout.Height(64f));
					if (m.mapTexture != mTex)
					{
						Resources.UnloadUnusedAssets();
						m.mapTexture = mTex;
						NJGEditorTools.RegisterUndo("Map texture change");
					}
				}
				else
				{
					if (saveTexture)
					{
						if (NJGEditorTools.GetTexture() != null) AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(NJGEditorTools.GetTexture()));

						m.userMapTexture = mUserTex = NJGEditorTools.SaveTexture(m.userMapTexture);
						m.SetTexture(m.userMapTexture);

						saveTexture = false;
						NJGEditorTools.RegisterUndo("Map texture change");
						if (!Application.isPlaying)
						{
							if (NJGMapRenderer.instance != null) NJGTools.DestroyImmediate(NJGMapRenderer.instance);
							if (GameObject.Find("_NJGMapRenderer")) NJGTools.DestroyImmediate(GameObject.Find("_NJGMapRenderer"));
						}
						Resources.UnloadUnusedAssets();

						//ToggleCameras(true);
					}

					mUserTex = (Texture2D)EditorGUILayout.ObjectField(m.userMapTexture, typeof(Texture2D), false, GUILayout.Width(64f), GUILayout.Height(64f));

					if (m.userMapTexture != mUserTex)
					{
						m.useTextureGenerated = true;
						Resources.UnloadUnusedAssets();
						m.userMapTexture = mUserTex;
						m.SetTexture(m.userMapTexture);
						NJGEditorTools.RegisterUndo("Map texture change");
					}
				}
				GUI.enabled = true;
				EditorGUILayout.BeginVertical();

				GUILayout.Space(20f);

				GUILayout.Space(-20f);

#if UNITY_4_3
				EditorGUIUtility.LookLikeControls(100f);
#else
				EditorGUIUtility.labelWidth = 100f;
#endif

				GUI.enabled = !m.generateMapTexture && !Application.isPlaying || m.generateMapTexture && Application.isPlaying;
				GUI.backgroundColor = !m.generateMapTexture || m.generateMapTexture && Application.isPlaying ? Color.green : Color.gray;
				if (GUILayout.Button(new GUIContent(m.generateMapTexture ? (Application.isPlaying ? "Regenerate" : "Click Play to generate") : "Generate New Map Texture", "Click to generate map texture"), GUILayout.Height(40f)))
				{
					//ToggleCameras(false);
					Resources.UnloadUnusedAssets();
					if (m.mapTexture != null && !Application.isPlaying) NJGTools.DestroyImmediate(m.mapTexture);

					//Debug.Log("Generating texture...");
					if (NJGEditorTools.GetTexture() != null)
					{
						//Debug.Log("Destroying current texture...");
						AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(NJGEditorTools.GetTexture()));
					}

					//Debug.Log("Swith to Game View...");
					NJGEditorTools.GetMainGameView().Focus();
					//Debug.Log("Lets take the photo...");
					m.GenerateMap();
					saveTexture = true;
					NJGEditorTools.RegisterUndo("Map texture change");
				}
				GUI.backgroundColor = Color.white;
				//EditorGUILayout.HelpBox("If you get a blank texture just try again", MessageType.None);
				EditorGUILayout.EndVertical();				
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Separator();
				EditorGUILayout.EndVertical();
				EditorGUILayout.Separator();

#if UNITY_4_3
				EditorGUIUtility.LookLikeControls(160f);
#else
				EditorGUIUtility.labelWidth = 160f;
#endif
				GUI.enabled = true;

				GUI.backgroundColor = Color.black;

				//if (!Application.isPlaying)
				Vector2 screenSize = new Vector2(NJGEditorTools.GetGameViewSize().x, NJGEditorTools.GetGameViewSize().y);
				//else
				//	m.mapSize = new Vector2(Screen.width, Screen.height);

				GUI.backgroundColor = Color.white;

				EditorGUILayout.LabelField("Texture Information");

				if (m.mapSize != screenSize) m.mapSize = screenSize;

				string info = "Screen Size width:" + screenSize.x + " height:" + screenSize.y;
				float memSize = 0f;

				if (m.mapTexture != null)
				{
					memSize = (float)CalculateTextureSizeBytes(m.mapTexture);
					info += "\nTexture Memory: " + System.Math.Round(((memSize / 1024) / 1024), 1) + " MB";
					info += "\nTexture Size: " + m.mapTexture.width + " x " + m.mapTexture.height;
				}
				else if (m.userMapTexture != null && !m.generateMapTexture)
				{
					memSize = (float)CalculateTextureSizeBytes(m.userMapTexture);
					info += "\nTexture Memory: " + System.Math.Round(((memSize / 1024) / 1024), 1) + " MB";
					info += "\nTexture Size: " + m.userMapTexture.width + " x " + m.userMapTexture.height;
				}

				EditorGUILayout.HelpBox(info, MessageType.Info);
				EditorGUILayout.Separator();

#if UNITY_4_3
				EditorGUIUtility.LookLikeControls(120f);
#else
				EditorGUIUtility.labelWidth = 120f;
#endif
				//if (m.optimize) GUI.enabled = !Application.isPlaying;
				//m.transparentTexture = EditorGUILayout.Toggle("Remove BG Color", m.transparentTexture, GUILayout.Width(140f));

				GUILayout.BeginHorizontal();
				m.optimize = EditorGUILayout.Toggle("Optimize", m.optimize, GUILayout.Width(140f));
				GUI.contentColor = m.optimize ? Color.yellow : Color.gray;
				EditorGUILayout.LabelField("Saves memory but can render only once.");
				GUI.contentColor = Color.white;
				GUILayout.EndHorizontal();

				if (!m.generateMapTexture) m.compressQuality = (TextureCompressionQuality)EditorGUILayout.EnumPopup("Compress Quality", m.compressQuality);
				mColor = EditorGUILayout.ColorField("Background Color", m.cameraBackgroundColor);
				NJG.NJGMapBase.NJGCameraClearFlags cameraClearFlags = (NJG.NJGMapBase.NJGCameraClearFlags)EditorGUILayout.EnumPopup("Camera Clear Flags", m.cameraClearFlags);
				FilterMode mapFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", m.mapFilterMode);
				TextureWrapMode mapWrapMode = (TextureWrapMode)EditorGUILayout.EnumPopup("Texture Wrap Mode", m.mapWrapMode);
				NJG.NJGMapBase.NJGTextureFormat textureFormat = (NJG.NJGMapBase.NJGTextureFormat)EditorGUILayout.EnumPopup("Texture Format", m.textureFormat);

				if (m.generateMapTexture != generateMapTexture ||
					m.cameraClearFlags != cameraClearFlags ||
					m.mapFilterMode != mapFilterMode ||
					m.mapWrapMode != mapWrapMode ||
					m.textureFormat != textureFormat)
				{
					m.textureFormat = textureFormat;
					m.mapWrapMode = mapWrapMode;
					m.mapFilterMode = mapFilterMode;
					m.cameraClearFlags = cameraClearFlags;
					m.generateMapTexture = generateMapTexture;
					NJGEditorTools.RegisterUndo("Render Settings");
				}

				if (m.cameraBackgroundColor != mColor)
				{
					m.cameraBackgroundColor = mColor;
					NJGEditorTools.RegisterUndo("Map color");
					if (UIMiniMapBase.inst != null)
					{
						if (UIMiniMapBase.inst.shaderType == NJG.NJGMapBase.ShaderType.ColorMask || m.transparentTexture)
							UIMiniMapBase.inst.material.SetColor("_MaskColor", mColor);
					}

					if (UIWorldMapBase.inst != null)
					{
						if (UIWorldMapBase.inst.shaderType == NJG.NJGMapBase.ShaderType.ColorMask || m.transparentTexture)
							UIWorldMapBase.inst.material.SetColor("_MaskColor", mColor);
					}
				}

				m.generateMipmaps = EditorGUILayout.Toggle("Generate Mipmaps", m.generateMipmaps);

				NJGEditorTools.EndContents();
			}

			GUI.enabled = true;

			if (GUI.changed)
			{
				m.UpdateBounds();
				NJGEditorTools.RegisterUndo("Map Properties", m);
			}
		}

		protected virtual void DrawComponentSelector() { }
		protected virtual void DrawMapNotFound() { }
		protected virtual void DrawEditButtons() { }
		#endregion

		#region Icons

		protected virtual void DrawCustomIconsUI() { }

		bool iconsToggle;
		void DrawIconsUI()
		{
			EditorGUILayout.Separator();
			bool errorName = false;
			
			DrawCustomIconsUI();

			EditorGUILayout.LabelField("Map Icon Settings", EditorStyles.boldLabel);

			DrawComponentSelector();

			m.iconSize = (int)EditorGUILayout.Slider(new GUIContent("Icon Size", "Global size of the map icons"), (float)m.iconSize, 1f, 128f);
			m.arrowSize = (int)EditorGUILayout.Slider(new GUIContent("Arrow Size", "Global size of the map arrows"), (float)m.arrowSize, 1f, 128f);
			m.borderSize = (int)EditorGUILayout.Slider(new GUIContent("Selection Size", "Global size of the map icon selection"), (float)m.borderSize, 1f, 128f);

			GUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(new GUIContent("Map Marker Types", "Create as many marker types as you want."));

			//var collapseIcon = '\u2261'.ToString();
			//bool collapse = GUILayout.Button(new GUIContent("Collapse All", "Click to collapse all"));

			//var expandIcon = '\u25A1'.ToString();
			//bool expand = GUILayout.Button(new GUIContent("Expand All", "Click to expand all"));

			if (GUILayout.Button(new GUIContent("Add New", "Click to add")))
				m.mapItems.Add(new NJG.NJGMapBase.MapItemType());

			bool iconsToggle = GUILayout.Button(new GUIContent(!m.typesFolded ? "Expand All" : "Collapse All", "Click to expand all"));

			if (iconsToggle)
			{
				m.typesFolded = !m.typesFolded;
			}

			GUILayout.EndHorizontal();

			if (m.mapItems[0].type != "None")
			{
				NJG.NJGMapBase.MapItemType defaultItem = new NJG.NJGMapBase.MapItemType();
				defaultItem.type = "None";
				m.mapItems.Insert(0, defaultItem);
			}

			GUILayout.BeginVertical("AppToolbar");
			for (int i = 0; i < m.mapItems.Count; ++i)
			{
				NJG.NJGMapBase.MapItemType mapItem = m.mapItems[i];				

				GUILayout.BeginHorizontal(EditorStyles.toolbarButton);
				GUILayout.Space(10f);
				mapItem.folded = EditorGUILayout.Foldout(mapItem.folded, i + ". " + mapItem.type);
				if (iconsToggle) mapItem.folded = m.typesFolded;
				//if (toggle) mapItem.folded = true;

				GUI.enabled = i > 1;
				var upArrow = '\u25B2'.ToString();
				if (GUILayout.Button(new GUIContent(upArrow, "Click to shift up"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
				{
					if (i > 0)
					{
						NJG.NJGMapBase.MapItemType shiftItem = m.mapItems[i];
						m.mapItems.RemoveAt(i);
						m.mapItems.Insert(i - 1, shiftItem);
					}
				}
				GUI.enabled = i > 0;
				var dnArrow = '\u25BC'.ToString();
				if (GUILayout.Button(new GUIContent(dnArrow, "Click to shift down"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
				{
					if (i + 1 < m.mapItems.Count)
					{
						NJG.NJGMapBase.MapItemType shiftItem = m.mapItems[i];
						m.mapItems.RemoveAt(i);
						m.mapItems.Insert(i + 1, shiftItem);
					}
				}
				GUI.enabled = true;

				GUI.backgroundColor = Color.green;
				if (GUILayout.Button(new GUIContent("+", "Click to add"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
					m.mapItems.Add(new NJG.NJGMapBase.MapItemType());

				GUI.backgroundColor = Color.white;

				GUI.enabled = i > 0;
				GUI.backgroundColor = Color.red;
				if (GUILayout.Button(new GUIContent("-", "Click to remove"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
				{
					mapItem.deleteRequest = true;
				}
				GUI.backgroundColor = Color.white;
				GUI.enabled = true;

				GUILayout.EndHorizontal();

				if (mapItem.deleteRequest)
				{
					// Show the confirmation dialog
					GUILayout.Label("Are you sure you want to delete '" + mapItem.type + "'?");
					NJGEditorTools.DrawSeparator();

					GUILayout.BeginHorizontal();
					{
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button("Cancel")) mapItem.deleteRequest = false;
						GUI.backgroundColor = Color.red;

						if (GUILayout.Button("Delete"))
						{
							m.mapItems.RemoveAt(i);
							--i;
							mapItem.deleteRequest = false;
						}
						GUI.backgroundColor = Color.white;
					}
					GUILayout.EndHorizontal();
					EditorGUILayout.Separator();

				}
				else if (mapItem.folded)
				{
					GUILayout.BeginVertical();

					GUI.enabled = i > 0;
					//if (i == 0) mapItem.type = "Default";
					mapItem.type = EditorGUILayout.TextField("Marker Type", mapItem.type);
					GUI.enabled = true;

					if (i > 0)
					{
						DrawIconSpriteUI(mapItem);

						// Depth
						GUILayout.Space(2f);
						GUILayout.BeginHorizontal();
						{
							EditorGUILayout.PrefixLabel("Depth");

							int depth = mapItem.depth;
							if (GUILayout.Button("Back", GUILayout.Width(60f))) --depth;
							depth = EditorGUILayout.IntField(depth);
							if (GUILayout.Button("Forward", GUILayout.Width(60f))) ++depth;

							if (mapItem.depth != depth)
							{
								mapItem.depth = depth;
							}
						}
						GUILayout.EndHorizontal();

						mapItem.enableInteraction = EditorGUILayout.Toggle("Enable Interaction", mapItem.enableInteraction);
						mapItem.color = EditorGUILayout.ColorField("Color", mapItem.color);
						mapItem.useCustomSize = EditorGUILayout.Toggle("Custom Icon Size", mapItem.useCustomSize);
						GUI.enabled = mapItem.useCustomSize;
						mapItem.size = (int)EditorGUILayout.Slider(new GUIContent("Icon Size", "Overrides global size of the map icon"), (float)mapItem.size, 1f, 128f);
						GUI.enabled = true;
						mapItem.useCustomBorderSize = EditorGUILayout.Toggle("Custom Border Size", mapItem.useCustomBorderSize);
						GUI.enabled = mapItem.useCustomBorderSize;
						mapItem.borderSize = (int)EditorGUILayout.Slider(new GUIContent("Border Size", "Overrides global size of the map icon selection"), (float)mapItem.borderSize, 1f, 128f);
						GUI.enabled = true;
						mapItem.updatePosition = EditorGUILayout.Toggle("Update Position", mapItem.updatePosition);
						mapItem.animateOnVisible = EditorGUILayout.Toggle("Animate On Visible", mapItem.animateOnVisible);
						mapItem.loopAnimation = EditorGUILayout.Toggle("Loop Animation", mapItem.loopAnimation);
						mapItem.fadeOutAfterDelay = EditorGUILayout.FloatField("Fade Out Delay", mapItem.fadeOutAfterDelay);
						mapItem.rotate = EditorGUILayout.Toggle("Rotate", mapItem.rotate);
						mapItem.haveArrow = EditorGUILayout.Toggle("Display Arrow", mapItem.haveArrow);
						//mapItem.revealFOW = EditorGUILayout.Toggle(new GUIContent("Reveal FOW", "This will affect only if Fog of War is enabled"), mapItem.revealFOW);

						DrawArrowSpriteUI(mapItem);

						GUI.contentColor = Color.white;
						GUI.backgroundColor = Color.white;
					}

					GUILayout.EndVertical();
				}

				if (string.IsNullOrEmpty(mapItem.sprite) && i != 0)
					EditorGUILayout.HelpBox("You need to assign a sprite name", MessageType.Error);

				errorName = NameIsDifferent(mapItem.type, i);

				if (errorName && i != 0)
					EditorGUILayout.HelpBox("Type names must be different", MessageType.Error);
			}
			GUILayout.EndVertical();
		}
		protected virtual void DrawArrowSpriteUI(NJGMapBase.MapItemType mapItem) { }
		protected virtual void DrawIconSpriteUI(NJGMapBase.MapItemType mapItem) { }
		#endregion

		#region Levels
		void DrawLevelsUI()
		{
			EditorGUILayout.Separator();
			bool errorName = false;

			EditorGUILayout.LabelField("Levels & Zones Settings", EditorStyles.boldLabel);			

			GUILayout.BeginHorizontal();
			GUI.backgroundColor = Color.green;
			if (GUILayout.Button("Add New Zone Game Object"))
			{
				NJGMenu.AddMapZone();
			}
			GUI.backgroundColor = Color.white;
			GUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			m.zoneColor = EditorGUILayout.ColorField("Default Zone Color", m.zoneColor);

			GUILayout.BeginHorizontal();
			//EditorGUILayout.LabelField(new GUIContent("Zones", "Create as many zones as you want."));

			if (GUILayout.Button(new GUIContent("Add New", "Click to add")))
			{
				NJG.NJGMapBase.MapLevel ml2 = new NJG.NJGMapBase.MapLevel();
				ml2.level = Application.loadedLevelName;
				m.levels.Add(ml2);
			}

			bool toggle = GUILayout.Button(new GUIContent(!m.zonesFolded ? "Expand All" : "Collapse All", "Click to expand all"));

			if (toggle)
				m.zonesFolded = !m.zonesFolded;

			GUILayout.EndHorizontal();
			EditorGUILayout.Separator();

			GUILayout.BeginVertical("AppToolbar");
			for (int i = 0; i < m.levels.Count; ++i)
			{
				NJG.NJGMapBase.MapLevel item = m.levels[i];

				GUILayout.BeginHorizontal(EditorStyles.toolbarButton);
				GUILayout.Space(10f);
				item.folded = EditorGUILayout.Foldout(item.folded, item.level);
				if (toggle) item.folded = m.zonesFolded;

				var upArrow = '\u25B2'.ToString();
				if (GUILayout.Button(new GUIContent(upArrow, "Click to shift up"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
				{
					if (i > 0)
					{
						NJG.NJGMapBase.MapLevel shiftItem = m.levels[i];
						m.levels.RemoveAt(i);
						m.levels.Insert(i - 1, shiftItem);
					}
				}

				var dnArrow = '\u25BC'.ToString();
				if (GUILayout.Button(new GUIContent(dnArrow, "Click to shift down"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
				{
					if (i + 1 < m.mapItems.Count)
					{
						NJG.NJGMapBase.MapLevel shiftItem = m.levels[i];
						m.levels.RemoveAt(i);
						m.levels.Insert(i + 1, shiftItem);
					}
				}

				GUI.backgroundColor = Color.green;
				if (GUILayout.Button(new GUIContent("+", "Click to add"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
				{
					NJG.NJGMapBase.MapLevel ml = new NJG.NJGMapBase.MapLevel();
					ml.level = Application.loadedLevelName;
					m.levels.Add(ml);
				}

				GUI.backgroundColor = Color.white;

				GUI.backgroundColor = Color.red;
				if (GUILayout.Button(new GUIContent("-", "Click to remove"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
				{
					item.deleteRequest = true;
				}
				GUI.backgroundColor = Color.white;

				GUILayout.EndHorizontal();

				if (item.deleteRequest)
				{
					// Show the confirmation dialog
					GUILayout.Label("Are you sure you want to delete '" + item.level + "'?");
					NJGEditorTools.DrawSeparator();

					GUILayout.BeginHorizontal();
					{
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button("Cancel")) item.deleteRequest = false;
						GUI.backgroundColor = Color.red;

						if (GUILayout.Button("Delete"))
						{
							m.levels.RemoveAt(i);
							--i;
							item.deleteRequest = false;
						}
						GUI.backgroundColor = Color.white;
					}
					GUILayout.EndHorizontal();
					EditorGUILayout.Separator();

				}
				else if (item.folded)
				{
					GUILayout.BeginVertical();
					DrawZoneUI(item);
					GUILayout.EndVertical();
				}

				errorName = SceneIsDifferent(item.level, i);

				if (errorName)
					EditorGUILayout.HelpBox("Type names must be different", MessageType.Error);
			}
			GUILayout.EndVertical();
		}


		void DrawZoneUI(NJG.NJGMapBase.MapLevel l)
		{
			bool errorName = false;

			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();

			l.level = EditorGUILayout.TextField("Level", l.level);

			if (GUILayout.Button(new GUIContent("+", "Click to add"), GUILayout.Width(24.0f)))
				l.zones.Add(new NJG.NJGMapBase.MapZone());

			var collapseIcon = '\u2261'.ToString();
			var expandIcon = '\u25A1'.ToString();

			bool subToggle = GUILayout.Button(new GUIContent(!l.itemsFolded ? expandIcon : collapseIcon, "Click to expand all"), GUILayout.Width(24.0f));

			GUILayout.EndHorizontal();
			EditorGUILayout.Space();

			if (subToggle)
				l.itemsFolded = !l.itemsFolded;

			GUILayout.BeginHorizontal();
			GUILayout.Space(4f);
			GUILayout.BeginVertical();
			for (int i = 0; i < l.zones.Count; ++i)
			{
				NJG.NJGMapBase.MapZone item = l.zones[i];

				GUILayout.BeginHorizontal();
				GUILayout.BeginHorizontal(EditorStyles.toolbarButton);
				GUILayout.Space(10f);
				item.folded = EditorGUILayout.Foldout(item.folded, item.type);
				if (subToggle) item.folded = l.itemsFolded;

				var upArrow = '\u25B2'.ToString();
				if (GUILayout.Button(new GUIContent(upArrow, "Click to shift up"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
				{
					if (i > 0)
					{
						NJG.NJGMapBase.MapZone shiftItem = l.zones[i];
						l.zones.RemoveAt(i);
						l.zones.Insert(i - 1, shiftItem);
					}
				}

				var dnArrow = '\u25BC'.ToString();
				if (GUILayout.Button(new GUIContent(dnArrow, "Click to shift down"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
				{
					if (i + 1 < m.mapItems.Count)
					{
						NJG.NJGMapBase.MapZone shiftItem = l.zones[i];
						l.zones.RemoveAt(i);
						l.zones.Insert(i + 1, shiftItem);
					}
				}

				GUI.backgroundColor = Color.green;
				if (GUILayout.Button(new GUIContent("+", "Click to add"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
					l.zones.Add(new NJG.NJGMapBase.MapZone());

				GUI.backgroundColor = Color.white;

				GUI.backgroundColor = Color.red;
				if (GUILayout.Button(new GUIContent("-", "Click to remove"), EditorStyles.toolbarButton, GUILayout.Width(24.0f)))
				{
					item.deleteRequest = true;
				}
				GUI.backgroundColor = Color.white;
				GUI.enabled = true;
				
				GUILayout.EndHorizontal();
				GUILayout.Space(4f);
				GUILayout.EndHorizontal();

				if (item.deleteRequest)
				{
					// Show the confirmation dialog
					GUILayout.Label("Are you sure you want to delete '" + item.type + "'?");
					NJGEditorTools.DrawSeparator();

					GUILayout.BeginHorizontal("AppToolbar");
					{
						GUI.backgroundColor = Color.green;
						if (GUILayout.Button("Cancel")) item.deleteRequest = false;
						GUI.backgroundColor = Color.red;

						if (GUILayout.Button("Delete"))
						{
							l.zones.RemoveAt(i);
							--i;
							item.deleteRequest = false;
						}
						GUI.backgroundColor = Color.white;
					}
					GUILayout.EndHorizontal();
					EditorGUILayout.Separator();

				}
				else if (item.folded)
				{
					GUILayout.BeginVertical();

					item.type = EditorGUILayout.TextField("Zone", item.type);
					GUI.enabled = true;

					item.color = EditorGUILayout.ColorField("Color", item.color);
					item.fadeOutAfterDelay = EditorGUILayout.FloatField("Fade Out Delay", item.fadeOutAfterDelay);

					GUI.contentColor = Color.white;
					GUI.backgroundColor = Color.white;

					GUILayout.EndVertical();
					EditorGUILayout.Separator();
				}

				errorName = NameIsDifferent(item.type, i);

				if (errorName)
					EditorGUILayout.HelpBox("Type names must be different", MessageType.Error);
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.Space(4f);
		}

		#endregion

		#region Helper Methods

		static List<Camera> cameras = new List<Camera>();

		static void ToggleCameras(bool flag)
		{
			//find enabled cameras
			if (cameras.Count == 0)
			{
				UnityEngine.Object[] cams = FindObjectsOfType(typeof(Camera));

				Debug.Log("cams " + cams.Length);

				foreach (Camera c in cams)
					if (c.enabled) cameras.Add(c);
			}

			Debug.Log("cameras " + cameras.Count);
			foreach (Camera c in cameras)
				c.enabled = flag;
		}

		/// <summary>
		/// Sets the layer of the passed transform and all of its children
		/// </summary>

		protected static void SetLayerRecursively(Transform root, int layer)
		{
			root.gameObject.layer = layer;
			foreach (Transform child in root)
				SetLayerRecursively(child, layer);
		}

		protected void Save(bool force)
		{
			if (GUI.changed || force)
			{
				NJGEditorTools.RegisterUndo("NJG Map Settings", m);
			}
		}

		/// <summary>
		/// Check if name is different
		/// </summary>

		bool NameIsDifferent(string name, int index)
		{
			int count = 0;

			for (int i = 0, imax = m.mapItems.Count; i < imax; i++)
				if (i != index && name == m.mapItems[i].type) count++;

			return count >= 1;
		}

		bool SceneIsDifferent(string name, int index)
		{
			int count = 0;

			for (int i = 0, imax = m.levels.Count; i < imax; i++)
				if (i != index && name == m.levels[i].level) count++;

			return count >= 1;
		}

		int CalculateTextureSizeBytes(Texture tTexture)
		{

			int tWidth = tTexture.width;
			int tHeight = tTexture.height;
			if (tTexture is Texture2D)
			{
				Texture2D tTex2D = tTexture as Texture2D;
				int bitsPerPixel = GetBitsPerPixel(tTex2D.format);
				int mipMapCount = tTex2D.mipmapCount;
				int mipLevel = 1;
				int tSize = 0;
				while (mipLevel <= mipMapCount)
				{
					tSize += tWidth * tHeight * bitsPerPixel / 8;
					tWidth = tWidth / 2;
					tHeight = tHeight / 2;
					mipLevel++;
				}
				return tSize;
			}
			return 0;
		}

		int GetBitsPerPixel(TextureFormat format)
		{
			switch (format)
			{
				case TextureFormat.Alpha8: //	 Alpha-only texture format.
					return 8;
				case TextureFormat.ARGB4444: //	 A 16 bits/pixel texture format. Texture stores color with an alpha channel.
					return 16;
				case TextureFormat.RGB24:	// A color texture format.
					return 24;
				case TextureFormat.RGBA32:	//Color with an alpha channel texture format.
					return 32;
				case TextureFormat.ARGB32:	//Color with an alpha channel texture format.
					return 32;
				case TextureFormat.RGB565:	//	 A 16 bit color texture format.
					return 16;
				case TextureFormat.DXT1:	// Compressed color texture format.
					return 4;
				case TextureFormat.DXT5:	// Compressed color with alpha channel texture format.
					return 8;
				/*
				case TextureFormat.WiiI4:	// Wii texture format.
				case TextureFormat.WiiI8:	// Wii texture format. Intensity 8 bit.
				case TextureFormat.WiiIA4:	// Wii texture format. Intensity + Alpha 8 bit (4 + 4).
				case TextureFormat.WiiIA8:	// Wii texture format. Intensity + Alpha 16 bit (8 + 8).
				case TextureFormat.WiiRGB565:	// Wii texture format. RGB 16 bit (565).
				case TextureFormat.WiiRGB5A3:	// Wii texture format. RGBA 16 bit (4443).
				case TextureFormat.WiiRGBA8:	// Wii texture format. RGBA 32 bit (8888).
				case TextureFormat.WiiCMPR:	//	 Compressed Wii texture format. 4 bits/texel, ~RGB8A1 (Outline alpha is not currently supported).
					return 0;  //Not supported yet
				*/
				case TextureFormat.PVRTC_RGB2://	 PowerVR (iOS) 2 bits/pixel compressed color texture format.
					return 2;
				case TextureFormat.PVRTC_RGBA2://	 PowerVR (iOS) 2 bits/pixel compressed with alpha channel texture format
					return 2;
				case TextureFormat.PVRTC_RGB4://	 PowerVR (iOS) 4 bits/pixel compressed color texture format.
					return 4;
				case TextureFormat.PVRTC_RGBA4://	 PowerVR (iOS) 4 bits/pixel compressed with alpha channel texture format
					return 4;
				case TextureFormat.ETC_RGB4://	 ETC (GLES2.0) 4 bits/pixel compressed RGB texture format.
					return 4;
				// case TextureFormat.ETC_RGB4://	 ATC (ATITC) 4 bits/pixel compressed RGB texture format.
				// 	return 4;
				case TextureFormat.ETC2_RGBA8://	 ATC (ATITC) 8 bits/pixel compressed RGB texture format.
					return 8;
				case TextureFormat.BGRA32://	 Format returned by iPhone camera
					return 32;
                default:
				//case TextureFormat.ATF_RGB_DXT1://	 Flash-specific RGB DXT1 compressed color texture format.
				//case TextureFormat.ATF_RGBA_JPG://	 Flash-specific RGBA JPG-compressed color texture format.
				//case TextureFormat.ATF_RGB_JPG://	 Flash-specific RGB JPG-compressed color texture format.
					return 0; //Not supported yet
			}
			return 0;
		}

		#endregion

		#region Draw Inspector Preview

		/// <summary>
		/// Draw the sprite preview.
		/// </summary>

		/*public override void OnPreviewGUI(Rect rect, GUIStyle background)
		{
			Texture2D tex = m.mapTexture;
			if (tex == null) tex = m.userMapTexture;
			if (tex == null) return;

			int size = 256;
			Material mat = AssetDatabase.LoadAssetAtPath("Assets/NinjutsuGames/NG GameMap/Materials/WorldNJGMap.mat", typeof(Material)) as Material;

			// We only want to draw into this rectangle
			if (Event.current.type == EventType.Repaint)
			{
				UnityEditor.EditorGUI.DrawPreviewTexture(new Rect(rect.x + size / 2f + rect.x / 2f, rect.y, size, size), tex, mat == null ? null : mat);
			}
		}

		public override bool HasPreviewGUI() { return m == null ? false : (m.mapTexture != null || m.userMapTexture != null); }*/

		#endregion

		#region Draw Sprite Preview

		/// <summary>
		/// Draw an enlarged sprite within the specified texture atlas.
		/// </summary>

		public Rect DrawSprite(Texture2D tex, Rect sprite, Material mat, Vector2 space) { return DrawSprite(tex, sprite, mat, true, 0, space); }

		/// <summary>
		/// Draw an enlarged sprite within the specified texture atlas.
		/// </summary>

		public Rect DrawSprite(Texture2D tex, Rect sprite, Material mat, bool addPadding, Vector2 space)
		{
			return DrawSprite(tex, sprite, mat, addPadding, 0, space);
		}

		/// <summary>
		/// Draw an enlarged sprite within the specified texture atlas.
		/// </summary>

		public Rect DrawSprite(Texture2D tex, Rect sprite, Material mat, bool addPadding, int maxSize, Vector2 space)
		{
			float paddingX = addPadding ? 4f / tex.width : 0f;
			float paddingY = addPadding ? 4f / tex.height : 0f;
			float ratio = (sprite.height + paddingY) / (sprite.width + paddingX);

			ratio *= (float)tex.height / tex.width;

			// Draw the checkered background
			Color c = GUI.color;
			Rect rect = GUILayoutUtility.GetRect(0f, 0f);
			rect.width = Screen.width - rect.xMin;
			rect.height = rect.width * ratio;

			//Debug.Log("rect " + rect.xMin + " - " + rect);

			rect = new Rect(250 - space.x, rect.yMin - space.y, maxSize, maxSize);

			GUI.color = c;

			/*if (maxSize > 0)
			{
				float dim = maxSize / Mathf.Max(rect.width, rect.height);
				rect.width *= dim;
				rect.height *= dim;
			}*/

			// We only want to draw into this rectangle
			if (Event.current.type == EventType.Repaint)
			{
				if (mat == null)
				{
					GUI.DrawTextureWithTexCoords(rect, tex, sprite);
				}
				else
				{
					// NOTE: DrawPreviewTexture doesn't seem to support BeginGroup-based clipping
					// when a custom material is specified. It seems to be a bug in Unity.
					// Passing 'null' for the material or omitting the parameter clips as expected.
					Debug.Log("tex " + tex + " / " + rect + " / mMat " + mat + " size " + maxSize);
					UnityEditor.EditorGUI.DrawPreviewTexture(sprite, tex, mat);
					//UnityEditor.EditorGUI.DrawPreviewTexture(drawRect, tex);
					//GUI.DrawTexture(drawRect, tex);
				}
				rect = new Rect(sprite.x + rect.x, sprite.y + rect.y, sprite.width, sprite.height);
			}
			return rect;
		}

		#endregion
	}
}
