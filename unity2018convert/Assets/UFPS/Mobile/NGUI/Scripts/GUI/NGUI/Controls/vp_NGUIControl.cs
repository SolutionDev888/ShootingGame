/////////////////////////////////////////////////////////////////////////////////
//
//	vp_NGUIControl.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	base class for any NGUI Control that should be managed by
//					NGUI Manager
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public class vp_NGUIControl : vp_UIControl
{	
    
#if UNITY_EDITOR
    public override void OnDrawGizmos(){}
#endif
	
}
