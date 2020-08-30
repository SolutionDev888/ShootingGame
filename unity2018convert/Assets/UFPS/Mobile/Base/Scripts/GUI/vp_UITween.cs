/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UITween.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	vp_UITween is a script extension for tweening properties of
//					objects in Unity.
//
/////////////////////////////////////////////////////////////////////////////////

//#define DEBUG	// uncomment to display tweens in the Unity Editor Hierarchy

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class vp_UITween : MonoBehaviour
{

	/////////////////////////////////////////////////////////////////////////////////
	//
	//	vp_UITween.Handle
	//
	//	description:	This class is used to keep track of a vp_UITween
	//
	/////////////////////////////////////////////////////////////////////////////////
	public class Handle
	{
	
		public bool Custom = false;
		public Tween TweenObject = null;
		protected bool m_Active = false;
		public bool Active
		{
			get{ return m_Active; }
			set{
				if(value) TweenObject.ElapsedTime = 0;
				m_Active = value;
			}	
		}
		
	}
	
	
	/////////////////////////////////////////////////////////////////////////////////
	//
	//	vp_UITween.Tween
	//
	//	description:	This class stores all the information for a tween and allows
	//					it to be reused if a vp_UITween.Handle was provided
	//
	/////////////////////////////////////////////////////////////////////////////////
	public class Tween
	{
	
		public object Object = null;
		public Transform Transform = null;
		public vp_UITween.Handle Handle = new vp_UITween.Handle();
		public string Name = null;
		public float ElapsedTime = 0;
		public float Duration;
		public Action Callback;
		public Color StartColor;
		public Color EndColor;
		public Color CurrentColor;
		public Vector3 StartScale;
		public Vector3 EndScale;
		public Vector3 CurrentScale;
		public Quaternion StartRotation;
		public Quaternion EndRotation;
		public Quaternion CurrentRotation;
		public vp_UITween.vp_UITweenType Type = vp_UITween.vp_UITweenType.None;
		
		
		/// <summary>
		/// Initializes a new instance of the vp_UITween.tween
		/// class and will set values from parameters provided
		/// </summary>
		public Tween( object obj, params object[] parameters )
		{
			
			Object = obj;
			
			for(int i=0;i<parameters.Length;i++)
			{
				object o = parameters[i];
				if((i % 2) == 0)
				{
					object value = parameters[i+1];
					if(((string)o) == "duration")
						Duration = Convert.ToSingle(value);
					if(((string)o) == "startColor")
						StartColor = (Color)value;
					if(((string)o) == "endColor")
						EndColor = (Color)value;
					if(((string)o) == "scale")
						EndScale = (Vector3)value;
					if(((string)o) == "rotation")
						EndRotation = (Quaternion)value;
					if(((string)o) == "handle")
						Handle = (vp_UITween.Handle)value;
					if(((string)o) == "onComplete")
						Callback = (Action)value;
					if(((string)o) == "type")
						Type = (vp_UITween.vp_UITweenType)value;
				}
			}
			if(Handle != null)
				Handle.Custom = true;
				
			if(Handle == null)
				Handle = new Handle();
				
			Handle.TweenObject = this;
			
			if(obj.GetType() == typeof(GameObject))
			{
				GameObject go = (GameObject)obj;
				Transform = go.transform;
				Name = go.name;
				StartScale = Transform.localScale;
			}
		
		}
		
		
		/// <summary>
		/// checks to see if color properties have changed for this tween
		/// </summary>
		public virtual void ColorCheck( object obj, params object[] parameters )
		{
		
			Color startColor = (Color)parameters[0];
			Color to = (Color)parameters[1];
			float duration = Convert.ToSingle(parameters[2]);
			Action callback = (Action)parameters[3];
		
			if(Object != obj || startColor != CurrentColor || EndColor != to || Duration != duration || Callback != callback)
			{
				Object = obj;
				StartColor = startColor;
				EndColor = to;
				Duration = duration;
				Callback = callback;
				Handle.Active = true;
			}
		
		}
		
		
		/// <summary>
		/// checks to see if scale properties have changed for this tween
		/// </summary>
		public virtual void ScaleCheck( object obj, params object[] parameters )
		{
		
			Vector3 scale = (Vector3)parameters[0];
			float duration = Convert.ToSingle(parameters[1]);
			Action callback = (Action)parameters[2];
		
			if(Object != obj || Transform.localScale != CurrentScale || EndScale != scale || Duration != duration || Callback != callback)
			{
				Object = obj;
				StartScale = Transform.localScale;
				EndScale = scale;
				Duration = duration;
				Callback = callback;
				Handle.Active = true;
			}
		
		}
		
		
		/// <summary>
		/// checks to see if rotation properties have changed for this tween
		/// </summary>
		public virtual void RotationCheck( object obj, params object[] parameters )
		{
		
			Quaternion rotation = (Quaternion)parameters[0];
			float duration = Convert.ToSingle(parameters[1]);
			Action callback = (Action)parameters[2];
		
			if(Object != obj || Transform.localScale != CurrentScale || EndRotation != rotation || Duration != duration || Callback != callback)
			{
				Object = obj;
				StartRotation = Transform.localRotation;
				EndRotation = rotation;
				Duration = duration;
				Callback = callback;
				Handle.Active = true;
			}
		
		}
		
		
		/// <summary>
		/// sets the color. should be called from an update loop.
		/// </summary>
		public virtual void ColorUpdate()
		{
		
			GameObject go = (GameObject)Object;
			go.GetComponent<Renderer>().material.color = CurrentColor;
		
		}
		
		
		/// <summary>
		/// sets the scale. should be called from an update loop.
		/// </summary>
		public virtual void ScaleUpdate()
		{
		
			Transform.localScale = CurrentScale;
		
		}
	
	}
	
	
	// type's of tween that can be used
	public enum vp_UITweenType
	{
		None,
		Color,
		Scale,
		Rotation
	}
	
	
	public Dictionary<Handle, Tween> Tweens = new Dictionary<Handle, Tween>(); // a list of all the tweens being managed
	protected static List<string> AllowedHashParameters = new List<string>(){ "color", "startColor", "endColor", "type", "handle", "duration", "onComplete" }; // TODO: impliment this
	
	
	/// <summary>
	/// Retrieves the the vp_UITweenInstance
	/// </summary>
	protected static vp_UITween m_Instance;
	public static vp_UITween Instance
	{
		get
		{
			if(m_Instance == null)
			{
				GameObject go = new GameObject("vp_UITween");
				m_Instance = go.AddComponent<vp_UITween>();
#if (UNITY_EDITOR && !DEBUG)
				m_Instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
			}
			return m_Instance;
		}
	}
	
	
	/// <summary>
	/// call update as soon as possible
	/// </summary>
	protected virtual void Start(){ Update(); }
	
	
	/// <summary>
	/// loop through all the tweens and run their update functions
	/// </summary>
	protected virtual void Update()
	{
	
		foreach(vp_UITween.Tween tween in Tweens.Values)
		{
			ColorUpdate(tween);
			ScaleUpdate(tween);
			RotationUpdate(tween);
		}
	
	}
	
	
	/// <summary>
	/// updates the color for a tween
	/// </summary>
	protected virtual void ColorUpdate( vp_UITween.Tween tween )
	{
	
		if(tween.Type != vp_UITween.vp_UITweenType.Color)
			return;
			
	    if(tween.ElapsedTime < tween.Duration)
	    {
	    	// lerp the color
	    	tween.CurrentColor = Color.Lerp(tween.StartColor, tween.EndColor, (tween.ElapsedTime / tween.Duration));
	    	
	    	// update the color on the object
	    	tween.ColorUpdate();
	    	
	    	// update the elapsed time
	    	tween.ElapsedTime += Time.deltaTime;
	    	
	    	return;
	    }
	    
	    if(tween.CurrentColor != tween.EndColor)
	    {
		    tween.CurrentColor = tween.EndColor;
		    tween.ColorUpdate();
		}
	    
	    // callback
    	if(tween.Callback != null)
    		tween.Callback();
    
    	if(!tween.Handle.Custom)	// destroy the tween if no handle was provided
    		Instance.Tweens.Remove(tween.Handle);
    	else 						// set to inactive if a handle was provided
    		tween.Handle.Active = false;
	
	}
	
	
	/// <summary>
	/// updates the scale for a tween
	/// </summary>
	protected virtual void ScaleUpdate( vp_UITween.Tween tween )
	{
	
		if(tween.Type != vp_UITween.vp_UITweenType.Scale || !tween.Handle.Active)
			return;
			
	    if(tween.ElapsedTime < tween.Duration)
	    {
	    	// lerp the color
	    	tween.CurrentScale = Vector3.Lerp(tween.StartScale, tween.EndScale, (tween.ElapsedTime / tween.Duration));
	    	
	    	// update the color on the object
	    	tween.Transform.localScale = tween.CurrentScale;
	    	
	    	// update the elapsed time
	    	tween.ElapsedTime += Time.deltaTime;
	    	
	    	return;
	    }
	    
	    if(tween.Transform.localScale != tween.EndScale)
		    tween.Transform.localScale = tween.EndScale;
	    
	    // callback
    	if(tween.Callback != null)
    		tween.Callback();
    
    	if(!tween.Handle.Custom)	// destroy the tween if no handle was provided
    		Instance.Tweens.Remove(tween.Handle);
    	else 						// set to inactive if a handle was provided
    		tween.Handle.Active = false;
	
	}
	
	
	/// <summary>
	/// updates the rotation for a tween
	/// </summary>
	protected virtual void RotationUpdate( vp_UITween.Tween tween )
	{
	
		if(tween.Type != vp_UITween.vp_UITweenType.Rotation || !tween.Handle.Active)
			return;
			
	    if(tween.ElapsedTime < tween.Duration)
	    {
	    	// lerp the color
	    	tween.CurrentRotation = Quaternion.Lerp(tween.StartRotation, tween.EndRotation, (tween.ElapsedTime / tween.Duration));
	    	
	    	// update the color on the object
	    	tween.Transform.localRotation = tween.CurrentRotation;
	    	
	    	// update the elapsed time
	    	tween.ElapsedTime += Time.deltaTime;
	    	
	    	return;
	    }
	    
	    if(tween.Transform.localRotation != tween.EndRotation)
		    tween.Transform.localRotation = tween.EndRotation;
	    
	    // callback
    	if(tween.Callback != null)
    		tween.Callback();
    
    	if(!tween.Handle.Custom)	// destroy the tween if no handle was provided
    		Instance.Tweens.Remove(tween.Handle);
    	else 						// set to inactive if a handle was provided
    		tween.Handle.Active = false;
	
	}
	
	
	/// <summary>
	/// helper to create a hashtable from provided parameters
	/// should be Hash("string", object, "string", object) where
	/// string is key and object is value
	/// </summary>
	public static Hashtable Hash( params object[] parameters )
	{
	
		if((parameters.Length % 2) != 0)
		{
			Debug.LogError("Invalid parameter length");
			return null;
		}
	
		Hashtable ht = new Hashtable();
		string key = null;
		for(int i=0;i<parameters.Length;i++)
		{
			object obj = parameters[i];
			if((i % 2) == 0)
			{
				key = (string)obj;
			}
			else
			{
				ht.Add(key, obj);
				key = null;
			}
		}
		
		return ht;
	
	}
	
	
	/// <summary>
	/// Gets the tween for specified object
	/// </summary>
	public static vp_UITween.Tween GetTweenForObject( object obj )
	{
	
		return Instance.Tweens.Values.FirstOrDefault(t => t.Object == obj);
	
	}
	
	
	/// <summary>
	/// Gets the handle for the specified object
	/// </summary>
	public static vp_UITween.Handle GetHandleForObject( object obj )
	{
	
		vp_UITween.Tween tween = GetTweenForObject(obj);
		return tween != null ? tween.Handle : null;
	
	}
	
	
	/// <summary>
	/// Creates or updates a Color Tween and fades to the alpha specified
	/// </summary>
	public static void FadeTo( object obj, float alpha, float duration ){ ColorTo( obj, vp_UITween.Hash("color", alpha, "duration", duration, "handle", null, "onComplete", null) ); }
	public static void FadeTo( object obj, float alpha, float duration, vp_UITween.Handle handle ){ ColorTo( obj, vp_UITween.Hash("color", alpha, "duration", duration, "handle", handle, "onComplete", null) ); }
	public static void FadeTo( object obj, float alpha, float duration, vp_UITween.Handle handle, Action onComplete ){ ColorTo( obj, vp_UITween.Hash("color", alpha, "duration", duration, "handle", handle, "onComplete", onComplete) ); }
	
	
	/// <summary>
	/// Creates or updates a Color Tween
	/// </summary>
	public static void ColorTo( object obj, Color color, float duration ){ ColorTo( obj, vp_UITween.Hash("color", color, "duration", duration, "handle", null, "onComplete", null) ); }
	public static void ColorTo( object obj, Color color, float duration, vp_UITween.Handle handle ){ ColorTo( obj, vp_UITween.Hash("color", color, "duration", duration, "handle", handle, "onComplete", null) ); }
	public static void ColorTo( object obj, Color color, float duration, vp_UITween.Handle handle, Action onComplete ){ ColorTo( obj, vp_UITween.Hash("color", color, "duration", duration, "handle", handle, "onComplete", onComplete) ); }
	public static void ColorTo( object obj, Hashtable ht )
	{
		
		if(obj.GetType() != typeof(GameObject))
		{
			Debug.LogError("This object must be of type GameObject. vp_UITween.ColorTo cannot proceed.");
			return;
		}
		
		GameObject go = (GameObject)obj;
		
		if(go.GetComponent<Renderer>() == null)
		{
			Debug.LogError("This GameObject does not have a renderer. vp_UITween.ColorTo cannot proceed.");
			return;
		}
		
		Color alpha = go.GetComponent<Renderer>().material.color;
		alpha.a = ht["color"].GetType() == typeof(float) ? (float)ht["color"] : alpha.a;
		Color color = ht["color"].GetType() == typeof(float) ? alpha : (Color)ht["color"];
		
		vp_UITween.Tween tween = null;
		if(!Instance.Tweens.TryGetValue((vp_UITween.Handle)ht["handle"], out tween))
		{
			tween = new vp_UITween.Tween( obj, "startColor", go.GetComponent<Renderer>().material.color, "endColor", color, "duration", ht["duration"], "handle", ht["handle"], "onComplete", ht["onComplete"], "type", vp_UITween.vp_UITweenType.Color );
			Instance.Tweens.Add((vp_UITween.Handle)ht["handle"], tween);
		}
		
		tween.ColorCheck(obj, go.GetComponent<Renderer>().material.color, color, ht["duration"], ht["onComplete"]);
	
	}
	
	
	/// <summary>
	/// Creates or updates a Scale Tween
	/// </summary>
	public static void ScaleTo( object obj, Hashtable ht )
	{
	
		if(obj.GetType() != typeof(GameObject))
		{
			Debug.LogError("This object must be of type GameObject. vp_UITween.ColorTween cannot proceed.");
			return;
		}
		
		vp_UITween.Tween tween = null;
		if(!Instance.Tweens.TryGetValue((vp_UITween.Handle)ht["handle"], out tween))
		{
			tween = new vp_UITween.Tween( obj, "scale", ht["scale"], "duration", ht["duration"], "handle", ht["handle"], "onComplete", ht["onComplete"], "type", vp_UITween.vp_UITweenType.Scale );
			Instance.Tweens.Add((vp_UITween.Handle)ht["handle"], tween);
		}
		
		if(Convert.ToSingle(ht["duration"]) == 0)
			tween.Transform.localScale = (Vector3)ht["scale"];
		
		tween.ScaleCheck(obj, ht["scale"], ht["duration"], ht["onComplete"]);
	
	}
	
	
	/// <summary>
	/// Creates or updates a Rotate Tween
	/// </summary>
	public static void RotateTo( object obj, Hashtable ht )
	{
	
		if(obj.GetType() != typeof(GameObject))
		{
			Debug.LogError("This object must be of type GameObject. vp_UITween.ColorTween cannot proceed.");
			return;
		}
		
		vp_UITween.Tween tween = null;
		if(!Instance.Tweens.TryGetValue((vp_UITween.Handle)ht["handle"], out tween))
		{
			tween = new vp_UITween.Tween( obj, "rotation", ht["rotation"], "duration", ht["duration"], "handle", ht["handle"], "onComplete", ht["onComplete"], "type", vp_UITween.vp_UITweenType.Rotation );
			Instance.Tweens.Add((vp_UITween.Handle)ht["handle"], tween);
		}
		
		if(Convert.ToSingle(ht["duration"]) == 0)
			tween.Transform.localRotation = (Quaternion)ht["rotation"];
		
		tween.RotationCheck(obj, ht["rotation"], ht["duration"], ht["onComplete"]);
	
	}
	
}
