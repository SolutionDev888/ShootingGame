/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UITouchController.cs
//	Â© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	Manages the gui and touch controls for a controller that
//					controls the players movement
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(BoxCollider))]
public class vp_UITouchController : vp_UIControl
{

	// types of controllers
	public enum vp_TouchControllerType
	{
		DynamicJoystick,
		StaticJoystick,
		TouchPad
	}

	public Transform Knob = null;							// transform of the knob
	public Transform Background = null;						// transform of the background
	[SerializeField] protected vp_UITouchController.vp_TouchControllerType m_ControllerType = vp_UITouchController.vp_TouchControllerType.DynamicJoystick;
	public float AnimationSpeed = 20;						// speed at which fading and animation occurs
	public Vector2 Deadzone = new Vector2(.25f, .25f);		// if knob movement is below this threshold, no movement occurs
	public Color KnobOnColor = new Color(1,1,1,1);			// Color of the knob when shown
	public Color BackgroundOnColor = new Color(1,1,1,0.4f);	// color of the background when shown
	public Color KnobOffColor = new Color(1,1,1,0);			// color of the knob when hidden
	public Color BackgroundOffColor = new Color(1,1,1,0);	// color of the background when hidden
	public delegate void ControllerTypeChangedEventHandler(vp_UITouchController.vp_TouchControllerType type);
	public ControllerTypeChangedEventHandler ControllerTypeChanged = null;
	
	/// <summary>
	/// Gets or sets the type of controller used
	/// </summary>
	public vp_UITouchController.vp_TouchControllerType ControllerType
	{
		get{ return m_ControllerType; }
		set{
			m_ControllerType = value;
			OnControllerTypeChanged(m_ControllerType);
		}
	}
	
	/// <summary>
	/// Gets or sets the color of the knob.
	/// </summary>
	public virtual Color32 KnobColor
	{
		get{ return Knob.GetComponent<Renderer>().material.color; }
		set{ Knob.GetComponent<Renderer>().material.color = value; }
	}
	
	/// <summary>
	/// Gets or sets the color of the background.
	/// </summary>
	public virtual Color32 BackgroundColor
	{
		get{ return Background.GetComponent<Renderer>().material.color; }
		set{ Background.GetComponent<Renderer>().material.color = value; }
	}
	
	protected Vector3 m_DefaultKnobPosition = Vector3.zero;		// cached initial position of the knob
	protected Vector3 m_DefaultPanelPosition = Vector3.zero;
	protected Vector2 m_RawMove = Vector2.zero;					// the raw movement vector obtained from the knob being moved
	protected Transform m_PanelTransform = null;				// cached panel that the background and knob reside in
	protected Bounds m_KnobArea;								// cached collider bounds of the knob
	protected ParticleSystem m_PadParticles = null;
	protected List<BoxCollider> m_BoxColliders = new List<BoxCollider>();
    

