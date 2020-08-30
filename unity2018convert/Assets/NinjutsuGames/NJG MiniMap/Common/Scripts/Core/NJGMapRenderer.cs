//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using System.Collections;
using UnityEngine;

/// <summary>
/// Very basic game map -- renderer component. It's able to draw a map into a 2D texture.
/// </summary>

[ExecuteInEditMode]
public class NJGMapRenderer : MonoBehaviour
{
	static NJGMapRenderer mInst;

	/// <summary>
	/// Get instance.
	/// </summary>
	
	static public NJGMapRenderer instance
	{
		get
		{
			if (mInst == null)
			{
				mInst = GameObject.FindObjectOfType(typeof(NJGMapRenderer)) as NJGMapRenderer;
				if (mInst == null)
				{
					GameObject go = new GameObject("_NJGMapRenderer");
					go.transform.parent = NJG.NJGMapBase.instance.transform;
					// Isolate this camera to prevent any interference with other cameras
					go.layer = LayerMask.NameToLayer("TransparentFX");
					//go.hideFlags = HideFlags.HideInInspector;
					mInst = go.AddComponent<NJGMapRenderer>();					
				}
			}
			return mInst;
		}
	}

	/// <summary>
	/// Cached transform for speed.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	public int mapImageIndex = 0;

	Vector2 lastSize;
	Vector2 mSize;
	Transform mTrans;
	bool canRender = true;
	bool mGenerated = false;
	bool mWarning = false;
	bool mReaded = false;
	bool mApplied = false;
	float lastRender = 0;
	NJG.NJGMapBase map;

	void Awake()
	{
		map = NJG.NJGMapBase.instance;
		if (map == null)
		{
			Debug.LogWarning("Can't render map photo. NJGMiniMap instance not found.");
			NJGTools.Destroy(gameObject);
			return;
		}		

		if (gameObject.GetComponent<Camera>() == null) 
			gameObject.AddComponent<Camera>();

		GetComponent<Camera>().useOcclusionCulling = false;

		Render();
	}

	void Start()
	{
		if (map.boundLayers.value == 0)
		{
			Debug.LogWarning("Can't render map photo. You have not choosen any layer for bounds calculation. Go to the NJGMiniMap inspector.", map);
			NJGTools.DestroyImmediate(gameObject);
			return;
		}

		if (map.renderLayers.value == 0)
		{
			Debug.LogWarning("Can't render map photo. You have not choosen any layer for rendering. Go to the NJGMiniMap inspector.", map);
			NJGTools.DestroyImmediate(gameObject);
			return;
		}

		ConfigCamera();

		//if (map.optimize) StartCoroutine(DelayedDestroy(gameObject, 2));
		if(!Application.isPlaying) Render();
	}

