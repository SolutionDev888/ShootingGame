/////////////////////////////////////////////////////////////////////////////////
//
//	vp_NGUIManager.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	manages all UI components that are a child of this gameobject
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_NGUIManager : vp_UIManager
{
	
	/// <summary>
	/// Makes sure all the necessary properies are set
	/// </summary>
	protected override void Init()
	{
	
		if(Transforms == null || Transforms.Count == 0 || Application.isPlaying)
			Transforms = transform.ChildComponentsToList<Transform>();
			
		if(Controls == null || Controls.Count == 0 || Application.isPlaying)
		{
			Controls.Clear();
			List<vp_UIControl> controls = transform.ChildComponentsToList<vp_UIControl>();
			foreach(vp_UIControl control in controls)
				RegisterControl(control);
		}
		
		if(UICamera == null)
			UICamera = vp_UIManager.GetUICamera(transform);
	
	}
	
	
	/// <summary>
	/// No need for this, NGUI has it's own system for this
	/// </summary>
	public override void ForceUIRefresh(){ Init(); }
#if UNITY_EDITOR
	protected override void OnDrawGizmos(){}
    protected override void OnDrawGizmosSelected(){}
#endif
	
}
