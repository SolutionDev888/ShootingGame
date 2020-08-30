/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIManager.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	manages all vp_UIControls that are a child of this gameobject
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(vp_InputMobile))]
public class vp_UIManager : MonoBehaviour
{

	public int UILayer = 24;										// Layer that UI children and the ui camera will be set to
	public vp_FPPlayerEventHandler Player = null;					// the player that this UI pertains to
	public AudioSource AudioSource = null;							// the audio source that should play sounds for this UI
	public Vector2 Resolution = new Vector2(1024, 768);				// resolution that all other ui calculations will be based on
	public Vector2 RelativeResolution = new Vector2(1024, 768);		// actual resolution of the screen and calculated base on the Resolution
	public Bounds ScreenArea = new Bounds();						// bounds of the resolution
	public Camera UICamera = null;									// cached ui camera
	public bool SimulateTouchWithMouse = true;						// used for testing touch events with the mouse
	public List<vp_UIControl> Controls = new List<vp_UIControl>();	// list of child vp_UIControls
	public List<vp_UIAnchor> Anchors = new List<vp_UIAnchor>();		// list of child vp_UIAnchors
	public List<Transform> Transforms = new List<Transform>();		// list of child transforms
	public float DoubleTapTimeout = .25f;							// the amount of delay controls will use for their double tap events
	public Dictionary<int, List<vp_UIControl>> FingerIDs = new Dictionary<int, List<vp_UIControl>>();
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{
	
		if(!Application.isEditor)
			SimulateTouchWithMouse = true;
	
		ForceUIRefresh();
		
	}
	
	
	/// <summary>
	/// Makes sure all the necessary properies are set
	/// </summary>
	protected virtual void Init()
	{
	
		if(ScreenSize.x > ScreenSize.y)
		{
			float newHeight = Resolution.y / Resolution.x * ScreenSize.x;
			RelativeResolution = new Vector2(Resolution.x / Resolution.y * newHeight, ScreenSize.y);
		}
		else
		{
			float newWidth = Resolution.x / Resolution.y * ScreenSize.y;
			RelativeResolution = new Vector2(ScreenSize.x, Resolution.y / Resolution.x * newWidth);
		}
	
		if(UICamera == null)
			UICamera = GetUICamera(transform);
	
		Anchors = transform.ChildComponentsToList<vp_UIAnchor>();	
		Transforms = transform.ChildComponentsToList<Transform>();
		if(!Application.isPlaying)
		{
			Controls = transform.ChildComponentsToList<vp_UIControl>();
		}
		else
		{
			Controls.Clear();
			List<vp_UIControl> controls = transform.ChildComponentsToList<vp_UIControl>();
			foreach(vp_UIControl control in controls)
				RegisterControl(control);
		}
			
		if(Player == null || Application.isPlaying)
			Player = GameObject.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler;
			
		if(AudioSource == null && Application.isPlaying)
		{
			AudioSource = Player.gameObject.GetComponent<AudioSource>();
			if(AudioSource == null)
				AudioSource = Player.gameObject.AddComponent<AudioSource>();
		}
	
	}
	
	
	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected virtual void OnEnable()
	{
			
		vp_GlobalEventReturn<bool>.Register("SimulateTouchWithMouse", CheckMouseSimulationMode);

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected virtual void OnDisable()
	{
		
		vp_GlobalEventReturn<bool>.Unregister("SimulateTouchWithMouse", CheckMouseSimulationMode);

	}
	
	
	/// <summary>
	/// Checks the mouse simulation mode
	/// </summary>
	protected virtual bool CheckMouseSimulationMode(){ return SimulateTouchWithMouse; }
	
	
	/// <summary>
	/// Registers the control with events and the UI Manager
	/// </summary>
	public virtual void RegisterControl( vp_UIControl control )
	{
	
		if(!Controls.Contains(control))
		{
			vp_InputMobile.Register(control);
			Controls.Add(control);
		}
	
	}
	
	
	/// <summary>
	/// Unregisters the control from events and the UI Manager
	/// </summary>
	public virtual void UnregisterControl( vp_UIControl control )
	{
	
		vp_InputMobile.Unregister(control);
		Controls.Remove(control);
	
	}
	
	
	/// <summary>
	/// finds an instance of vp_UICamera which designates the
	/// camera for this manager
	/// </summary>
	public static Camera GetUICamera( Transform t )
	{
	
		vp_UICamera[] cameras = t.root.GetComponentsInChildren<vp_UICamera>(true);
		return cameras.Length > 0 ? cameras[0].GetComponent<Camera>() : null;
	
	}
	
	
	/// <summary>
	/// Forces the user interface to be refreshed.
	/// </summary>
	public virtual void ForceUIRefresh()
	{
	
		Init();
			
		if(!Application.isPlaying)
		{
			if((((LayerMask)UICamera.cullingMask).value & 1 << UILayer) == 0)
				UICamera.cullingMask = UICamera.cullingMask | 1 << UILayer;
		
			foreach(Transform t in Transforms)
				t.gameObject.layer = UILayer;
		}
	
		foreach(vp_UIAnchor anchor in Anchors)
			anchor.RefreshUI();
			
		if(Application.isPlaying)
			vp_GlobalEvent.Send("Update UI Positions");
	
	}
	
#if UNITY_EDITOR
	/// <summary>
	/// draws a gizmo to reflect the resolution
	/// </summary>
	protected virtual void OnDrawGizmos()
	{
	
		Gizmos.color = new Color(1, 1, 1, .175f);
        Gizmos.DrawWireCube(ScreenArea.center, ScreenArea.size);
        
    }
    
    
    /// <summary>
	/// draws a gizmo to reflect the resolution
	/// </summary>
	protected virtual void OnDrawGizmosSelected()
	{
	
		Gizmos.color = Color.green;
        Gizmos.DrawWireCube(ScreenArea.center, ScreenArea.size);
        
    }
#endif
    
    
    /// <summary>
    /// 
    /// </summary>
    public static Vector3 PixelPerfect( Vector3 vector, int decimals = 3 )
    {
    
    	Vector3 newVector = vector;
		if(newVector.x - System.Math.Round(newVector.x) != 0)
			newVector.x = (float)System.Math.Round(newVector.x, decimals);
		if(newVector.y - System.Math.Round(newVector.y) != 0)
			newVector.y = (float)System.Math.Round(newVector.y, decimals);
		if(newVector.z - System.Math.Round(newVector.z) != 0)
			newVector.z = (float)System.Math.Round(newVector.z, decimals);
		return newVector;
    
    }
    
    
    [SerializeField]
    protected Vector2 m_ScreenSize = Vector2.one;
    public virtual Vector2 ScreenSize
	{
#if UNITY_EDITOR
		get{
	    	System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
		    System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
		    System.Object Res = GetSizeOfMainGameView.Invoke(null,null);
		    if(((Vector2)Res).x > 1 && ((Vector2)Res).y > 1 && (((Vector2)Res).x != 640 && ((Vector2)Res).y != 480))
			    m_ScreenSize = (Vector2)Res;
			    
		    return m_ScreenSize;
		 }
#else
		get{
			return new Vector2(Screen.width, Screen.height);
		}
#endif
	}
	
}
