/////////////////////////////////////////////////////////////////////////////////
//
//	vp_InputMobile.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This class handles touch, mouse and keyboard input. All buttons
//					from the VP Input Manager are registered as events by this
//					class allowing buttons to fire the buttons from vp_Input.
//					Touches events are managed from this class as well and any class
//					that registers with the touches event handler (vp_UIControl's
//					are setup by default to register) with will have access
//					to the below methods:
//
//					TouchesBegan
//					TouchesMoved
//					TouchesStationary
//					TouchesCancelled
//					TouchesEnded
//					TouchesFinished
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A simple object for storing a touch's properties
/// </summary>
public class vp_Touch
{

	public int FingerID;
	public Vector2 Position;
	public Vector2 DeltaPosition;

}

// touch event handlers
public delegate void vp_TouchesEventHandler( vp_Touch touch ); // For all touches events
public delegate void vp_TouchButtonEventBinding(); // for vp_UIButton bindings
public delegate void vp_TouchChangedEventBinding( vp_UIControl control ); // for vp_UIButton bindings
public delegate bool vp_TouchButtonActionEventHandler( string action, bool hold = false ); // for vp_UIButton actions

// axis event handler
public delegate float vp_RawAxisEventHandler(string id);

public class vp_InputMobile : vp_Input
{

	// callbacks for button actions (buttons that are setup in the VP Input Manager)
	public static Dictionary<string, vp_TouchButtonActionEventHandler> ButtonDownCallbacks = new Dictionary<string, vp_TouchButtonActionEventHandler>();
	public static Dictionary<string, vp_TouchButtonActionEventHandler> ButtonUpCallbacks = new Dictionary<string, vp_TouchButtonActionEventHandler>();
	public static Dictionary<string, vp_TouchButtonActionEventHandler> ButtonHoldCallbacks = new Dictionary<string, vp_TouchButtonActionEventHandler>();
	
	// callbacks stored for all touches events
	protected static Dictionary<string, vp_TouchesEventHandler> TouchEventBindings = 
		new Dictionary<string, vp_TouchesEventHandler>(){ 
															{ "TouchesBegan", null },
															{ "TouchesMoved", null },
															{ "TouchesStationary", null },
															{ "TouchesEnded", null },
															{ "TouchesCancelled", null },
															{ "TouchesFinished", null }
														};
														
	// cached list of TouchesEvents for touches
	protected static Dictionary<string, vp_TouchesEventHandler> TouchesDelegates = new Dictionary<string, vp_TouchesEventHandler>();
	
	// axis delegate
    public static vp_RawAxisEventHandler LookAxisCallback;
    public static vp_RawAxisEventHandler MoveAxisCallback;
    

