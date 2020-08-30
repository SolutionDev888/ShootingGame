//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using NJG;
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace NJG
{

	public class UIWorldMapInspectorBase : Editor
	{
		protected UIWorldMapBase m;
		Texture mMask;
		Color mColor;
		//Color mColorMask;
		//Material mMat;
		LeanTweenType mEasing;
		NJG.NJGMapBase.ShaderType mShader;
		float zoom;
		float minZoom;
		float maxZoom;
		float zoomSpeed;
		KeyCode zoomInKey;
		KeyCode zoomOutKey;
		bool mouseWheel;
		float mapBorderRadius;
		bool calculateBorder;
		Texture2D mTempTex = new Texture2D(1, 1);

		protected virtual void DrawNotFound() { }

		/// <summary>
		/// Draw the inspector.
		/// </summary>

		public override void OnInspectorGUI()
		{
#if UNITY_4_3
			EditorGUIUtility.LookLikeControls(90f);
#else
			EditorGUIUtility.labelWidth = 90f;
#endif
			m = target as UIWorldMapBase;

			PrefabType type = PrefabUtility.GetPrefabType(m.gameObject);

			if (m.material == null && m.planeRenderer != null) m.material = NJGEditorTools.GetMaterial(m, false);

			NJGEditorTools.DrawEditMap();

			DrawNotFound();

			GUILayout.BeginVertical();
			EditorGUILayout.Separator();

			mShader = (NJGMapBase.ShaderType)EditorGUILayout.EnumPopup("Shader Type", m.shaderType);
			if (mShader == NJGMapBase.ShaderType.ColorMask)
			{
				EditorGUILayout.HelpBox("Use the camera background color for masking", MessageType.Info);
				//m.colorMask = EditorGUILayout.ColorField("Color Mask", m.colorMask);
			}

			/*if (NJGMapBase.instance.fow.enabled && mShader != NJGMapBase.ShaderType.FOW)
			{
				m.shaderType = mShader = NJGMapBase.ShaderType.FOW;
				m.material.shader = Shader.Find("NinjutsuGames/Map FOW");
				EditorUtility.SetDirty(m);
				//EditorGUILayout.HelpBox("Fog of War is enabled\nIn order to make it work the shader type should be FOW\nIgnore this warning if you don't want to use FOW for this instance.", MessageType.Warning);
			}
			else if (!NJGMapBase.instance.fow.enabled && mShader == NJGMapBase.ShaderType.FOW)
			{
				m.shaderType = mShader = NJGMapBase.ShaderType.TextureMask;
				m.material.shader = Shader.Find("NinjutsuGames/Map TextureMask");
				EditorUtility.SetDirty(m);
			}*/

			if (m.maskTexture == null) m.maskTexture = mTempTex;

			mMask = (Texture2D)EditorGUILayout.ObjectField("Map Mask", m.maskTexture, typeof(Texture2D), false);

			if (m.maskTexture != mMask)
			{
				m.maskTexture = mMask;
				m.material.SetTexture("_Mask", m.maskTexture);
				EditorUtility.SetDirty(m);
			}

			if (m.shaderType != mShader)
			{
				m.shaderType = mShader;

				Shader s = Shader.Find("NinjutsuGames/Map TextureMask");
				switch (mShader)
				{
					case NJGMapBase.ShaderType.TextureMask:
						s = Shader.Find("NinjutsuGames/Map TextureMask");
						NJGMapBase.instance.fow.enabled = false;
						break;
					case NJGMapBase.ShaderType.ColorMask:
						s = Shader.Find("NinjutsuGames/Map ColorMask");
						m.material.SetColor("_MaskColor", NJGMapBase.instance.cameraBackgroundColor);
						NJGMapBase.instance.fow.enabled = false;
						break;
					case NJGMapBase.ShaderType.FOW:
						s = Shader.Find("NinjutsuGames/Map FOW");
						NJGMapBase.instance.fow.enabled = true;
						break;
				}
				m.material.shader = s;
				if (m.planeRenderer != null)
				{
					if (Application.isPlaying) m.planeRenderer.material.shader = s;
					else m.planeRenderer.sharedMaterial.shader = s;
				}
				NJGEditorTools.RegisterUndo("UIWorldMap Shader Type", m);
			}

			EditorGUILayout.Separator();
			GUILayout.EndVertical();

#if UNITY_4_3
			EditorGUIUtility.LookLikeControls(120f);
#else
			EditorGUIUtility.labelWidth = 120f;
#endif

			bool shouldBeOn = NJGEditorTools.DrawHeader("UI Settings");
			if (shouldBeOn)
			{
				Color c = Color.grey;
				c.a = 0.5f;
				GUI.backgroundColor = c;
				NJGEditorTools.BeginContents();
				GUI.backgroundColor = Color.white;

				DrawDepth(type == PrefabType.Prefab);

				m.mapColor = EditorGUILayout.ColorField("Color", m.mapColor);

				if (mColor != m.mapColor)
				{
					mColor = m.mapColor;
					if (m.material != null)
						m.material.color = m.mapColor;
					EditorUtility.SetDirty(m);
				}

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(new GUIContent("Culling Radius", "If icons get farther this radius they will dissapear"), GUILayout.Width(116f));
				GUI.enabled = !m.calculateBorder;
#if UNITY_4_3
				mapBorderRadius = EditorGUILayout.FloatField(m.mapBorderRadius);
#else
				mapBorderRadius = EditorGUILayout.FloatField(m.mapBorderRadius, GUILayout.Width(158f));
#endif
				GUI.enabled = true;
				calculateBorder = EditorGUILayout.Toggle(new GUIContent("Automatic", "Check this option if you want this value to be autmatically calculated at start."), m.calculateBorder);
#if UNITY_4_3
				EditorGUIUtility.LookLikeControls(120f);
#else
				EditorGUIUtility.labelWidth = 120f;
#endif
				GUILayout.EndHorizontal();

				/*GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Dimensions", GUILayout.Width(116f));
				EditorGUIUtility.LookLikeControls(10f);
				mMapScale.x = EditorGUILayout.IntField((int)m.mapScale.x);
				EditorGUIUtility.LookLikeControls(12f);
				EditorGUILayout.LabelField("x", GUILayout.Width(15f));
				mMapScale.y = EditorGUILayout.IntField((int)m.mapScale.y);
				EditorGUIUtility.LookLikeControls(80f);
				GUILayout.EndHorizontal();

				if (m.mapScale != mMapScale)
				{
					mMapScale.x = (int)mMapScale.x;
					mMapScale.y = (int)mMapScale.y;
					m.mapScale = mMapScale;
					EditorUtility.SetDirty(m);
				}*/

				NJGEditorTools.EndContents();
				EditorGUILayout.Separator();
			}

			shouldBeOn = NJGEditorTools.DrawHeader("Zoom Settings");
			if (shouldBeOn)
			{
				Color c = Color.grey;
				c.a = 0.5f;
				GUI.backgroundColor = c;
				NJGEditorTools.BeginContents();
				GUI.backgroundColor = Color.white;

				GUILayout.BeginHorizontal();
				mouseWheel = EditorGUILayout.Toggle("Mouse Wheel", m.mouseWheelEnabled, GUILayout.Width(140f));
				GUI.contentColor = mouseWheel ? Color.cyan : Color.gray;
				EditorGUILayout.LabelField("Enable Mouse Wheel zoom.");
				GUI.contentColor = Color.white;
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				zoom = EditorGUILayout.Slider(new GUIContent("Zoom", "Current zoom level"), m.zoom, m.minZoom, m.maxZoom);
				if (m.zoom != zoom)
				{
					if (!LeanTween.isTweening(m.gameObject)) m.zoom = Mathf.Clamp(zoom, m.minZoom, m.maxZoom);
					EditorUtility.SetDirty(m);
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();

				EditorGUILayout.LabelField(new GUIContent("Range", "Min and Max level of zoom"), GUILayout.Width(116.0f));
				minZoom = EditorGUILayout.FloatField(m.minZoom, GUILayout.Width(25.0f));
				EditorGUILayout.MinMaxSlider(ref minZoom, ref maxZoom, 1, 30);
				maxZoom = EditorGUILayout.FloatField(m.maxZoom, GUILayout.Width(25.0f));
				GUILayout.EndHorizontal();

				mEasing = (LeanTweenType)EditorGUILayout.EnumPopup("Easing", m.zoomEasing);
				if (m.zoomEasing != mEasing)
				{
					m.zoomEasing = mEasing;
					EditorUtility.SetDirty(m);
				}

				zoomSpeed = EditorGUILayout.Slider(new GUIContent("Speed", "Zoom animation speed"), m.zoomSpeed, 0, 2);

				ShowSelector("Zoom In Key", m.zoomInKey, delegate(KeyCode k) { m.keysInUse[0] = k; m.zoomInKey = k; EditorUtility.SetDirty(m); });
				ShowSelector("Zoom Out Key", m.zoomOutKey, delegate(KeyCode k) { m.keysInUse[1] = k; m.zoomOutKey = k; EditorUtility.SetDirty(m); });

				NJGEditorTools.EndContents();
				EditorGUILayout.Separator();
			}			

			shouldBeOn = NJGEditorTools.DrawHeader("Panning Settings");
			if (shouldBeOn)
			{
				Color c = Color.grey;
				c.a = 0.5f;
				GUI.backgroundColor = c;
				NJGEditorTools.BeginContents();
				GUI.backgroundColor = Color.white;

				m.panning = EditorGUILayout.Toggle("Enabled", m.panning, GUILayout.Width(140f));
				GUI.enabled = m.panning;
				m.panningEasing = (LeanTweenType)EditorGUILayout.EnumPopup("Easing", m.panningEasing);
				m.panningSpeed = EditorGUILayout.Slider(new GUIContent("Speed", "How fast the panning should move"), m.panningSpeed, 0, 5);
				m.panningSensitivity = EditorGUILayout.Slider(new GUIContent("Sensitivy", "How fast the panning should respond on mouse move"), m.panningSensitivity, 0.1f, 10f);
				m.panningMoveBack = EditorGUILayout.Toggle(new GUIContent("Return on Release", "Moves back the panning to its original position"), m.panningMoveBack, GUILayout.Width(140f));
				GUI.enabled = true;
				EditorGUILayout.Separator();

				if (mColor != m.mapColor)
				{
					mColor = m.mapColor;
					if (m.material != null)
					{
						m.material.color = m.mapColor;
						m.material.SetColor("_Color", m.mapColor);
					}
				}

				if (m.minZoom != minZoom ||
					m.maxZoom != maxZoom ||
					m.zoomSpeed != zoomSpeed ||
					m.mouseWheelEnabled != mouseWheel ||
					m.mapBorderRadius != mapBorderRadius ||
					m.calculateBorder != calculateBorder)
				{
					m.mapBorderRadius = mapBorderRadius;
					m.calculateBorder = calculateBorder;
					m.minZoom = minZoom;
					m.maxZoom = maxZoom;
					m.zoomSpeed = zoomSpeed;
					m.mouseWheelEnabled = mouseWheel;
					EditorUtility.SetDirty(m);
				}
				NJGEditorTools.EndContents();
				EditorGUILayout.Separator();
			}

			/*if (mColorMask != m.colorMask)
			{
				mColorMask = m.colorMask;
				if (m.uiTexture != null) m.uiTexture.material.SetColor("_MaskColor", m.colorMask);
			}*/

			if (GUI.changed)
				EditorUtility.SetDirty(m);
		}

		void ShowSelector(string fieldName, KeyCode key, KeySelector.CallbackKeyCode callback)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(fieldName, GUILayout.Width(117f));

			if (GUILayout.Button(key.ToString(), "MiniPullDown"))
			{
				KeySelector.Show(fieldName, key, m.keysInUse, callback);
			}
			GUILayout.EndHorizontal();
		}

		/// <summary>
		/// Draw widget's depth.
		/// </summary>

		void DrawDepth(bool isPrefab)
		{
			if (isPrefab) return;

			GUILayout.Space(2f);
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.PrefixLabel("Depth");

				if (GUILayout.Button("Back", GUILayout.Width(60f)))
				{
					m.depth = m.depth - 1;
					NJGEditorTools.RegisterUndo("Map depth");
				}

				int depth = EditorGUILayout.IntField(m.depth);
				if (m.depth != depth)
				{
					NJGEditorTools.RegisterUndo("Map depth");
					m.depth = depth;
				}

				if (GUILayout.Button("Forward", GUILayout.Width(68f)))
				{
					m.depth = m.depth + 1;
					NJGEditorTools.RegisterUndo("Map depth");
				}
			}
			GUILayout.EndHorizontal();
		}

	}
}
