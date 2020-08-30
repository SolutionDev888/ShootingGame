/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIPlaceOnGround.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class allows the AI to be placed on the ground with an
//					optional offset. This is useful for not needing exact ground
//					placement on start.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

[System.Serializable]
public class vp_AIPlaceOnGround : vp_AIPlugin
{

	public override string Title{ get{ return "Place On Ground"; } }
	public override int SortOrder{ get{ return 600; } }
	
	
	public virtual Vector3 PositionOnGround( bool setPosition )
	{
		
		Vector3 newPos = m_Transform.position;
		
		RaycastHit hit;
		if(Physics.Raycast(newPos, Vector3.down, out hit, 500, m_AI.PlaceOnGroundGroundLayers))
			newPos = hit.point+m_AI.PlaceOnGroundOffset;
			
		if(setPosition)
			m_Transform.position = newPos;
			
		return newPos;
	
	}
	
	
	/// <summary>
	/// optionally positions the AI on the ground while
	/// returning the new ground position
	/// </summary>
	protected virtual Vector3 OnMessage_PositionOnGround( bool setPosition )
	{
	
		return PositionOnGround(setPosition);
	
	}
	
}
