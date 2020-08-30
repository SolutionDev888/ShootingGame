/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIAnimation.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class enables legacy animation control over the AI
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[System.Serializable]
public class vp_AIAnimation : vp_AIPlugin
{

	public override string Title{ get{ return "Animation States"; } }
	public override int SortOrder{ get{ return 0; } }

	// animation
	public Animation Animation = null; // the animation component used to animate the AI
	
	/// <summary>
	/// 
	/// </summary>
	public override void Start()
	{
	
		base.Start();
		
		Animation.Stop(); //stop any animations that might be set to play automatically
		Animation.wrapMode = WrapMode.Loop; //set all animations to loop by default
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	public override void Update()
	{
	
		foreach(vp_AIState state in m_AI.States)
			if(state.IsAnimationState)
				if(state.StateManager.IsEnabled(state.Name) && state.AutoPlayAnimations)
					m_EventHandler.Animate.Send(state);
	
	}
	
	
	/// <summary>
	/// performs an animation for the AI and returns
	/// whether or not the animation was successfully executed
	/// </summary>
	protected virtual bool OnMessage_Animate( object obj )
	{
	
		if(obj == null)
			return false;
	
		if(obj.GetType() != typeof(vp_AIState))
			return false;
			
		vp_AIState state = (vp_AIState)obj;
		if(state == null)
			return false;
		
		if(state.AnimationClips == null)
			return false;
			
		if(state.AnimationClips.Count == 0)
			return false;
			
		reroll:
		vp_AIState.vp_AnimationState clip = state.AnimationClips[ Random.Range(0, state.AnimationClips.Count) ];
		
		if(clip == null)
			return false;
			
		if(clip.Clip == null)
			return false;
			
		if(Animation.GetClip(clip.Clip.name) == null)
			return false;
			
		// if the animation was the last one played, reroll for another animation
		if (clip == state.LastAnimationClip && state.AnimationClips.Count > 1)
			goto reroll;
			
		Animation[clip.Clip.name].layer = clip.Layer;
		Animation[clip.Clip.name].speed = clip.AnimationSpeed;
		Animation[clip.Clip.name].wrapMode = clip.WrapMode;
		
		state.LastAnimationClip = clip;
		state.LastAnimationClipLength = Animation[clip.Clip.name].length / Animation[clip.Clip.name].speed;
			
		Animation.CrossFade( clip.Clip.name, clip.CrossfadeLength, clip.PlayMode );
		
		return true;
		
	}
	
}
