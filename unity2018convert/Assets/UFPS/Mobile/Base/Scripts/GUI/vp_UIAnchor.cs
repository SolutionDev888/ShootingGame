/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIAnchor.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This class will anchor a game object to the specified side
//					and it's based on the resolution set on vp_UIManager. This
//					is helpful when trying to create resolution independence.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class vp_UIAnchor : MonoBehaviour
{

	public enum vp_UIAnchorSide
	{
		Middle,
		TopLeft,
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		BottomLeft,
		Left
	}
	
	
	public vp_UIManager Manager = null;								// cached UI Manager
	public vp_UIAnchorSide Side = vp_UIAnchorSide.Middle;			// side of which to anchor to
	public List<vp_UIControl> Controls = new List<vp_UIControl>();	// list of child vp_UIControls
	
	protected Transform m_Transform = null;							// cached transform
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{
	
		Init();
	
	}
	
	
	/// <summary>
	/// Makes sure all the necessary properties are set
	/// </summary>
	protected virtual void Init()
	{
	
		m_Transform = transform;
	
		if(Manager == null)
			Manager = m_Transform.root.GetComponent<vp_UIManager>();
			
		if(Controls == null || Controls.Count == 0 || Application.isPlaying)
			Controls = m_Transform.ChildComponentsToList<vp_UIControl>();
		
	}
	
	
	/// <summary>
	/// Anchors this GameObject to the specified side
	/// </summary>
	public virtual void RefreshUI()
	{
	
		Init();
		
		// get the bounds based on the UI Managers resolution
		float x = (Manager.RelativeResolution.x / Manager.RelativeResolution.y) * 2;
		Bounds newBounds = new Bounds(Manager.UICamera.transform.position, new Vector3(x, 2, 0));
		
		Vector3 newPos = new Vector3(0, 0, 0);
		if(Side == vp_UIAnchorSide.BottomRight || Side == vp_UIAnchorSide.Right || Side == vp_UIAnchorSide.TopRight) newPos.x = newBounds.max.x;
		else if(Side == vp_UIAnchorSide.Top || Side == vp_UIAnchorSide.Bottom || Side == vp_UIAnchorSide.Middle) newPos.x = newBounds.center.x;
		else newPos.x = newBounds.min.x;
			
		if(Side == vp_UIAnchorSide.Top || Side == vp_UIAnchorSide.TopLeft || Side == vp_UIAnchorSide.TopRight) newPos.y = newBounds.max.y;
		else if(Side == vp_UIAnchorSide.Left || Side == vp_UIAnchorSide.Middle || Side == vp_UIAnchorSide.Right) newPos.y = newBounds.center.y;
		else newPos.y = newBounds.min.y;
			
		m_Transform.position = newPos;
			
		m_Transform.localScale = Vector3.one;
		m_Transform.position = vp_UIManager.PixelPerfect(transform.position);
	
	}
	
	
	/// <summary>
	/// constantly refreshes this component while game is not playing
	/// </summary>
	protected virtual void Update()
	{
	
		if(Manager == null || Application.isPlaying)
			return;
			
		Manager.ForceUIRefresh();
	
	}
	
}
