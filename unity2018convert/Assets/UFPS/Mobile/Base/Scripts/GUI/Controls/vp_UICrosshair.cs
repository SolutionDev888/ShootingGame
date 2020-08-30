/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UICrosshair.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this script manages a textured crosshair on a mesh
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

public class vp_UICrosshair : vp_UIControl
{

	public Color EnemyColor = Color.red;				// color of crosshair when over an enemy
	public float ColorChangeDuration = .25f;			// speed at which the color is changed
	public LayerMask EnemyMask = 1 << vp_Layer.Enemy;	// enemy layers
	public float InteractIconSize = 2;					// size that this gameobject will change to when interaction is available
	public Sprite EnemyCrosshair = null;				// textre that will be changed to when crosshair is over an enemy
	public bool ScaleOnRun = true;						// should the crosshair scale when running is active
	public float ScaleMultiplier = 1.25f;				// multiplies by the original crosshair size for ScaleOnRun
	public bool AssistedTargeting = true;				// should assisted targeting be allowed
	public float AssistedTargetingSpeed = 20;			// speed of assisted targeting interpolation
	public float AssistedTargetingRadius = 1f;			// the radius of the raycast that will be used to detect enemies
	public float AssistedTargetingTrackingRadius = 2f;	// when an enemy is being tracked, this is the radius threshold to stay within to keep tracking
	public bool AssistedTargetingFoldout = false;		// for editor use
	
