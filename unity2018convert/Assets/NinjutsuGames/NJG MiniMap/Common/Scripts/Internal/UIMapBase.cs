//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UIMap base class.
/// </summary>

namespace NJG
{
	public abstract class UIMapBase : MonoBehaviour
	{

		public enum Pivot
		{
			BottomLeft,
			Left,
			TopLeft,
			Top,
			TopRight,
			Right,
			BottomRight,
			Bottom,
			Center,
		}

		/// <summary>
		/// Cache transform for speed.
		/// </summary>

		public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

		/// <summary>
		/// Shader Type.
		/// </summary>

		public NJG.NJGMapBase.ShaderType shaderType = NJG.NJGMapBase.ShaderType.TextureMask;

		/// <summary>
		/// Map texture color.
		/// </summary>

		public virtual Color mapColor { get { return mColor; } set { mColor = value; material.color = value; } }

		/// <summary>
		/// Mask texture for the map.
		/// </summary>

		public Texture maskTexture;

		/// <summary>
		/// If icons get farther this radius they will dissapear.
		/// </summary>

		public float mapBorderRadius = 0;

		/// <summary>
		/// Map margin.
		/// </summary>

		public Vector2 margin = Vector2.zero;

		/// <summary>
		/// Game map's pivot point.
		/// </summary>

		public Pivot pivot = Pivot.Center;

		/// <summary>
		/// Current level of zoom of the Minimap.
		/// </summary>

		[SerializeField]
		public float zoom = 1;

		/// <summary>
		/// How much zoom in/out.
		/// </summary>

		[SerializeField]
		public float zoomAmount = 0.5f;

		/// <summary>
		/// The target that Minimap is going to follow.
		/// </summary>

		public Transform target;

		/// <summary>
		/// The target can be found using this tag.
		/// </summary>

		public string targetTag = "Player";

		/// <summary>
		/// Minimun level of zoom.
		/// </summary>

		[SerializeField]
		public float minZoom = 1;

		/// <summary>
		/// Maximun level of zoom.
		/// </summary>

		[SerializeField]
		public float maxZoom = 30f;

		/// <summary>
		/// Zoom easing method.
		/// </summary>

		[SerializeField]
        public LeanTweenType zoomEasing { set { mZoomEasing = value; } get { return mZoomEasing; } }

		/// <summary>
		/// Zoom speed.
		/// </summary>

		[SerializeField]
        [Range(0,1)] 
		public float zoomSpeed = 0.5f;

		/// <summary>
		/// Key to zoom in the minimap.
		/// </summary>

		[SerializeField]
		public KeyCode zoomInKey = KeyCode.KeypadPlus;

		/// <summary>
		/// Key to zoom out the minimap.
		/// </summary>

		[SerializeField]
		public KeyCode zoomOutKey = KeyCode.KeypadMinus;

		/// <summary>
		/// Limit bounds
		/// </summary>

		[SerializeField]
		public bool limitBounds = true;

		/// <summary>
		/// Rotate map with target.
		/// </summary>

		[SerializeField]
		public bool rotateWithPlayer = false;

		/// <summary>
		/// Enable Mouse Wheel Zoom.
		/// </summary>

		[SerializeField]
		public bool mouseWheelEnabled = true;

		/// <summary>
		/// Enable Map panning.
		/// </summary>

		[SerializeField]
		public bool panning = true;

		/// <summary>
		/// Panning ease type
		/// </summary>

		[SerializeField]
        public LeanTweenType panningEasing = LeanTweenType.easeOutCirc;

		/// <summary>
		/// How fast the panning should move
		/// </summary>

		[SerializeField]
		public float panningSpeed = 1;

		/// <summary>
		/// How fast the panning should go when mouse is moving.
		/// </summary>

		[SerializeField]
		public float panningSensitivity = 5;

		/// <summary>
		/// If true returns the panning position to the targets position.
		/// </summary>

		[SerializeField]
		public bool panningMoveBack = true;

		/// <summary>
		/// Array of keys in use.
		/// </summary>

		[SerializeField]
		public KeyCode[] keysInUse = new KeyCode[3];

		/// <summary>
		/// Current panning position.
		/// </summary>

		[SerializeField]
		public Vector2 panningPosition = Vector2.zero;

		/// <summary>
		/// Current map angle.
		/// </summary>

		public float mapAngle;

		/// <summary>
		/// Current scroll position.
		/// </summary>

		public Vector2 scrollPosition = Vector2.zero;

		/// <summary>
		/// Enable directonal lines drawing.
		/// </summary>

		public bool drawDirectionalLines = false;

		/// <summary>
		/// Lines shader.
		/// </summary>

		public Shader linesShader;

		/// <summary>
		/// Number of line points.
		/// </summary>

		public int linePoints = 20;

		/// <summary>
		/// Lines color.
		/// </summary>

		public Color lineColor = Color.red;

		/// <summary>
		/// Lines width.
		/// </summary>

		public float lineWidth = 0.1f;

		/// <summary>
		/// List of line points that are going to be drawn.
		/// </summary>

		public List<Transform> controlPoints = new List<Transform>();

		[SerializeField]
		public bool calculateBorder = true;	

		/// <summary>
		/// Root of map icons.
		/// </summary>

