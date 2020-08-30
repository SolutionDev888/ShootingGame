/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIContextualControls.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This class manages the visibility of action buttons
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
using System.Linq;

public class vp_UIContextualControls : MonoBehaviour
{

	/// <summary>
	/// an object for storing data for an action control
	/// </summary>
	public class vp_UIContextualControl
	{
	
		public object Button;
		public vp_UITween.Handle Handle = new vp_UITween.Handle();
		
		public vp_UIContextualControl( GameObject obj, bool active = false )
		{
		
			Button = obj;
			if(obj.GetComponent<Renderer>() != null)
			{
				Color color = obj.GetComponent<Renderer>().material.color;
				color.a = active ? 1 : 0;
				obj.GetComponent<Renderer>().material.color = color;
			}
		
		}
	
	}

	public float FadeDuration = .25f;			// Amount of time in seconds a fade occurs
	public GameObject AttackButton = null;		// Attack Button GameObject
	public GameObject JumpButton = null;		// Jump Button GameObject
	public GameObject ReloadButton = null;		// Reload Button GameObject
	public GameObject ZoomButton = null;		// Zoom Button GameObject
	public GameObject RunButton = null;			// Run Button GameObject
	
	protected vp_UIManager m_Manager = null;	// Cached UIManager
	protected List<vp_UIContextualControl> m_Buttons = new List<vp_UIContextualControl>();	// Cached list of buttons for quick iteration
	protected bool m_Initialized = false;		// initialization flag
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{
	
		m_Manager = GetComponent<vp_UIManager>();
		SetupButtons();
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Start()
	{
	
		m_Initialized = true;
	
	}
	

	/// <summary>
	/// Check Actions for displaying buttons
	/// </summary>
	protected virtual void Update()
	{
	
		UpdateVisibility();
	
	}
	
	
	/// <summary>
	/// create new UIContextualControl objects
	/// for each control and store them in a list
	/// </summary>
	protected virtual void SetupButtons()
	{
	
		if(m_Buttons.Count != 0)
			return;
			
		m_Buttons.Add(new vp_UIContextualControl(AttackButton));
		m_Buttons.Add(new vp_UIContextualControl(JumpButton, true));
		m_Buttons.Add(new vp_UIContextualControl(ReloadButton));
		m_Buttons.Add(new vp_UIContextualControl(ZoomButton));
		m_Buttons.Add(new vp_UIContextualControl(RunButton, true));
	
	}
	
	
	/// <summary>
	/// checks different states and sets visibility
	/// accordingly for each control
	/// </summary>
	protected virtual void UpdateVisibility()
	{
	
		if(m_Manager.Player == null || !m_Initialized)
			return;
	
		bool attack = true;
		bool jump = true;
		bool reload = true;
		bool zoom = true;
		bool run = true;
		bool grabbing = m_Manager.Player.Interactable.Get() != null && m_Manager.Player.Interactable.Get().GetType() == typeof(vp_Grab);
	
		// checks interaction climbing and weapon wielded states
		if(m_Manager.Player.Climb.Active || (!m_Manager.Player.CurrentWeaponWielded.Get() && !grabbing))
		{
			attack = zoom = reload = false;
			if(m_Manager.Player.Climb.Active)
				run = false;
		}
		
		// check grab state, and disable zoom & reload buttons on weapons without ammo
		if (grabbing || ((m_Manager.Player.CurrentWeaponIndex.Get() > 0) && m_Manager.Player.CurrentWeaponMaxAmmoCount.Get() == 0))
			zoom = reload = false;
			
		// don't mess with the controls if the weapon is changing
		if(m_Manager.Player.SetWeapon.Active)
			return;
		
		TweenColor(m_Buttons.FirstOrDefault(b => b.Button == AttackButton), attack);
		TweenColor(m_Buttons.FirstOrDefault(b => b.Button == JumpButton), jump);
		TweenColor(m_Buttons.FirstOrDefault(b => b.Button == ReloadButton), reload);
		TweenColor(m_Buttons.FirstOrDefault(b => b.Button == ZoomButton), zoom);
		TweenColor(m_Buttons.FirstOrDefault(b => b.Button == RunButton), run);
	
	}
	
	
	/// <summary>
	/// Tweens the color to set visibility based on active state
	/// </summary>
	protected virtual void TweenColor( vp_UIContextualControl button, bool active )
	{
	
		if(button == null)
			return;
	
		GameObject go = (GameObject)button.Button;
		
		if(go == null)
			return;
	
		vp_UITween.FadeTo(go, active ? 1 : 0, FadeDuration, button.Handle);
	
	}
	
}
