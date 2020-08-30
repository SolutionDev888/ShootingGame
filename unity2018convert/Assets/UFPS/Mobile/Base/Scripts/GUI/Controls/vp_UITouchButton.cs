/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UITouchButton.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	Manages the touch controls for a button. This control can
//					be used to directly access a button setup in the VP Input Manager
//					by setting the Action dropdown to a button name. Alternatively
//					a event binding can be specified in the dropdown and will
//					allow for an unlimited amount of objects to have a callback
//					setup to the specified method.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(BoxCollider))]
public class vp_UITouchButton : vp_UIControl
{

	// states a button can have
	public enum vp_UIButtonState
	{
		OnPress,
		OnRelease,
		OnHold,
		OnDoublePress,
		OnDoubleRelease
	}
	
	public bool RequireStayInBounds = false;
	public bool OverrideTouches = false;			// if enabled and this button recieves a touch event, all events for that finger will directed only to this button
	public string Action;							// The Action (button from VP Input Manager) that will be performed
	public vp_UIButtonState Event = vp_UIButtonState.OnPress;	// The state this button will use to send events
	
	// flags for different events and states
	protected bool m_ButtonReleased = true;
	protected bool m_ButtonUp = false;
	protected bool m_IsEventBinding = false;
	protected bool m_ButtonOverride = false;
	
	
	/// <summary>
	/// 
	/// </summary>
	protected override void Start()
	{
		
		m_IsEventBinding = Action == "Event Binding \u21C6";
		
		base.Start();
	
	}
	
	
	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected override void OnEnable()
	{
		
		if(!m_Initialized)
			return;
	
		base.OnEnable();
			
		RegisterTouchCallbacks();

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected override void OnDisable()
	{

		base.OnDisable();
			
		UnregisterTouchCallbacks();

	}
	
	
	/// <summary>
	/// Registers the touch callbacks.
	/// </summary>
	protected virtual void RegisterTouchCallbacks()
	{
	
		// Register Action Events
		if(Event == vp_UIButtonState.OnHold)
			if(vp_InputMobile.ButtonHoldCallbacks.ContainsKey(Action))
				vp_InputMobile.ButtonHoldCallbacks[Action] += OnButtonHoldAction;
		
		if(Event == vp_UIButtonState.OnPress || Event == vp_UIButtonState.OnDoublePress)
			if(vp_InputMobile.ButtonDownCallbacks.ContainsKey(Action))
				vp_InputMobile.ButtonDownCallbacks[Action] += OnButtonDownAction;
		
		if(Event == vp_UIButtonState.OnRelease || Event == vp_UIButtonState.OnDoubleRelease)
			if(vp_InputMobile.ButtonUpCallbacks.ContainsKey(Action))
				vp_InputMobile.ButtonUpCallbacks[Action] += OnButtonUpAction;
	
	}
	
	
	/// <summary>
	/// Unregisters the touch callbacks.
	/// </summary>
	protected virtual void UnregisterTouchCallbacks()
	{
	
		// Unregister Action Events
		if(Event == vp_UIButtonState.OnHold)
			if(vp_InputMobile.ButtonHoldCallbacks.ContainsKey(Action))
				vp_InputMobile.ButtonHoldCallbacks[Action] -= OnButtonHoldAction;
			
		if(Event == vp_UIButtonState.OnPress || Event == vp_UIButtonState.OnDoublePress)
			if(vp_InputMobile.ButtonDownCallbacks.ContainsKey(Action))
				vp_InputMobile.ButtonDownCallbacks[Action] -= OnButtonDownAction;
			
		if(Event == vp_UIButtonState.OnRelease || Event == vp_UIButtonState.OnDoubleRelease)
			if(vp_InputMobile.ButtonUpCallbacks.ContainsKey(Action))
				vp_InputMobile.ButtonUpCallbacks[Action] -= OnButtonUpAction;
	
	}
	
	
	/// <summary>
	/// will fire the OnHoldCallback if conditions are met
	/// </summary>
	protected override void Update()
	{
	
		HoldUpdate();
	
	}
	
	
	/// <summary>
	/// checks if this control is being held
	/// </summary>
	protected virtual void HoldUpdate()
	{
	
		if(Event != vp_UIButtonState.OnHold)
			return;
	
		if(LastFingerID == -1)
			return;
			
		if(!m_IsEventBinding)
			return;
	
		if(HoldControl != null)
    		HoldControl();
	
	}
	
	
	/// <summary>
	/// execute if the button is being held
	/// </summary>
	protected virtual bool OnButtonHoldAction( string action, bool hold = false )
	{
	
		if(Action != action)
			return false;
			
		if(m_IsEventBinding)
			return false;
			
		if(LastFingerID == -1)
			return false;
			
		if(!m_ButtonReleased && !hold)
			return false;
			
		if(Event != vp_UIButtonState.OnHold)
			return false;
		
    	m_ButtonReleased = false;
    	
		return true;
	
	}
	
	
	/// <summary>
	/// Execute a single time when the button is first touched
	/// </summary>
	protected virtual bool OnButtonDownAction( string action, bool hold = false )
	{
	
		if(Action != action)
			return false;
			
		if(m_IsEventBinding)
			return false;
		
		if(LastFingerID == -1)
			return false;
			
		if(!m_ButtonReleased)
			return false;
			
		if(Event != vp_UIButtonState.OnPress && Event != vp_UIButtonState.OnDoublePress)
			return false;
			
		if(Event == vp_UIButtonState.OnDoublePress && m_TapCount != 2)
			return false;
			
		if(!enabled)
			return false;
			
		StartCoroutine(WaitFrames(5));
    	
		return true;
	
	}
	
	
	protected virtual IEnumerator WaitFrames( int frames )
	{
	
		for(int i=0; i<frames; i++)
			yield return new WaitForEndOfFrame();
			
		m_ButtonReleased = false;
	
	}
	
	
	/// <summary>
	/// Execute a single time when the button is released
	/// </summary>
	protected virtual bool OnButtonUpAction( string action, bool hold = false )
	{
	
		if(Action != action)
			return false;
			
		if(m_IsEventBinding)
			return false;
		
		if(LastFingerID == -1)
			return false;
				
		if(!m_ButtonUp)
			return false;
			
		if(Event != vp_UIButtonState.OnRelease && Event != vp_UIButtonState.OnDoubleRelease)
			return false;
			
		if(!enabled)
			return false;
			
		LastFingerID = -1;
		m_ButtonUp = false;
			
		return true;
	
	}
    
    
    /// <summary>
    /// When a touch hits this control
    /// </summary>
    protected override void TouchesBegan( vp_Touch touch )
    {
    
    	if(LastFingerID != -1)
    		return;
    		
    	if(!RaycastControl(touch))
    		return;
    
    	base.TouchesBegan(touch);
    	
    	m_ButtonReleased = true;
    
    	if(PressControl != null)
    		PressControl();
    	
    	if(m_TapCount == 2 && DoublePressControl != null)
    		DoublePressControl();
    
    }
    
    
    /// <summary>
    /// processed when a touch with this finger id is moved
    /// </summary>
    protected virtual void TouchesMoved( vp_Touch touch )
    {
    
    	if(RequireStayInBounds && (Event == vp_UIButtonState.OnDoubleRelease || Event == vp_UIButtonState.OnRelease) && LastFingerID != 1)
			if(!RaycastControl(touch))
				LastFingerID = -1;
    
    	if(!OverrideTouches)
    		return;
    		
    	if(Manager == null)
    		return;
    		
    	if(!RaycastControl(touch))
    		return;
    		
    	if(!Manager.FingerIDs.ContainsKey(touch.FingerID))
    		return;
    		
    	if(m_ButtonOverride)
    		return;
    		
    	List<vp_UIControl> controls = Manager.FingerIDs[touch.FingerID];
    	for(int i=0;i<controls.Count;i++)
    		controls[i].TouchesFinished( touch );
    		
    	LastFingerID = touch.FingerID;
    	m_ButtonOverride = true;
    
    }
    
    
    /// <summary>
    /// When a touch is finished with this control
    /// </summary>
    public override void TouchesFinished( vp_Touch touch )
    {
    
    	if ( LastFingerID != touch.FingerID )
    		return;
    	
    	if((Event != vp_UIButtonState.OnRelease && Event != vp_UIButtonState.OnDoubleRelease) || !RaycastControl(touch))
	    	LastFingerID = -1;
	    	
	    if(RaycastControl( touch ))
	    {
	    	m_ButtonUp = true;
	    		
	    	if(ReleaseControl != null)
	    		ReleaseControl();
	    		
	    	if(m_TapCount == 2 && DoubleReleaseControl != null)
	    		DoubleReleaseControl();
	    		
	    	if(Event == vp_UIButtonState.OnDoubleRelease && m_TapCount != 2)
			{
				m_LastFingerID = -1;
				m_ButtonUp = false;
			}
	    }
	    	
	    m_ButtonOverride = false;
    	m_ButtonReleased = true;
    
    }
    
}