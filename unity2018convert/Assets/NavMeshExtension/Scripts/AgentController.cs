/*  This file is part of the "NavMesh Extension" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NavMeshExtension;

/// <summary>
/// Example integration of NavMesh Agents with portal behavior.
/// <summary>
[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class AgentController : MonoBehaviour
{
    /// <summary>
    /// Target destination object, set by mouse input.
    /// <summary>
    public GameObject pointer;

    //reference to pointer object
    private static GameObject pointerObj;
    //reference to agent
    private UnityEngine.AI.NavMeshAgent agent;
    //resulting path from the PortalManager call
    private Vector3[] path;


    //get components
    void Start()
    {
        if(!pointerObj) 
            pointerObj = (GameObject)Instantiate(pointer, transform.position, Quaternion.identity);
        
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }


    //check for mouse input
    void Update()
    {
        RaycastHit hit;

        //on left mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //the mouse ray has hit a collider in the scene
            if (Physics.Raycast(ray, out hit))
            {
                //reposition pointer object to the hit point
                pointerObj.transform.position = hit.point;

                //construct path:
                //starting at the current gameobject position
                //ending at the position of the pointer object
                path = PortalManager.GetPath(transform.position, pointerObj.transform.position);

                //stop existing movement routines
                StopAllCoroutines();
                //start new agent movement to the destination
                StartCoroutine(GoToDestination());
            }
        }
    }


    //loops over path positions, sets the 
    //current target destination of this agent
    IEnumerator GoToDestination()
    {
        //path index
        int i = 0;

        //iterate over all positions
        while(i < path.Length)
        {
            //teleport to the current position
            agent.Warp(path[i]);
            i++;

            //walk to the next position
            agent.SetDestination(path[i]);
            while (agent.pathPending)
                yield return null;

            //wait until we reached this position
            float remain = agent.remainingDistance;
            while (remain == Mathf.Infinity || remain - agent.stoppingDistance > float.Epsilon
            || agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
                remain = agent.remainingDistance;
                yield return null;
            }

            //increase counter
            i++;
        }

        //agent reached the final destination
        #if UNITY_4_6 || UNITY_4_7
        agent.Stop(true);
        #else
        agent.Stop();
        #endif
    }
}
