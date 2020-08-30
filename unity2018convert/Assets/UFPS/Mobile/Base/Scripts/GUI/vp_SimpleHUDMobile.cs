/////////////////////////////////////////////////////////////////////////////////
//
//	vp_SimpleHUDMobile.cs
//	Â© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a version of the vp_SimpleHUD with a classic mobile FPS layout
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class vp_SimpleHUDMobile : vp_SimpleHUD
{

	public float FadeDuration = 3f;				// duration of the fade for HUD Messages
	public bool ShowTips = true;				// enable or disable the display of HUD Messages
	public GameObject AmmoLabel = null;			// a gameobject that has a TextMesh component for ammo label
	public GameObject HealthLabel = null;		// a gameobject that has a TextMesh component for Health label
	public GameObject HintsLabel = null;		// a gameobject that has a TextMesh component for Hints label
	
	private TextMesh m_AmmoLabel = null;		// cached TextMesh component for ammo label
	private TextMesh m_HealthLabel = null;		// cached TextMesh component for ammo label
	private TextMesh m_HintsLabel = null;		// cached TextMesh component for ammo label
	protected vp_UITween.Handle m_HUDTextTweenHandle = new vp_UITween.Handle();
	protected Color m_MessageColorMobile = new Color(2, 2, 0, 2);
	protected Color m_InvisibleColorMobile = new Color(1, 1, 0, 0);
	protected string m_PickupMessageMobile = "";
	protected static GUIStyle m_MessageStyleMobile = null;
	public static GUIStyle MessageStyleMobile
	{
		get
		{
			if (m_MessageStyleMobile == null)
			{
				m_MessageStyleMobile = new GUIStyle("Label");
				m_MessageStyleMobile.alignment = TextAnchor.MiddleCenter;
			}
			return m_MessageStyleMobile;
		}
	}

	protected vp_FPPlayerEventHandler m_PlayerEventHandler = null;
	protected int m_Health{
		get{
			int health = (int)(m_PlayerEventHandler.Health.Get()/ m_PlayerEventHandler.MaxHealth.Get() * 100.0f);
			return health < 0 ? 0 : health;
		}
	}


	/// <summary>
	///
	/// </summary>
	protected override void Awake()
	{

		m_PlayerEventHandler = transform.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
		
		if(AmmoLabel != null)	m_AmmoLabel = AmmoLabel.GetComponentInChildren<TextMesh>();
		if(HealthLabel != null)	m_HealthLabel = HealthLabel.GetComponentInChildren<TextMesh>();
		if(HintsLabel != null)	m_HintsLabel = HintsLabel.GetComponentInChildren<TextMesh>();
		
		if(m_HintsLabel != null)
		{
			m_HintsLabel.text = "";
			m_HintsLabel.GetComponent<Renderer>().material.color = Color.clear;
		}
		
	}


	/// <summary>
	/// registers this component with the event handler (if any)
	/// </summary>
	protected override void OnEnable()
	{

		if (m_PlayerEventHandler != null)
			m_PlayerEventHandler.Register(this);

	}


	/// <summary>
	/// unregisters this component from the event handler (if any)
	/// </summary>
	protected override void OnDisable()
	{


		if (m_PlayerEventHandler != null)
			m_PlayerEventHandler.Unregister(this);

	}


	/// <summary>
	/// this is here to mute the 'desktop' version of the simplehud
	/// </summary>
	protected override void OnGUI()
	{

	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Update()
	{

		if (m_AmmoLabel != null)
		{
			if (m_PlayerEventHandler.CurrentWeaponIndex.Get() > 0)
			{
				m_AmmoLabel.text = (m_PlayerEventHandler.CurrentWeaponAmmoCount.Get() + "/" +
				(m_PlayerEventHandler.CurrentWeaponClipCount.Get())).ToString();
			}
			else
				m_AmmoLabel.text = "0/0";
		}

		if(m_HealthLabel != null)
			m_HealthLabel.text = m_Health + "%";
	
	}
	
	
	/// <summary>
	/// updates the HUD message text and makes it fully visible
	/// then fades it out
	/// </summary>
	protected override void OnMessage_HUDText(string message)
	{

		if(!ShowTips || m_HintsLabel == null)
			return;

		m_PickupMessageMobile = (string)message;
		m_HintsLabel.text = m_PickupMessageMobile;
		vp_UITween.ColorTo(HintsLabel, Color.white, .25f, m_HUDTextTweenHandle, delegate {
			vp_UITween.ColorTo(HintsLabel, m_InvisibleColorMobile, FadeDuration, m_HUDTextTweenHandle);
		});

	}
	
	
}