		public Transform iconRoot
		{
			get
			{
				if (mIconRoot == null && Application.isPlaying)
				{
					mIconRoot = NJGTools.AddChild(gameObject).transform;
					if (rendererTransform != null)
					{
						mIconRoot.parent = rendererTransform.parent;
						mIconRoot.localPosition = new Vector3(rendererTransform.localPosition.x, rendererTransform.localPosition.y, 1);
						mIconRoot.localEulerAngles = rendererTransform.localEulerAngles;
					}

					mIconRoot.name = "_MapIcons";
				}
				return mIconRoot;
			}
		}

		/// <summary>
		/// Check if mouse is over.
		/// </summary>

		public virtual bool isMouseOver { get; set; }

		public virtual Transform rendererTransform { get { if (planeRenderer != null) if (mMapTrans == null) mMapTrans = planeRenderer.transform; return planeRenderer == null ? transform : mMapTrans; } }

		/// <summary>
		/// Plane used to display the map.
		/// </summary>

		public virtual Renderer planeRenderer 
		{ 
			get 
			{
				if (mRenderer == null) mRenderer = gameObject.GetComponent<Renderer>();
				if (mRenderer == null) mRenderer = gameObject.GetComponentInChildren<Renderer>(); 
				return mRenderer; 
			} 
			set { mRenderer = value; } }

		public virtual Vector2 mapScale 
		{ 
			get 
			{
				/*if (mMapScaleSize == Vector2.zero)
				{
					mMapScaleSize = new Vector2(rendererTransform.localScale.x, rendererTransform.localScale.z);
				}*/
				return rendererTransform.localScale; 
			} 
			set
			{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5
				rendererTransform.hasChanged = true;				
#else
				mMapScaleChanged = true; 
#endif
				rendererTransform.localScale = value; 
			} 
		}

