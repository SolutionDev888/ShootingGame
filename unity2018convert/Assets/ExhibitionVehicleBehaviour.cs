using UnityEngine;
using System.Collections;

public class ExhibitionVehicleBehaviour : MonoBehaviour {
	private UnityEngine.AI.NavMeshAgent navAgent;
	public Transform waypoint;
	// Use this for initialization
	void Start () {
		navAgent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
		navAgent.SetDestination(waypoint.position);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
