/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIAStarGraphsManager.cs
//	Â© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class provides methods of updating an A*Pathfinding graph
//					when objects are moved around.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public class vp_AIAStarGraphsManager : MonoBehaviour
{

	public float UpdateGraphRate = 2f;						// rate at which an update should occur
	public float DistanceRequiredForUpdate = 5;				// distance an object is required to move in order for a graph update to fire
	public LayerMask MovableObjectsMask = new LayerMask();	// objects that will trigger a graph update
	public int MaxPerIteration = 25;						// Max objects that'll be updated per graph update iteration

	protected List<CachedMovableObject> m_MovableObjects = new List<CachedMovableObject>();
	protected List<CachedMovableObject> m_NeedToIterate = new List<CachedMovableObject>();
	
	/// <summary>
	///	A class for caching the objects properties
	/// </summary>
	public class CachedMovableObject
	{
	
		public Transform Transform = null;
		public Collider Collider = null;
		public Vector3 LastPosition = Vector3.zero;
		public Bounds LastPositionBounds;
	
	}

	/// <summary>
	/// 
	/// </summary>
	void Start ()
	{
	
		Rigidbody[] rigidbodies = GameObject.FindSceneObjectsOfType(typeof(Rigidbody)) as Rigidbody[];
		foreach(Rigidbody rb in rigidbodies)
			if(rb.GetComponent<Collider>() != null && (MovableObjectsMask.value & 1 << rb.gameObject.layer) != 0)
				m_MovableObjects.Add(new CachedMovableObject(){ Transform = rb.transform, Collider = rb.GetComponent<Collider>(), LastPosition = rb.transform.position });
				
		m_NeedToIterate.AddRange(m_MovableObjects);
				
		StartCoroutine("GraphsUpdate");
				
	}
	
	
	/// <summary>
	/// checks if graph update should occur
	/// based on properties from each managed object
	/// </summary>
	protected virtual IEnumerator GraphsUpdate()
	{
	
		yield return new WaitForSeconds(UpdateGraphRate);
	
		int count = 0;
		for(int i=0;i<m_NeedToIterate.Count;i++)
		{	
			if(count == MaxPerIteration)
				break;
		
			CachedMovableObject obj = m_NeedToIterate[i];
			if(Vector3.Distance(obj.Transform.position, obj.LastPosition) > DistanceRequiredForUpdate)
			{
				obj.LastPosition = obj.Transform.position;
				AstarPath.active.UpdateGraphs(obj.LastPositionBounds);
				obj.LastPositionBounds = obj.Collider.bounds;
				AstarPath.active.UpdateGraphs(obj.Collider.bounds);
				
				yield return new WaitForEndOfFrame();
			}
			
			count++;
			m_NeedToIterate.RemoveAt(i);
			i--;
		}
		
		if(m_NeedToIterate.Count == 0)
			m_NeedToIterate.AddRange(m_MovableObjects);
			
		StopCoroutine("GraphsUpdate");
		StartCoroutine("GraphsUpdate");
	
	}
}
