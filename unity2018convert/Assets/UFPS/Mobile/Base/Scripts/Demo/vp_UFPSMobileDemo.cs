/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UFPSMobileDemo.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a simple script for handling events from the in game pause menu
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_UFPSMobileDemo : MonoBehaviour
{

	public vp_UITouchController TouchController = null;
//	public GameObject Menu = null;
	public GameObject UI = null;
	public vp_UITouchLook TouchLookPad = null;
	public vp_SimpleHUDMobile SimpleHUD = null;
	
	protected bool m_Initialized = false;
	protected float m_AutoPitchSpeed = 0;
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{
	
		if(TouchLookPad)
			m_AutoPitchSpeed = TouchLookPad.AutoPitchSpeed;
	
	}
	
	
	/// <summary>
	/// event recieved when a new controller type is chosen from the dropdown
	/// </summary>
	public virtual void ChangeControllerType( vp_UIControl control )
	{
	
		string value = ((vp_UIDropdownList)control).CurrentItem;
		
		if(value == "Dynamic Joystick")
			TouchController.ControllerType = vp_UITouchController.vp_TouchControllerType.DynamicJoystick;
		else if(value == "Static Joystick")
			TouchController.ControllerType = vp_UITouchController.vp_TouchControllerType.StaticJoystick;
		else
			TouchController.ControllerType = vp_UITouchController.vp_TouchControllerType.TouchPad;
	
	}
	
	
	/// <summary>
	/// Toggles the display of the menu
	/// </summary>
	public virtual void ToggleMenu()
	{
	
//		vp_Utility.Activate(Menu, !vp_Utility.IsActive(Menu));
		vp_Utility.Activate(UI, !vp_Utility.IsActive(UI));
		vp_TimeUtility.Paused = !vp_TimeUtility.Paused;
	
	}
	
	
	/// <summary>
	/// event when the autopitch checkbox is toggled
	/// </summary>
	public virtual void AutoPitchToggle( vp_UIControl control )
	{
	
		if(TouchLookPad == null)
			return;
			
		bool value = ((vp_UIToggle)control).State;
			
		TouchLookPad.AutoPitchSpeed = value ? m_AutoPitchSpeed == 0 ? 2.75f : m_AutoPitchSpeed : 0;
	
	}
	
	
	/// <summary>
	/// event when the tips checkbox is toggled
	/// </summary>
	public virtual void HelpTipsToggle( vp_UIControl control )
	{
	
		if(SimpleHUD == null)
			return;
			
		bool value = ((vp_UIToggle)control).State;
			
		SimpleHUD.ShowTips = value;
	
	}
	
}
