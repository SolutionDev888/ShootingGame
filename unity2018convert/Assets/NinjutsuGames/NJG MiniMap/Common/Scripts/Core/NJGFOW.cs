//#define TasharenFOW // Uncomment this line if you have Tasharen FOW System installed.


//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using NJG;
using UnityEngine;
using System.Collections;
using System.Threading;

public class NJGFOW : MonoBehaviour 
{
	public enum State
	{
		Blending,
		NeedUpdate,
		UpdateTexture0,
		UpdateTexture1,
	}

	static NJGFOW mInst;
	static public NJGFOW instance 
	{ 
		get 
		{
			if (mInst == null)
			{
				mInst = GameObject.FindObjectOfType(typeof(NJGFOW)) as NJGFOW;
				if (mInst == null)
				{
					GameObject go = new GameObject("_NJGFOW");
					go.hideFlags = HideFlags.HideInHierarchy;
					mInst = go.AddComponent<NJGFOW>();
				}
			}
			return mInst; 
		} 
	}

	/// <summary>
	/// Returns the range [0-1] explored of the map. 
	/// </summary>

	public float exploredRatio
	{
		get
		{
			if (mBuffer0 == null) return 0;
			float count = 0;
			for (int i = 0, imax = mBuffer0.Length; i < imax; i++) if (mBuffer0[i].g > 0) count++;
			return count / (float)mBuffer0.Length;
		}
	}

	const string FOW_ID = "NJGFOW";

	// Revealers that the thread is currently working with
	static FastList<Revealer> mRevealers = new FastList<Revealer>();

	// Revealers that have been added since last update
	static FastList<Revealer> mAdded = new FastList<Revealer>();

	// Revealers that have been removed since last update
	static FastList<Revealer> mRemoved = new FastList<Revealer>();

	//public Vector2 modifier;

	public class Revealer
	{
		public bool isActive = false;
		public Vector2 pos = Vector2.zero;
		public int revealDistance = 10;
	}

	/*public bool trailEffect;	

	/// <summary>
	/// How long it takes for textures to blend from one to another.
	/// </summary>

	public float textureBlendTime = 0.5f;*/

	// Whether some color buffer is ready to be uploaded to VRAM
	float mBlendFactor = 0f;
	State mState = State.Blending;

	Texture2D mTexture0;
	//Texture2D mTexture1;
	Texture2D hiddenTexture;
	Vector2 mOrigin;

	Color32[] mBuffer0;
	Color32[] mBuffer1;
	Color32[] mBuffer2;
	Color32[] mBuffer3;

	NJGMapBase map;
	float mNextUpdate = 0f;
	Thread mThread;
	//float mZoom = 0;
	//bool initialized;

	void Awake() { map = NJGMapBase.instance; }