		public virtual Vector2 mapHalfScale 
		{ 
			get
			{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5
				if (rendererTransform.hasChanged)	
				{					
					rendererTransform.hasChanged = false;				
#else
				if (mMapScaleChanged)
				{
					mMapScaleChanged = false;
#endif									
					mMapHalfScale = mapScale * 0.5f;
				}
				return mMapHalfScale; 
			} 
		}

		[SerializeField]
		public virtual Material material
		{
			get { return Application.isPlaying ? planeRenderer.material : planeRenderer.sharedMaterial; }
			set { planeRenderer.material = value; }
		}

		/// <summary>
		/// Depth controls the rendering order -- lowest to highest.
		/// </summary>

		public int depth
		{
			get
			{
				material.renderQueue = 3000 + mDepth;
				return mDepth;
			}
			set
			{
				if (mDepth != value)
				{
					mDepth = value;
					material.renderQueue = 3000 + mDepth;
#if UNITY_EDITOR
					UnityEditor.EditorUtility.SetDirty(this);
#endif					
				}				
			}
		}

		/*[SerializeField]
		public virtual Vector3 iconScale
		{
			get
			{
				if (mIconSize != map.iconSize)
				{
					mIconSize = map.iconSize;
					mSize.x = mSize.y = map.iconSize;
				}
				return mSize;
			}
		}*/

		public virtual Vector3 arrowScale
		{
			get
			{
				if (mArrowSize != map.arrowSize)
				{
					mArrowSize = map.arrowSize;
					mArrScale.x = mArrScale.y = map.arrowSize;
				}
				return mArrScale;
			}
		}

		/// <summary>
		/// True if the map is visible.
		/// </summary>

		public bool isVisible { get { if (mChild == null) mChild = cachedTransform.GetChild(0); return mChild.gameObject.activeInHierarchy; } }

		/// <summary>
		/// Check if mouse is out of the screen.
		/// </summary>

		public bool isMouseOut { get { Vector3 p = Input.mousePosition; return p.x > Screen.width || p.y > Screen.height || p.x < 0 || p.y < 0; } }

		[HideInInspector] [SerializeField] protected Transform mMapTrans;
		[HideInInspector] [SerializeField] protected Vector3 mZoom = Vector3.zero;
		[HideInInspector] [SerializeField] protected List<UIMapIconBase> mList = new List<UIMapIconBase>();
		[HideInInspector] [SerializeField] protected Transform mIconRoot;
		[HideInInspector] [SerializeField] protected Transform mTrans;
		[HideInInspector] [SerializeField] protected Vector2 mMapScale;
		[HideInInspector] [SerializeField] protected Vector2 mMapHalfScale;
		[HideInInspector] [SerializeField] protected Vector2 mIconScale;
		[HideInInspector] [SerializeField] protected Matrix4x4 mMatrix;
		[HideInInspector] [SerializeField] protected Matrix4x4 rMatrix;
		[HideInInspector] [SerializeField] protected bool mMapScaleChanged = true;
		[HideInInspector] [SerializeField] protected int mDepth = 0;
		protected Vector3 mLastScale = Vector3.zero;
		//public Vector3 targetPos;
		//public Vector3 targetForward;
		protected Vector3 mMapPos = Vector3.zero;
		
		protected int mLastHeight = 0;
		float mNextUpdate = 0f;
		protected Texture mMask;
		//Vector3 mLastPos = Vector3.zero;
		//float mLastSize = 0f;
		protected List<UIMapIconBase> mUnused = new List<UIMapIconBase>();

		protected int mCount = 0;
		protected NJGMapBase map;
		protected bool isZooming;
		[SerializeField] protected Renderer mRenderer;
		[SerializeField] protected Color mColor = Color.white;
		protected Quaternion mapRotation;
		protected Vector3 rotationPivot = new Vector3(0.5f, 0.5f);

		[SerializeField]
        LeanTweenType mZoomEasing = LeanTweenType.easeOutExpo;
		
		Vector2 mPanningMousePosLast = Vector2.zero;
		bool mTargetWarning;
		protected Vector3 mIconEulers = Vector3.zero;
		//int mIconSize = 0;
		int mArrowSize = 0;
		//Vector3 mSize = Vector3.one;
		[SerializeField]
		Vector3 mArrScale = Vector3.one;
		Camera mUICam;
		bool mIsPanning;
		//TweenParms mResetPan;
		Transform mLinesRoot;
		LineRenderer mLineRenderer;		

		[SerializeField]
		protected Vector2 mMapScaleSize;

		//void OnTextureChanged(Texture2D texture)
		//{
		//	if (material != null && texture != null) material.mainTexture = texture;
		//}

		protected virtual void CleanIcons()
		{
			if (Application.isPlaying)
			{
				// Remove invalid entries
				//mList.ForEach(ic => { if (!ic.isValid) Delete(ic); });

				// Remove invalid entries
				for (int i = mList.Count; i > 0; )
				{
					UIMapIconBase icon = mList[--i];
					//icon.marker.Reset();

					if (!icon.isValid)
					{
						Delete(icon);
					}
				}
			}
		}

		/// <summary>
		/// Get the map icon entry associated with the specified unit.
		/// </summary>

		protected virtual UIMapIconBase GetEntry(NJGMapItem marker) { return null; }

		/// <summary>
		/// Delete the specified entry, adding it to the unused list.
		/// </summary>

		protected virtual void Delete(UIMapIconBase ent)
		{
			mList.Remove(ent);
			mUnused.Add(ent);
			NJGTools.SetActive(ent.gameObject, false);
		}

		protected virtual void OnEnable()
		{
			if (Application.isPlaying)
			{
				material.renderQueue = 3000 + depth;	
				if(!map.queue.Contains(UpdateCode)) map.queue.Add(UpdateCode);
			}
		}

		protected virtual void OnDisable()
		{
			if (Application.isPlaying)
			{
				if (map.queue.Contains(UpdateCode)) map.queue.Remove(UpdateCode);
			}
		}

		protected virtual void OnDestroy()
		{
			if (Application.isPlaying)
			{
				if (map.queue.Contains(UpdateCode)) map.queue.Remove(UpdateCode);				
			}
			//if (map != null) map.onTextureChanged -= OnTextureChanged;
		}		

		/// <summary>
		/// Update the icon icon for the specified unit, assuming it's visible.
		/// </summary>

		protected virtual void UpdateIcon(NJGMapItem item, float x, float y) { }

		protected virtual void Awake()
		{
			mMapScaleChanged = true;
			rendererTransform.hasChanged = true;
			map = NJGMapBase.instance;
			mTrans = transform;
			//map.onTextureChanged += OnTextureChanged;
			mUICam = NJGTools.FindCameraForLayer(gameObject.layer); //NJGMapBase.instance.layer

			if (material != null)
			{
				if (shaderType == NJGMapBase.ShaderType.FOW)
					material.shader = Shader.Find("NinjutsuGames/Map FOW");
				else if (shaderType == NJGMapBase.ShaderType.TextureMask)
					material.shader = Shader.Find("NinjutsuGames/Map TextureMask");
				else if (shaderType == NJGMapBase.ShaderType.ColorMask)
					material.shader = Shader.Find("NinjutsuGames/Map ColorMask");
			}

			if (NJGMapBase.instance.fow.enabled && shaderType != NJGMapBase.ShaderType.FOW)
			{
				shaderType = NJGMapBase.ShaderType.FOW;
				material.shader = Shader.Find("NinjutsuGames/Map FOW");
			}
			else if (!NJGMapBase.instance.fow.enabled && shaderType == NJGMapBase.ShaderType.FOW)
			{
				shaderType = NJGMapBase.ShaderType.TextureMask;
				material.shader = Shader.Find("NinjutsuGames/Map TextureMask");
			}

			if (maskTexture == null && material != null)
				maskTexture = material.GetTexture("_Mask");

			if (drawDirectionalLines && Application.isPlaying)
			{
				if(linesShader == null) linesShader = Shader.Find("Particles/Additive");

				GameObject go = NJGTools.AddChild(gameObject);
				mLinesRoot = go.transform;
				mLinesRoot.parent = iconRoot;
				mLinesRoot.localPosition = Vector3.zero;
				mLinesRoot.localEulerAngles = Vector3.zero;

				mLineRenderer = go.GetComponent<LineRenderer>();

				if (mLineRenderer == null)
					go.AddComponent<LineRenderer>();

				mLineRenderer = go.GetComponent<LineRenderer>();
				mLineRenderer.useWorldSpace = true;
				mLineRenderer.material = new Material(linesShader);

				mLinesRoot.name = "_Lines";
			}
		}		

		/// <summary>
		/// Create the child object that will be used for map indicators.
		/// </summary>

		protected virtual void Start()
		{
			map = NJGMapBase.instance;		

			if (material == null)
			{
				if(Application.isPlaying) Debug.LogWarning("The UITexture does not have a material assigned", this);
			}
			else
			{
				if (map.generateMapTexture)
					material.mainTexture = map.mapTexture;
				else
					material.mainTexture = NJG.NJGMapBase.instance.userMapTexture;

				if (maskTexture != null) material.SetTexture("_Mask", maskTexture);
				material.color = mapColor;							
			}

			if (Application.isPlaying)
			{
				if (mChild == null)
				{
					if (cachedTransform.childCount > 0) mChild = cachedTransform.GetChild(0);
					else mChild = cachedTransform;
				}
			}

			OnStart();
			Update();
		}

		/// <summary>
		/// This method is updated on a another thread.
		/// </summary>	

		protected virtual void UpdateCode()
		{
			Debug.Log("UpdateCode ");
				
		}

		/// <summary>
		/// Update Minimap scroll position.
		/// </summary>

		Vector3 mExt;
		public float mMod = 1f;

		void UpdateScrollPosition()
		{
			Bounds bounds = map.bounds;
			Vector3 extents = bounds.extents;

			/*if (map.boundsOffset > 0)
			{
				extents.x += map.boundsOffset;
				extents.y += map.boundsOffset;
				extents.z += map.boundsOffset;
			}*/

			float nZoom = 1f / zoom;

			if (target == null) return;

			scrollPosition = Vector3.zero;

			Vector3 vector = target.position - bounds.center;			

			mExt.x = (0.5f / extents.x);
			mExt.y = (0.5f / extents.y);
			mExt.z = (0.5f / extents.z);

			if (map.mapResolution == NJGMapBase.Resolution.Double)
			{
				mExt.x *= mMod;
				mExt.y *= mMod;
				mExt.z *= mMod;
			}

			scrollPosition.x = vector.x * mExt.x;
			if (map.orientation == NJGMapBase.Orientation.XZDefault)
				scrollPosition.y = vector.z * mExt.z;
			else
				scrollPosition.y = vector.y * mExt.y;

			if (panning) scrollPosition = scrollPosition + panningPosition;

			// Limit minimap position
			if (limitBounds)
			{
				scrollPosition.x = Mathf.Max(-((1f - nZoom) * 0.5f), scrollPosition.x);
				scrollPosition.x = Mathf.Min((1f - nZoom) * 0.5f, scrollPosition.x);
				scrollPosition.y = Mathf.Max(-((1f - nZoom) * 0.5f), scrollPosition.y);
				scrollPosition.y = Mathf.Min((1f - nZoom) * 0.5f, scrollPosition.y);
			}

			mMapPos.x = ((1f - nZoom) * 0.5f) + scrollPosition.x;
			mMapPos.y = ((1f - nZoom) * 0.5f) + scrollPosition.y;
			mMapPos.z = 0;			

			// Relative zoom.
			mZoom.x = mZoom.y = mZoom.z = nZoom;
			
			UpdateMatrix();
		}

		/// <summary>
		/// Updates texture matrix
		/// </summary>

		protected virtual void UpdateMatrix()
		{
			// Move and scale matrix
			Matrix4x4 m = Matrix4x4.TRS(mMapPos, Quaternion.identity, mZoom);

			if (rotateWithPlayer)
			{
				// Get target angle.
				Vector3 mForward = target.forward;
				mForward.Normalize();
				mapAngle = ((Vector3.Dot(mForward, Vector3.Cross(Vector3.up, Vector3.forward)) <= 0f) ? 1f : -1f) * Vector3.Angle(mForward, Vector3.forward);

				mapRotation = Quaternion.Euler(0, 0, mapAngle);

				// Rotation matrix
				Matrix4x4 mPivotInventory = Matrix4x4.TRS(-rotationPivot, Quaternion.identity, Vector3.one);
				Matrix4x4 mRot = Matrix4x4.TRS(Vector3.zero, mapRotation, Vector3.one);
				Matrix4x4 mPivot = Matrix4x4.TRS(rotationPivot, Quaternion.identity, Vector3.one);

				rMatrix = m * mPivot * mRot * mPivotInventory;

				if (!mMatrix.Equals(rMatrix))
				{
					mMatrix = rMatrix;
					material.SetMatrix("_Matrix", rMatrix);
				}

				if (iconRoot != null)
				{
					mIconEulers.z = -mapAngle;
					if (iconRoot.localEulerAngles != mIconEulers) iconRoot.localEulerAngles = mIconEulers;
				}
			}
			else
			{
				if (!mMatrix.Equals(m))
				{
					mMatrix = m;
					material.SetMatrix("_Matrix", m);
					if (iconRoot != null) if (iconRoot.localEulerAngles != Vector3.zero) iconRoot.localEulerAngles = Vector3.zero;
				}
			}
		}

		/// <summary>
		/// Update what's necessary.
		/// </summary>		

		protected virtual void Update()
		{			
			if (!Application.isPlaying) return;

			if (target == null)
			{
				if (!string.IsNullOrEmpty(targetTag))
				{
					if (GameObject.FindGameObjectWithTag(targetTag) != null) 
						target = GameObject.FindGameObjectWithTag(targetTag).transform;
				}
			}

			if (isMouseOut && isMouseOver) isMouseOver = false;

			// If there is no target defined lets use the mainCamera
			/*if (target == null && Camera.main != null)
			{
				if (target != Camera.main.transform) target = Camera.main.transform;
			}*/
			//else if (target != null && mTarget != target) mTarget = target;

			if (target != null && controlPoints.Count == 0)
			{
				if(!controlPoints.Contains(target)) controlPoints.Add(target);
			}

			if (target == null && !mTargetWarning)
			{
				mTargetWarning = true;
			}

			UpdateZoomKeys();

			if (mouseWheelEnabled)
			{				
				if (isMouseOver)
				{
					float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
					if (scrollWheel != 0)
					{
						if (scrollWheel > 0.1)
							ZoomIn(zoomAmount);
						else if (scrollWheel < 0.1)
							ZoomOut(zoomAmount);
					}
				}
			}

			if (panning)
			{
				UnityEngine.Profiling.Profiler.BeginSample("Panning");
				UpdatePanning();
				UnityEngine.Profiling.Profiler.EndSample();
			}			

			int height = Screen.height;
			bool heightChanged = (mLastHeight != height);

			if (mLastScale != rendererTransform.localScale)
			{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5
				rendererTransform.hasChanged = true;
#else
				mMapScaleChanged = true;
#endif
				mLastScale = rendererTransform.localScale;
				if (calculateBorder) mapBorderRadius = (rendererTransform.localScale.x / 2f) / 4f;
			}

			if (mNextUpdate < Time.time) //heightChanged || 
			{
				mLastHeight = height;
				mNextUpdate = Time.time + map.updateFrequency;	

				//bool sizeChanged = false;	
					

				UnityEngine.Profiling.Profiler.BeginSample("UpdateIcons");
				UpdateIcons();
				UnityEngine.Profiling.Profiler.EndSample();

				UnityEngine.Profiling.Profiler.BeginSample("CleanIcons");
				CleanIcons();
				UnityEngine.Profiling.Profiler.EndSample();

				//Profiler.BeginSample("UpdateFrustm");
				//UpdateFrustum();
				//Profiler.EndSample();

				if (drawDirectionalLines)
				{
					UnityEngine.Profiling.Profiler.BeginSample("DrawLines");
					DrawLines();
					UnityEngine.Profiling.Profiler.EndSample();
				}

				UnityEngine.Profiling.Profiler.BeginSample("UpdateScrollPosition");
				UpdateScrollPosition();
				UnityEngine.Profiling.Profiler.EndSample(); 

				/*if (mLastPos != NJG.NJGMapBase.instance.mapRenderer.transform.position || mLastSize != NJG.NJGMapBase.instance.mapRenderer.cachedCamera.orthographicSize)
				{
					posChanged = true;
					mLastPos = NJG.NJGMapBase.instance.mapRenderer.transform.position;
					mLastSize = NJG.NJGMapBase.instance.mapRenderer.cachedCamera.orthographicSize;
				}*/
				if (OnUpdate() || heightChanged)
				{
					if (NJG.NJGMapBase.instance.renderMode != NJGMapBase.RenderMode.Once && (this is UIMiniMapBase))
					{
						NJG.NJGMapBase.instance.GenerateMap();
					}
				}
			}
		}

		protected virtual void UpdateZoomKeys()
		{
			if (Input.GetKeyDown(zoomInKey))
				ZoomIn(zoomAmount);

			if (Input.GetKeyDown(zoomOutKey))
				ZoomOut(zoomAmount);
		}

		/// <summary>
		/// Anything you need to do in Start.
		/// </summary>

		protected virtual void OnStart() { }

		/// <summary>
		/// Anything else you might want to update (target indicators and such). Return 'true' if the map should be redrawn.
		/// </summary>

		protected virtual bool OnUpdate() 
		{			
			return false; 
		}

		/// <summary>
		/// Update the coordinates and colors of map indicators.
		/// </summary>

		protected void UpdateIcons()
		{
			// Mark all entries as invalid
			for (int i = mList.Count; i > 0; )
			{
				UIMapIconBase ic = mList[--i];
				ic.isValid = false;

				if (drawDirectionalLines)
				{
					if (ic.item.cachedTransform != target)
					{
						if (ic.item.drawDirection)
						{
							if (!controlPoints.Contains(ic.cachedTransform)) controlPoints.Add(ic.cachedTransform);
						}
						else
						{
							if (controlPoints.Contains(ic.cachedTransform)) controlPoints.Remove(ic.cachedTransform);
						}
					}
					else
					{
						if (controlPoints[0] != ic.cachedTransform) controlPoints[0] = ic.cachedTransform;
					}
				}
			}

			// Update all entries, marking them as valid in the process
			for (int i = 0; i < NJGMapItem.list.Count; ++i)
			{
				NJGMapItem item = NJGMapItem.list[i];
				if (item.type < 1) continue;
				/*if (drawDirectionalLines)
				{
					if (item.cachedTransform != target)
					{
						if (item.drawDirection)
						{
							if (!controlPoints.Contains(item.cachedTransform)) controlPoints.Add(item.cachedTransform);
						}
						else
						{
							if (controlPoints.Contains(item.cachedTransform)) controlPoints.Remove(item.cachedTransform);
						}
					}
				}*/

				Vector2 pos = WorldToMap(item.cachedTransform.position);

				if (map.fow.enabled && item.cachedTransform != target)
				{
					Vector2 pos2 = WorldToMap(item.cachedTransform.position, false);
					bool shouldReveal = NJGFOW.instance.IsExplored(pos2, 150) || NJGFOW.instance.IsVisible(pos2, 150);

					if (item.isRevealed != shouldReveal)
					{
						// If FOW is enabled, unit is covered by FOW and if doesn't reveal FOW, don't consider it.						
						item.isRevealed = item.revealFOW ? true : shouldReveal;
					}
				}
				else
					if (!item.isRevealed) item.isRevealed = true;

				if (item.isRevealed) UpdateIcon(item, pos.x, pos.y);
			}
		}

		#region Camera Frustum

		GameObject mFrustum;
		Mesh mFrustumMesh = null;
		Material mFrustumMat;

		/// <summary>
		/// Updates and draws the camera frustum on the map.
		/// </summary>
		
		protected virtual void UpdateFrustum()
		{
			if (map.cameraFrustum == null) return;
			if (map.orientation == NJG.NJGMapBase.Orientation.XYSideScroller) return;
			
			if (mFrustumMesh == null)
			{
				mFrustum = new GameObject();// GameObject.CreatePrimitive(PrimitiveType.Quad);
				mFrustumMat = new Material(Shader.Find("NinjutsuGames/Map TextureMask"));
				mFrustum.AddComponent<MeshRenderer>().material = mFrustumMat;
				//Destroy(mFrustum.collider);
				mFrustum.name = "_Frustum";
				mFrustum.transform.parent = iconRoot;
				mFrustum.transform.localEulerAngles = new Vector3(270, 0, 0);
				mFrustum.transform.localPosition = Vector3.zero;
				mFrustum.transform.localScale = Vector3.one;
				mFrustum.layer = gameObject.layer;
				mFrustumMesh = mFrustum.AddComponent<MeshFilter>().mesh = NJGTools.CreatePlane();
			}

			Vector3[] vertices = mFrustumMesh.vertices;

			vertices[1] = map.cameraFrustum.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height / 2, map.cameraFrustum.farClipPlane));
			vertices[2] = map.cameraFrustum.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height / 2, map.cameraFrustum.nearClipPlane));
			vertices[3] = map.cameraFrustum.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, map.cameraFrustum.nearClipPlane));
			vertices[0] = map.cameraFrustum.ScreenToWorldPoint(new Vector3(0, Screen.height / 2, map.cameraFrustum.farClipPlane));

			float height = map.orientation == NJGMapBase.Orientation.XZDefault ? 
				map.bounds.min.y - 1.0f + 0.1f : map.bounds.max.z + 1.0f + 0.1f;

			for (int i = 0; i < 4; i++)
			{
				vertices[i].y = height;
			}

			mFrustumMesh.vertices = vertices;
			mFrustumMesh.RecalculateBounds();

			mFrustumMat.SetColor("_Color", map.cameraFrustumColor);
		}

		#endregion

		#region Panning

		/// <summary>
		/// Checks if panning.
		/// </summary>

		public bool isPanning { get { if (!panning) return false; return mIsPanning; } }

		/// <summary>
		/// Update panning position.
		/// </summary>

		void UpdatePanning()
		{
			if (!panning)
			{
				panningPosition = Vector2.zero;
				return;
			}

			if (isMouseOver)
			{
				if (Input.GetMouseButtonDown(0))
				{
					mPanningMousePosLast = mUICam.ScreenToViewportPoint(Input.mousePosition);
                    if (LeanTween.isTweening(gameObject)) LeanTween.cancel(gameObject);
				}

				if (!mIsPanning)
				{
					if (Input.GetMouseButton(0))
					{
						if (Vector2.Distance((Vector2)mUICam.ScreenToViewportPoint(Input.mousePosition), mPanningMousePosLast) > 0.01f)
						{							
							mIsPanning = true;
						}
					}
				}
			}

			if (isPanning)
			{
				if (Input.GetMouseButton(0))
				{
					Vector2 d = (Vector2)mUICam.ScreenToViewportPoint(Input.mousePosition) - mPanningMousePosLast;
					Vector2 r = GetDirection(d) * panningSensitivity;
					panningPosition -= r / zoom;
					mPanningMousePosLast = mUICam.ScreenToViewportPoint(Input.mousePosition);
				}

				if (Input.GetMouseButtonUp(0))
				{					
					if(panningMoveBack) ResetPanning();
					else mIsPanning = false;
				}
			}			
		}

		/// <summary>
		/// Moves the map back to the target position.
		/// </summary>		

		public void ResetPanning()
		{
			if (panningPosition == Vector2.zero)
			{
				mIsPanning = false;
				return;
			}

			//if(mResetPan == null)
			//	mResetPan = new TweenParms().Prop("panningPosition", Vector2.zero).OnComplete(OnPanningComplete);

            //LeanTween.value(.move(gameObject, Vector2.zero, panningSpeed).onComplete = OnPanningComplete;
			//HOTween.To(this, panningSpeed, mResetPan).easeType = panningEasing;		

            LeanTween.value(gameObject, UpdatePanningPos, panningPosition, Vector3.zero, panningSpeed).setEase(panningEasing).onComplete = OnPanningComplete;

		}

        void UpdatePanningPos(Vector3 val)
        {
            panningPosition = val;
        }

		/// <summary>
		/// Resets panning position when tween completes.
		/// </summary>

		void OnPanningComplete()
		{
			panningPosition = Vector2.zero;
			mIsPanning = false;
		}

		#endregion

		#region Lines Drawing

		int mVertextCount = 0;
		int mLastVertextCount = 0;

		Color mLastColor;
		float mLastWidth;

		void DrawLines()
		{
			// not enough points specified
			if (null == mLineRenderer || controlPoints == null || controlPoints.Count < 2)
				return;

			// update line renderer
			if (mLastColor != lineColor)
			{
				mLastColor = lineColor;
				mLineRenderer.SetColors(lineColor, lineColor);
			}

			if (mLastWidth != lineWidth)
			{
				mLastWidth = lineWidth;
				mLineRenderer.SetWidth(lineWidth * 0.1f, lineWidth * 0.1f);
			}

			if (linePoints < 2)
				linePoints = 2;

			mVertextCount = linePoints * (controlPoints.Count - 1);

			if (mLastVertextCount != mVertextCount)
			{
				mLastVertextCount = mVertextCount;
				mLineRenderer.SetVertexCount(mVertextCount);
			}

			// loop over segments of spline
			Vector3 p0;
			Vector3 p1;
			Vector3 m0;
			Vector3 m1;
			for (int j = 0, jmax = controlPoints.Count - 1; j < jmax; j++)
			{
				// check control points
				if (controlPoints[j] == null ||
				   controlPoints[j + 1] == null ||
				   (j > 0 && controlPoints[j - 1] == null) ||
				   (j < controlPoints.Count - 2 &&
				   controlPoints[j + 2] == null))
				{
					return;
				}
				// determine control points of segment
				p0 = controlPoints[j].position;
				p1 = controlPoints[j + 1].position;

				if (j > 0)				
					m0 = 0.5f * (controlPoints[j + 1].position - controlPoints[j - 1].position);				
				else				
					m0 = controlPoints[j + 1].position - controlPoints[j].position;				
				if (j < controlPoints.Count - 2)				
					m1 = 0.5f * (controlPoints[j + 2].position - controlPoints[j].position);				
				else				
					m1 = controlPoints[j + 1].position - controlPoints[j].position;
				
				/*p0 = WorldToMap(controlPoints[j].position);
				Debug.Log("Pos " + p0 + " / " + controlPoints[j].position + " - " + j + " / " + controlPoints[j]);
				p1 = WorldToMap(controlPoints[j + 1].position);
				if (j > 0)
				{
					m0 = 0.5f * (WorldToMap(controlPoints[j + 1].position) - WorldToMap(controlPoints[j - 1].position));
				}
				else
				{
					m0 = WorldToMap(controlPoints[j + 1].position) - WorldToMap(controlPoints[j].position);
				}
				if (j < controlPoints.Count - 2)
				{
					m1 = 0.5f * (WorldToMap(controlPoints[j + 2].position) - WorldToMap(controlPoints[j].position));
				}
				else
				{
					m1 = WorldToMap(controlPoints[j + 1].position) - WorldToMap(controlPoints[j].position);
				}*/

				// set points of Hermite curve
				Vector3 position;
				float t;
				float pointStep = 1.0f / linePoints;

				// last point of last segment should reach p1
				if (j == controlPoints.Count - 2) pointStep = 1.0f / (linePoints - 1.0f);

				for (int i = 0; i < linePoints; i++)
				{
					t = i * pointStep;
					position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * p0
					   + (t * t * t - 2.0f * t * t + t) * m0
					   + (-2.0f * t * t * t + 3.0f * t * t) * p1
					   + (t * t * t - t * t) * m1;
					mLineRenderer.SetPosition(i + j * linePoints, position);
				}
			}
		}

		#endregion

        void UpdateZoom(float zoom)
        {
            this.zoom = zoom;
        }

		#region Public Methods

		/// <summary>
		/// Zoom out the minimap
		/// </summary>

		public void ZoomIn(float amount)
		{
			if (zoom == maxZoom) return;
			//if (HOTween.IsTweening(this)) HOTween.Complete(this);
			//HOTween.To(this, zoomSpeed, "zoom", Mathf.Clamp(zoom + amount, (int)minZoom, (int)maxZoom)).easeType = zoomEasing;
            if (LeanTween.isTweening(gameObject)) LeanTween.cancel(gameObject);
            float newZoom = Mathf.Clamp(zoom + amount, (int)minZoom, (int)maxZoom);
            LeanTween.value(gameObject, UpdateZoom, zoom, newZoom, zoomSpeed).setEase(zoomEasing);
		}

		/// <summary>
		/// Zoom out the minimap
		/// </summary>

		public void ZoomOut(float amount)
		{
			if (zoom == minZoom) return;
			//if (HOTween.IsTweening(this)) HOTween.Complete(this);
			//HOTween.To(this, zoomSpeed, "zoom", Mathf.Clamp(zoom - amount, (int)minZoom, (int)maxZoom)).easeType = zoomEasing;

            if (LeanTween.isTweening(gameObject)) LeanTween.cancel(gameObject);
            float newZoom = Mathf.Clamp(zoom - amount, (int)minZoom, (int)maxZoom);
            LeanTween.value(gameObject, UpdateZoom, zoom, newZoom, zoomSpeed).setEase(zoomEasing);
		}

		/// <summary>
		/// Transform map coords to world coords.
		/// </summary>		

		public Vector3 MapToWorld(Vector2 pos)
		{
			Bounds bounds = map.bounds;
			Vector3 extents = bounds.extents;
			Vector3 v = (Vector3)pos + bounds.center;
			float x = mapHalfScale.x / extents.x;
			float y = mapHalfScale.y / ((map.orientation == NJGMapBase.Orientation.XZDefault) ? extents.z : extents.y);
			Vector3 v2 = WorldScrollPosition();

			x *= zoom;
			y *= zoom;

			Vector3 mClickPos;
			mClickPos.x = (v.x + v2.x) * x;
			mClickPos.z = (map.orientation == NJGMapBase.Orientation.XZDefault) ? (v.z + v2.z) * y : (v.y + v2.y) * y;

			//mClickPos.x = pos.x / zoom;
			//mClickPos.z = pos.y / zoom;

			// Get cameras current height ( TO DO: sanity check that y position is above terrain )
			mClickPos.y = target.position.y;

			return mClickPos;
		}

		/// <summary>
		/// Transform world coords to map coords.
		/// </summary>	

		public Vector2 WorldToMap(Vector3 worldPos)
		{
			return WorldToMap(worldPos, true);
		}

		/// <summary>
		/// Transform world coords to map coords.
		/// </summary>			

		public Vector2 WorldToMap(Vector3 worldPos, bool calculateZoom)
		{
			if (map == null) map = NJGMapBase.instance;
			Bounds bounds = map.bounds;
			Vector3 extents = bounds.extents;
			Vector3 v = worldPos - bounds.center;

			float x = mapHalfScale.x / extents.x;
			float y = mapHalfScale.y / ((map.orientation == NJGMapBase.Orientation.XZDefault) ? extents.z : extents.y);
			Vector3 v2 = WorldScrollPosition();			

			if (calculateZoom)
			{
				x *= zoom;
				y *= zoom;
			}
			else
			{
				x *= 1f;
				y *= 1f;
				v2 = Vector3.zero;
			}

			Vector2 mWTM = Vector2.zero;
			mWTM.x = (v.x - v2.x) * x;
			mWTM.y = (map.orientation == NJGMapBase.Orientation.XZDefault) ? (v.z - v2.z) * y : (v.y - v2.y) * y;

			if (map.mapResolution == NJGMapBase.Resolution.Double)
			{
				x *= mMod;
				y *= mMod;
			}

			return mWTM;
		}

		Vector3 mScrollPos;

		public Vector3 WorldScrollPosition()
		{
			Vector3 v = map.bounds.size;
			mScrollPos.x = scrollPosition.x * v.x;
			mScrollPos.y = scrollPosition.y * v.y;
			mScrollPos.z = scrollPosition.y * v.z;
			return mScrollPos;
		}		

		protected Transform mChild;
		Animation mAnim;
		bool mAnimCheck;
		bool mVisible;

		/// <summary>
		/// Toggle map.
		/// </summary>

		public void Toggle()
		{
			if (mVisible) Hide();
			else Show();
		}

		/// <summary>
		/// Show map.
		/// </summary>	

		public virtual void Show()
		{
			if (!mVisible)
			{
				if (mChild == null) mChild = cachedTransform.GetChild(0);
				if (mChild == null) mChild = transform;

				if (mAnim == null && !mAnimCheck)
				{
                    mAnim = mChild.GetComponent<Animation>();
                    if (mAnim == null) mAnim = mChild.GetComponentInChildren<Animation>();
					mAnimCheck = true;
				}

				if (mAnim != null)
				{
					NJGTools.SetActive(mChild.gameObject, true);
					mAnim[mAnim.clip.name].speed = 1;
					mAnim[mAnim.clip.name].time = 0;
					if (mAnim.clip != null) mAnim.Play();
				}
				else NJGTools.SetActive(mChild.gameObject, true);
				mVisible = true;
				enabled = true;
			}
		}

		/// <summary>
		/// Hide map.
		/// </summary>	

		public virtual void Hide()
		{
			if (mVisible)
			{
				if (mChild == null) mChild = cachedTransform.GetChild(0);
				if (mChild == null) mChild = transform;

				if (mAnim == null && !mAnimCheck)
				{
					mAnim = gameObject.GetComponentInChildren<Animation>();
					mAnimCheck = true;
				}

				if (mAnim != null)
				{					
					if (mAnim.clip != null)
					{
						mAnim[mAnim.clip.name].speed = -1;
						mAnim[mAnim.clip.name].time = mAnim[mAnim.clip.name].length;	
						mAnim.Play();
						StartCoroutine(DisableOnFinish());
					}
				}
				else NJGTools.SetActive(mChild.gameObject, false);
				mVisible = false;
				enabled = false;
			}
		}

		IEnumerator DisableOnFinish()
		{
			yield return new WaitForSeconds(mAnim[mAnim.clip.name].length);
			NJGTools.SetActive(mChild.gameObject, false);
		}

		/// <summary>
		/// Gets direction of position based on map rotation.
		/// </summary>

		public Vector2 GetDirection(Vector2 position) { return mapRotation * position; }

		/// <summary>
		/// Gets direction of position based on map rotation.
		/// </summary>

		public Vector3 GetDirection(Vector3 position) { return mapRotation * position; }

		#endregion
	}
}