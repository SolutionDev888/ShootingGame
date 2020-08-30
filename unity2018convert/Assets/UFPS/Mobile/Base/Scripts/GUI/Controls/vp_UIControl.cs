/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIControl.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	base class for any UI Control that should be managed by UI Manager.
//					Any class that is derived from this class will have the following
//					methods available to it if a box collider is present:
//					
//					OnPressControl
//					OnReleaseControl
//					OnHoldControl
//					OnDoublePressControl
//					OnDoubleReleaseControl
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class vp_UIControl : MonoBehaviour
{

	[HideInInspector] public vp_UIManager Manager = null;			// cached UI manager
	public vp_TouchButtonEventBinding PressControl;					// when a control is first pressed
	public vp_TouchButtonEventBinding HoldControl;					// when a control is held
	public vp_TouchButtonEventBinding ReleaseControl;				// when a control is released
	public vp_TouchButtonEventBinding DoublePressControl;			// when a control detects a double press
	public vp_TouchButtonEventBinding DoubleReleaseControl;			// when a control detects a double release
	public vp_TouchChangedEventBinding ChangeControl;				// when a control is changed
	
	[HideInInspector] public List<MonoBehaviour> Objects = new List<MonoBehaviour>();	// List of components to check for methods to bind to
	[HideInInspector] public List<int> MethodIndexes = new List<int>();				// Index of the method for the editor
	[HideInInspector] public List<string> Methods = new List<string>();				// List of methods from the components
	
	protected List<Delegate> m_ChangedEvents = new List<Delegate>(); // list of changed events for binding
	protected List<Delegate> m_ButtonEvents = new List<Delegate>(); // list of events for binding

	protected Transform m_Transform = null;				// cached transform
	protected Camera m_Camera = null;					// cached ui camera
	protected BoxCollider m_Collider = null;			// cached box collider
	protected Transform m_Parent = null;				// cached parent transform
	protected bool m_Initialized = false;				// gets set to true when Start has been ran on this control
	protected float m_DoubleTapTime = 0;
	protected float m_TapCount = 0;
	
	protected int m_LastFingerID;						// cached last finger id
	public int LastFingerID								// Update manager's FingerIDs list when finger id is changed 
	{
		get{ return m_LastFingerID; }
		set{
			if(Manager != null)
			{
				if(value == -1)
				{
					if(Manager.FingerIDs.ContainsKey(m_LastFingerID))
					{
						if(Manager.FingerIDs[m_LastFingerID].Count == 1)
						{
							Manager.FingerIDs.Remove(m_LastFingerID);
						}
						else if(Manager.FingerIDs[m_LastFingerID].Contains(this))
						{
							Manager.FingerIDs[m_LastFingerID].Remove(this);
						}
					}
				}
				else if(!Manager.FingerIDs.ContainsKey(value))
				{
					Manager.FingerIDs.Add(value, new List<vp_UIControl>(){ this });
				}
				else if(Manager.FingerIDs.ContainsKey(value) && !Manager.FingerIDs[value].Contains(this))
				{
					Manager.FingerIDs[value].Add(this);
				}
			}
				
			m_LastFingerID = value;
		}
	}
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{
	
		m_Transform = transform;
		m_Camera = vp_UIManager.GetUICamera(m_Transform) == null ? m_Transform.root.GetComponentInChildren<Camera>() : vp_UIManager.GetUICamera(m_Transform);
		m_Parent = m_Transform.parent;
		Manager = m_Transform.root.GetComponentInChildren<vp_UIManager>();
		m_LastFingerID = -1;
		if(GetComponent<Collider>() != null) m_Collider = GetComponents<BoxCollider>().FirstOrDefault();
		
		// Auto register events
		List<string> supportedButtonMethods = new List<string>(){ "OnPressControl", "OnHoldControl", "OnReleaseControl", "OnDoublePressControl", "OnDoubleReleaseControl" };
		List<string> supportedToggleMethods = new List<string>(){ "OnChangeControl" };
		
		List<MonoBehaviour> components = GetComponents<MonoBehaviour>().ToList();
		for(int i=0;i<components.Count; i++)
		{
			MonoBehaviour component = components[i];
			MethodInfo[] methodInfos = component.GetType().GetMethods( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance );
			foreach(MethodInfo methodInfo in methodInfos)
			{
				if(methodInfo.GetParameters().Length == 0 && methodInfo.ReturnType == typeof(void))
				{
					foreach(string methodName in supportedButtonMethods)
						if(methodInfo.Name == methodName)
							m_ButtonEvents.Add((vp_TouchButtonEventBinding)Delegate.CreateDelegate(typeof(vp_TouchButtonEventBinding), component, methodName));
				}
				else if(methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters()[0].ParameterType == typeof(vp_UIControl) && methodInfo.ReturnType == typeof(void))
				{
					foreach(string methodName in supportedToggleMethods)
						if(methodInfo.Name == methodName)
							m_ChangedEvents.Add((vp_TouchChangedEventBinding)Delegate.CreateDelegate(typeof(vp_TouchChangedEventBinding), component, methodName));
				}
			}
		}
		
		
		// setup changed events
		for(int i = 0; i<Objects.Count; i++)
		{
			if(Objects[i] == null || Methods[i] == "")
				continue;
				
			string[] info = Methods[i].Split(new string[1]{ "." }, System.StringSplitOptions.None);
			Component component = (Component)Objects[i].GetComponent(info[0]);
			if(component.GetType().GetMethod(info[1]).GetParameters().Length > 0)
				m_ChangedEvents.Add((vp_TouchChangedEventBinding)Delegate.CreateDelegate(typeof(vp_TouchChangedEventBinding), component, info[1]));
			else if(component.GetType().GetMethod(info[1]).GetParameters().Length == 0)
				m_ButtonEvents.Add((vp_TouchButtonEventBinding)Delegate.CreateDelegate(typeof(vp_TouchButtonEventBinding), component, info[1]));
		}
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Start()
	{
	
		m_Initialized = true;
		
		OnEnable();
	
	}
	
	
	/// <summary>
	/// registers this component with the event handlers (if any)
	/// </summary>
	protected virtual void OnEnable()
	{
	
		if(!m_Initialized)
			return;

		// allow this control to talk to the player event handler
		if (Manager != null && Manager.Player != null)
			Manager.Player.Register(this);
		
		// register with the vp_UIManager if available or register with vp_InputMobile if not	
		if(Manager != null)
			Manager.RegisterControl(this);
		else
			vp_InputMobile.Register(this);
			
		// button event binding callbacks
		foreach(vp_TouchButtonEventBinding d in m_ButtonEvents)
			if(d.Method.Name == "OnPressControl") PressControl += d;
			else if(d.Method.Name == "OnHoldControl") HoldControl += d;
			else if(d.Method.Name == "OnDoublePressControl") DoublePressControl += d;
			else if(d.Method.Name == "OnDoubleReleaseControl") DoubleReleaseControl += d;
			else ReleaseControl += d;
			
		// changed event binding callbacks
		foreach(vp_TouchChangedEventBinding d in m_ChangedEvents)
			ChangeControl += d;

	}


	/// <summary>
	/// unregisters this component from the event handlers (if any)
	/// </summary>
	protected virtual void OnDisable()
	{

		// unregister this monobehaviour from the player event handler
		if (Manager != null && Manager.Player != null)
			Manager.Player.Unregister(this);
			
		// unregister from the vp_UIManager if available or unregister from vp_InputMobile if not
		if(Manager != null)
			Manager.UnregisterControl(this);
		else
			vp_InputMobile.Unregister(this);
			
		// button event binding callbacks
		foreach(vp_TouchButtonEventBinding d in m_ButtonEvents)
			if(d.Method.Name == "OnPressControl") PressControl -= d;
			else if(d.Method.Name == "OnHoldControl") HoldControl -= d;
			else if(d.Method.Name == "OnDoublePressControl") DoublePressControl -= d;
			else if(d.Method.Name == "OnDoubleReleaseControl") DoubleReleaseControl -= d;
			else ReleaseControl -= d;
			
		// changed event binding callbacks
		foreach(vp_TouchChangedEventBinding d in m_ChangedEvents)
			ChangeControl -= d;

	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Update()
	{
	
		if(LastFingerID == -1)
			return;
			
		if(HoldControl != null)
			HoldControl();
	
	}
	
	
	/// <summary>
	/// execute when this control is touched
	/// </summary>
	protected virtual void TouchesBegan( vp_Touch touch )
    {
    
    	if(m_Collider == null)
    		return;
    		
    	if(LastFingerID != -1)
    		return;
    		
    	if(!RaycastControl(touch))
    		return;
    		
    	if(PressControl != null)
	    	PressControl();
	    
	    if(Time.time > m_DoubleTapTime) m_TapCount = 0;
	    if(m_TapCount == 0) m_DoubleTapTime = Time.time + (Manager != null ? Manager.DoubleTapTimeout : .25f);
    	m_TapCount++;
    	
    	if(m_TapCount == 2 && DoublePressControl != null)
    		DoublePressControl();
    	
    	LastFingerID = touch.FingerID; // cache this finger id
    
    }
    
    
    /// <summary>
    /// Executes when this control loses focus
    /// </summary>
    public virtual void TouchesFinished( vp_Touch touch )
    {
    
    	if ( LastFingerID != touch.FingerID )
    		return;
    	
    	if(ReleaseControl != null)
	    	ReleaseControl();
	    	
	    if(m_TapCount == 2 && DoubleReleaseControl != null)
    		DoubleReleaseControl();
    	
    	LastFingerID = -1;
    
    }
	
	
	/// <summary>
	/// Helper method that returns whether or
	/// not a touch hit this control
	/// </summary>
	public virtual bool RaycastControl( vp_Touch touch )
	{
	
		return Physics.RaycastAll(m_Camera.ScreenPointToRay(touch.Position)).Where(hit => hit.collider == m_Collider).ToList().Count > 0;
	
	}
	
	
#if UNITY_EDITOR
	public virtual void OnDrawGizmos()
	{

		if(GetComponent<Collider>() == null)
			return;

		Gizmos.color = new Color( 1, 1, 1, 0.175f );
		Gizmos.DrawWireCube( GetComponent<Collider>().bounds.center, GetComponent<Collider>().bounds.size );

	}
#endif
	
}
