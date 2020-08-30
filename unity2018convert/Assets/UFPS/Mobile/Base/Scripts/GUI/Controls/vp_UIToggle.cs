/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIToggle.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	A simple checkbox toggle
//					
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class vp_UIToggle : vp_UIControl
{
	
	public GameObject Background = null;
	public GameObject Checkmark = null;
	public bool State = true;
	
	
	/// <summary>
	/// event that is raised when the control is pressed
	/// </summary>
	protected virtual void OnPressControl()
	{
	
		State = !State;
		vp_Utility.Activate(Checkmark, State);
		if(ChangeControl != null)
			ChangeControl(this);
	
	}
	
}
