/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIEventHandler.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class handles communication between all the behaviours that
//					make up an AI character. it declares all events that will be
//					available to objects in the AIs hierarchy, and binds several
//					of the object component states to AI activity events
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;
using System.Collections.Generic;

public class vp_AIEventHandler : vp_AIStateEventHandler
{

	// these declarations determine which events are supported by the
	// player event handler. it is then up to external classes to fill
	// them up with delegates for communication.

	// TIPS:
	//  1) mouse-over on the event types (e.g. vp_Message) for usage info.
	//  2) to find the places where an event is SENT, you can do 'Find All
	// References' on the event in your IDE. if this is not available, you
	// can search the project for the event name preceded by '.' (.Reload)
	//  3) to find the methods that LISTEN to an event, search the project
	// for its name preceded by '_' (_Reload)
	
	// properties
	public vp_Value<float> Health;
	public vp_Value<Transform> Target;
	public vp_Value<Transform> HostileTarget;
	public vp_Value<Transform> TrackedTarget;
	public vp_Value<object> TargetProperties;
	public vp_Value<Transform> SpawnPoint;
	
	// pathfinding properties
	public vp_Value<bool> Pathfinding;
	public vp_Value<Vector3> TargetPosition;
	public vp_Value<Vector3> TargetDirection;
	
	// inventory properties
	public vp_Value<int> CurrentAmmoCount;
	
	// movement properties
	public vp_Value<float> SlowdownDistance;
	public vp_Value<float> EndReachedDistance;
	public vp_Value<float> MovementSpeed;
	
	// ai activities
	public vp_Activity Dead;
	public vp_Activity Idle;
	public vp_Activity Walk;
	public vp_Activity Run;
	public vp_Activity Roaming;
	public vp_Activity Alerted;
	public vp_Activity Attack;
	public vp_Activity<Transform> Attacking;
	public vp_Activity Damaged;
	public vp_Activity Reload;
	public vp_Activity Retreat;
	public vp_Activity<List<object>> Stop;
	
	// inventory
	public vp_Attempt DepleteAmmo;
	
	// misc
	public vp_Message<Vector3> Reset;
	public vp_Message TargetReached;
	public vp_Message ReturnToSpawn;
	public vp_Message ReturnToLastWaypoint;
	public vp_Message<float> TakeDamage;
	public vp_Message<Dictionary<string, object>> CauseDamage;
	public vp_Message<object, bool> Animate;
	public vp_Message<object, bool> PlaySound;
	public vp_Message<object, AudioClip> GetAudioClipForState;
	public vp_Message<Dictionary<string, object>> SetProperties;
	public vp_Message<bool, Vector3> PositionOnGround;
	public vp_Message<GameObject, bool> IsValidTarget;
	public vp_Message AlertFriendly;
	public vp_Message Respawn;


	/// <summary>
	/// 
	/// </summary>
	protected override void Awake()
	{

		base.Awake();

		// TIP: please see the manual for the difference
		// between (player) activities and (component) states

		// --- activity state bindings ---
		// whenever these activities are toggled they will enable and
		// disable any component states with the same names. disable
		// these lines to make states independent of activities
		BindStateToActivity(Idle);
		BindStateToActivity(Roaming);
		BindStateToActivity(Run);
		BindStateToActivity(Walk);
		BindStateToActivity(Dead);
		BindStateToActivity(Damaged);
		BindStateToActivity(Retreat);
		BindStateToActivity(Alerted);
		BindStateToActivity(Reload);
		BindStateToActivity(Stop);
		BindStateToActivity(Attacking);
		BindStateToActivity(Attack);	// <--
		// in this default setup the 'Attack' activity will enable
		// - but not disable - the component attack states when toggled.
		
		Stop.AutoDuration = 0;

	}

}

