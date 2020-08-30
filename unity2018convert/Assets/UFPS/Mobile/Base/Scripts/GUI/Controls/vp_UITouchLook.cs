/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UITouchLook.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	Manages the touch control for a players camera rotation
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Linq;

//[RequireComponent(typeof(BoxCollider))]
public class vp_UITouchLook : vp_UIControl
{

	public float AutoPitchSpeed = 2.75f;							// speed at which the camera will return level. set to 0 to disable.
	public float AutoPitchDelay = 3.5f;								// time in seconds before autopitch will start
	public bool AutoPitchWhenIdle = true;							// allow autopitch when idle
	public Vector2 RotationSensitivity = new Vector2(0.3f, 0.2f);	// sensitivity of the camera rotation
	public bool AutopitchFoldout = false;
	
	protected Vector2 m_LookVector = Vector2.zero;					// panning axis
	protected bool m_ShouldAutoPitch = false;
	protected vp_Timer.Handle m_AutoPitchTimer = new vp_Timer.Handle();
	
	
	/// <summary>
	/// 
	/// </summary>
	protected override void Start()
	{
	
		base.Start();
		
		OnEnable();
	
	}
	
	
	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected override void OnEnable()
	{
	
		if(!m_Initialized)
			return;

		base.OnEnable();
		
		vp_InputMobile.LookAxisCallback += GetLook;

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected override void OnDisable()
	{

		base.OnDisable();
		
		vp_InputMobile.LookAxisCallback -= GetLook;

	}
	

	/// <summary>
	/// set look vector to zero if no touch
	/// </summary>
	protected override void Update()
    {
    
    	AutoPitchUpdate();
	        
    }
    
    
    /// <summary>
    /// handles auto pitching the camera
    /// </summary>
    protected virtual void AutoPitchUpdate()
    {
    
    	if(LastFingerID != -1)
    		return;
    		
    	m_LookVector = Vector2.zero;
    	
    	if(AutoPitchSpeed == 0)
    		return;
    		
    	if(!m_ShouldAutoPitch)
    		return;
    		
    	if(Manager.Player.Climb.Active)
    		return;
    		
    	if(Manager.Player.Interactable.Get() != null && Manager.Player.Interactable.Get().GetType() == typeof(vp_Grab))
    		return;
    		
    	if(Manager.Player.Zoom.Active)
    		return;
    		
    	// return when not using mouse touches and in the editor
    	if(!vp_GlobalEventReturn<bool>.Send("SimulateTouchWithMouse") && Application.isEditor)
    		return;
    		
    	if(vp_GlobalEventReturn<bool>.Send("Tracking Enemy"))
    		return;
        
        bool isMoving = vp_Input.GetAxisRaw("Horizontal") != 0 || vp_Input.GetAxisRaw("Vertical") != 0;
        if( isMoving || AutoPitchWhenIdle )
        	Manager.Player.Rotation.Set(Vector2.Lerp(Manager.Player.Rotation.Get(), new Vector2(0.0f, Manager.Player.Rotation.Get().y), Time.deltaTime * (isMoving ? AutoPitchSpeed : AutoPitchSpeed * .5f)));
    
    }
    
    
    /// <summary>
	/// execute when this control is touched
	/// </summary>
	protected override void TouchesBegan( vp_Touch touch )
    {
    		
    	if(LastFingerID != -1)
    		return;
    		
    	if(!RaycastControl(touch))
    		return;
    		
    	base.TouchesBegan( touch );
    	
    	m_ShouldAutoPitch = false;
    	m_AutoPitchTimer.Cancel();
    
    }
    
    
    /// <summary>
    /// When the joystick is moved
    /// </summary>
    protected virtual void TouchesMoved( vp_Touch touch )
    {
    
    	if ( LastFingerID != touch.FingerID )
    		return;
    		
    	m_LookVector = new Vector2(((touch.DeltaPosition*.25f)*RotationSensitivity.x).x, ((touch.DeltaPosition*.25f)*RotationSensitivity.y).y);
    
    }
    
    
    /// <summary>
    /// When the joystick is stationary
    /// </summary>
    protected virtual void TouchesStationary( vp_Touch touch )
    {
    
    	if ( LastFingerID != touch.FingerID )
    		return;
    		
    	m_LookVector = Vector2.zero;
    
    }
    
    
    /// <summary>
    /// When touch finished reset last finger id
    /// </summary>
    public override void TouchesFinished( vp_Touch touch )
    {
    
    	if ( LastFingerID != touch.FingerID )
    		return;
    		
    	base.TouchesFinished(touch);
    	
    	if(AutoPitchSpeed == 0)
    		return;
    		
    	vp_Timer.In( AutoPitchDelay, delegate() {
    		m_ShouldAutoPitch = true;
    	}, m_AutoPitchTimer);
    
    }
    
    
    /// <summary>
    /// returns the look vector for the LookAxisCallback
    /// </summary>
    protected virtual float GetLook(string id)
    {
    
    	return id == "Mouse X" ? m_LookVector.x : id == "Mouse Y" ? m_LookVector.y : 0;
    
    }
	
}