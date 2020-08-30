/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIState.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

[System.Serializable]	// retain instance upon build and save
public class vp_AIState
{
	
	public vp_AIStateManager StateManager = null;
	public string TypeName = null;
	public string Name = null;
	public TextAsset TextAsset = null;
	public vp_AIComponentPreset Preset = null;				// (runtime only)
	public List<int> StatesToBlock;							// a list of states that this state will block when enabled
	
	public bool IsAnimationState = false;					// should be used as an animation state
	public bool IsAudioState = false;						// should be used as an audio state
	
	/// <summary>
	/// A class for storing values for an animation state
	/// </summary>
	[System.Serializable]
	public class vp_AnimationState
	{
		public bool Foldout = true;							// foldout for editor usage
		public AnimationClip Clip = null;					// the animation clip
		public WrapMode WrapMode = WrapMode.Loop;			// wrapmode for the animation in this state
		public float AnimationSpeed = 1;					// speed of this animation
		public float CrossfadeLength = 0.3f;				// length of the crossfade
		public int Layer = 0;								// layer for this animation
		public PlayMode PlayMode = PlayMode.StopSameLayer;	// play mode for this animation
	}
	public bool AutoPlayAnimations = false;					// Should the animation play automatically if this state is enabled
	public bool AnimationFoldout = false;					// used for the editor
	public List<vp_AnimationState> AnimationClips = new List<vp_AnimationState>(){ new vp_AnimationState() }; // list of animations for this state
	public float LastAnimationClipLength = 0;				// cached length of the last animation played for this state
	public vp_AnimationState LastAnimationClip = null;		// cached last animation for this state
	
	// Audio States
	public float AudioProbability = 1;						// probability at which the sound will play. 1 will always play the sound.
	public bool AudioClipsFoldout = false;					// used for the editor
	public List<AudioClip> AudioClips = new List<AudioClip>(){ null }; // list of animations for this state
	public AudioClip LastAudioClip = null;					// caches the last audio clip played
	public float LastTimeAudioPlayed = 0;					// the time audio on this state was last played
	public float AudioPlayInterval = 0;						// interval at which to play audio for this state
	public Vector2 AudioPitch = new Vector2(1.0f, 1.0f);	// random pitch range the sound will play at

	protected bool m_Enabled = false;
	protected List<vp_AIState> m_CurrentlyBlockedBy = null;	// (runtime) a list of states that is currently blocking this state

	
	/// <summary>
	/// represents a snapshot of all or some of a component's properties.
	/// controlled by the state manager, and may be enabled, disabled or
	/// blocked
	/// </summary>
	public vp_AIState(string typeName, string name = "Untitled", string path = null, TextAsset asset = null)
	{

		TypeName = typeName;
		Name = name;
		TextAsset = asset;

	}


	/// <summary>
	/// enables or disables this state and imposes or relaxes
	/// its blocking list, respectively
	/// </summary>
	public bool Enabled
	{
		get { return m_Enabled; }
		set
		{

			m_Enabled = value;

			if (!Application.isPlaying)
				return;

			if (StateManager == null)
				return;

			if (m_Enabled)
				StateManager.ImposeBlockingList(this);
			else
				StateManager.RelaxBlockingList(this);
		}
	}


	/// <summary>
	/// whether this state is currently blocked
	/// </summary>
	public bool Blocked
	{
		get
		{
			return CurrentlyBlockedBy.Count > 0;
		}
	}


	/// <summary>
	/// how many states are currently blocking this state
	/// </summary>
	public int BlockCount
	{
		get
		{
			return CurrentlyBlockedBy.Count;
		}
	}


	/// <summary>
	/// the list of states that are currently blocking this one
	/// </summary>
	protected List<vp_AIState> CurrentlyBlockedBy
	{
		get
		{
			if (m_CurrentlyBlockedBy == null)
				m_CurrentlyBlockedBy = new List<vp_AIState>();
			return m_CurrentlyBlockedBy;
		}
	}


	/// <summary>
	/// adds a state to the list of states that blocks this one
	/// </summary>
	public void AddBlocker(vp_AIState blocker)
	{

		if (!CurrentlyBlockedBy.Contains(blocker))
			CurrentlyBlockedBy.Add(blocker);

	}


	/// <summary>
	/// removes a state from the list of states that blocks this one
	/// </summary>
	public void RemoveBlocker(vp_AIState blocker)
	{

		if (CurrentlyBlockedBy.Contains(blocker))
			CurrentlyBlockedBy.Remove(blocker);

	}


}