	public void Init()
	{
		if (hiddenTexture == null) hiddenTexture = new Texture2D(4, 4);
		if (mBuffer3 == null) mBuffer3 = new Color32[4 * 4];

		// Fill the texture with white (you could also paint it black, then draw with white)
		for (int i = 0, imax = mBuffer3.Length; i < imax; ++i)
		{
			mBuffer3[i] = map.fow.fogColor;
		}

		hiddenTexture.SetPixels32(mBuffer3);

		// Apply all SetPixel calls
		hiddenTexture.Apply();

		// Use Built-in FOW
		if (map.fow.fowSystem == NJGMapBase.FOW.FOWSystem.BuiltInFOW)
		{
			//initialized = true;

			// Create a new texture and assign it to the renderer's material
			if (mTexture0 == null)
			{
				mTexture0 = new Texture2D(map.fow.textureSize, map.fow.textureSize, TextureFormat.ARGB32, false);
				mTexture0.wrapMode = TextureWrapMode.Clamp;
			}

			mOrigin = Vector2.zero;// new Vector2(rendererTransform.localPosition.x, rendererTransform.localPosition.y);
			mOrigin.x -= map.fow.textureSize * 0.5f;
			mOrigin.y -= map.fow.textureSize * 0.5f;

			int size = map.fow.textureSize * map.fow.textureSize;
			if (mBuffer0 == null) mBuffer0 = new Color32[size];
			if (mBuffer1 == null) mBuffer1 = new Color32[size];
			if (mBuffer2 == null) mBuffer2 = new Color32[size];

			for (int i = 0, imax = mBuffer0.Length; i < imax; ++i)
			{
				mBuffer0[i] = Color.clear;
				mBuffer1[i] = Color.clear;
				mBuffer2[i] = Color.clear;
			}			

			UpdateBuffer();
			UpdateTexture();

			// Set textures for FOW
			if (UIMiniMapBase.inst != null)
			{
#if TasharenFOW
				UIMiniMapBase.inst.material.SetTexture("_Revealed", FOWSystem.instance.texture0);
#else
				UIMiniMapBase.inst.material.SetTexture("_Revealed", mTexture0);
#endif
				UIMiniMapBase.inst.material.SetTexture("_Hidden", hiddenTexture);
			}

			if (UIWorldMapBase.inst != null)
			{
#if TasharenFOW
				UIWorldMapBase.inst.material.SetTexture("_Revealed", FOWSystem.instance.texture1);
#else
				UIWorldMapBase.inst.material.SetTexture("_Revealed", mTexture0);
#endif
				UIWorldMapBase.inst.material.SetTexture("_Hidden", hiddenTexture);
			}

			mNextUpdate = Time.time + map.fow.updateFrequency;

			// Add a thread update function -- all visibility checks will be done on a separate thread
			if (mThread == null)
			{
				mThread = new Thread(ThreadUpdate);
				mThread.Start();
			}
		}
		// Use Tasharen FOW
		else if (map.fow.fowSystem == NJGMapBase.FOW.FOWSystem.TasharenFOW)
		{
#if TasharenFOW
			StartCoroutine(SetTasharenFOW());
#endif
		}
	}

#if TasharenFOW
	IEnumerator SetTasharenFOW()
	{
		Debug.Log("SetTasharenFOW " + FOWSystem.instance);

		while (FOWSystem.instance == null)
			yield return null;

		Debug.Log("Init TasharenFOW");

		while (FOWSystem.instance.texture0 == null)
			yield return null;

		while (FOWSystem.instance.texture1 == null)
			yield return null;

		// Set textures for FOW
		if (UIMiniMapBase.inst != null)
		{
			//Debug.Log("UIMiniMapBase TasharenFOW FOWSystem.instance.texture0 " + FOWSystem.instance.texture0 + " / " + FOWSystem.instance.texture0.width);
			//Debug.Log("UIMiniMapBase TasharenFOW FOWSystem.instance.texture1 " + FOWSystem.instance.texture1 + " / " + FOWSystem.instance.texture1.width);
			UIMiniMapBase.inst.material.SetTexture("_Revealed", FOWSystem.instance.texture0);
			UIMiniMapBase.inst.material.SetTexture("_Hidden", hiddenTexture);
		}

		if (UIWorldMapBase.inst != null)
		{
			//Debug.Log("UIWorldMapBase TasharenFOW FOWSystem.instance.texture1 " + FOWSystem.instance.texture1);
			UIWorldMapBase.inst.material.SetTexture("_Revealed", FOWSystem.instance.texture1);
			UIWorldMapBase.inst.material.SetTexture("_Hidden", hiddenTexture);
		}
	}
#endif

	/// <summary>
	/// Ensure that the thread gets terminated.
	/// </summary>

	void OnDestroy()
	{
		if (mThread != null)
		{
			mThread.Abort();
			while (mThread.IsAlive) Thread.Sleep(1);
			mThread = null;
		}
	}

	/// <summary>
	/// If it's time to update, do so now.
	/// </summary>

	void ThreadUpdate()
	{
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

		for (; ; )
		{
			if (mState == State.NeedUpdate)
			{
				sw.Reset();
				sw.Start();
				UpdateBuffer();
				sw.Stop();
				//mElapsed = 0.001f * (float)sw.ElapsedMilliseconds;
				mState = State.UpdateTexture0;
			}
			Thread.Sleep(1);
		}		
	}

	void Update()
	{
		if (map == null) return;

		if (map.fow.textureBlendTime > 0f)
			mBlendFactor = Mathf.Clamp01(mBlendFactor + Time.deltaTime / map.fow.textureBlendTime);
		else mBlendFactor = 1f;

		//mZoom = UIMiniMapBase.inst.zoom;

		if (mState == State.Blending)
		{
			float time = Time.time;

			if (mNextUpdate < time)
			{
				mNextUpdate = time + map.fow.updateFrequency;
				mState = State.NeedUpdate;
			}
		}
		else if (mState != State.NeedUpdate)
		{
			UpdateTexture();
		}
	}

