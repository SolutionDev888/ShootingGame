/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIAudioSource.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This class allows objects to send noise notifications out
//					to the world. Uses vp_GlobalEvent to send messages.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class vp_AIAudioSource : vp_Component
{

	public float Distance = 25;								// Distance sounds from the audio source get sent
	public LayerMask ListeningLayers = 1<<vp_Layer.Default;	// Layers to notify
	public string GlobalEventName = "Send Sound";			// Notification name that is sent
	public float SendRate = 1;								// Rate at which the sounds are sent
	public List<AudioSource> m_AudioSources = new List<AudioSource>();	// List of audio sources
	
	protected float m_NextSendTime = 0;						// next time a sound should be sent
	
	
	/// <summary>
	/// in 'Start' we do things that need to be run once at the
	/// beginning, but potentially depend on all other scripts
	/// first having run their 'Awake' calls.
	/// NOTE: 1) don't do anything here that depends on activity
	/// in other 'Start' calls. 2) if adding code here, remember
	/// to call it using 'base.Start();' on the first line of
	/// the 'Start' method in the derived classes
	/// </summary>
	protected override void Start()
	{
	
		base.Start();
	
		// get all of the audio sources attached to this object
		foreach(AudioSource a in GetComponents<AudioSource>())
			m_AudioSources.Add(a);
	
	}
	
	
	/// <summary>
	/// NOTE: to provide the 'Init' functionality, this method
	/// must be called using 'base.Update();' on the first line
	/// of the 'Update' method in the derived class
	/// </summary>
	protected override void Update ()
	{
	
		base.Update();
	
		SendSoundUpdate();
		
	}
	
	
	/// <summary>
	/// Sends the sound notification
	/// </summary>
	protected virtual void SendSoundUpdate()
	{
	
		bool isPlaying = false;
		foreach(AudioSource a in m_AudioSources)
			if(a.isPlaying)
			{
				isPlaying = true;
				break;
			}
			
		if(!isPlaying)
			return;
			
		if(Time.time < m_NextSendTime)
			return;
			
		m_NextSendTime = Time.time + SendRate;
			
		Collider[] colliders = Physics.OverlapSphere(m_Transform.position, Distance, ListeningLayers);
		foreach(Collider c in colliders)
			vp_GlobalEvent<Transform, Transform>.Send(GlobalEventName, m_Transform, c.transform);
	
	}
	
}