	void ConfigCamera()
	{
		map.UpdateBounds();
		Bounds bounds = map.bounds;
		//bounds.Expand(new Vector3(-bounds.extents.x, 0f, -bounds.extents.z));
		GetComponent<Camera>().depth = -100;
		GetComponent<Camera>().backgroundColor = map.cameraBackgroundColor;
		GetComponent<Camera>().cullingMask = map.renderLayers;
		GetComponent<Camera>().clearFlags = (CameraClearFlags)map.cameraClearFlags;
		GetComponent<Camera>().orthographic = true;

		float z = 0;
		//float n = 0;

		if (map.orientation == NJG.NJGMapBase.Orientation.XYSideScroller)
		{
			GetComponent<Camera>().farClipPlane = bounds.size.z * 1.1f;

			//n = bounds.extents.x / bounds.extents.y;
			//if (n < camera.aspect)
			z = bounds.extents.y;
			//else
			//z = bounds.extents.x / camera.aspect;

			GetComponent<Camera>().aspect = bounds.size.x / bounds.size.y;
		}
		else if (map.orientation == NJG.NJGMapBase.Orientation.XZDefault)
		{
			GetComponent<Camera>().farClipPlane = bounds.size.y * 1.1f;

			//n = bounds.extents.x / bounds.extents.z;
			//if (n < camera.aspect)
				z = bounds.extents.z;
			//else
			//	z = bounds.extents.x / camera.aspect;

			GetComponent<Camera>().aspect = bounds.size.x / bounds.size.z;
		}
		GetComponent<Camera>().farClipPlane = GetComponent<Camera>().farClipPlane * 5f;
		GetComponent<Camera>().orthographicSize = z;

		if (map.orientation == NJG.NJGMapBase.Orientation.XZDefault)
		{
			cachedTransform.eulerAngles = new Vector3(90f, 0, 0);
			//cachedTransform.position = new Vector3(bounds.max.x - bounds.extents.x, bounds.size.y, bounds.center.z);//-(Mathf.Abs(bounds.max.z) + Mathf.Abs(bounds.extents.z))
			if (map.mapResolution == NJG.NJGMapBase.Resolution.Double)
			{
				for (int i = 0; i < 4; i++)
				{
					switch (i)
					{
						case 0:
							cachedTransform.position = new Vector3(bounds.center.x - bounds.extents.x, (bounds.center.y + bounds.extents.y) + 1f, bounds.center.z - bounds.extents.z);
							break;

						case 1:
							cachedTransform.position = new Vector3(bounds.center.x + bounds.extents.x, (bounds.center.y + bounds.extents.y) + 1f, bounds.center.z - bounds.extents.z);
							break;

						case 2:
							cachedTransform.position = new Vector3(bounds.center.x + bounds.extents.x, (bounds.center.y + bounds.extents.y) + 1f, bounds.center.z + bounds.extents.z);
							break;

						case 3:
							cachedTransform.position = new Vector3(bounds.center.x - bounds.extents.x, (bounds.center.y + bounds.extents.y) + 1f, bounds.center.z + bounds.extents.z);
							break;
					}
					Debug.Log("cachedTransform.position " + cachedTransform.position + " / mapImageIndex " + mapImageIndex);
					
					GetComponent<Camera>().enabled = true;
					mapImageIndex = i;
					//camera.Render();
				}
			}
			else
			{
				cachedTransform.position = new Vector3(bounds.max.x - bounds.extents.x, bounds.size.y * 2f, bounds.center.z);
				GetComponent<Camera>().enabled = true;
				//camera.Render();
			}
		}
		else
		{
			cachedTransform.eulerAngles = new Vector3(0, 0, 0);
			cachedTransform.position = new Vector3(bounds.max.x - bounds.extents.x, bounds.center.y, -((Mathf.Abs(bounds.min.z) + Mathf.Abs(bounds.max.z)) + 10));
		}
	}


	/*IEnumerator DelayedDestroy(UnityEngine.Object obj, float delay)
	{
		yield return new WaitForSeconds(delay);

		NJGTools.DestroyImmediate(obj);
	}*/

	/*bool ClearColor(object obj)
	{
		Color32[] list = obj as Color32[];
		newColors = new Color32[list.Length];

		int i = 0;
		int imax = list.Length;

		for (; i < imax; i++)
		{
			Color c = list[i];
			if (c == bgColor) c = Color.clear;
			newColors[i] = c;
		}
		done = true;
		return false;
	}*/

