/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIMovement.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class handles moving the AI around the world
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[System.Serializable]
public class vp_AIMovement : vp_AIPlugin
{

	public override string Title{ get{ return "Movement"; } }
	public override int SortOrder{ get{ return 300; } }
	
	protected vp_Timer.Handle m_MoveAgainTimer = new vp_Timer.Handle();
	protected vp_Timer.Handle m_RoamingTimer = new vp_Timer.Handle();
	protected vp_Timer.Handle m_WaitingForPathTimer = new vp_Timer.Handle();
	protected vp_Timer.Handle m_RetreatTimer = new vp_Timer.Handle();
	protected vp_Timer.Handle m_StationaryTimer = new vp_Timer.Handle();
	protected float m_RetreatStartTimer = 0;
	protected bool m_Pathfinding = false;
	protected Vector3 m_TargetDirection = Vector3.zero;
	protected Vector3 m_TargetPosition = Vector3.zero;
	protected List<Transform> m_Waypoints = new List<Transform>();
	protected List<Transform> m_OldWaypoints = new List<Transform>();
	protected float m_MaxHealth = -1;
	protected bool m_CanRetreat = false;
	protected float m_NextStationaryCheckTime = 0;
	protected Vector3 m_LastPosition = Vector3.zero;
	
	protected float m_CurrentHealth{
		get{ return (m_EventHandler.Health.Get() - 0f) / (m_MaxHealth - 0f); }
	}
	
	/// <summary>
	/// used by run and walk to see if movement
	/// can take place
	/// </summary>
	protected virtual bool m_CanMove
	{
	
		get{
			List<vp_Activity> activities = new List<vp_Activity>(){ m_EventHandler.Dead, m_EventHandler.Damaged, m_EventHandler.Attack, m_EventHandler.Reload, m_EventHandler.Alerted };
			foreach(vp_Activity activity in activities)
				if(activity.Active)
					return false;
			
			if(m_EventHandler.TrackedTarget.Get() == null)
				return false;
			
			return true;
		}
	
	}
	
	
	/// <summary>
	/// in 'Awake' we do things that need to be run once at the
	/// very beginning. NOTE: 1) this method must be run using
	/// 'base.Awake();' on the first line of the 'Awake' method
	/// in any derived class. 2) keep in mind that as of Unity 4,
	/// gameobject hierarchy can not be altered in 'Awake'
	/// </summary>
	public override void Awake(vp_AI ai)
	{
	
		base.Awake(ai);
		
		SetupWaypoints(true);
	
	}
	
	
	/// <summary>
	/// in 'Start' we do things that need to be run once at the
	/// beginning, but potentially depend on all other scripts
	/// first having run their 'Awake' calls.
	/// NOTE: 1) don't do anything here that depends on activity
	/// in other 'Start' calls. 2) if adding code here, remember
	/// to call it using 'base.Start();' on the first line of
	/// the 'Start' method in the derived classes
	/// </summary>
	public override void Start()
	{
	
		base.Start();
	
		m_AI.MovementRoamingTarget = new GameObject("RoamingTarget").transform;
		m_AI.MovementRoamingTarget.gameObject.layer = m_GameObject.layer;
		vp_Timer.In(0.01f, delegate {
			m_AI.MovementRoamingTarget.parent = m_Transform.parent;
			m_AI.MovementRoamingTarget.position = m_Transform.position;
		});
		
		m_MaxHealth = m_EventHandler.Health.Get();
		
		vp_Timer.In(.1f, delegate {
			m_EventHandler.Animate.Send(m_AI.MovementIdleState);
		});
	
	}
	
	
	/// <summary>
	/// get the children from the WaypointsParent and
	/// add them to the waypoints list
	/// </summary>
	protected virtual void SetupWaypoints( bool setup = false )
	{
	
		if(!m_AI.MovementAllowRoaming)
			return;
			
		if(m_AI.MovementWaypointsParent == null)
			return;
			
		if(m_Waypoints.Except(m_OldWaypoints).ToList().Count == 0 && !setup)
			return;
		
		m_Waypoints.Clear();
		m_OldWaypoints.Clear();
			
		foreach(Transform t in m_AI.MovementWaypointsParent)
			m_Waypoints.Add(t);
			
		m_OldWaypoints = m_Waypoints;
	
	}
	

