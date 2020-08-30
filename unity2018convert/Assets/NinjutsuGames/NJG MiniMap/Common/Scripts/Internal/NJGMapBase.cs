//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NJG
{
	[ExecuteInEditMode]
	public class NJGMapBase : MonoBehaviour
	{
		#region Internal Classes
		[System.Serializable]
		public class FOW
		{

			public enum FOWSystem
			{
				BuiltInFOW,
				TasharenFOW
			}

			public bool enabled = false;

			public FOWSystem fowSystem;

			//public bool hideIcons = true;

			/// <summary>
			/// Enables a trail effect when on reveal.
			/// </summary>

			public bool trailEffect;

			/// <summary>
			/// How long it takes to reveal the texture.
			/// </summary>

			public float textureBlendTime = 0.5f;

			/// <summary>
			/// How often FOW textures should be updated.
			/// </summary>

			public float updateFrequency = 0.15f;

			/// <summary>
			/// The FOW color.
			/// </summary>

			public Color fogColor = Color.black;

			/// <summary>
			/// How much should be revealed around each unit.
			/// </summary>

			public int revealDistance = 10;

			/// <summary>
			/// Size of your world in units. For example, if you have a 256x256 terrain, then just leave this at '256'.
			/// </summary>

			//public Vector2 worldSize = new Vector2(256, 256);

			/// <summary>
			/// Size of the fog of war texture. Higher resolution will result in more precise fog of war, at the cost of performance.
			/// </summary>

			public int textureSize = 200;

			/// <summary>
			/// If debugging is enabled, the time it takes to calculate the fog of war will be shown in the log window.
			/// </summary>

			public bool debug = false;

			/// <summary>
			/// How many blur iterations will be performed. More iterations result in smoother edges.
			/// Blurring happens on a separate thread and does not affect performance.
			/// </summary>

			public int blurIterations = 2;
		}

		[System.Serializable]
		public class MapItemType
		{
			public bool enableInteraction = true;
			public string type = "New Marker";
			public string sprite;
			public string selectedSprite;
			public bool useCustomSize = false;
			public bool useCustomBorderSize = false;
			public int size = 32;
			public int borderSize = 32;
			public Color color = Color.white;
			public bool animateOnVisible = true;
			public bool showOnAction = false;
			public bool loopAnimation = false;
			public float fadeOutAfterDelay = 0;
			public bool rotate = true;
			public bool updatePosition = true;
			public bool haveArrow = false;
			public string arrowSprite;
			public bool folded = true;
			public int depth = 0;
			public bool deleteRequest = false;
			public int arrowOffset = 20;
			public int arrowDepth = 5;
			public bool arrowRotate = true;
			//public bool revealFOW = false;

			public void OnSelectSprite(string spriteName)
			{
				sprite = spriteName;
			}

			public void OnSelectBorderSprite(string spriteName)
			{
				selectedSprite = spriteName;
			}

			public void OnSelectArrowSprite(string spriteName)
			{
				arrowSprite = spriteName;
			}
		}

		[System.Serializable]
		public class MapLevel
		{
			public string level = "Level";
			public List<MapZone> zones = new List<MapZone>();
			public bool folded = true;
			public bool itemsFolded = true;
			public bool deleteRequest = false;
		}

		[System.Serializable]
		public class MapZone
		{
			public string type = "New Zone";
			public Color color = Color.white;
			public float fadeOutAfterDelay = 3;
			public bool folded = true;
			public int depth = 0;
			public bool deleteRequest = false;
		}
		#endregion

		#region Enums

		public enum SettingsScreen
		{
			General,
			Icons,
			FOW,
			Zones,
			_LastDoNotUse
		}

		public enum Resolution
		{
			Normal,
			Double
		}

		public enum RenderMode
		{
			/// <summary>
			/// Render the map only once the system is initiated.
			/// </summary>
			Once,

			/// <summary>
			/// Render every time the screen changes of size.
			/// </summary>
			ScreenChange,

			/// <summary>
			/// Use this mode if you need to update the map often manually.
			/// </summary>
			Dynamic
		}

		public enum NJGTextureFormat
		{
			ARGB32 = TextureFormat.ARGB32,
			RGB24 = TextureFormat.RGB24
		}

		public enum NJGCameraClearFlags
		{
			Skybox = CameraClearFlags.Skybox,
			Depth = CameraClearFlags.Depth,
			Color = CameraClearFlags.Color,
			Nothing = CameraClearFlags.Nothing,
		}

		public enum ShaderType
		{
			TextureMask = 0,
			ColorMask = 1,
			FOW = 2
		}

		[SerializeField]
		public enum Orientation
		{
			XZDefault = 0,
			XYSideScroller = 1
		}

		#endregion

		[SerializeField]
		public UIMiniMapBase miniMap;
		[SerializeField]
		public UIWorldMapBase worldMap;

		static NJGMapBase mInst;
		static public NJGMapBase instance { get { if (mInst == null) mInst = GameObject.FindObjectOfType(typeof(NJGMapBase)) as NJGMapBase; return mInst; } }

		static GameObject mZRoot;
		static public GameObject zonesRoot { get { if (mZRoot == null) mZRoot = GameObject.Find("_MapZones"); return mZRoot; } set { mZRoot = value; } }

		//public delegate void OnWorldNameChanged(string name);
		public System.Action<string> onWorldNameChanged;

		[SerializeField]
		public FOW fow;

		/// <summary>
		/// The name of the world that is going to be displayed on World Map and Mini Map titles.
		/// </summary>

		[SerializeField]
		public string worldName
		{
			get { return mWorldName; }
			set
			{
				mWorldName = value;

				// Only trigger this event if the name is different than the last one
				if (mLastWorldName != mWorldName)
				{
					mLastWorldName = mWorldName;
					if(onWorldNameChanged != null) onWorldNameChanged(mWorldName);
				}
			}
		}

		/// <summary>
		/// Draw bounds on the scene view.
		/// </summary>

		[SerializeField]
		public bool showBounds = true;

		/// <summary>
		/// Current zone color.
		/// </summary>

		[SerializeField]
		public Color zoneColor = Color.white;

		/// <summary>
		/// List of map item types.
		/// </summary>

		public List<MapItemType> mapItems = new List<MapItemType>(new MapItemType[] { new MapItemType(){ type = "None" } });

		/// <summary>
		/// List of zones.
		/// </summary>

		public List<MapLevel> levels = new List<MapLevel>();

		/// <summary>
		/// Map render mode.
		/// </summary>

		[SerializeField]
		public RenderMode renderMode = RenderMode.Once;

		/// <summary>
		/// This instance wont be destroyed if true.
		/// </summary>

		public bool persistOnLevelLoad;

		/// <summary>
		/// If true it will call NJGMap.instance.GenerateMap(); everytime a level is loaded.
		/// </summary>

		public bool renderOnLevelLoad;

		/// <summary>
		/// If true the instance of NJGMap will be persistence.
		/// </summary>

		public bool dontDestroy;

		/// <summary>
		/// Map resolution.
		/// </summary>

		[SerializeField]
		public Resolution mapResolution = Resolution.Normal;

		/// <summary>
		/// If Dynamic render mode is choosen use this parameter to make the map render every each (dynamicRenderTime).
		/// </summary>

		[SerializeField]
		public float dynamicRenderTime = 1f;

		/// <summary>
		/// Map orientation.
		/// </summary>

		//[SerializeField]
		public Orientation orientation = Orientation.XZDefault;

		/// <summary>
		/// Current settings screen. Internal use only.
		/// </summary>

		//[SerializeField]
		public SettingsScreen screen = SettingsScreen.General;

		/// <summary>
		/// Which layers is the map going to render.
		/// </summary>

		[SerializeField]
		public LayerMask renderLayers = 1;

		/// <summary>
		/// Which layers are going to be used for bounds calculation.
		/// </summary>

		[SerializeField]
		public LayerMask boundLayers = 1;

		/// <summary>
		/// Global size of the map icons.
		/// </summary>

		public int iconSize = 16;

		/// <summary>
		/// Global size of the border map icons.
		/// </summary>

		public int borderSize = 16;

		/// <summary>
		/// Global size of the map arrows.
		/// </summary>

		public int arrowSize = 16;

		/// <summary>
		/// How often the map will be updated.
		/// </summary>

		public float updateFrequency = 0.01f;

		/// <summary>
		/// True if you want to define the bounds manually.
		/// </summary>

		public bool setBoundsManually = false;

		/// <summary>
		/// You can set the bounds manually if setBoundsManually is true.
		/// </summary>

		[SerializeField]
		public Vector3 customBounds = new Vector3(10, 10, 10);

		public Vector3 customBoundsCenter = Vector3.zero;

		/// <summary>
		/// World bounds.
		/// </summary>

		[SerializeField]
		public Bounds bounds;

		/// <summary>
		/// Internally used by the inspector to save fold state of map item types.
		/// </summary>

		public bool typesFolded;

		/// <summary>
		/// Internally used by the inspector to save fold state of zones.
		/// </summary>

		public bool zonesFolded;

		/// <summary>
		/// Texture of the map.
		/// </summary>

		public Texture2D mapTexture;

		/// <summary>
		/// User defined texture of the map.
		/// </summary>

		public Texture2D userMapTexture;

		/// <summary>
		/// If true the map texture will be generated.
		/// </summary>

		public bool generateMapTexture;

		/// <summary>
		/// If true the map texture will be generated at start, if false the method GenerateMap() must be called manually.
		/// </summary>

		public bool generateAtStart = true;

		/// <summary>
		/// The camera that is going to be used to draw the frustum.
		/// </summary>

		public Camera cameraFrustum;

		/// <summary>
		/// Color for the frustum object.
		/// </summary>

		public Color cameraFrustumColor = new Color(255f, 255f, 255f, 50f);

		/// <summary>
		/// Use this to check when mouse is over the minimap UI.
		/// </summary>

		public virtual bool isMouseOver
		{
			get
			{
				return ((UIMiniMapBase.inst == null) || UIMiniMapBase.inst.isMouseOver) || ((UIWorldMapBase.inst == null) || UIWorldMapBase.inst.isMouseOver);
			}
		}

		public bool useTextureGenerated;

		[SerializeField]
		public FilterMode mapFilterMode = FilterMode.Bilinear;
		[SerializeField]
		public TextureWrapMode mapWrapMode = TextureWrapMode.Clamp;
		[SerializeField]
		public NJGTextureFormat textureFormat = NJGTextureFormat.ARGB32;
		[SerializeField]
		public NJGCameraClearFlags cameraClearFlags = NJGCameraClearFlags.Skybox;
		public Color cameraBackgroundColor = Color.red;
		public bool transparentTexture = false;
#if UNITY_EDITOR
		[SerializeField]
		public TextureCompressionQuality compressQuality = TextureCompressionQuality.Fast;
#endif
		public bool optimize = false;
		public bool generateMipmaps = false;
		public int boundsOffset = 10;
		public int layer = 0;

		[SerializeField]
		public Vector3 mapOrigin { get { if (NJGMapRenderer.instance != null) mMapOrigin = NJGMapRenderer.instance.cachedTransform.position; return mMapOrigin; } }
		[SerializeField]
		public Vector3 mapEulers { get { if (NJGMapRenderer.instance != null) mMapEulers = NJGMapRenderer.instance.cachedTransform.eulerAngles; return mMapEulers; } }
		[SerializeField]
		public float ortoSize { get { if (NJGMapRenderer.instance != null) mOrtoSize = NJGMapRenderer.instance.GetComponent<Camera>().orthographicSize; return mOrtoSize; } }
		[SerializeField]
		public float aspect { get { if (NJGMapRenderer.instance != null) mAspect = NJGMapRenderer.instance.GetComponent<Camera>().aspect; return mAspect; } }

		/// <summary>
		/// Map renderer.
		/// </summary>

		//public NJGMapRenderer mapRenderer;

		/// <summary>
		/// Map size.
		/// </summary>
		[SerializeField]
		public Vector2 mapSize
		{
			set { mSize = value; }
			get
			{
				if (Application.isPlaying)
				{
					mSize.x = Screen.width;
					mSize.y = Screen.height;
				}
				return mSize;
			}
		}

		public float elapsed { get { return mElapsed; } }

		//public System.Action<Texture2D> onTextureChanged;

		public List<System.Action> queue = new List<System.Action>();

		public const string VERSION = "1.5.6";

        public Camera mCam;

		Vector2 mSize = new Vector2(1024, 1024);

		Bounds mBounds;
		[SerializeField]
		string mWorldName = "My Epic World";
		string mLastWorldName;
		Vector3 mMapOrigin = Vector2.zero;
		Vector3 mMapEulers = Vector2.zero;
		float mOrtoSize = 0;
		float mAspect = 0;
		Thread mThread;
		float mElapsed = 0f;

		/// <summary>
		/// Get a string list of map item types.
		/// </summary>

		[SerializeField]
		public string[] mapItemTypes
		{
			get
			{
				List<string> types = new List<string>();
				//types.Add("None");
				//if (mapItems != null)
				//{
					for (int i = 0, imax = mapItems.Count; i < imax; i++)
						types.Add(mapItems[i].type);
				//}

				return types.Count == 0 ? new string[] { "No types defined" } : types.ToArray();
			}
		}

		/// <summary>
		/// Setup map renderer.
		/// </summary>

		void Awake()
		{
			if (fow.textureSize < 200) fow.textureSize = 200;
			if(miniMap == null) miniMap = GameObject.FindObjectOfType(typeof(UIMiniMapBase)) as UIMiniMapBase;
			if (worldMap == null) worldMap = GameObject.FindObjectOfType(typeof(UIWorldMapBase)) as UIWorldMapBase;
			//Holoville.HOTween.HOTween.Init(false, true, true);
			//Holoville.HOTween.HOTween.EnableOverwriteManager(false);			
		
			if (Application.isPlaying)
			{
				if (mapTexture != null) NJGTools.Destroy(mapTexture);
				if(generateAtStart) GenerateMap();
			}
		}

		void OnDrawGizmos()
		{
			if (showBounds)
			{
				Gizmos.color = new Color(1, 0, 0, 0.5F);
				//Gizmos.DrawCube(bounds.center, bounds.size);
				Gizmos.DrawWireCube(bounds.center, bounds.size);
			}
		}

		/// <summary>
		/// Need to re-generate the texture map? Use this method at will.
		/// </summary>

		public void GenerateMap()
		{
			if ((Application.isPlaying && generateMapTexture) || (!Application.isPlaying && !generateMapTexture))
			{
				//if (mapRenderer == null)
				//	mapRenderer = NJGMapRenderer.instance;

				NJGMapRenderer.instance.Render();
			}
		}

		/// <summary>
		/// Update bounds on start.
		/// </summary>

		void Start()
		{
			if (onWorldNameChanged != null) onWorldNameChanged(worldName);
			UpdateBounds();

			if (!Application.isPlaying) return;

			// Add a thread update function -- all checks will be done on a separate thread
			/*if (mThread == null)
			{
				mThread = new Thread(ThreadUpdate);
				Debug.Log("Start Thread");
				mThread.Start();
			}*/

			if (fow.enabled)
			{
				NJGFOW.instance.Init();
			}
		}


		/// <summary>
		/// Ensure that the thread gets terminated.
		/// </summary>

		/*protected virtual void OnDestroy()
		{
			if (!Application.isPlaying) return;

			if (mThread != null)
			{
				mThread.Abort();
				while (mThread.IsAlive) Thread.Sleep(1);
				mThread = null;
			}
			queue.Clear();
		}*/

		/// <summary>
		/// If it's time to update, do so now.
		/// </summary>

		void ThreadUpdate()
		{
			System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

			for (; ; )
			{
				//if (mNeedUpdate)
				//{
				sw.Reset();
				sw.Start();

#if UNITY_EDITOR
				Debug.Log("ThreadUpdate " + queue.Count);
				try
				{
					queue.ForEach(a => a.Invoke());
				}
				catch (System.Exception error)
				{
					Debug.LogWarning("Error " + error.ToString() + " / " + error.Message);
				}
#else
				queue.ForEach(a => a());
#endif
				/*for (int i = 0, imax = mapItems.Count; i < imax; i++)
				{
					if (mapItems[i].type == mSearchType)
					{
						mType = mapItems[i];
						mSearchType = null;
						break;
					}
				}*/
				sw.Stop();
				//if (debug) Debug.Log(sw.ElapsedMilliseconds);
				mElapsed = 0.001f * (float)sw.ElapsedMilliseconds;
				//mNeedUpdate = false;
				//}
				Thread.Sleep(1);
			}
		}

		/// <summary>
		/// Sets the texture for the UITexture of the Minimap and World Map.
		/// </summary>

		public void SetTexture(Texture2D tex)
		{
			if (UIMiniMapBase.inst != null) UIMiniMapBase.inst.material.mainTexture = tex;
			if(UIWorldMapBase.inst != null) UIWorldMapBase.inst.material.mainTexture = tex;
			//if (onTextureChanged != null) onTextureChanged(tex);
		}

		/// <summary>
		/// Checks if GameObject is within render layers range.
		/// </summary>

		public static bool IsInRenderLayers(GameObject obj, LayerMask mask)
		{
			return (mask.value & (1 << obj.layer)) > 0;
		}

		/// <summary>
		/// Create bounding box and scale it to contain all scene game objects, if terrain is found it is used
		/// </summary>

		Terrain[] mTerrains;

		public void UpdateBounds()
		{
			if (setBoundsManually)
			{
				//mBounds = new Bounds(customBounds * 0.5f, customBounds);
				mBounds.size = customBounds;
				mBounds.center = customBoundsCenter;
				mBounds.Expand(new Vector3(boundsOffset, 0, boundsOffset));
				bounds = mBounds;
				return;
			}

			bool flag = false;
			int i, imax = 0;

			mTerrains = FindObjectsOfType(typeof(Terrain)) as Terrain[];
			bool multiTerrain = mTerrains != null;
			if (multiTerrain) multiTerrain = mTerrains.Length > 1;

			// First lets see if there is more than one terrain. Multi-terrain handling
			if (multiTerrain)
			{
#if UNITY_EDITOR
			//	Debug.Log("NJGMap: Calculating bounds for multiple terrains ("+mTerrains.Length+")");
#endif
				for (i = 0, imax = mTerrains.Length; i < imax; i++ )
				{
					Terrain t = mTerrains[i];
					MeshRenderer mMeshRenderer = t.GetComponent<MeshRenderer>();

					if (!flag)
					{
						//t.transform.position, new Vector3(1f, 1f, 1f)
						mBounds = new Bounds();
						flag = true;
					}

					if (mMeshRenderer != null)
					{
						//Debug.Log("Terrain Mesh Renderer " + i + " : " + mMeshRenderer.bounds.size + " / " + t.name);
						mBounds.Encapsulate(mMeshRenderer.bounds);
					}
					else
					{
						TerrainCollider mTerrainCollider = t.GetComponent<TerrainCollider>();
						if (mTerrainCollider != null)
						{
							//Debug.Log("Terrain Collider " + i + " : " + mTerrainCollider.bounds.size+" / "+t.name);
							mBounds.Encapsulate(mTerrainCollider.bounds);
						}
						else
						{
							Debug.LogError("Could not get measure bounds of terrain.", this);
							return;
						}
					}
				}				
			}
			// If not then check if there is one activeTerrain
			else if (Terrain.activeTerrain != null)
			{
#if UNITY_EDITOR
				//Debug.Log("NJGMap: Calculating bounds for active terrain");
#endif
				Terrain t = Terrain.activeTerrain;
				MeshRenderer mMeshRenderer = t.GetComponent<MeshRenderer>();

				if (!flag)
				{
					//t.transform.position, new Vector3(1f, 1f, 1f)
					mBounds = new Bounds();
					flag = true;
				}

				if (mMeshRenderer != null)
				{
					//Debug.Log("Terrain Mesh Renderer " + i + " : " + mMeshRenderer.bounds.size + " / " + t.name);
					mBounds.Encapsulate(mMeshRenderer.bounds);
				}
				else
				{
					TerrainCollider mTerrainCollider = t.GetComponent<TerrainCollider>();
					if (mTerrainCollider != null)
					{
						//Debug.Log("Terrain Collider " + i + " : " + mTerrainCollider.bounds.size+" / "+t.name);
						mBounds.Encapsulate(mTerrainCollider.bounds);
					}
					else
					{
						Debug.LogError("Could not get measure bounds of terrain.", this);
						return;
					}
				}
			}
			
			GameObject[] mGameObjects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			if (mGameObjects != null)
			{
#if UNITY_EDITOR
				//Debug.Log("NJGMap: Calculating bounds for multiple gameObjects (" + mGameObjects.Length + ")");
#endif
				for (i = 0, imax = mGameObjects.Length; i < imax; i++)
				{
					GameObject go = mGameObjects[i];

					// Dont consider this game object
					if (go.layer == gameObject.layer)
						continue;

					// Only use objects from the layer mask
					if (!IsInRenderLayers(go, boundLayers))
						continue;

					if (!flag)
					{
						mBounds = new Bounds(go.transform.position, new Vector3(1f, 1f, 1f));
						flag = true;
					}
					Renderer renderer = go.GetComponent<Renderer>();
					if (renderer != null)
					{
						mBounds.Encapsulate(renderer.bounds);
					}
					else
					{
						Collider collider = go.GetComponent<Collider>();
						if (collider != null)
						{
							mBounds.Encapsulate(collider.bounds);
						}
					}
				}
			}			

			if (!flag)
			{
				Debug.Log("Could not find terrain nor any other bounds in scene", this);
				mBounds = new Bounds(gameObject.transform.position, new Vector3(1f, 1f, 1f));
			}

			mBounds.Expand(new Vector3(boundsOffset, 0, boundsOffset));

			if (mapResolution == Resolution.Double)
			{
				//mBounds.Expand(new Vector3(-mBounds.extents.x, 0f, -mBounds.extents.z));
			}

			// Set bounds
			bounds = mBounds;
		}

		#region Getters

		public string[] GetZones(string level)
		{
			List<string> list = new List<string>();

			if (levels != null)
			{
				for (int i = 0, imax = levels.Count; i < imax; i++)
				{
					if (levels[i].level == level)
					{
						for (int e = 0, emax = levels[i].zones.Count; e < emax; e++)
						{
							list.Add(levels[i].zones[e].type);
						}
					}
				}
			}

			return list.Count == 0 ? new string[] { "No Zones defined" } : list.ToArray();
		}

		/// <summary>
		/// Get zone by scene.
		/// </summary>

		public string[] GetLevels()
		{
			List<string> list = new List<string>();

			if (levels != null)
			{
				for (int i = 0, imax = levels.Count; i < imax; i++)
					list.Add(levels[i].level);
			}

			return list.Count == 0 ? new string[] { "No Levels defined" } : list.ToArray();
		}

		/// <summary>
		/// Get color from zone.
		/// </summary>

		public Color GetZoneColor(string level, string zone)
		{
			Color c = Color.white;
			for (int i = 0, imax = levels.Count; i < imax; i++)
			{
				if (levels[i].level == level)
				{
					for (int e = 0, emax = levels[i].zones.Count; e < emax; e++)
					{
						if (levels[i].zones[e].type.Equals(zone)) return levels[i].zones[e].color;
					}
				}
			}

			return c;
		}

		/// <summary>
		/// Get interaction.
		/// </summary>

		public bool GetInteraction(int type) { return Get(type) == null ? false : Get(type).enableInteraction; }

		/// <summary>
		/// Get color from type.
		/// </summary>

		public Color GetColor(int type) { return Get(type) == null ? Color.white : Get(type).color; }

		/// <summary>
		/// Get animate from type.
		/// </summary>

		public bool GetAnimateOnVisible(int type) { return Get(type) == null ? false : Get(type).animateOnVisible; }

		/// <summary>
		/// Get animate on action from type.
		/// </summary>

		public bool GetAnimateOnAction(int type) { return Get(type) == null ? false : Get(type).showOnAction; }

		/// <summary>
		/// Get animate from type.
		/// </summary>

		public bool GetLoopAnimation(int type) { return Get(type) == null ? false : Get(type).loopAnimation; }

		/// <summary>
		/// Get have arrow from type.
		/// </summary>

		public bool GetHaveArrow(int type) { return Get(type) == null ? false : Get(type).haveArrow; }

		/// <summary>
		/// Get animate from type.
		/// </summary>

		public float GetFadeOutAfter(int type) { return Get(type) == null ? 0 : Get(type).fadeOutAfterDelay; }

		/// <summary>
		/// Get rotate flag.
		/// </summary>

		public bool GetRotate(int type) { return Get(type) == null ? false : Get(type).rotate; }

		/// <summary>
		/// Get arrow rotate flag.
		/// </summary>

		public bool GetArrowRotate(int type) { return Get(type) == null ? false : Get(type).arrowRotate; }

		/// <summary>
		/// Get update position flag.
		/// </summary>

		public bool GetUpdatePosition(int type) { return Get(type) == null ? false : Get(type).updatePosition; }

		/// <summary>
		/// Get custom icon size.
		/// </summary>

		public int GetSize(int type) { return Get(type) == null ? 0 : Get(type).size; }

		/// <summary>
		/// Get custom icon size.
		/// </summary>

		public int GetBorderSize(int type) { return Get(type) == null ? 0 : Get(type).borderSize; }

		/// <summary>
		/// Get custom icon size flag.
		/// </summary>

		public bool GetCustom(int type) { return Get(type) == null ? false : Get(type).useCustomSize; }

		/// <summary>
		/// Get custom icon size flag.
		/// </summary>

		public bool GetCustomBorder(int type) { return Get(type) == null ? false : Get(type).useCustomBorderSize; }

		/// <summary>
		/// Get depth.
		/// </summary>

		public int GetDepth(int type) { return Get(type) == null ? 0 : Get(type).depth; }

		/// <summary>
		/// Get arrow depth.
		/// </summary>

		public int GetArrowDepth(int type) { return Get(type) == null ? 0 : Get(type).arrowDepth; }

		/// <summary>
		/// Get arrow offset.
		/// </summary>

		public int GetArrowOffset(int type) { return Get(type) == null ? 0 : Get(type).arrowOffset; }

		/// <summary>
		/// Get map item type.
		/// </summary>

		public MapItemType Get(int type)
		{
			/*for (int i = 0, imax = mapItems.Count; i < imax; ++i)
			{
				if (mapItems[i].type.Equals(type))
					return mapItems[type];
			}*/
			if (type == -1) return null;
			if (type > mapItems.Count) return null;
			MapItemType mRes = mapItems[type];
			return mRes == null ? null : mRes;
		}

		#endregion

#if UNITY_EDITOR

		/// <summary>
		/// Update layer mask for UICamera and Camera inside
		/// </summary>

		protected virtual void Update()
		{
			if (!Application.isPlaying)
			{
				//if (mCam == null) mCam = GetComponentInChildren<Camera>();
				//if (mCam != null) if (mCam.cullingMask != 1 << gameObject.layer) mCam.cullingMask = 1 << gameObject.layer;

				if (mapTexture != null) NJGTools.DestroyImmediate(mapTexture);
			}
		}
#endif
	}
}