	IEnumerator OnPostRender()
	{
		if (!Application.isPlaying || NJG.NJGMapBase.instance.renderMode == NJG.NJGMapBase.RenderMode.Dynamic) ConfigCamera();

		// Can't re-generate the texture map is makeNoLongReadable flag is turned on.
		if (mGenerated && map.optimize && Application.isPlaying && !mWarning)
		{
			mWarning = true;
			Debug.LogWarning("Can't Re-generate the map texture since 'optimize' is activated");
			canRender = false;
		}
		else
		{
			if (canRender)
			{
				if (map.mapTexture == null)
				{
					mSize = map.mapSize;
					if (map.mapResolution == NJG.NJGMapBase.Resolution.Double) mSize = map.mapSize * 2;
					map.mapTexture = new Texture2D((int)mSize.x, (int)mSize.y, (TextureFormat)map.textureFormat, map.generateMipmaps);
					map.mapTexture.name = "_NJGMapTexture";
					map.mapTexture.filterMode = map.mapFilterMode;
					map.mapTexture.wrapMode = map.mapWrapMode;
					lastSize = mSize;
				}

				if (!mReaded || !Application.isPlaying)
				{					
					//mReaded = true;

					// First take a screenshot from game view when camera is rendering.
					if (map.generateMapTexture && canRender)
					{
						if (NJG.NJGMapBase.instance.renderMode != NJG.NJGMapBase.RenderMode.Once)
						{
							mSize = map.mapSize;
							if (map.mapResolution == NJG.NJGMapBase.Resolution.Double) mSize = map.mapSize * 2;
							if (mSize.x >= lastSize.x || mSize.y >= lastSize.y)
							{
								lastSize = mSize;
								map.mapTexture.Resize((int)mSize.x, (int)mSize.y);
							}
						}

						if (map.mapResolution == NJG.NJGMapBase.Resolution.Double)
						{

							Bounds bounds = map.bounds;
							//bounds.Expand(new Vector3(-bounds.extents.x, 0f, -bounds.extents.z));
							for (int i = 0; i < 4; i++)
							{
								switch (i)
								{
									case 0:
										
										//camera.Render();
										map.mapTexture.ReadPixels(new Rect(0f, 0f, map.mapSize.x, map.mapSize.y), 0, 0, map.generateMipmaps);
										cachedTransform.position = new Vector3(bounds.center.x - bounds.extents.x, (bounds.center.y + bounds.extents.y) + 1f, bounds.center.z - bounds.extents.z);
										//yield return new WaitForEndOfFrame();
										//map.mapTexture.Apply(map.generateMipmaps, map.optimize);
										break;

									case 1:
										cachedTransform.position = new Vector3(bounds.center.x + bounds.extents.x, (bounds.center.y + bounds.extents.y) + 1f, bounds.center.z - bounds.extents.z);
										//camera.Render();
										map.mapTexture.ReadPixels(new Rect(0f, 0f, map.mapSize.x, map.mapSize.y), (int)map.mapSize.x, 0, map.generateMipmaps);
										//yield return new WaitForEndOfFrame();
										//map.mapTexture.Apply(map.generateMipmaps, map.optimize);
										break;

									case 2:
										cachedTransform.position = new Vector3(bounds.center.x + bounds.extents.x, (bounds.center.y + bounds.extents.y) + 1f, bounds.center.z + bounds.extents.z);
										//camera.Render();
										map.mapTexture.ReadPixels(new Rect(0f, 0f, map.mapSize.x, map.mapSize.y), (int)map.mapSize.x, (int)map.mapSize.y, map.generateMipmaps);
										//yield return new WaitForEndOfFrame();
										//map.mapTexture.Apply(map.generateMipmaps, map.optimize);
										break;

									case 3:
										cachedTransform.position = new Vector3(bounds.center.x - bounds.extents.x, (bounds.center.y + bounds.extents.y) + 1f, bounds.center.z + bounds.extents.z);
										//camera.Render();
										mReaded = true;
										map.mapTexture.ReadPixels(new Rect(0f, 0f, map.mapSize.x, (map.mapSize.y)), 0, (int)map.mapSize.y, map.generateMipmaps);
										//map.mapTexture.Apply(map.generateMipmaps, map.optimize);
										break;
								}
								Debug.Log("mapImageIndex " + i + " / map.mapSize " + map.mapSize + " / cachedTransform.position " + cachedTransform.position + " / mReaded " + mReaded);
							}
							/*if (mapImageIndex == 0)
							{
								map.mapTexture.ReadPixels(new Rect(0f, 0f, map.mapSize.x, map.mapSize.y), 0, 0, map.generateMipmaps);
							}
							else if (mapImageIndex == 1)
							{
								map.mapTexture.ReadPixels(new Rect(0f, 0f, map.mapSize.x, map.mapSize.y), (int)map.mapSize.x, 0, map.generateMipmaps);
							}
							else if (mapImageIndex == 2)
							{
								map.mapTexture.ReadPixels(new Rect(0f, 0f, map.mapSize.x, map.mapSize.y), (int)map.mapSize.x, (int)map.mapSize.y, map.generateMipmaps);
							}
							else if (mapImageIndex == 3)
							{
								mReaded = true;
								map.mapTexture.ReadPixels(new Rect(0f, 0f, map.mapSize.x, (map.mapSize.y)), 0, (int)map.mapSize.y, map.generateMipmaps);
							}*/

							//Debug.Log("mapImageIndex " + mapImageIndex + " / map.mapSize " + map.mapSize + " / cachedTransform.position " + cachedTransform.position + " / mReaded " + mReaded);
						}
						else
						{
							//yield return new WaitForEndOfFrame();
							mReaded = true;
							map.mapTexture.ReadPixels(new Rect(0f, 0f, map.mapSize.x, map.mapSize.y), 0, 0, map.generateMipmaps);
						}
					}
					else
					{
						if (map.userMapTexture != null)
							map.userMapTexture.ReadPixels(new Rect(0f, 0f, map.mapSize.x, map.mapSize.y), 0, 0, map.generateMipmaps);
					}
				}

				//yield return new WaitForEndOfFrame();

				if (!mApplied)
				{
					mApplied = true;
					if (map.generateMapTexture)
					{
						if (map.optimize)
						{
							map.mapTexture.Compress(true);
							canRender = false;
						}
						map.mapTexture.Apply(map.generateMipmaps, map.optimize);
					}
					else
					{
						/*if (map.transparentTexture)
						{
							ClearColor(map.userMapTexture.GetPixels32());
							map.userMapTexture.SetPixels32(newColors);
						}*/
						//if (map.userMapTexture != null && canRender)
						map.userMapTexture.Apply(map.generateMipmaps, map.optimize);
					}
				}

				yield return new WaitForEndOfFrame();

				if (canRender && !mGenerated)
				{ 
					if (Application.isPlaying)
						mGenerated = true;

					map.SetTexture(map.generateMapTexture ? map.mapTexture : map.userMapTexture);
				}
				if (GetComponent<Camera>().enabled && Application.isPlaying) GetComponent<Camera>().enabled = false;				
			}
		}
		lastRender = Time.time + 1f;
	}

