/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;
using SWS;

/// <summary>
/// Example: some methods invoked by messages, demonstrating runtime adjustments.
/// <summary>
public class MessageReceiver : MonoBehaviour
{
    void MyMethod()
    {
        //your own method!
    }


    //prints text to the console
    void PrintText(string text)
    {
        Debug.Log(text);
    }


    //sets the transform's y-axis to the desired rotation
    //used in the 2D sample for rotating the sprite at path ends
    void RotateSprite(float newRot)
    {
        Vector3 currentRot = transform.eulerAngles;
        currentRot.y = newRot;
        transform.eulerAngles = currentRot;
    }
    

    //sets a new destination for a navmesh agent,
    //leaving its path and returning to it after a few seconds.
    //used in the message sample for redirecting the agent
    IEnumerator SetDestination(Object target)
    {
        //get references
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        navMove myMove = GetComponent<navMove>();
        GameObject tar = (GameObject)target as GameObject;

        //pause movement routine and increase agent speed
        myMove.Pause(false);
        myMove.ChangeSpeed(4);
        //set new destination of the navmesh agent
        agent.SetDestination(tar.transform.position);

        //wait until the path has been calculated
        while (agent.pathPending)
            yield return null;
        //wait until agent reached its destination
        float remain = agent.remainingDistance;
        while (remain == Mathf.Infinity || remain - agent.stoppingDistance > float.Epsilon
        || agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            remain = agent.remainingDistance;
            yield return null;
        }

        //wait a few seconds at the destination,
        //then decrease agent speed and resume movement routine
        yield return new WaitForSeconds(4);
        myMove.ChangeSpeed(1.5f);
        myMove.Resume();
    }


    //activates an object for an amount of time
    //used in the message sample for activating a particle effect
    IEnumerator ActivateForTime(Object target)
    {
        GameObject tar = (GameObject)target as GameObject;
        tar.SetActive(true);

        yield return new WaitForSeconds(6);
        tar.SetActive(false);
    }
}
