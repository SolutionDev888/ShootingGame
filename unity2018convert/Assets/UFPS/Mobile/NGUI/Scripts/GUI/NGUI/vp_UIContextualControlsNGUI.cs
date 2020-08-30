/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIContextualControlsNGUI.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This class manages the visibility of action buttons for NGUI
//
//					Actions:	Attack
//								Jump
//								Reload
//								Zoom
//								Run
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_UIContextualControlsNGUI : vp_UIContextualControls
{

	/// <summary>
	/// an object for storing data for an NGUI action control
	/// </summary>
	public class vp_UIContextualControlNGUI : vp_UIContextualControl
	{
	
		public UI2DSprite Sprite = null;
		
		public vp_UIContextualControlNGUI( GameObject obj, bool active = false ) : base(obj, active)
		{
		
			Sprite = obj.GetComponentInChildren<UI2DSprite>();
			Color color = Sprite.color;
			color.a = active ? 1 : 0;
			Sprite.color = color;
		
		}
		
	}

	/// <summary>
	/// create new vp_UIContextualControlNGUI objects
	/// for each control and store them in a list
	/// </summary>
	protected override void SetupButtons()
	{
	
		if(m_Buttons.Count != 0)
			return;
			
		m_Buttons.Add( new vp_UIContextualControlNGUI(AttackButton, true) );
		m_Buttons.Add( new vp_UIContextualControlNGUI(JumpButton, true) );
		m_Buttons.Add( new vp_UIContextualControlNGUI(ReloadButton, true) );
		m_Buttons.Add( new vp_UIContextualControlNGUI(ZoomButton, true) );
		m_Buttons.Add( new vp_UIContextualControlNGUI(RunButton, true) );
	
	}
	
	
	/// <summary>
	/// Tweens the color to set visibility based on active state
	/// </summary>
	protected override void TweenColor( vp_UIContextualControl button, bool active )
	{
	
		vp_UIContextualControlNGUI nguiButton = (vp_UIContextualControlNGUI)button;
	
		if(nguiButton == null)
			return;

        UI2DSprite sprite = (UI2DSprite)nguiButton.Sprite;
	
		if(sprite == null)
			return;
	
		vp_NGUITween.FadeTo(sprite, active ? 1 : 0, FadeDuration, button.Handle);
	
	}
	
}