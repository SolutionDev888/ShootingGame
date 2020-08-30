/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIAStarPathfinding.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class integrates Aron Granberg's excellent A*Pathfinding
//					Project with UFPS. Much of this class was written with the
//					help of the AIPath.cs script provided with the A*Pathfinding
//					Project package.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Reflection;

[System.Serializable]
public class vp_AIAStarPathfinding : vp_AIPlugin
{

	public override string Title{ get{ return "A*Pathfinding"; } }
	public override int SortOrder{ get{ return 200; } }

	protected float m_MinMoveScale = 0.05F;
	protected Seeker m_Seeker;					// Cached Seeker component
	private float m_LastRepath = -9999;			// Time when the last path request was sent
	protected Path m_Path;						// Current path which is followed
	protected int m_CurrentWaypointIndex = 0;	// Current index in the path which is current target
	protected bool m_TargetReached = false;		// Holds if the end-of-path is reached
	protected bool m_CanSearchAgain = true;		// Only when the previous path has been returned should be search for a new path
	private bool waitingForRepath = false;		// Is WaitForRepath running
	protected Vector3 m_TargetDirection;		// Relative direction to where the AI is heading.
	protected vp_Timer.Handle m_WaitForPathTimer = new vp_Timer.Handle();
	protected vp_Timer.Handle m_RepeatTrySearchPathTimer = new vp_Timer.Handle();
	protected bool m_ShouldRepath = false;
	
	// cached components
	protected virtual Vector3 m_FeetPosition
	{
		get{ return m_Controller == null ? m_Transform.position : m_Transform.position - Vector3.up*m_Controller.height*0.5F; }
	}
	
	
	// Returns if the end-of-path has been reached
	public bool TargetReached {
		get { return m_TargetReached; }
	}
	
	
	/// <summary>
	/// in 'Awake' we do things that need to be run once at the
	/// very beginning. NOTE: as of Unity 4, gameobject hierarchy
	/// can not be altered in 'Awake'
	/// </summary>
	public override void Awake(vp_AI ai)
	{
		
		base.Awake(ai);
		
		m_Seeker = m_EventHandler.GetComponent<Seeker>();
		
		//Make sure we receive callbacks when paths complete
		m_Seeker.pathCallback += OnPathComplete;

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
		
		m_EventHandler.Pathfinding.Set(true);
	
	}
	
	
	/// <summary>
	/// NOTE: to provide the 'Init' functionality, this method
	/// must be called using 'base.Update();' on the first line
	/// of the 'Update' method in the derived class
	/// </summary>
	public override void Update()
	{
		
		base.Update();
		
		RepeatTrySearchPath();
		
		Move();
	
	}
	
	
	/// <summary>
	/// set the target position for the movement class
	/// </summary>
	protected virtual void Move()
	{
	
		if(!m_EventHandler.Run.Active && !m_EventHandler.Walk.Active)
			return;
	
		m_EventHandler.TargetPosition.Set( CalculateVelocity( m_FeetPosition ) );
	
	}
	

