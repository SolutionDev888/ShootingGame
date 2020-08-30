/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIPlugin.cs
//	Â© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	A base class that all plugins for the AI should derive from
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[System.Serializable]
public class vp_AIPlugin
{

	public bool Enabled = true;
	public virtual string Title{ get{ return "AI Plugin"; } }
	public virtual int SortOrder{ get{ return 0; } }

#if UNITY_EDITOR
	[HideInInspector]
	public bool MainFoldout;
#endif

	protected vp_AIEventHandler m_EventHandler = null;	// cached event handler
	protected vp_AI m_AI = null;						// cached AI component
	protected Transform m_Transform = null;				// cached transform
	protected GameObject m_GameObject = null;			// cached gameobject
	protected Rigidbody m_Rigidbody = null;				// cached rigidbody
	protected AudioSource m_Audio = null;				// cached audio component
	protected CharacterController m_Controller = null;	// cached character controller component
	protected bool m_Enabled = false;


	/// <summary>
	/// This is equivalent to Unity's Awake and will
	/// be called by the AI components Awake method.
	/// The AI component needs to be passed to this method.
	/// </summary>
	public virtual void Awake( vp_AI ai )
	{
	
		m_Enabled = Enabled;
		m_AI = ai;
		m_EventHandler = (vp_AIEventHandler)m_AI.EventHandler;
		m_Transform = ai.transform;
		m_GameObject = ai.gameObject;
		m_Rigidbody = ai.GetComponent<Rigidbody>();
		m_Audio = ai.GetComponent<AudioSource>();
		m_Controller = m_Transform.root.GetComponentInChildren<CharacterController>();
	
	}
	
	
	/// <summary>
	/// Register with the event handler
	/// </summary>
	public virtual void OnEnable()
	{
	
		if(m_EventHandler != null)
			m_EventHandler.Register(this);
			
		if(m_Enabled)
			Enabled = true;
	
	}
	
	
	/// <summary>
	/// Unregister from the event handler
	/// </summary>
	public virtual void OnDisable()
	{
	
		if(m_EventHandler != null)
			m_EventHandler.Unregister(this);	
		
		if(m_Enabled)
			Enabled = false;
	
	}
	
	/// <summary>
	/// 
	/// </summary>
	public virtual void OnDestroy(){}
	/// <summary>
	/// 
	/// </summary>
	public virtual void Start(){}
	/// <summary>
	/// 
	/// </summary>
	public virtual void LateUpdate(){}
	/// <summary>
	/// 
	/// </summary>
	public virtual void Update(){}
	/// <summary>
	/// 
	/// </summary>
	public virtual void FixedUpdate(){}
	
}
