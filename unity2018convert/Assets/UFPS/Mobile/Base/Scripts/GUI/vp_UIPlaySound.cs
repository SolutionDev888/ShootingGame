/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIPlaySound.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	a simple implementation that uses OnPressControl and
//					OnReleaseControl events to play a sound when a vp_UI control
//					receives interaction
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class vp_UIPlaySound : MonoBehaviour
{

	// enumeration of possible events
	public enum vp_UIPlaySoundTrigger{
		OnPress,
		OnRelease
	}

	public AudioClip Sound = null;	// sound to play
	public vp_UIPlaySoundTrigger Trigger = vp_UIPlaySoundTrigger.OnPress;	// the event that will cause the sound to play
	public float Volume = 1;		// volume of the sound
	
	protected vp_UIManager m_Manager = null;	// cached vp_UIManager
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{
	
		m_Manager = transform.root.GetComponent<vp_UIManager>();
	
	}
	

	/// <summary>
	/// Event that is fired when this control
	/// recieves a press
	/// </summary>
	protected virtual void OnPressControl()
	{
	
		if(Trigger != vp_UIPlaySoundTrigger.OnPress)
			return;
			
		PlaySound();
	
	}
	
	
	/// <summary>
	/// Event that is fired when this control
	/// is released
	/// </summary>
	protected virtual void OnReleaseControl()
	{
	
		if(Trigger != vp_UIPlaySoundTrigger.OnRelease)
			return;
			
		PlaySound();
	
	}
	
	
	/// <summary>
	/// plays the sound
	/// </summary>
	protected virtual void PlaySound()
	{
	
		if(Sound == null)
			return;
			
		if(m_Manager == null)
			return;

		if(m_Manager.AudioSource != null)
        {
            if (LevelSetting.isSound)
                m_Manager.AudioSource.mute = false;
            else
                m_Manager.AudioSource.mute = true;
            m_Manager.AudioSource.PlayOneShot(Sound);
        }
			
	
	}
	
}
