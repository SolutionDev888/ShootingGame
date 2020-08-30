/////////////////////////////////////////////////////////////////////////////////
//
//	vp_NGUITouchJoystick.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	Manages the NGUI and touch controls for a controller that
//					controls the players movement
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(BoxCollider))]
public class vp_NGUITouchController : vp_UITouchController
{

	protected UI2DSprite m_Knob = null;			// sprite of the knob
	protected UI2DSprite m_Background = null;		// sprite of the background
	
	/// <summary>
	/// Gets or sets the color of the knob.
	/// </summary>
	public override Color32 KnobColor
	{
		get{ return m_Knob.color; }
		set{ m_Knob.color = value; }
	}
	
	/// <summary>
	/// Gets or sets the color of the background.
	/// </summary>
	public override Color32 BackgroundColor
	{
		get{ return m_Background.color; }
		set{ m_Background.color = value; }
	}

	/// <summary>
	/// 
	/// </summary>
	protected override void Awake()
	{
	
		m_Knob = Knob.GetComponent<UI2DSprite>();
		m_Background = Background.GetComponent<UI2DSprite>();
	
		base.Awake();

	}
    
    
    /// <summary>
    /// When the joystick is moved
    /// </summary>
    protected override void TouchesMoved( vp_Touch touch )
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
        float maxX = (m_Knob.localSize.x - m_KnobArea.max.x) / 2;
        float maxY = (m_Knob.localSize.y - m_KnobArea.max.y) / 2;
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
    
}