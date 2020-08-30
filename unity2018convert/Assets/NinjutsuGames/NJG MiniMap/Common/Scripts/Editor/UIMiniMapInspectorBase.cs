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

	public class UIMiniMapInspectorBase : Editor
	{
		protected UIMiniMapBase m;
		Texture mMask;
		Color mColor;
		//Material mMat;
		LeanTweenType mEasing;
		bool usePanelForIcons;
		bool limitBounds;
		bool rotateWithPlayer;
		bool mouseWheel;
		Transform targetObj;
		string targetTag;
		float zoom;
		float zoomAmount;
		float minZoom;
		float maxZoom;
		float zoomSpeed;
		float mapBorderRadius;
		GameObject northIcon;
		int northIconOffset;
		KeyCode mapKey;
		KeyCode zoomInKey;
		KeyCode zoomOutKey;
		bool calculateBorder;
		NJGMapBase.ShaderType mShader;
		UIMiniMapBase.Pivot mPivot;
		Vector2 mMargin;
		Vector2 mMapScale;
		bool panning;
		float panningSpeed;
		float panningSensitivity;
        LeanTweenType panningEase;
		bool panningMoveBack;
		//UIAnchor mAnchor;

		protected virtual void DrawNotFound() { }

		/// <summary>
		/// Draw the inspector.
		/// </summary>

		public override void OnInspectorGUI()
		{
#if UNITY_4_3
			EditorGUIUtility.LookLikeControls(120f);
#else
			EditorGUIUtility.labelWidth = 120f;
#endif
			m = target as UIMiniMapBase;

			PrefabType type = PrefabUtility.GetPrefabType(m.gameObject);

			if (m.material == null) m.material = NJGEditorTools.GetMaterial(m, true);

			NJGEditorTools.DrawEditMap();

			DrawNotFound();

			targetObj = (Transform)EditorGUILayout.ObjectField(new GUIContent("Map Target", "The object that this map is going to follow"), m.target, typeof(Transform), true);

			if (m.target == null)
			{
				if (Application.isPlaying)
					EditorGUILayout.HelpBox("No target has been found, assign the tag to your target or drag it manually to the target field.", MessageType.Warning);
				else
					EditorGUILayout.HelpBox("No target has been assigned, the target can be set automatically by using a tag or drag it manually to the target field.", MessageType.Warning);
			}

			targetTag = EditorGUILayout.TagField(new GUIContent("Target Tag", "Assign a tag to auto search for the Map Target"), m.targetTag);

			GUILayout.BeginHorizontal();
			limitBounds = EditorGUILayout.Toggle("Limit Map Bounds", m.limitBounds, GUILayout.Width(140f));
			GUI.contentColor = limitBounds ? Color.cyan : Color.gray;
			EditorGUILayout.LabelField("Prevent map to display beyond borders.");
			GUI.contentColor = Color.white;
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			rotateWithPlayer = EditorGUILayout.Toggle("Lock Rotation", m.rotateWithPlayer, GUILayout.Width(140f));
			GUI.contentColor = rotateWithPlayer ? Color.cyan : Color.gray;
			EditorGUILayout.LabelField("Makes the map follow target rotation.");
			GUI.contentColor = Color.white;
			GUILayout.EndHorizontal();

			ShowSelector("World Map Key", m.mapKey, delegate(KeyCode k) { m.keysInUse[2] = k; m.mapKey = k; EditorUtility.SetDirty(m); });
			ShowSelector("Lock Key", m.lockKey, delegate(KeyCode k) { m.keysInUse[0] = k; m.lockKey = k; EditorUtility.SetDirty(m); });

			mShader = (NJGMapBase.ShaderType)EditorGUILayout.EnumPopup("Shader Type", m.shaderType);
			if (mShader == NJGMapBase.ShaderType.ColorMask)
			{
				EditorGUILayout.HelpBox("Use the camera background color for masking", MessageType.Info);
				//m.colorMask = EditorGUILayout.ColorField("Color Mask", m.colorMask);
			}

			/*if (mShader != NJGMapBase.ShaderType.FOW)
			{
				NJGMapBase.instance.fow.enabled = true;
				m.shaderType = mShader = NJGMapBase.ShaderType.FOW;
				m.material.shader = Shader.Find("NinjutsuGames/Map FOW");
				NJGEditorTools.RegisterUndo("UIMiniMap Setting", m);
				//EditorGUILayout.HelpBox("Fog of War is enabled\nIn order to make it work the shader type should be FOW\nIgnore this warning if you don't want to use FOW for this instance.", MessageType.Warning);
			}
			else
			{
				NJGMapBase.instance.fow.enabled = false;
				m.shaderType = mShader = NJGMapBase.ShaderType.TextureMask;
				m.material.shader = Shader.Find("NinjutsuGames/Map TextureMask");
				NJGEditorTools.RegisterUndo("UIMiniMap Setting", m);
			}*/

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
					if(Application.isPlaying) m.planeRenderer.material.shader = s;
					else m.planeRenderer.sharedMaterial.shader = s;
				}
				NJGEditorTools.RegisterUndo("UIMiniMap Shader Type", m);
			}

			m.maskTexture = (Texture2D)EditorGUILayout.ObjectField("Mask", m.maskTexture, typeof(Texture2D), false);

			if (mMask != m.maskTexture)
			{
				mMask = m.maskTexture;
				m.material.SetTexture("_Mask", m.maskTexture);
				NJGEditorTools.RegisterUndo("UIMiniMap Setting", m);
			}

			bool shouldBeOn = NJGEditorTools.DrawHeader("UI Settings");
			if (shouldBeOn)
			{
				Color c = Color.grey;
				c.a = 0.5f;
				GUI.backgroundColor = c;
				NJGEditorTools.BeginContents();
				GUI.backgroundColor = Color.white;

				DrawDepth(type == PrefabType.Prefab);

				mPivot = (UIMiniMapBase.Pivot)EditorGUILayout.EnumPopup("Alignment", m.pivot);
				if (m.pivot != mPivot)
				{
					m.pivot = mPivot;
					EditorUtility.SetDirty(m);
					m.UpdateAlignment();
				}

				m.mapColor = EditorGUILayout.ColorField("Color", m.mapColor);

				if (mColor != m.mapColor)
				{
					mColor = m.mapColor;
					if (m.material != null)
						m.material.color = m.mapColor;

					NJGEditorTools.RegisterUndo("UIMiniMap Setting", m);
				}

				/*GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Dimensions", GUILayout.Width(116f));
				EditorGUIUtility.LookLikeControls(10f);
				mMapScale.x = EditorGUILayout.IntField((int)m.mapScale.x);
				EditorGUIUtility.LookLikeControls(12f);
				EditorGUILayout.LabelField("x", GUILayout.Width(15f));				
				mMapScale.y = EditorGUILayout.IntField((int)m.mapScale.y);
				EditorGUIUtility.LookLikeControls(80f);
				GUILayout.EndHorizontal();*/

				mMapScale = EditorGUILayout.Vector2Field("Dimensions", m.mapScale);

				if (m.mapScale != mMapScale)
				{
					mMapScale.x = (int)mMapScale.x;
					mMapScale.y = (int)mMapScale.y;
					m.mapScale = mMapScale;
					NJGEditorTools.RegisterUndo("UIMiniMap Setting", m);
					m.UpdateAlignment();
				}

				/*GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Margin", GUILayout.Width(116f));
				EditorGUIUtility.LookLikeControls(10f);
				mMargin.x = EditorGUILayout.IntField((int)m.margin.x);
				EditorGUIUtility.LookLikeControls(12f);
				EditorGUILayout.LabelField("x", GUILayout.Width(15f));
				mMargin.y = EditorGUILayout.IntField((int)m.margin.y);
				EditorGUIUtility.LookLikeControls(80f);
				GUILayout.EndHorizontal();*/

				mMargin = EditorGUILayout.Vector2Field("Margin", m.margin);

				if (m.margin != mMargin)
				{
					m.margin.x = (int)mMargin.x;
					m.margin.y = (int)mMargin.y;
					NJGEditorTools.RegisterUndo("UIMiniMap Setting", m);
					m.UpdateAlignment();
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

				DrawFrameUI();

				northIcon = (GameObject)EditorGUILayout.ObjectField(new GUIContent("North Icon", "Optional north icon. Will be automatically placed if its assigned."), m.northIcon, typeof(GameObject), true);
				//if (northIcon != null)
				//	northIconOffset = EditorGUILayout.IntField(new GUIContent("North Icon Offset", "Adjust the north icon distance from map border"), m.northIconOffset);
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

				//EditorGUILayout.LabelField("Zoom Settings", EditorStyles.boldLabel);

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
					NJGEditorTools.RegisterUndo("UIMiniMap Setting", m);
				}
				GUILayout.EndHorizontal();

				zoomAmount = EditorGUILayout.Slider(new GUIContent("Amount", "How much should zoom in/out when zoom function is called."), m.zoomAmount, 0.01f, 5);

				GUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(new GUIContent("Range", "Min and Max level of zoom"), GUILayout.Width(116.0f));
				minZoom = EditorGUILayout.FloatField(m.minZoom, GUILayout.Width(25.0f));
				EditorGUILayout.MinMaxSlider(ref minZoom, ref maxZoom, 1, 30);
				maxZoom = EditorGUILayout.FloatField(m.maxZoom, GUILayout.Width(25.0f));
				//minZoom = Mathf.Round(minZoom);
				//maxZoom = Mathf.Round(maxZoom);
				GUILayout.EndHorizontal();

                mEasing = (LeanTweenType)EditorGUILayout.EnumPopup("Easing", m.zoomEasing);
				if (m.zoomEasing != mEasing)
				{
					m.zoomEasing = mEasing;
					NJGEditorTools.RegisterUndo("UIMiniMap Setting", m);
				}

				zoomSpeed = EditorGUILayout.Slider(new GUIContent("Speed", "Zoom animation speed"), m.zoomSpeed, 0f, 2f);

				ShowSelector("Zoom In Key", m.zoomInKey, delegate(KeyCode k) { m.keysInUse[0] = k; m.zoomInKey = k; NJGEditorTools.RegisterUndo("UIMiniMap Setting", m); });
				ShowSelector("Zoom Out Key", m.zoomOutKey, delegate(KeyCode k) { m.keysInUse[1] = k; m.zoomOutKey = k; NJGEditorTools.RegisterUndo("UIMiniMap Setting", m); });

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

				panning = EditorGUILayout.Toggle("Enabled", m.panning, GUILayout.Width(140f));
				GUI.enabled = panning;
                panningEase = (LeanTweenType)EditorGUILayout.EnumPopup("Easing", m.panningEasing);
				panningSpeed = EditorGUILayout.Slider(new GUIContent("Speed", "How fast the panning should move"), m.panningSpeed, 0, 5);
				panningSensitivity = EditorGUILayout.Slider(new GUIContent("Sensitivy", "How fast the panning should respond on mouse move"), m.panningSensitivity, 0.1f, 10f);
				panningMoveBack = EditorGUILayout.Toggle(new GUIContent("Return on Release", "Moves back the panning to its original position"), m.panningMoveBack, GUILayout.Width(140f));
				GUI.enabled = true;
				NJGEditorTools.EndContents();
				EditorGUILayout.Separator();
			}

			if (m.limitBounds != limitBounds ||
				m.rotateWithPlayer != rotateWithPlayer ||
				m.target != targetObj ||
				m.targetTag != targetTag ||
				m.minZoom != minZoom ||
				m.maxZoom != maxZoom ||
				m.zoomSpeed != zoomSpeed ||
				m.mapBorderRadius != mapBorderRadius ||
				m.northIcon != northIcon ||
				//m.northIconOffset != northIconOffset ||
				m.calculateBorder != calculateBorder ||
				m.mouseWheelEnabled != mouseWheel ||
				m.panning != panning ||
				m.panningSpeed != panningSpeed ||
				m.panningEasing != panningEase ||
				m.panningMoveBack != panningMoveBack ||
				m.panningSensitivity != panningSensitivity ||
				m.zoomAmount != zoomAmount)
			{
				m.zoomAmount = zoomAmount;
				m.panningSensitivity = panningSensitivity;
				m.panningMoveBack = panningMoveBack;
				m.panning = panning;
				m.panningSpeed = panningSpeed;
				m.panningEasing = panningEase;
				m.limitBounds = limitBounds;
				m.rotateWithPlayer = rotateWithPlayer;
				m.target = targetObj;
				m.targetTag = targetTag;
				m.minZoom = minZoom;
				m.maxZoom = maxZoom;
				m.zoomSpeed = zoomSpeed;
				m.mapBorderRadius = mapBorderRadius;
				m.northIcon = northIcon;
				//m.northIconOffset = northIconOffset;
				m.calculateBorder = calculateBorder;
				m.mouseWheelEnabled = mouseWheel;
				NJGEditorTools.RegisterUndo("Changed Minimap Settings", m);
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty(m);
			}
		}

		protected virtual void DrawFrameUI() { }

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
	}
}