	/// <summary>
	/// Redraw the map's texture.
	/// </summary>
	
	public void Render()
	{
		if (Time.time >= lastRender)
		{			
			if (Application.isPlaying) lastRender = Time.time + 1f;
			mReaded = false;
			mApplied = false;			
			mGenerated = false;
			mWarning = false;

			if (!map.optimize) canRender = true;

			if (map.mapSize.x == 0 || map.mapSize.y == 0)
			{
				map.mapSize = new Vector2(Screen.width, Screen.height);
			}

			if (map.generateMapTexture)
			{
				if (map.userMapTexture != null)
				{
					NJGTools.Destroy(map.userMapTexture);
					map.userMapTexture = null;
				}

				/*if (map.mapTexture != null)
				{
					NJGTools.DestroyImmediate(map.mapTexture);
					map.mapTexture = null;
				}*/

				if (map.mapTexture == null)
				{
					mSize = map.mapSize;
					if (map.mapResolution == NJG.NJGMapBase.Resolution.Double) mSize = map.mapSize * 2;
					map.mapTexture = new Texture2D((int)mSize.x, (int)mSize.y, (TextureFormat)map.textureFormat, map.generateMipmaps);
					map.mapTexture.name = "_NJGMapTexture";
					map.mapTexture.filterMode = map.mapFilterMode;
					map.mapTexture.wrapMode = map.mapWrapMode;
					lastSize = mSize;
				}
			}
			else if (!Application.isPlaying)
			{
				if (map.mapTexture != null)
				{
					NJGTools.DestroyImmediate(map.mapTexture);
					map.mapTexture = null;
				}

				//if (map.userMapTexture != null)
				//{
				map.userMapTexture = new Texture2D((int)map.mapSize.x, (int)map.mapSize.y, (TextureFormat)map.textureFormat, map.generateMipmaps);
				map.userMapTexture.name = "_NJGTempTexture";
				map.userMapTexture.filterMode = map.mapFilterMode;
				map.userMapTexture.wrapMode = map.mapWrapMode;
				//}
			}

			
			ConfigCamera();
			GetComponent<Camera>().enabled = true;
			//if (!Application.isPlaying) camera.Render();
		}
	}
}