	/// <summary>
	/// Create a new fog revealer.
	/// </summary>

	static public Revealer CreateRevealer()
	{
		Revealer rev = new Revealer();
		rev.isActive = false;
		lock (mAdded) mAdded.Add(rev);
		return rev;
	}

	/// <summary>
	/// Delete the specified revealer.
	/// </summary>

	static public void DeleteRevealer(Revealer rev) { lock (mRemoved) mRemoved.Add(rev); }

	/// <summary>
	/// Retrieve the revealed buffer. 
	/// This can be used to save FOW state into a Database.
	/// </summary>

	public byte[] GetRevealedBuffer()
	{
		int size = map.fow.textureSize * map.fow.textureSize;
		byte[] buff = new byte[size];
		for (int i = 0; i < size; ++i) buff[i] = mBuffer1[i].g;
		return buff;
	}

	/// <summary>
	/// Reveal the area, given the specified array of bytes. 
	/// You can load FOW bytearray from a Database.
	/// </summary>

	public void RevealFOW(byte[] arr)
	{
		int mySize = map.fow.textureSize * map.fow.textureSize;

		if (arr.Length != mySize)
		{
			Debug.LogError("Buffer size mismatch. Fog is " + mySize + ", but passed array is " + arr.Length);
		}
		else
		{
			if (mBuffer0 == null)
			{
				mBuffer0 = new Color32[mySize];
				mBuffer1 = new Color32[mySize];
			}

			for (int i = 0; i < mySize; ++i)
			{
				mBuffer0[i].g = arr[i];
				mBuffer1[i].g = arr[i];
			}
		}
	}

	/// <summary>
	/// Reveal the area, given the specified array of strings. 
	/// You can load FOW string array from PlayerPrefs.
	/// </summary>

	public void RevealFOW(string fowData)
	{
		int mySize = map.fow.textureSize * map.fow.textureSize;
		string[] arr = fowData.Split('|');

		if (arr.Length != mySize)
		{
			Debug.LogError("Buffer size mismatch. Fog is " + mySize + ", but passed array is " + arr.Length);
		}
		else
		{
			if (mBuffer0 == null)
			{
				mBuffer0 = new Color32[mySize];
				mBuffer1 = new Color32[mySize];
			}

			for (int i = 0; i < mySize; ++i)
			{
				mBuffer0[i].g = byte.Parse(arr[i]);
				mBuffer1[i].g = byte.Parse(arr[i]);
			}
		}
	}

	/// <summary>
	/// Serialize fog of war to string.
	/// </summary>

	string SerializeFOW()
	{
		int size = map.fow.textureSize * map.fow.textureSize;
		string[] buff = new string[size];
		for (int i = 0; i < size; ++i) buff[i] = "" + mBuffer1[i].g;
		return string.Join("|", buff);
	}

	/// <summary>
	/// Save current fog of war state
	/// </summary>

	void Save(string gameName)
	{
		string fowData = SerializeFOW();
		if (!string.IsNullOrEmpty(fowData))
			PlayerPrefs.SetString(gameName + FOW_ID, fowData);
	}

	/// <summary>
	/// Load fog of war state
	/// </summary>

	void Load(string gameName) { RevealFOW(PlayerPrefs.GetString(gameName + FOW_ID, null)); }	

	/// <summary>
	/// Reset Fog of War clearing all explored areas.
	/// </summary>

	public void ResetFOW() 
	{
		if (map.fow.fowSystem == NJGMapBase.FOW.FOWSystem.BuiltInFOW)
		{
			for (int i = 0; i < NJGMapItem.list.Count; ++i)
			{
				NJGMapItem item = NJGMapItem.list[i];
				if (item.cachedTransform != NJG.UIMiniMapBase.inst.target) item.isRevealed = false;
			}
		}

		Init();
	}

	/// <summary>
	/// Update the specified texture with the new color buffer.
	/// </summary>