	/// <summary>
	/// Tries to search for a path every #repathRate seconds.
	/// \see TrySearchPath
	/// </summary>
	public void RepeatTrySearchPath()
	{
	
		if(!m_ShouldRepath)
			return;	
		
		if(m_RepeatTrySearchPathTimer.Active)
			return;
				
		vp_Timer.In(m_AI.AStarRepathRate, delegate() {
			TrySearchPath();
		}, m_RepeatTrySearchPathTimer);
		
	}
	
	
	/// <summary>
	/// Tries to search for a path.
	/// Will search for a new path if there was a sufficient time since the last repath and both
	/// #canSearchAgain and #canSearch are true.
	/// Otherwise will start WaitForPath function.
	/// </summary>
	public void TrySearchPath()
	{
	
		if (Time.time - m_LastRepath >= m_AI.AStarRepathRate && m_CanSearchAgain && m_AI.AStarCanSearch) {
			SearchPath();
		} else {
			if(!m_WaitForPathTimer.Active)
				WaitForRepath();
		}
		
	}
	
	
	/// <summary>
	/// Wait a short time til Time.time-lastRepath >= repathRate.
	/// Then call TrySearchPath
	/// \see TrySearchPath
	/// </summary>
	protected void WaitForRepath()
	{
	
		if(waitingForRepath)
			return;
	
		waitingForRepath = true;
		
		vp_Timer.In(m_AI.AStarRepathRate - (Time.time-m_LastRepath), delegate(){
			waitingForRepath = false;
			//Try to search for a path again
			TrySearchPath ();
		}, m_WaitForPathTimer);
		
	}
	
	
	/// <summary>
	/// Requests a path to the target
	/// </summary>
	public virtual void SearchPath ()
	{
	
		if(m_EventHandler.Dead.Active)
			return;
		
		if(m_EventHandler.TrackedTarget.Get() == null)
		{
			//Debug.LogWarning("Target is null, aborting all search");
			m_AI.AStarCanSearch = false;
			return;
		}
		
		m_LastRepath = Time.time;
		//This is where we should search to
		Vector3 targetPosition = m_EventHandler.Attacking.Active ? m_EventHandler.HostileTarget.Get().position : m_EventHandler.Target.Get().position;
		
		m_CanSearchAgain = false;
		
		//Alternative way of requesting the path
		//Path p = PathPool<Path>.GetPath().Setup(GetFeetPosition(),targetPoint,null);
		//seeker.StartPath (p);
		
		//We should search from the current position
		m_Seeker.StartPath (m_FeetPosition, targetPosition);
		
	}
	
	
	/// <summary>
	/// sends the TargetReached event when
	/// the target point has been reached
	/// </summary>
	public virtual void OnTargetReached ()
	{
	
		m_EventHandler.TargetReached.Send();

	}

	
	/// <summary>
	/// 
	/// </summary>
	public override void OnDestroy ()
	{
	
		if (m_Path != null)
			m_Path.Release (this);
			
	}
	
	
	/// <summary>
	/// Called when a requested path has finished calculation.
	/// A path is first requested by #SearchPath, it is then calculated, probably in the same or the next frame.
	/// Finally it is returned to the seeker which forwards it to this function.\n
	/// </summary>
	public virtual void OnPathComplete(Path _p)
	{
	
		ABPath p = _p as ABPath;
		if (p == null) throw new System.Exception ("This function only handles ABPaths, do not use special path types");
		
		//Release the previous path
		if (m_Path != null) m_Path.Release (this);
		
		//Claim the new path
		p.Claim (this);
		
		//Replace the old path
		m_Path = p;
		
		//Reset some variables
		m_CurrentWaypointIndex = 0;
		m_TargetReached = false;
		m_CanSearchAgain = true;
		
		//The next row can be used to find out if the path could be found or not
		//If it couldn't (error == true), then a message has probably been logged to the console
		//however it can also be got using p.errorLog
		//if (!p.error) m_AI.HasPath.Set(true);
		
		if (m_AI.AStarClosestOnPathCheck) {
			Vector3 p1 = p.startPoint;
			Vector3 p2 = m_FeetPosition;
			float magn = Vector3.Distance (p1,p2);
			Vector3 dir = p2-p1;
			dir /= magn;
			int steps = (int)(magn/m_AI.AStarPickNextWaypointDist);
			for (int i=0;i<steps;i++) {
				CalculateVelocity(p1);
				p1 += dir;
			}
		}
		
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected float XZSqrMagnitude (Vector3 a, Vector3 b)
	{
	
		float dx = b.x-a.x;
		float dz = b.z-a.z;
		return dx*dx + dz*dz;
		
	}
	
	
	/// <summary>
	///  Calculates desired velocity.
	/// Finds the target path segment and returns the forward direction,
	/// scaled with speed. A whole bunch of restrictions on the velocity
	/// is applied to make sure it doesn't overshoot, does not look too
	/// far ahead, and slows down when close to the target.
	/// /see speed
	/// /see endReachedDistance
	/// /see slowdownDistance
	/// /see CalculateTargetPoint
	/// /see targetPoint
	/// /see targetDirection
	/// /see currentWaypointIndex
	/// </summary>
	protected virtual Vector3 CalculateVelocity(Vector3 currentPosition)
	{
	
		if (m_Path == null || m_Path.vectorPath == null || m_Path.vectorPath.Count == 0) return Vector3.zero; 
		
		List<Vector3> vPath = m_Path.vectorPath;
		//Vector3 currentPosition = GetFeetPosition();
		
		if (vPath.Count == 1) {
			vPath.Insert (0,currentPosition);
		}
		
		if (m_CurrentWaypointIndex >= vPath.Count) { m_CurrentWaypointIndex = vPath.Count-1; }
		
		if (m_CurrentWaypointIndex <= 1) m_CurrentWaypointIndex = 1;
		
		while (true) {
			if (m_CurrentWaypointIndex < vPath.Count-1) {
				//There is a "next path segment"
				float dist = XZSqrMagnitude (vPath[m_CurrentWaypointIndex], currentPosition);
					//Mathfx.DistancePointSegmentStrict (vPath[currentWaypointIndex+1],vPath[currentWaypointIndex+2],currentPosition);
				if (dist < m_AI.AStarPickNextWaypointDist*m_AI.AStarPickNextWaypointDist) {
					m_CurrentWaypointIndex++;
				} else {
					break;
				}
			} else {
				break;
			}
		}
		
		Vector3 dir = vPath[m_CurrentWaypointIndex] - vPath[m_CurrentWaypointIndex-1];
		Vector3 targetPosition = CalculateTargetPoint (currentPosition,vPath[m_CurrentWaypointIndex-1] , vPath[m_CurrentWaypointIndex]);
			//vPath[currentWaypointIndex] + Vector3.ClampMagnitude (dir,forwardLook);
		
		
		
		dir = targetPosition-currentPosition;
		dir.y = 0;
		float targetDist = dir.magnitude;
		
		float slowdown = Mathf.Clamp01 (targetDist / m_EventHandler.SlowdownDistance.Get());
		
		m_EventHandler.TargetDirection.Set(dir);
		
		if (m_CurrentWaypointIndex == vPath.Count-1 && targetDist <= m_EventHandler.EndReachedDistance.Get()) {
			if (!m_TargetReached) { m_TargetReached = true; OnTargetReached (); }
			
			//Send a move request, this ensures gravity is applied
			return Vector3.zero;
		}
		
		Vector3 forward = m_Transform.forward;
		float dot = Vector3.Dot (dir.normalized,forward);
		float sp = m_EventHandler.MovementSpeed.Get() * Mathf.Max (dot,m_MinMoveScale) * slowdown;
		
		
		if (Time.deltaTime	> 0) {
			sp = Mathf.Clamp (sp,0,targetDist/(Time.deltaTime*2));
		}
		return forward*sp;
		
	}
	
	
	/// <summary>
	///  Calculates target point from the current line segment.
	/// \param p Current position
	/// \param a Line segment start
	/// \param b Line segment end
	/// The returned point will lie somewhere on the line segment.
	/// \see #forwardLook
	/// </summary>
	protected Vector3 CalculateTargetPoint(Vector3 p, Vector3 a, Vector3 b)
	{
	
		a.y = p.y;
		b.y = p.y;
		
		float magn = (a-b).magnitude;
		if (magn == 0) return a;
		
		float closest = Mathfx.Clamp01 (Mathfx.NearestPointFactor (a, b, p));
		Vector3 point = (b-a)*closest + a;
		float distance = (point-p).magnitude;
		
		float lookAhead = Mathf.Clamp (m_AI.AStarForwardLook - distance, 0.0F, m_AI.AStarForwardLook);
		
		float offset = lookAhead / magn;
		offset = Mathf.Clamp (offset+closest,0.0F,1.0F);
		return (b-a)*offset + a;
		
	}
	
	
	/// <summary>
	/// stop trying to get a path on death
	/// </summary>
	protected virtual void OnStart_Dead()
	{
	
		m_ShouldRepath = false;
	
	}
	
	
	/// <summary>
	/// reset to initial state
	/// </summary>
	protected virtual void OnMessage_Reset(Vector3 newSpawnPosition)
	{
	
		m_AI.AStarCanSearch = false;
		m_TargetReached = false;
		m_CanSearchAgain = true;
		waitingForRepath = false;
		m_LastRepath = -9999;
		m_CurrentWaypointIndex = 0;
		
		m_RepeatTrySearchPathTimer.Cancel();
		m_ShouldRepath = true;
	
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
	
}
