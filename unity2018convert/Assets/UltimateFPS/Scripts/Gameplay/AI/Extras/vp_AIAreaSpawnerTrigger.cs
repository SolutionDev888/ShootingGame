/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIAreaSpawnerTrigger.cs
//	Â© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class allows a trigger to be set for an area spawner
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_AIAreaSpawnerTrigger : MonoBehaviour
{

	public vp_AIAreaSpawner Spawner = null;					// spawner to use for this trigger
	public LayerMask TriggerMask = 1<<vp_Layer.LocalPlayer;	// mask for checking which layers can trigger this object
	public int SpawnObjectsOnTrigger = 0;					// list of objects from the spawner that will spawn upon trigger
	public List<vp_AIAreaSpawner.AISpawnerObject> SpawnObjectsList = new List<vp_AIAreaSpawner.AISpawnerObject>();
	
	protected bool m_Triggered = false;

	
	/// <summary>
	/// Setup stuff
	/// </summary>
	protected virtual void Awake()
	{
	
		if(GetComponent<Collider>() == null)
		{
			Debug.LogWarning("This component requires a collider. Disabling this component.");
			enabled = false;
			return;
		}
		
		GetComponent<Collider>().isTrigger = true;
		
		if(Spawner == null)
		{
			Debug.LogWarning("A Spawner must be set. Disabling this component.");
			enabled = false;
		}
		
		if(SpawnObjectsList.Count == 0)
		{
			Debug.LogWarning("No Spawn Objects have been selected. Disabling this component.");
			enabled = false;
		}
	
	}
	
	
	/// <summary>
	/// if this trigger hasn't gone off
	/// spawn the provided spawners AI
	/// </summary>
	public virtual void OnTriggerEnter( Collider other )
	{
	
		if(!enabled)
			return;
	
		if((TriggerMask.value & 1<<other.gameObject.layer) == 0)
			return;
			
		if(m_Triggered)
			return;
			
		m_Triggered = true;
		
		for(int i=0; i<Spawner.AISpawnerObjects.Count; i++)
			if(Spawner.AISpawnerObjects[i].Enabled)
				Spawner.Spawn(i);
	
	}
}