	void UpdateTexture()
	{
		if (mState == State.UpdateTexture0)
		{
			// Texture updates are spread between two frames to make it even less noticeable when they get updated
			mTexture0.SetPixels32(mBuffer0);
			mTexture0.Apply();
			mState = State.UpdateTexture1;
			mBlendFactor = 0f;
		}
		else if (mState == State.UpdateTexture1)
		{
			//mTexture1.SetPixels32(mBuffer1);
			//mTexture1.Apply();
			mState = State.Blending;
		}		
	}

	/// <summary>
	/// Update the fog of war's visibility.
	/// </summary>

	void UpdateBuffer()
	{
		// Add all items scheduled to be added
		if (mAdded.size > 0)
		{
			lock (mAdded)
			{
				while (mAdded.size > 0)
				{
					int index = mAdded.size - 1;
					mRevealers.Add(mAdded.buffer[index]);
					mAdded.RemoveAt(index);
				}
			}
		}

		// Remove all items scheduled for removal
		if (mRemoved.size > 0)
		{
			lock (mRemoved)
			{
				while (mRemoved.size > 0)
				{
					int index = mRemoved.size - 1;
					mRevealers.Remove(mRemoved.buffer[index]);
					mRemoved.RemoveAt(index);
				}
			}
		}

		// Use the texture blend time, thus estimating the time this update will finish
		// Doing so helps avoid visible changes in blending caused by the blended result being X milliseconds behind.
		float factor = (map.fow.textureBlendTime > 0f) ? Mathf.Clamp01(mBlendFactor + map.elapsed / map.fow.textureBlendTime) : 1f;

		// Clear the buffer's red channel (channel used for current visibility -- it's updated right after)
		if (mBuffer0 != null)
		{
			for (int i = 0, imax = mBuffer0.Length; i < imax; ++i)
			{
				mBuffer0[i] = Color32.Lerp(mBuffer0[i], mBuffer1[i], factor);
				mBuffer0[i].r = 0;
			}
		}

		// For conversion from world coordinates to texture coordinates
		float worldToTex = (float)(map.fow.textureSize / map.fow.textureSize);

		// Update the visibility buffer, one revealer at a time
		for (int i = 0; i != mRevealers.size; ++i)
		{
			Revealer rev = mRevealers[i];
			if (!rev.isActive) continue;

			RevealAtPosition(rev, worldToTex);
		}

		// Blur the final visibility data
		for (int i = 0; i != map.fow.blurIterations; ++i) BlurVisibility();

		// Reveal the map based on what's currently visible
		RevealMap();
	}

	/// <summary>
	/// Blur the visibility data.
	/// </summary>

	void BlurVisibility()
	{
		Color32 c;

		for (int y = 0; y < map.fow.textureSize; ++y)
		{
			int yw = y * map.fow.textureSize;
			int yw0 = (y - 1);
			if (yw0 < 0) yw0 = 0;
			int yw1 = (y + 1);
			if (yw1 == map.fow.textureSize) yw1 = y;

			yw0 *= map.fow.textureSize;
			yw1 *= map.fow.textureSize;

			for (int x = 0; x < map.fow.textureSize; ++x)
			{
				int x0 = (x - 1);
				if (x0 < 0) x0 = 0;
				int x1 = (x + 1);
				if (x1 == map.fow.textureSize) x1 = x;

				int index = x + yw;
				int val = mBuffer1[index].r;

				val += mBuffer1[x0 + yw].r;
				val += mBuffer1[x1 + yw].r;
				val += mBuffer1[x + yw0].r;
				val += mBuffer1[x + yw1].r;

				val += mBuffer1[x0 + yw0].r;
				val += mBuffer1[x1 + yw0].r;
				val += mBuffer1[x0 + yw1].r;
				val += mBuffer1[x1 + yw1].r;

				c = mBuffer2[index];
				c.r = (byte)(val / 9);
				mBuffer2[index] = c;

				if (map.fow.trailEffect)
				{
					mBuffer2[index].a = 0;
					mBuffer2[index].g = 0;
					mBuffer2[index].b = 0;
				}
			}
		}

		// Swap the buffer so that the blurred one is used
		Color32[] temp = mBuffer1;
		mBuffer1 = mBuffer2;
		mBuffer2 = temp;
	}

	//public float mod2 = 1;

	/// <summary>
	/// Reveals FOW at position.
	/// </summary>

