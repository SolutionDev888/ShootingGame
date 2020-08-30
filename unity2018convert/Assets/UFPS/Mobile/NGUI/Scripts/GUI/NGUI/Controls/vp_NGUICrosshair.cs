/////////////////////////////////////////////////////////////////////////////////
//
//	vp_NGUICrosshair.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this script manages a NGUI Sprite used as a crosshair
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public class vp_NGUICrosshair : vp_UICrosshair
{

	public Sprite StartingTexture = null;
	
	protected UI2DSprite m_Sprite = null;
	protected Sprite m_CurrentTexture = null;
	
	
	/// <summary>
	/// 
	/// </summary>
	protected override void Awake()
	{
	
		base.Awake();
	
		m_Sprite = GetComponent<UI2DSprite>();
		m_DefaultColor = m_Sprite.color;
		m_CurrentTexture = StartingTexture;
	
	}
	
	
	protected override void HandleCrosshairColor()
	{
	
		if(m_ShowCrosshair)
			vp_NGUITween.ColorTo(m_Sprite, m_CrosshairColor, ColorChangeDuration, m_ColorHandle);
		else
			m_Sprite.color = m_CrosshairColor;
	
	}
	
	
	/// <summary>
	/// Displays the crosshair based on the specified value.
	/// </summary>
	protected override void DisplayCrosshair( bool val = true )
	{
	
		m_ShowCrosshair = val;
	
	}
	
	
	/// <summary>
	/// Gets or sets the value of the Crosshair texture
	/// </summary>
	protected override Sprite OnValue_Crosshair
	{
		get { return m_CurrentTexture; }
		set
		{ 
			if(m_Tracking && EnemyCrosshair != null)
			{
				m_CurrentTexture = EnemyCrosshair;
				m_Sprite.sprite2D = m_CurrentTexture;
			}
			else if(value.name == "")
			{
				m_ShowCrosshair = false;
				m_CurrentTexture = null;
			}
			else
			{
				if(!Manager.Player.Zoom.Active)
					m_ShowCrosshair = true;
				
				m_CurrentTexture = value;
				m_Sprite.sprite2D = m_CurrentTexture;
			}
			
			if(m_Tracking)
				return;
		
			Vector3 localScale = value.name == StartingTexture.name ? m_CachedScale : new Vector3(m_CachedScale.x * InteractIconSize, m_CachedScale.y * InteractIconSize, m_CachedScale.z);
			localScale = Manager.Player.Interactable.Get() != null && Manager.Player.Interactable.Get().GetType() == typeof(vp_Grab) && value.name == "" ? m_CachedScale * (InteractIconSize * 3) : localScale;
			
			vp_UITween.ScaleTo(m_GameObject, vp_UITween.Hash("scale", localScale, "duration", 0, "handle", m_ScaleCrosshairHandle) );
		}
	}

}

