//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using NJG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("NJG MiniMap/Map Zone")]
[ExecuteInEditMode]
[RequireComponent(typeof(SphereCollider))]
public class NJGMapZone : MonoBehaviour 
{
	static public List<NJGMapZone> list = new List<NJGMapZone>();
	//static public int id = 0;

	public Color color { get { return map == null ? Color.white : map.GetZoneColor(level, zone); } }
	public string triggerTag = "Player";
	public string zone;
	public string level;
	public int colliderRadius = 10;
	//public int mId = 0;
	public bool generateOnTrigger;
	public SphereCollider zoneCollider 
	{ 
		get 
		{
			mCollider = gameObject.GetComponent<SphereCollider>();

			if (mCollider == null) 
				mCollider = gameObject.AddComponent<SphereCollider>(); 

			mCollider.isTrigger = true; 
			return mCollider; 
		} 
	}

	[SerializeField]SphereCollider mCollider;
	NJGMapBase map;

	void Awake()
	{
		map = NJGMapBase.instance;
		//id++;
		//mId = id;
		zoneCollider.radius = colliderRadius;
	}

	void OnTriggerEnter(Collider col)
	{
		if (col.CompareTag(triggerTag))
		{
			if (map != null)
			{
				map.zoneColor = color;
				map.worldName = zone;
				if (generateOnTrigger)
				{
					NJGMapBase.instance.GenerateMap();
				}
			}
		}
	}

	/// <summary>
	/// Add this unit to the list of in-game units.
	/// </summary>

	void OnEnable()
	{
		list.Add(this);
	}

	/// <summary>
	/// Remove this unit from the list.
	/// </summary>

	void OnDisable()
	{
		list.Remove(this);
	}

	/*void OnDestroy()
	{
		id--;
	}*/
}