  	/// <summary>
	/// 
	/// </summary>
	protected override void Awake()
	{
	
		base.Awake();
		
		m_PanelTransform = Knob.parent;
		m_DefaultKnobPosition = Knob.localPosition;
		m_DefaultPanelPosition = m_PanelTransform.localPosition;
		m_PadParticles = Knob.GetComponent<ParticleSystem>();
		if(m_PadParticles != null)
			m_PadParticles.enableEmission = false;
		
		if(Knob.GetComponent<Collider>() != null)
			m_KnobArea = Knob.GetComponent<Collider>().bounds;
			
		OnControllerTypeChanged( m_ControllerType );

	}
	
	
	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected override void OnEnable()
	{

		if(!m_Initialized)
			return;

		base.OnEnable();
		
		vp_InputMobile.MoveAxisCallback += GetMove;

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected override void OnDisable()
	{
		
		base.OnDisable();
		
		vp_InputMobile.MoveAxisCallback -= GetMove;

	}


	/// <summary>
	/// 
	/// </summary>
	protected override void Update()
    {    
        
        JoystickUpdate(); // joystick gui and position update
        
    }  
    
    
    /// <summary>
    /// Raises the controller type changed event.
    /// </summary>
    protected virtual void OnControllerTypeChanged( vp_UITouchController.vp_TouchControllerType type )
    {
    
    	if(!Application.isPlaying)
    		return;
    		
    	Color backgroundOnColor = BackgroundOnColor;
    	Color backgroundOffColor = BackgroundOffColor;
    	Color knobOnColor = KnobOnColor;
    	Color knobOffColor = KnobOffColor;
    	
    	m_PanelTransform.localPosition = m_DefaultPanelPosition;
    	
		MeshRenderer knobMeshRenderer = Knob.GetComponent<MeshRenderer>();
    	MeshRenderer bgMeshRenderer = Background.GetComponent<MeshRenderer>();
    		
    	if(m_ControllerType == vp_UITouchController.vp_TouchControllerType.TouchPad)
		{
			backgroundOnColor = Color.clear;
			backgroundOffColor = Color.clear;
			knobOnColor = Color.clear;
			knobOffColor = Color.clear;
			if(knobMeshRenderer != null) knobMeshRenderer.enabled = false;
			if(bgMeshRenderer != null) bgMeshRenderer.enabled = false;
		}
		else
		{
			if(knobMeshRenderer != null) knobMeshRenderer.enabled = true;
			if(bgMeshRenderer != null) bgMeshRenderer.enabled = true;
		}
		
		KnobColor = m_ControllerType == vp_UITouchController.vp_TouchControllerType.StaticJoystick ? knobOnColor : knobOffColor;
		BackgroundColor = m_ControllerType == vp_UITouchController.vp_TouchControllerType.StaticJoystick ? backgroundOnColor : backgroundOffColor;
		
		if(m_PadParticles != null)
			m_PadParticles.enableEmission = false;
			
		if(ControllerTypeChanged != null)
			ControllerTypeChanged(m_ControllerType);
    
    }
    
    
    /// <summary>
    /// Fired when a touch event occures
    /// </summary>
    protected override void TouchesBegan( vp_Touch touch )
    {
    
    	if(LastFingerID != -1)
    		return;
    		
    	if(m_ControllerType == vp_UITouchController.vp_TouchControllerType.StaticJoystick)
			if(Physics.RaycastAll(m_Camera.ScreenPointToRay(touch.Position)).Where(hit => hit.collider == Knob.GetComponent<Collider>()).ToList().Count == 0)
				return;
    	
    	if(!RaycastControl(touch))
    		return;
    
    	base.TouchesBegan(touch);
    
		if(m_ControllerType != vp_UITouchController.vp_TouchControllerType.StaticJoystick)
			m_PanelTransform.position = m_Camera.ScreenToWorldPoint( touch.Position ); // position joystick at touch
			
		m_KnobArea.center = m_PanelTransform.position; // position the knob area at the joysticks center for constraining
    
    }
    
    
    /// <summary>
    /// When the joystick is moved
    /// </summary>
    protected virtual void TouchesMoved( vp_Touch touch )
    {
    
    	if ( LastFingerID != touch.FingerID )
    		return;
    	
		// adjust position and constrain to bounds
		Vector3 defaultPosition = Knob.position;
		Vector3 pos = ConstrainToBounds( m_Camera.ScreenToWorldPoint( touch.Position ) );
		if(m_ControllerType == vp_UITouchController.vp_TouchControllerType.TouchPad)
		{
			HandlePadParticles(true);
			pos = m_Camera.ScreenToWorldPoint( touch.Position );
		}
			
		pos.z = defaultPosition.z;
		Knob.position = pos;
        
        // a little math for the controls
        float maxX = (Knob.localScale.x - m_KnobArea.max.x) / 2;
        float maxY = (Knob.localScale.y - m_KnobArea.max.y) / 2;
        Vector3 distance = Knob.localPosition - m_DefaultKnobPosition;
        float x = Mathf.Clamp( distance.x / maxX, -1-Deadzone.x, 1+Deadzone.x );
        float y = Mathf.Clamp( distance.y / maxY, -1-Deadzone.y, 1+Deadzone.y );
        Vector2 movement = new Vector2(x,y);
        
        // adjust for threshold x
        if(movement.x <= Deadzone.x && movement.x >= -Deadzone.x) movement.x = 0;
        else if(movement.x > 0) movement.x -= Deadzone.x;
        else if(movement.x < 0) movement.x += Deadzone.x;
        
        // adjust for threshold y
        if(movement.y <= Deadzone.y && movement.y >= -Deadzone.y) movement.y = 0;
        else if(movement.y > 0) movement.y -= Deadzone.y;
        else if(movement.y < 0) movement.y += Deadzone.y;
        
        // set raw movement vector
     	m_RawMove = movement;
    
    }
    
    
    /// <summary>
    /// Constrains the knob to the bounds
    /// </summary>
    protected virtual Vector3 ConstrainToBounds( Vector3 pos )
    {
		
		// constrain to the bounds
		if(pos.x > m_KnobArea.max.x)
			pos.x = m_KnobArea.max.x;
		if(pos.x < m_KnobArea.min.x)
			pos.x = m_KnobArea.min.x;
		if(pos.y > m_KnobArea.max.y)
			pos.y = m_KnobArea.max.y;
		if(pos.y < m_KnobArea.min.y)
			pos.y = m_KnobArea.min.y;
			
		return pos;
    
    }
    
    
    protected virtual void HandlePadParticles( bool enable )
    {
    
    	if(m_ControllerType != vp_UITouchController.vp_TouchControllerType.TouchPad)
    		return;
    
    	if(m_PadParticles == null)
    		return;
    
    	m_PadParticles.enableEmission = enable;
    	
    }
    
    
    /// <summary>
    /// returns the movement vector for the MoveAxisCallback
    /// </summary>
    protected virtual float GetMove(string id)
    {
    
    	return id == "Horizontal" ? m_RawMove.x : id == "Vertical" ? m_RawMove.y : 0;
    
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    public override void TouchesFinished( vp_Touch touch )
    {
    
    	if ( LastFingerID != touch.FingerID )
    		return;
    		
    	HandlePadParticles(false);
    	
    	Manager.Player.Run.TryStop();
    	LastFingerID = -1;
    
    }
    
    
    /// <summary>
    /// Handles displaying and hiding of the joystick
    /// </summary>
    protected virtual void JoystickUpdate()
    {
    
    	if(LastFingerID != -1 && !Manager.Player.Run.Active)
		{
			if(m_ControllerType != vp_UITouchController.vp_TouchControllerType.DynamicJoystick)
				return;
				
			// show the joystick
			if(Knob != null)
				KnobColor = Color.Lerp( KnobColor, KnobOnColor, Time.deltaTime * AnimationSpeed );
			if(Background != null)
				BackgroundColor = Color.Lerp( BackgroundColor, BackgroundOnColor, Time.deltaTime * AnimationSpeed );
		}
		else
		{
			// hide the joystick and reposition the knob to it's center
			if(m_ControllerType == vp_UITouchController.vp_TouchControllerType.DynamicJoystick)
			{
				if(Knob != null)
					KnobColor = Color.Lerp( KnobColor, KnobOffColor, Time.deltaTime * AnimationSpeed );
				if(Background != null)
					BackgroundColor = Color.Lerp( BackgroundColor, BackgroundOffColor, Time.deltaTime * AnimationSpeed );
			}
			
			if(Knob != null && m_ControllerType != vp_UITouchController.vp_TouchControllerType.TouchPad)
				Knob.localPosition = Vector3.Slerp( Knob.localPosition, m_DefaultKnobPosition, Time.deltaTime * AnimationSpeed);
			
			if(Manager.Player.Run.Active)
			{
				// hide the controller if not of StaticJoystick type while running
				if(m_ControllerType != vp_UITouchController.vp_TouchControllerType.StaticJoystick)
				{
					KnobColor = KnobOffColor;
					BackgroundColor = BackgroundOffColor;
				}
			}
			else
				m_RawMove = Vector2.zero;
		}
    
    }
    
}