	/// <summary>
	/// NOTE: to provide the 'Init' functionality, this method
	/// must be called using 'base.Update();' on the first line
	/// of the 'Update' method in the derived class
	/// </summary>
	public override void Update()
	{
	
		base.Update();
	
		if(m_EventHandler.Attack.Active && m_EventHandler.TrackedTarget.Get() != null)
		{
			m_TargetDirection = m_EventHandler.TrackedTarget.Get().position - m_Transform.position;
			RotateTowards(m_TargetDirection);
		}
	
		MoveUpdate();
		
		RetreatUpdate();
	
	}


	/// <summary>
	/// moves the AI and plays an animation
	/// </summary>
	public virtual void MoveUpdate()
	{
	
		if(m_StationaryTimer.Active)
			return;
		
		if(!m_EventHandler.Walk.Active && !m_EventHandler.Run.Active)
		{
			if(m_Controller != null)
				m_Controller.SimpleMove(Vector3.zero);
			else if (m_Rigidbody != null && m_Controller == null)
				m_Rigidbody.velocity = Vector3.zero;
				
			return;
		}
			
		if(m_EventHandler.Damaged.Active)
			return;
			
		//Rotate towards targetDirection
		if(m_TargetDirection != Vector3.zero)
			RotateTowards(m_TargetDirection);
	
		if (m_Controller != null)
			m_Controller.SimpleMove(m_TargetPosition);
		else if (m_Rigidbody != null)
			m_Rigidbody.AddForce(m_TargetPosition);
		else
			m_Transform.Translate(m_TargetPosition*Time.deltaTime, Space.World);
		
		// play an animation if applicable
		if(m_TargetPosition != Vector3.zero)
			m_EventHandler.Animate.Send( m_EventHandler.Run.Active ? m_AI.MovementRunState : m_AI.MovementWalkState );
			
		// setup movement if no pathfinding
		NoPathfindingMovement();
		
		CheckStationary();
		
	}
	
	
	/// <summary>
	/// checks if AI is in a stationary state
	/// and stops all activities if so.
	/// This is basically brute force stuck prevention.
	/// </summary>
	protected virtual void CheckStationary()
	{
	
		if(m_EventHandler.Attack.Active)
			return;
	
		if(m_StationaryTimer.Active)
			return;
	
		if(Time.time < m_NextStationaryCheckTime)
			return;
			
		m_NextStationaryCheckTime = Time.time + m_AI.MovementStationaryCheckRate;
			
		if(m_Controller != null)	
			if(m_Controller.velocity.magnitude > 1)
				return;
			else
				m_EventHandler.Animate.Send( m_AI.MovementIdleState );
			
		m_EventHandler.Stop.Start();
		m_EventHandler.Idle.Start();
		vp_Timer.In(m_AI.MovementStationaryTimeout, delegate() {
			m_EventHandler.Roaming.Start();
		}, m_StationaryTimer);
	
	}
	
	
	/// <summary>
	/// Checks to see if AI should retreat or not
	/// </summary>
	protected virtual void RetreatUpdate()
	{
	
		if(!m_AI.MovementAllowRetreat)
			return;
			
		if(m_EventHandler.HostileTarget.Get() == null)
			return;
			
		if(vp_AIManager.GetPlayer(m_EventHandler, m_EventHandler.HostileTarget.Get()) == null)
			return;
			
		if(m_CurrentHealth <= m_AI.MovementRetreatHealthThreshold)
			m_EventHandler.Retreat.Start();
			
		if(Vector3.Distance(m_Transform.position, m_EventHandler.TrackedTarget.Get().position) < m_AI.MovementRetreatDistFromTarget.x && !m_CanRetreat)
			m_CanRetreat = true;
			
		if(Vector3.Distance( m_EventHandler.TrackedTarget.Get().position, m_Transform.position ) > m_AI.MovementRetreatDistFromTarget.y)
			m_EventHandler.Retreat.TryStart();
	
	}
	
	
	/// <summary>
	/// if no pathfinding is setup, this will move
	/// the AI instead of the pathfinding
	/// </summary>
	protected virtual void NoPathfindingMovement()
	{
	
		if(m_Pathfinding)
			return;
		
		Vector3 dir = m_EventHandler.TrackedTarget.Get().position - m_Transform.position;
		dir.y = 0;
		float targetDist = dir.magnitude;
		
		float slowdown = Mathf.Clamp01 (targetDist / m_AI.MovementSlowdownDistance);
		Vector3 forward = m_Transform.forward;
		float dot = Vector3.Dot (dir.normalized,forward);
		float sp = m_EventHandler.MovementSpeed.Get() * Mathf.Max (dot,.05f) * slowdown;
		m_TargetPosition = forward*sp;
		m_TargetDirection = dir;
		
		if(targetDist <= m_AI.MovementEndReachedDistance)
			m_EventHandler.TargetReached.Send();
	
	}
	
	
	/// <summary>
	/// Rotates in the specified direction
	/// around the Y-Axis
	/// </summary>
	protected virtual void RotateTowards(Vector3 dir) {
		Quaternion rot = m_Transform.rotation;
		Quaternion toTarget = Quaternion.LookRotation (dir);
		
		float turningSpeed = m_EventHandler.Run.Active ? m_AI.MovementRunTurningSpeed : m_AI.MovementWalkTurningSpeed;
		rot = Quaternion.Slerp (rot,toTarget,turningSpeed*Time.fixedDeltaTime);
		Vector3 euler = rot.eulerAngles;
		euler.z = 0;
		euler.x = 0;
		rot = Quaternion.Euler (euler);
		
		m_Transform.rotation = rot;
	}
	
	
	/// <summary>
	/// Sends the AI back to it's starting position
	/// </summary>
	protected virtual void OnMessage_ReturnToLastWaypoint()
	{
		
		m_EventHandler.Stop.TryStart();
		m_EventHandler.Target.Set(m_AI.MovementRoamingTarget);
		m_EventHandler.Walk.Start();
	
	}


