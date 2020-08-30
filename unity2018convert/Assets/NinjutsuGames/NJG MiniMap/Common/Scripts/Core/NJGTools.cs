using UnityEngine;
using System.Collections;

public class NJGTools {

	/// <summary>
	/// Destroy the specified object, immediately if in edit mode.
	/// </summary>

	static public void Destroy(UnityEngine.Object obj)
	{
		if (obj != null)
		{
			if (Application.isPlaying)
			{
				if (obj is GameObject)
				{
					GameObject go = obj as GameObject;
					go.transform.parent = null;
				}

				UnityEngine.Object.Destroy(obj);
			}
			else UnityEngine.Object.DestroyImmediate(obj);
		}
	}

	/// <summary>
	/// Destroy the specified object immediately, unless not in the editor, in which case the regular Destroy is used instead.
	/// </summary>

	static public void DestroyImmediate(UnityEngine.Object obj)
	{
		if (obj != null)
		{
			if (Application.isEditor) UnityEngine.Object.DestroyImmediate(obj);
			else UnityEngine.Object.Destroy(obj);
		}
	}

	/// <summary>
	/// Creates a simple 4 vertex plane mesh.
	/// </summary>

	static public Mesh CreatePlane()
	{
		Mesh mesh = new Mesh();

		Vector3[] vertices = new Vector3[]
        {
            new Vector3( -1, -1,  0),
            new Vector3( -1, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, -1, 0),
        };

		Vector2[] uv = new Vector2[]
        {
            new Vector2(1, 1),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(0, 0),
        };

		int[] triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3,
        };

		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		;

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5
		mesh.MarkDynamic();
#endif
		return mesh;
	}	

	/// <summary>
	/// Find all active objects of specified type.
	/// </summary>

	static public T[] FindActive<T>() where T : Component
	{
		return GameObject.FindObjectsOfType(typeof(T)) as T[];
	}

	/// <summary>
	/// Find the camera responsible for drawing the objects on the specified layer.
	/// </summary>

	static public Camera FindCameraForLayer(int layer)
	{
		int layerMask = 1 << layer;

		Camera cam;

		/*for (int i = 0; i < UICamera.list.size; ++i)
		{
			cam = UICamera.list.buffer[i].cachedCamera;
			if (cam && (cam.cullingMask & layerMask) != 0)
				return cam;
		}*/

		cam = Camera.main;
		if (cam && (cam.cullingMask & layerMask) != 0) return cam;

#if UNITY_4_3
		Camera[] cameras = NGUITools.FindActive<Camera>();
		for (int i = 0, imax = cameras.Length; i < imax; ++i)
#else
		Camera[] cameras = new Camera[Camera.allCamerasCount];
		int camerasFound = Camera.GetAllCameras(cameras);
		for (int i = 0; i < camerasFound; ++i)
#endif
		{
			cam = cameras[i];
			if (cam && cam.enabled && (cam.cullingMask & layerMask) != 0)
				return cam;
		}
		return null;
	}

	/// <summary>
	/// Unity4 has changed GameObject.active to GameObject.activeself.
	/// </summary>

	static public bool GetActive(GameObject go)
	{
#if UNITY_3_5
		return go && go.active;
#else
		return go && go.activeInHierarchy;
#endif
	}

	/// <summary>
	/// Add a new child game object.
	/// </summary>

	static public GameObject AddChild(GameObject parent) { return AddChild(parent, true); }

	/// <summary>
	/// Add a new child game object.
	/// </summary>

	static public GameObject AddChild(GameObject parent, bool undo)
	{
		GameObject go = new GameObject();
#if UNITY_EDITOR
		if (undo) UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
		if (parent != null)
		{
			Transform t = go.transform;
			t.parent = parent.transform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
			go.layer = parent.layer;
		}
		return go;
	}

	/// <summary>
	/// Instantiate an object and add it to the specified parent.
	/// </summary>

	static public GameObject AddChild(GameObject parent, GameObject prefab)
	{
		GameObject go = GameObject.Instantiate(prefab) as GameObject;
#if UNITY_EDITOR
		UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
		if (go != null && parent != null)
		{
			Transform t = go.transform;
			t.parent = parent.transform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
			t.localScale = Vector3.one;
			go.layer = parent.layer;
		}
		return go;
	}

	/// <summary>
	/// Activate the specified object and all of its children.
	/// </summary>

	static void Activate(Transform t) { Activate(t, false); }

	/// <summary>
	/// Activate the specified object and all of its children.
	/// </summary>

	static void Activate(Transform t, bool compatibilityMode)
	{
		SetActiveSelf(t.gameObject, true);

		if (compatibilityMode)
		{
			// If there is even a single enabled child, then we're using a Unity 4.0-based nested active state scheme.
			for (int i = 0, imax = t.childCount; i < imax; ++i)
			{
				Transform child = t.GetChild(i);
				if (child.gameObject.activeSelf) return;
			}

			// If this point is reached, then all the children are disabled, so we must be using a Unity 3.5-based active state scheme.
			for (int i = 0, imax = t.childCount; i < imax; ++i)
			{
				Transform child = t.GetChild(i);
				Activate(child, true);
			}
		}
	}

	/// <summary>
	/// Deactivate the specified object and all of its children.
	/// </summary>

	static void Deactivate(Transform t) { SetActiveSelf(t.gameObject, false); }

	/// <summary>
	/// SetActiveRecursively enables children before parents. This is a problem when a widget gets re-enabled
	/// and it tries to find a panel on its parent.
	/// </summary>

	static public void SetActive(GameObject go, bool state) { SetActive(go, state, true); }

	/// <summary>
	/// SetActiveRecursively enables children before parents. This is a problem when a widget gets re-enabled
	/// and it tries to find a panel on its parent.
	/// </summary>

	static public void SetActive(GameObject go, bool state, bool compatibilityMode)
	{
		if (go)
		{
			if (state)
			{
				Activate(go.transform, compatibilityMode);
			}
			else Deactivate(go.transform);
		}
	}

	/// <summary>
	/// Unity4 has changed GameObject.active to GameObject.SetActive.
	/// </summary>

	static public void SetActiveSelf(GameObject go, bool state)
	{
#if UNITY_3_5
		go.active = state;
#else
		go.SetActive(state);
#endif
	}

	/// <summary>
	/// Finds the specified component on the game object or one of its parents.
	/// </summary>

	static public T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null) return null;
		object comp = go.GetComponent<T>();

		if (comp == null)
		{
			Transform t = go.transform.parent;

			while (t != null && comp == null)
			{
				comp = t.gameObject.GetComponent<T>();
				t = t.parent;
			}
		}
		return (T)comp;
	}

	/// <summary>
	/// Finds the specified component on the game object or one of its parents.
	/// </summary>

	static public T FindInParents<T>(Transform trans) where T : Component
	{
		if (trans == null) return null;
		object comp = trans.GetComponent<T>();

		if (comp == null)
		{
			Transform t = trans.transform.parent;

			while (t != null && comp == null)
			{
				comp = t.gameObject.GetComponent<T>();
				t = t.parent;
			}
		}
		return (T)comp;
	}

	/// <summary>
	/// Add a collider to the game object containing one or more widgets.
	/// </summary>

	static public BoxCollider AddWidgetCollider(GameObject go) { return AddWidgetCollider(go, false); }

	/// <summary>
	/// Add a collider to the game object containing one or more widgets.
	/// </summary>

	static public BoxCollider AddWidgetCollider(GameObject go, bool considerInactive)
	{
		if (go != null)
		{
			Collider col = go.GetComponent<Collider>();
			BoxCollider box = col as BoxCollider;

			if (box == null)
			{
				if (col != null)
				{
					if (Application.isPlaying) GameObject.Destroy(col);
					else GameObject.DestroyImmediate(col);
				}
				box = go.AddComponent<BoxCollider>();
				box.isTrigger = true;
			}

			UpdateWidgetCollider(box, considerInactive);
			return box;
		}
		return null;
	}

	/// <summary>
	/// Adjust the widget's collider based on the depth of the widgets, as well as the widget's dimensions.
	/// </summary>

	static public void UpdateWidgetCollider(GameObject go)
	{
		UpdateWidgetCollider(go, false);
	}

	/// <summary>
	/// Adjust the widget's collider based on the depth of the widgets, as well as the widget's dimensions.
	/// </summary>

	static public void UpdateWidgetCollider(GameObject go, bool considerInactive)
	{
		if (go != null)
		{
			UpdateWidgetCollider(go.GetComponent<BoxCollider>(), considerInactive);
		}
	}

	/// <summary>
	/// Adjust the widget's collider based on the depth of the widgets, as well as the widget's dimensions.
	/// </summary>

	static public void UpdateWidgetCollider(BoxCollider bc)
	{
		UpdateWidgetCollider(bc, false);
	}

	/// <summary>
	/// Adjust the widget's collider based on the depth of the widgets, as well as the widget's dimensions.
	/// </summary>

	static public void UpdateWidgetCollider(BoxCollider box, bool considerInactive)
	{
		if (box != null)
		{
			Bounds b = new Bounds(Vector3.zero, Vector3.zero);
			box.center = b.center;
			box.size = new Vector3(b.size.x, b.size.y, 0f);		
	
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(box);
#endif
		}
	}
}