	/// <summary>
	/// In this method, vp_Input is loaded from resources
	/// and all its properties are copied here
	/// </summary>
	protected override void Awake()
	{
		
		GameObject go = Resources.Load("Input/vp_Input") as GameObject;
		vp_Input input = go != null ? go.GetComponent<vp_Input>() : new GameObject("vp_Input").AddComponent<vp_Input>();
		
		ControlType = input.ControlType;
		Buttons = input.Buttons;
		Axis = input.Axis;
		UnityAxis = input.UnityAxis;
		ButtonKeys = input.ButtonKeys;
		ButtonValues = input.ButtonValues;
		AxisKeys = input.AxisKeys;
		AxisValues = input.AxisValues;
		
		// set the vp_Input instance to this class to allow touch input
		m_Instance = this;
		
		SetupDefaults();
		
		// sets the target frame rate higher for mobile
		Application.targetFrameRate = 60;
	
	}
	
	
	/// <summary>
	/// adds the actions to proper dictionary
	/// </summary>
	public override void SetupDefaults( string type = "" )
	{
		
		base.SetupDefaults(type);
		
		foreach(string n in Buttons.Keys)
		{
			if(!ButtonDownCallbacks.ContainsKey(n))
				ButtonDownCallbacks.Add(n, null);
			if(!ButtonHoldCallbacks.ContainsKey(n))
				ButtonHoldCallbacks.Add(n, null);
			if(!ButtonUpCallbacks.ContainsKey(n))
				ButtonUpCallbacks.Add(n, null);
		}
	
	}
	
	
	/// <summary>
	/// handles mouse and/or touch input while a button is held
	/// </summary>
	public override bool DoGetButton( string id )
	{
		
		if(Input.touchCount == 0 && !vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
		{
			return base.DoGetButton(id);
		}
		else
		{
			if(ButtonHoldCallbacks.ContainsKey(id))
				if(ButtonHoldCallbacks[id] != null)
					foreach(vp_TouchButtonActionEventHandler f in ButtonHoldCallbacks[id].GetInvocationList())
					    if(f(id, true))
					    	return true;
		}
			
		return false;
			
	}
	
	
	/// <summary>
	/// handles mouse and/or touch input for a button down event
	/// </summary>
	public override bool DoGetButtonDown( string id )
	{
	
		if(Input.touchCount == 0 && !vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
		{
			return base.DoGetButtonDown(id);
		}
		else
		{
			if(ButtonDownCallbacks.ContainsKey(id))
				if(ButtonDownCallbacks[id] != null)
					foreach(vp_TouchButtonActionEventHandler f in ButtonDownCallbacks[id].GetInvocationList())
					    if(f(id))
					    	return true;
		}
			
		return false;
			
	}
	
	
	/// <summary>
	/// handles mouse and/or touch input for a button up event
	/// </summary>
	public override bool DoGetButtonUp( string id )
	{
	
		if(Input.touchCount == 0 && !vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
		{
			return base.DoGetButtonUp(id);
		}
		else
		{
			if(ButtonUpCallbacks.ContainsKey(id))
				if(ButtonUpCallbacks[id] != null)
					foreach(vp_TouchButtonActionEventHandler f in ButtonUpCallbacks[id].GetInvocationList())
					    if(f(id))
					    	return true;
		}
			
		return false;
			
	}
	
	
	/// <summary>
	/// handles mouse and/or touch input for
	/// any button state
	/// </summary>
	public override bool DoGetButtonAny( string id )
	{
	
		return DoGetButtonDown(id) || DoGetButton(id) || DoGetButtonUp(id);
	
	}
	
	
	/// <summary>
	/// handles keyboard and mouse or touch input
	/// for axises
	/// </summary>
	public override float DoGetAxisRaw( string id )
	{
	
		if(Input.touchCount == 0 && !vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse"))
			return base.DoGetAxisRaw(id);
		else
			if(id == "Horizontal" || id == "Vertical")
				return MoveAxisCallback != null ? MoveAxisCallback(id) : 0;
			else
				return LookAxisCallback != null ? LookAxisCallback(id) : 0;
			
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Update()
    {
    
    	InputMouse();
    	InputTouches();
	        
    }
    
    
    /// <summary>
    /// Process the mouse input
    /// </summary>
    protected virtual void InputMouse()
    {
    
    	if(!Application.isEditor)
    		return;
    		
    	if(Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
    	{
    		
    		vp_Touch touch = new vp_Touch();
    		touch.FingerID = 0;
    		touch.Position = Input.mousePosition;
    		touch.DeltaPosition = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 4;
    	
    		if(Input.GetMouseButtonDown(0) && TouchEventBindings["TouchesBegan"] != null)
    			TouchEventBindings["TouchesBegan"](touch);
    		
	    	if(Input.GetMouseButton(0) && TouchEventBindings["TouchesMoved"] != null)
	    		TouchEventBindings["TouchesMoved"](touch);
	    		
	    	if(touch.DeltaPosition == Vector2.zero && Input.GetMouseButton(0) && TouchEventBindings["TouchesStationary"] != null)
	    		TouchEventBindings["TouchesStationary"](touch);
	    		
	    	if(Input.GetMouseButtonUp(0))
	    	{
	    		if(TouchEventBindings["TouchesEnded"] != null) TouchEventBindings["TouchesEnded"](touch);
	    		if(TouchEventBindings["TouchesFinished"] != null) TouchEventBindings["TouchesFinished"](touch);
	    	}
    	}
    
    }
    
    
    /// <summary>
    /// sends touches events created from Input.touches
    /// </summary>
    protected virtual void InputTouches()
    {
    
    	foreach(Touch touch in Input.touches)
		{
			vp_Touch vpTouch = new vp_Touch();
    		vpTouch.FingerID = touch.fingerId;
    		vpTouch.Position = touch.position;
    		vpTouch.DeltaPosition = touch.deltaPosition;
		
			// touch began
			if(touch.phase != TouchPhase.Canceled && touch.phase == TouchPhase.Began && TouchEventBindings["TouchesBegan"] != null)
				TouchEventBindings["TouchesBegan"](vpTouch);
			
			// touch moved
			if(touch.phase == TouchPhase.Moved && TouchEventBindings["TouchesMoved"] != null)
				TouchEventBindings["TouchesMoved"](vpTouch);
			
			// has a touched but not moving
			if(touch.phase == TouchPhase.Stationary && TouchEventBindings["TouchesStationary"] != null)
				TouchEventBindings["TouchesStationary"](vpTouch);
        
        	// touch ended
        	if(touch.phase == TouchPhase.Ended && TouchEventBindings["TouchesEnded"] != null)
	    		TouchEventBindings["TouchesEnded"](vpTouch);
        	
        	// touch canceled
        	if(touch.phase == TouchPhase.Canceled && TouchEventBindings["TouchesCancelled"] != null)
	    		TouchEventBindings["TouchesCancelled"](vpTouch);
        	
        	// touch finished with TouchPhase.Ended or TouchPhase.Canceled
        	if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled && TouchEventBindings["TouchesFinished"] != null)
    			TouchEventBindings["TouchesFinished"](vpTouch);
        }
    
    }
    
    
    /// <summary>
    /// Register the specified control with the touches event handler
    /// </summary>
    public static void Register(vp_UIControl control)
    {
    
    	ControlRegistration(control, true);
    
    }
    
    
    /// <summary>
    /// Unregister the specified control from the touches event handler
    /// </summary>
    public static void Unregister(vp_UIControl control)
    {
    
    	ControlRegistration(control, false);
    
    }
    
    
    /// <summary>
    /// handles the registering and unregistering
    /// of all the touches events for a control
    /// </summary>
    protected static void ControlRegistration( vp_UIControl control, bool register )
    {
    
    	// get a list of components on the control
    	List<MonoBehaviour> components = control.GetComponents<MonoBehaviour>().ToList();
    	
    	// loop through the list of components
		for(int n=0;n<components.Count; n++)
		{
			// get the component
			MonoBehaviour component = components[n];
			
			// get a array of methodInfos from this component
			MethodInfo[] methodInfos = component.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			
			// loop through the methods
			foreach(MethodInfo methodInfo in methodInfos)
			{
				// make sure the method doesn't have a return value and the parameter is of type vp_Touch
				if (methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(vp_Touch)) != null && methodInfo.ReturnType == typeof(void) && TouchEventBindings.ContainsKey(methodInfo.Name))
				{
					string dKey = component.GetInstanceID().ToString()+"."+methodInfo.Name;
					vp_TouchesEventHandler d = null;
					if(!TouchesDelegates.TryGetValue(dKey, out d))
					{
						d = (vp_TouchesEventHandler)Delegate.CreateDelegate(typeof(vp_TouchesEventHandler), component, methodInfo.Name);
						TouchesDelegates.Add(dKey, d);
					}
					
					// register or unregister
					if(register)	TouchEventBindings[methodInfo.Name] += d;
					else 			TouchEventBindings[methodInfo.Name] -= d;
				}
			}
		}
    
    }
    	
}