	/// <summary>
	/// prevents the AI from attacking while the StationaryTimer is active
	/// </summary>
	protected virtual bool CanStart_Attacking()
	{
	
		if(m_StationaryTimer.Active)
			return false;
			
		return true;
	
	}
	
	
	/// <summary>
	/// set some stuff for retreating when attack starts
	/// </summary>
	protected virtual void OnStart_Attacking()
	{
	
		if(!m_AI.MovementAllowRetreat)
			return;
			
		m_RetreatStartTimer = Time.time + m_AI.MovementRetreatCanStartDelay;
	
	}
	
	
	/// <summary>
	/// checks to see if retreat can start
	/// </summary>
	protected virtual bool CanStart_Retreat()
	{
	
		if(m_EventHandler.Dead.Active)
			return false;
	
		if(!m_AI.MovementAllowRetreat)
			return false;
			
		if(m_EventHandler.Retreat.Active)
			return false;
	
		if(m_EventHandler.Target.Get() == null)
			return false;
			
		if(!m_CanRetreat)
			return false;
			
		if(Time.time < m_RetreatStartTimer)
			return false;
			
		return true;
	
	}
	
	
	/// <summary>
	/// start the retreat
	/// </summary>
	protected virtual void OnStart_Retreat()
	{
	
		if(m_CurrentHealth <= m_AI.MovementRetreatHealthThreshold)
		{
			Vector3 dir = m_EventHandler.Target.Get() != null ? (m_Transform.position-m_EventHandler.Target.Get().position).normalized : -m_Transform.forward;
			Vector3 newPos = m_AI.MovementRoamingTarget.position;
			newPos = m_Transform.position + ((dir * m_AI.MovementRetreatLowHealthDistance) + (m_Transform.up * 500));
			RaycastHit hit;
			if(Physics.Raycast(newPos, Vector3.down, out hit, 1000, vp_Layer.Mask.BulletBlockers))
				newPos = hit.point+new Vector3(0,.1f,0);
			m_AI.MovementRoamingTarget.position = newPos;
		}
		
		m_EventHandler.Stop.TryStart(new List<object>(){ m_EventHandler.Retreat });
		m_EventHandler.Run.Start();
		m_CanRetreat = false;
		m_EventHandler.Target.Set(m_AI.MovementRoamingTarget);
	
	}
	
	
	/// <summary>
	/// if health is below threshold, don't stop
	/// retreat will stop based on the retreat distance if can't stop
	/// </summary>
	protected virtual bool CanStop_Retreat()
	{
	
		if(m_CurrentHealth <= m_AI.MovementRetreatHealthThreshold)
			return false;	
		
		return true;
	
	}
	
	
	/// <summary>
	/// Stop retreating and start roaming
	/// </summary>
	protected virtual void OnStop_Retreat()
	{
	
		m_RetreatTimer.Cancel();
		if(m_CurrentHealth <= m_AI.MovementRetreatHealthThreshold)
		{
			m_EventHandler.Idle.Start();
			vp_Timer.In(Random.Range(m_AI.MovementRoamingWaitTime.x, m_AI.MovementRoamingWaitTime.y+1), delegate() {
				m_EventHandler.Roaming.TryStart();
			}, m_RoamingTimer);
		}
	
	}
	
	
	/// <summary>
	/// stop movement and start roaming
	/// </summary>
	protected virtual void OnMessage_Reset(Vector3 newSpawnPosition)
	{
	
		if (m_Controller != null) {
			m_Controller.SimpleMove(Vector3.zero);
		} else if (m_Rigidbody != null) {
			m_Rigidbody.velocity = Vector3.zero;
		} else {
			m_Transform.Translate(Vector3.zero);
		}
			
		m_CanRetreat = false;
		
		vp_Timer.In(Random.Range(m_AI.MovementRoamingWaitTime.x, m_AI.MovementRoamingWaitTime.y+1), delegate() {
			m_EventHandler.Roaming.TryStart();
		}, m_RoamingTimer);
	
	}
	
	
	/// <summary>
	/// stop timers if AI is told to stop
	/// </summary>
	protected virtual void OnStart_Stop()
	{
	
		object activity = m_EventHandler.Stop.Argument;
	
		m_MoveAgainTimer.Cancel();
		m_RoamingTimer.Cancel();
		m_WaitingForPathTimer.Cancel();
		if(activity != m_EventHandler.Retreat)
			m_RetreatTimer.Cancel();
	
	}
	
	
	/// <summary>
	/// stops all movement and starts the idle activity
	/// </summary>
	protected virtual void OnStart_Idle()
	{
		
		if(m_Rigidbody != null)
			m_Rigidbody.WakeUp();
			
		m_EventHandler.Run.Stop();
		m_EventHandler.Walk.Stop();
		m_EventHandler.Animate.Send(m_AI.MovementIdleState);
		
	}
	
	
	/// <summary>
	/// Determines if roaming can be started
	/// </summary>
	protected virtual bool CanStart_Roaming()
	{
	
		if(m_EventHandler.Dead.Active)
			return false;
		
		if(m_EventHandler.Attacking.Active)
			return false;
		
		if(!m_AI.MovementAllowRoaming)
			return false;
			
		if(m_EventHandler.Alerted.Active)
			return false;
		
		return true;
		
	}
	
	
	/// <summary>
	/// stop running, get a new waypoint
	/// and start moving towards that point
	/// </summary>
	protected virtual void OnStart_Roaming()
	{
	
		m_EventHandler.Run.Stop();
		
		// reset the waypoints if necessary
		SetupWaypoints();
		
		Vector3 tempPosition = Vector3.zero;
		
		reroll:
		
		// if waypoints, get a waypoint
		if(m_Waypoints.Count > 0)
			tempPosition = m_Waypoints[ Random.Range(0, m_Waypoints.Count) ].position;
		// if no waypoints, get a random point within the roaming distance threshold
		else
		{
			float modifier = Random.Range(m_AI.MovementRoamingDistance.x, m_AI.MovementRoamingDistance.y);
			tempPosition = new Vector3(Random.Range(m_Transform.position.x-modifier,m_Transform.position.x+modifier), m_Transform.position.y+50, Random.Range(m_Transform.position.z-modifier,m_Transform.position.z+modifier));
		}
		
		// if the distance is too close to the player, reroll
		if(Vector3.Distance(tempPosition, m_Transform.position) < m_AI.MovementRoamingDistance.x * .5f)
			goto reroll;
			
		// position the next waypoint on the ground
		RaycastHit hit;
		if(Physics.Raycast(tempPosition, Vector3.down, out hit, 500, vp_Layer.Mask.BulletBlockers))
			tempPosition = hit.point+new Vector3(0,.1f,0);
			
		// set the new waypoint position
		m_AI.MovementRoamingTarget.position = tempPosition;
		
		// set the target
		m_EventHandler.Target.Set(m_AI.MovementRoamingTarget);
		
		// start moving toward the new waypoint
		m_EventHandler.Walk.Start();
		
	}
	
	
	/// <summary>
	/// stop the timers that roaming uses
	/// </summary>
	protected virtual void OnStop_Roaming()
	{
		
		m_RoamingTimer.Cancel();
		m_WaitingForPathTimer.Cancel();
		
	}
	
	
	/// <summary>
	/// if roaming and target was reached,
	/// stop roaming and wait to roam again
	/// </summary>
	protected virtual void OnMessage_TargetReached()
	{
	
		if(m_EventHandler.Attacking.Active)
			return;
			
		if(m_EventHandler.Attack.Active)
			return;
	
		m_EventHandler.Retreat.Stop();
		m_EventHandler.Roaming.TryStop();
		m_EventHandler.Idle.Start();
		
		m_RoamingTimer.Cancel();
		vp_Timer.In(Random.Range(m_AI.MovementRoamingWaitTime.x, m_AI.MovementRoamingWaitTime.y+1), delegate() {
			m_EventHandler.Roaming.TryStart();
		}, m_RoamingTimer);
	
	}
	
	
	/// <summary>
	/// Determines if running can be started
	/// </summary>
	protected virtual bool CanStart_Run()
	{
		
		return m_CanMove;
		
	}
	
	
	/// <summary>
	/// stop attacking, idle, and walking
	/// when running starts
	/// </summary>
	protected virtual void OnStart_Run()
	{
		
		m_NextStationaryCheckTime = Time.time + m_AI.MovementStationaryCheckRate;
		m_EventHandler.Idle.Stop();
		m_EventHandler.Attack.Stop();
		m_EventHandler.Walk.Stop();
	
	}
	
	
	/// <summary>
	/// Determines if walking can be started
	/// </summary>
	protected virtual bool CanStart_Walk()
	{
		
		return m_CanMove;
		
	}
	
	
	/// <summary>
	/// stop running, idle and attack if walking starts
	/// </summary>
	protected virtual void OnStart_Walk()
	{
	
		m_NextStationaryCheckTime = Time.time + m_AI.MovementStationaryCheckRate;
		m_EventHandler.Idle.Stop();
		m_EventHandler.Attack.Stop();
		m_EventHandler.Run.Stop();
	
	}
	
	
	/// <summary>
	/// stop the moving time when reloading
	/// </summary>
	protected virtual void OnStart_Reload()
	{
	
		m_MoveAgainTimer.Cancel();
	
	}
	
	
	/// <summary>
	/// stop the roaming timers on death
	/// </summary>
	protected virtual void OnStart_Dead()
	{
		
		m_RoamingTimer.Cancel();
		m_WaitingForPathTimer.Cancel();
		
	}
	
	
	/// <summary>
	/// receives a dictionary with properties and overrides
	/// those properties in this class if they exist
	/// </summary>
	protected virtual void OnMessage_SetProperties( Dictionary<string, object> properties )
	{
	
		foreach(string k in properties.Keys)
		{
			FieldInfo p = this.GetType().GetField(k);
			if(p != null)
				p.SetValue(this, properties[k]);
		}
		
	}
	
	
	protected virtual float OnValue_MovementSpeed
	{
	
		get{ return m_EventHandler.Run.Active ? m_AI.MovementRunSpeed : m_AI.MovementWalkSpeed; }
	
	}
	
	
	protected virtual bool OnValue_Pathfinding
	{
	
		get{ return m_Pathfinding; }
		set{ m_Pathfinding = value; }
	
	}
	
	
	protected virtual Vector3 OnValue_TargetDirection
	{
	
		get{ return m_TargetDirection; }
		set{ m_TargetDirection = value; }
	
	}
	
	
	protected virtual Vector3 OnValue_TargetPosition
	{
	
		get{ return m_TargetPosition; }
		set{ m_TargetPosition = value; }
	
	}
	
	
	protected virtual float OnValue_SlowdownDistance
	{
	
		get{ return m_AI.MovementSlowdownDistance; }
		set{ m_AI.MovementSlowdownDistance = value; }
	
	}
	
	
	protected virtual float OnValue_EndReachedDistance
	{
	
		get{ return m_AI.MovementEndReachedDistance; }
		set{ m_AI.MovementEndReachedDistance = value; }
	
	}
	
}