	protected Renderer m_Renderer = null; 				// Cache the renderer
	protected GameObject m_GameObject = null;			// cache the gameobject
	protected Color m_DefaultColor;						// cache the default crosshair color
	protected Camera m_PlayerCamera = null;				// cache the player camera
	protected Vector3 m_ScreenCenter = Vector3.zero;	// cache the center of the screen
	protected Vector3 m_CachedScale = Vector3.one;		// cache the original scale of the crosshair
	protected string m_CachedTextureName = "";			// original crosshairs texture name
	protected Dictionary<Collider, Transform> m_Enemies = new Dictionary<Collider, Transform>(); // for testing interactable components
	protected Vector3 m_CrosshairNextPosition = Vector3.zero;	// next position the crosshair should be moved to (for assisted targeting)
	protected Transform m_CurrentTrackedEnemy = null;	// enemy that is currently being tracked
	protected bool m_Tracking = false;					// whether or not an enemy is being tracked
	protected virtual bool TrackingEnemy(){ return m_Tracking; }
	protected bool m_ShowCrosshair = true;				// should the crosshair be shown
	protected bool m_Interacting = false;				// is the player interacting
	protected vp_UITween.Handle m_ColorHandle = new vp_UITween.Handle();	// handle for the crosshairs color tween
	protected vp_UITween.Handle m_ScaleCrosshairHandle = new vp_UITween.Handle();	// handle for the crosshairs scale tween
	
	
	protected virtual Color m_CrosshairColor			// gets the color of the crosshair based on what it's over
	{
		get{
			Color color = m_DefaultColor;
			if(m_Tracking)
				color = EnemyColor;
				
			color.a = m_ShowCrosshair ? 1 : 0;
			
			return color;
		}
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected override void Awake()
	{
	
		base.Awake();
	
		m_Renderer = GetComponent<Renderer>();
		m_GameObject = gameObject;
		if (m_Renderer != null && m_Renderer.material != null && m_Renderer.material.mainTexture != null)
		{
			m_DefaultColor = m_Renderer.material.color;
			m_CachedTextureName = m_Renderer.material.mainTexture.name;
		}
		m_PlayerCamera = Manager.Player.GetComponentInChildren<vp_FPCamera>().GetComponent<Camera>();
		m_CachedScale = m_Transform.localScale;
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected override void Start()
	{
	
		m_ScreenCenter = Manager.UICamera.GetComponent<Camera>().ScreenToWorldPoint( new Vector3(Screen.width * .5f, Screen.height * .5f, m_Transform.position.z) );
		
		base.Start();
	
	}
	
	
	/// <summary>
	/// adds a global event for toggling the crosshair
	/// </summary>
	protected override void OnEnable()
	{
	
		base.OnEnable();
	
		vp_GlobalEvent<bool>.Register("Display Crosshair", DisplayCrosshair);
		vp_GlobalEventReturn<bool>.Register("Tracking Enemy", TrackingEnemy);
	
	}
	
	
	/// <summary>
	/// removes the global event for toggling the crosshair
	/// </summary>
	protected override void OnDisable()
	{
	
		base.OnDisable();
	
		vp_GlobalEvent<bool>.Unregister("Display Crosshair", DisplayCrosshair);
		vp_GlobalEventReturn<bool>.Unregister("Tracking Enemy", TrackingEnemy);
	
	}
	
	
	/// <summary>
	/// updates the crosshair color
	/// </summary>
	protected override void Update()
	{
	
		m_Interacting = (Manager.Player.Interactable.Get() != null && Manager.Player.Interactable.Get().GetType() == typeof(vp_Grab)) || Manager.Player.Climb.Active;
	
		EnemyCheck();

		HandleCrosshairColor();
		
		if(m_CurrentTrackedEnemy == null)
			m_CrosshairNextPosition = m_ScreenCenter;
			
		m_Transform.position = Vector3.Lerp( m_Transform.position, m_CrosshairNextPosition, Time.deltaTime * AssistedTargetingSpeed);
		
		TrackEnemy();
		
		if(m_Tracking && EnemyCrosshair != null)
			Manager.Player.Crosshair.Set(EnemyCrosshair);
	
	}
	
	
	/// <summary>
	/// Handles the color of the crosshair.
	/// </summary>
	protected virtual void HandleCrosshairColor()
	{
	
		if(m_ShowCrosshair)
			vp_UITween.ColorTo(m_GameObject, m_CrosshairColor, ColorChangeDuration, m_ColorHandle);
		else
			m_GameObject.GetComponent<Renderer>().material.color = m_CrosshairColor;
	
	}
	
	
	/// <summary>
	/// Tracks the enemy.
	/// </summary>
	protected virtual void TrackEnemy()
	{
		
		m_Tracking = false;
	
		if(m_CurrentTrackedEnemy == null || m_Interacting || Manager.Player.Dead.Active)
			return;
			
		Transform enemy = null;
		RaycastHit[] hits = Physics.SphereCastAll( m_PlayerCamera.transform.position, AssistedTargetingTrackingRadius, m_PlayerCamera.transform.forward, Mathf.Infinity);
		foreach(RaycastHit hit in hits)
		{
			if(!m_Enemies.TryGetValue(hit.collider, out enemy))
				m_Enemies.Add(hit.collider, enemy = hit.collider.transform);
				
			if(enemy != null && enemy == m_CurrentTrackedEnemy)
			{
				m_Tracking = true;
				
				if(AssistedTargeting)
				{
					// position crosshair
					Vector3 head = new Vector3(m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.center.x, m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.center.y + ((m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.max.y - m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.center.y) / 2), m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.center.z);
					Vector3 viewportPos = m_PlayerCamera.WorldToViewportPoint( head );
					viewportPos.z = m_ScreenCenter.z;
					m_CrosshairNextPosition = Manager.UICamera.ViewportToWorldPoint(viewportPos);
					
					// move camera toward target
					if(vp_Input.GetAxisRaw("Mouse X") == 0 && vp_Input.GetAxisRaw("Mouse Y") == 0)
					{	
						Quaternion newRotation = Quaternion.Slerp( m_PlayerCamera.transform.rotation, Quaternion.LookRotation(m_CurrentTrackedEnemy.GetComponent<Collider>().bounds.center-m_PlayerCamera.transform.position), Time.deltaTime * 3 );
						Manager.Player.Rotation.Set( new Vector2(newRotation.eulerAngles.x, newRotation.eulerAngles.y) );
					}
				}
			}
		}
		
		if(!m_Tracking)
			m_CurrentTrackedEnemy = null;
	
	}
	
	
	/// <summary>
	/// Hide crosshair when climbing starts
	/// </summary>
	protected virtual void OnStart_Climb(){ DisplayCrosshair(false); }
	
	
	/// <summary>
	/// Show crosshair when climbing stops
	/// </summary>
	protected virtual void OnStop_Climb(){ DisplayCrosshair(); }
	
	
	/// <summary>
	/// Hide crosshair if zooming
	/// </summary>
	protected virtual void OnStart_Zoom(){ DisplayCrosshair(false); }
	
	
	/// <summary>
	/// Show crosshair when zoom stopped
	/// </summary>
	protected virtual void OnStop_Zoom(){ DisplayCrosshair(); }
	
	protected virtual void OnStart_Run()
	{
	
		if(Manager.Player.CanInteract.Get() || !ScaleOnRun)
			return;
			
		vp_UITween.ScaleTo(m_GameObject, vp_UITween.Hash("scale", m_CachedScale * ScaleMultiplier, "duration", .15f, "handle", m_ScaleCrosshairHandle) );
		
	}
	
	
	/// <summary>
	/// scale the crosshair to origin when running stops
	/// </summary>
	protected virtual void OnStop_Run()
	{
	
		if(Manager.Player.CanInteract.Get() || !ScaleOnRun)
			return;
	
		vp_UITween.ScaleTo(m_GameObject, vp_UITween.Hash("scale", m_CachedScale, "duration", .15f, "handle", m_ScaleCrosshairHandle) );
		
	}
	
	
	/// <summary>
	/// Displays the crosshair based on the specified value.
	/// </summary>
	protected virtual void DisplayCrosshair( bool val = true )
	{
	
		m_ShowCrosshair = val;
	
	}
	
	
	/// <summary>
	/// Checks if the crosshair is over an enemy
	/// </summary>
	protected void EnemyCheck()
	{
	
		if(m_Interacting || Manager.Player.Dead.Active)
			return;
		
		Transform enemy = null;
		RaycastHit hit;
		if(Physics.SphereCast( m_PlayerCamera.transform.position, AssistedTargetingRadius, m_PlayerCamera.transform.forward, out hit, Mathf.Infinity, EnemyMask ))
			//if((EnemyMask.value & 1 << hit.collider.gameObject.layer) != 0)
				if(!m_Enemies.TryGetValue(hit.collider, out enemy))
					m_Enemies.Add(hit.collider, enemy = hit.collider.transform);
				
		if(enemy != null)
			m_CurrentTrackedEnemy = Physics.Linecast( m_PlayerCamera.transform.position, enemy.position, ~(EnemyMask | 1 << vp_Layer.Trigger) ) ? null : enemy;
	
	}
	
	
	/// <summary>
	/// Gets or sets the value of the Crosshair texture
	/// </summary>
	protected virtual Sprite OnValue_Crosshair
	{
		get{
			return null;
		}
//		get { return m_Renderer.material.mainTexture; }
		set {
//			if(m_Tracking && EnemyCrosshair != null)
//			{
//				m_Renderer.material.mainTexture = EnemyCrosshair;
//			}
//			else if(value.name == "")
//				m_ShowCrosshair = false;
//			else
//			{
//				if(!Manager.Player.Zoom.Active)
//					m_ShowCrosshair = true;
//				m_Renderer.material.mainTexture = value;
//			}
//			
//			if(m_Tracking)
//				return;
//		
//			// change the icon size if it's not the default 
//			Vector3 localScale = value.name == m_CachedTextureName ? m_CachedScale : m_CachedScale * InteractIconSize;
//			
//			// make the icon size bigger if no icon shown and grabbing
//			localScale = Manager.Player.Interactable.Get() != null && Manager.Player.Interactable.Get().GetType() == typeof(vp_Grab) && value.name == "" ? m_CachedScale * (InteractIconSize * 3) : localScale;
//			vp_UITween.ScaleTo(m_GameObject, vp_UITween.Hash("scale", localScale, "duration", 0, "handle", m_ScaleCrosshairHandle) );
		}
	}

}