	void RevealAtPosition(Revealer r, float worldToTex)
	{
		// Position relative to the fog of war
		Vector2 pos = r.pos - mOrigin;
		//if (pos == lastPos) return;

		// Coordinates we'll be dealing with
		int xmin = Mathf.RoundToInt((pos.x - r.revealDistance) * worldToTex);
		int ymin = Mathf.RoundToInt((pos.y - r.revealDistance) * worldToTex);
		int xmax = Mathf.RoundToInt((pos.x + r.revealDistance) * worldToTex);
		int ymax = Mathf.RoundToInt((pos.y + r.revealDistance) * worldToTex);

		int cx = Mathf.RoundToInt(pos.x * worldToTex);
		int cy = Mathf.RoundToInt(pos.y * worldToTex);

		int size = (int)(map.fow.textureSize);		

		cx = Mathf.Clamp(cx, 0, size - 1);
		cy = Mathf.Clamp(cy, 0, size - 1);

		int radius = Mathf.RoundToInt(r.revealDistance * r.revealDistance * worldToTex * worldToTex);

		for (int y = ymin; y < ymax; ++y)
		{
			if (y > -1 && y < size)
			{
				int yw = y * size;

				for (int x = xmin; x < xmax; ++x)
				{
					if (x > -1 && x < size)
					{
						int xd = x - cx;
						int yd = y - cy;
						int dist = xd * xd + yd * yd;

						// Reveal this pixel
						if (dist < radius) mBuffer1[x + yw].r = 255;
					}
				}
			}
		}

		//lastPos = pos;
	}

	/// <summary>
	/// Reveal the map by updating the green channel to be the maximum of the red channel.
	/// </summary>

	void RevealMap()
	{
		for (int y = 0; y < map.fow.textureSize; ++y)
		{
			int yw = y * map.fow.textureSize;

			for (int x = 0; x < map.fow.textureSize; ++x)
			{
				int index = x + yw;
				Color32 c = mBuffer1[index];

				if (c.g < c.r)
				{
					c.g = c.r;
					mBuffer1[index] = c;
				}
			}
		}
	}

	/// <summary>
	/// Checks to see if the specified position is currently visible.
	/// </summary>

	public bool IsVisible(Vector2 pos)
	{
		return IsVisible(pos, 0);
	}

	/// <summary>
	/// Checks to see if the specified position is currently visible.
	/// </summary>

	public bool IsVisible(Vector2 pos, int treshold)
	{
		if (mBuffer0 == null) return false;
		pos -= mOrigin;

		float worldToTex = (float)map.fow.textureSize / (float)map.fow.textureSize;
		//float worldToTex2 = (float)map.fow.textureSize / (float)map.fow.revealDistance;

		//Debug.Log("worldToTex " + worldToTex + " / worldToTex2 " + worldToTex2);
		int cx = Mathf.RoundToInt(pos.x * worldToTex);
		int cy = Mathf.RoundToInt(pos.y * worldToTex);

		cx = Mathf.Clamp(cx, 0, map.fow.textureSize - 1);
		cy = Mathf.Clamp(cy, 0, map.fow.textureSize - 1);
		int index = cx + cy * map.fow.textureSize;

		//Debug.Log("IsVisible r: " + mBuffer1[index].r + " / g: " + mBuffer1[index].g + " / b: " + mBuffer1[index].b + " / a: " + mBuffer1[index].a);
		return mBuffer1[index].r > treshold || mBuffer0[index].r > treshold;
	}

	public bool IsExplored(Vector2 pos)
	{
		return IsExplored(pos, 0);
	}

	/// <summary>
	/// Checks to see if the specified position has been explored.
	/// </summary>

	public bool IsExplored(Vector2 pos, int treshold)
	{
		if (mBuffer0 == null) return false;
		pos -= mOrigin;

		float worldToTex = (float)map.fow.textureSize / (float)map.fow.textureSize;
		int cx = Mathf.RoundToInt(pos.x * worldToTex);
		int cy = Mathf.RoundToInt(pos.y * worldToTex);

		cx = Mathf.Clamp(cx, 0, map.fow.textureSize - 1);
		cy = Mathf.Clamp(cy, 0, map.fow.textureSize - 1);
		return mBuffer0[cx + cy * map.fow.textureSize].g > treshold;
	}	
}
