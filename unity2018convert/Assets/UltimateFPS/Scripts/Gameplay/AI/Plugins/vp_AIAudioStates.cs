/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIAudioStates.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class enables unlimited audio clips to be assigned to
//					any state and will play those sounds automatically when the
//					state is enabled
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[System.Serializable]
public class vp_AIAudioStates : vp_AIPlugin
{

	public override string Title{ get{ return "Audio States"; } }
	public override int SortOrder{ get{ return 100; } }
	
	/// <summary>
	/// 
	/// </summary>
	public override void Start()
	{
	
		base.Start();
		
		m_Audio.playOnAwake = false;
		m_Audio.Stop();
	
	}
	
	
	/// <summary>
	/// Automatically plays a sound for the current state
	/// </summary>
	public override void Update()
	{
	
		foreach(vp_AIState state in m_AI.States)
			if(state.IsAudioState)
				if(state.StateManager.IsEnabled(state.Name))
					m_EventHandler.PlaySound.Send(state);
	
	}
	
	
	/// <summary>
	/// chooses a random sound to play from the list of sounds
	/// provided for this state
	/// </summary>
	protected virtual bool OnMessage_PlaySound( object obj )
	{
	
		if(m_Audio.isPlaying)
			return false;
	
		if(obj == null)
			return false;
	
		if(obj.GetType() != typeof(vp_AIState))
			return false;
			
		vp_AIState state = (vp_AIState)obj;
		if(state == null)
			return false;
		
		if(state.AudioClips == null)
			return false;
			
		if(state.AudioClips.Count == 0)
			return false;
			
		if(Time.time < state.LastTimeAudioPlayed)
			return false;
			
		state.LastTimeAudioPlayed = Time.time + state.AudioPlayInterval;
			
		reroll:
		AudioClip clip = state.AudioClips[ Random.Range(0, state.AudioClips.Count) ];
		
		if(clip == null)
			return false;
			
		if(clip == null)
			return false;
			
		// if the animation was the last one played, reroll for another animation
		if (clip == state.LastAudioClip && state.AudioClips.Count > 1)
			goto reroll;
			
		if(state.AudioProbability < 1)
			if((float)Random.Range(0f, 1f) > state.AudioProbability)
				return false;
			
		m_Audio.pitch = Random.Range(state.AudioPitch.x, state.AudioPitch.y) * Time.timeScale;
		
		m_Audio.clip = clip;
		m_Audio.Play();
		state.LastAudioClip = clip;
		
		return true;
		
	}
	
	
	/// <summary>
	/// gets a random audio clip for the specified state
	/// </summary>
	protected virtual AudioClip OnMessage_GetAudioClipForState( object obj )
	{
	
		if(obj == null)
			return null;
	
		if(obj.GetType() != typeof(vp_AIState))
			return null;
			
		vp_AIState state = (vp_AIState)obj;
		if(state == null)
			return null;
		
		if(state.AudioClips == null)
			return null;
			
		if(state.AudioClips.Count == 0)
			return null;
			
		reroll:
		AudioClip clip = state.AudioClips[ Random.Range(0, state.AudioClips.Count) ];
		
		if(clip == null)
			return null;
			
		if(clip == null)
			return null;
			
		// if the animation was the last one played, reroll for another animation
		if (clip == state.LastAudioClip && state.AudioClips.Count > 1)
			goto reroll;
			
		if(state.AudioProbability > 0)
			if((float)Random.Range(0f, 1f) > state.AudioProbability)
				return null;
		
		return clip;
		
	}
	
}
