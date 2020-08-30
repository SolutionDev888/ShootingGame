/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AISenses.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class allows the the AI to see and hear. Hearing gets
//					notified via vp_NotificationCenter and the vp_AIAudioSource
//					component.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


[System.Serializable]
public class vp_AISenses : vp_AIPlugin
{

	public override string Title{ get{ return "Senses"; } }
	public override int SortOrder{ get{ return 700; } }
	
	// different senses
	public enum Senses
	{
		Hearing,
		Vision
	}
	
	protected List<Senses> m_Senses = new List<Senses>(){ Senses.Vision, Senses.Hearing };

	protected float m_NextCheckTime = 0;
	protected virtual Vector3 m_CheckFromPosition
	{
		get
		{
			return new Vector3(m_Transform.position.x, m_Transform.position.y + m_Controller.height, m_Transform.position.z);
		}
	}
	
	
	/// <summary>
	/// Register with GlobalEvent
	/// </summary>
	public override void OnEnable()
	{
	
		base.OnEnable();
	
		vp_GlobalEvent<Transform, Transform>.Register("Send Sound", HeardSound);
	
	}
	
	
	/// <summary>
	/// Unregister from GlobalEvent
	/// </summary>
	public override void OnDisable()
	{
	
		base.OnDisable();
	
		vp_GlobalEvent<Transform, Transform>.Unregister("Send Sound", HeardSound);
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	public override void LateUpdate()
	{
	
		base.LateUpdate();

		CheckDistances();
	
	}
	
	
	/// <summary>
	/// start to look for the player if
	/// in the ViewDistance threshold
	/// </summary>
	protected virtual void CheckDistances()
	{
	
		if(m_EventHandler.Dead.Active)
			return;
			
		foreach(Senses sense in m_Senses)
		{
			
			float checkRate = sense == Senses.Vision ? m_AI.SensesViewCheckRate : m_AI.SensesHearingCheckRate;
			if(checkRate == 0)
				return;
				
			if(Time.time < m_NextCheckTime)
				return;
				
			m_NextCheckTime = Time.time + checkRate;
			
			if(!m_EventHandler.Attacking.Active && !m_EventHandler.Retreat.Active)
			{
				Collider[] colliders = Physics.OverlapSphere(m_CheckFromPosition, sense == Senses.Vision ? m_AI.SensesViewDistance : m_AI.SensesHearingDistance, sense == Senses.Vision ? m_AI.SensesVisionMask : m_AI.SensesHearingMask);
				foreach(Collider other in colliders)
				{
					if(!m_EventHandler.IsValidTarget.Send(other.gameObject))
						continue;
				
					// get the player event handler if missing for some reason	
					vp_FPPlayerEventHandler player = vp_AIManager.GetPlayer(m_EventHandler, other.transform);
					if(player == null)
						continue;
					
					// if the player is dead, no need to continue
					if(player.Dead.Active)
						continue;
					
					if(sense == Senses.Vision)
						CheckVision(other);
				}
			}
		
		}
		
	}
	
	
	/// <summary>
	/// checks if the AI can see an object
	/// </summary>
	protected virtual void CheckVision( Collider collider )
	{
	
		// get the angle of vision and see if the player is within that angle
		Vector3 targetDir = collider.bounds.center - m_CheckFromPosition;
		float angle = Vector3.Angle(targetDir, m_Transform.forward);
		if (angle < m_AI.SensesViewingAngle)
		{
			// check for line of sight
			RaycastHit hit;
		    if(Physics.Raycast(m_CheckFromPosition, targetDir, out hit, m_AI.SensesVisionMask))
				m_EventHandler.Attacking.TryStart(collider.transform);
		}
	
	}
	
	
	/// <summary>
	/// notified from notification center and then
	/// checks to see if AI can hear it.
	/// </summary>
	protected virtual void HeardSound( Transform sender, Transform reciever )
	{
	
		if(sender == null || reciever == null)
			return;
	
		if(reciever != m_Transform)
			return;
	
		if((m_AI.SensesHearingMask.value & 1<<sender.gameObject.layer) == 0)
    		return;
    		
    	if(Vector3.Distance(m_Transform.position, sender.transform.position) > m_AI.SensesHearingDistance)
    		return;
    		
		m_EventHandler.Attacking.TryStart(sender);
	
	}
	
	
	/// <summary>
	/// receives a dictionary with properties and overrides
	/// those properties in this class if they exist
	/// </summary>
	protected virtual void OnMessage_SetProperties( Dictionary<string, object> properties )
	{
	
		foreach(string k in properties.Keys)
		{
			FieldInfo p = this.GetType().GetField(k);
			if(p != null)
				p.SetValue(this, properties[k]);
		}
		
	}
	
}
