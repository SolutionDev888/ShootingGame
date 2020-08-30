/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SWS
{
    /// <summary>
    /// Movement script: Pathfinding using NavMesh agents.
    /// <summary>
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    [AddComponentMenu("Simple Waypoint System/navMove")]
    public class navMove : MonoBehaviour
    {
        /// <summary>
        /// Path component to use for movement.
        /// <summary>
        public PathManager pathContainer;

        /// <summary>
        /// Current waypoint indicator on the path. 
        /// <summary>
        public int currentPoint = 0;

        /// <summary>
        /// Whether this object should start its movement at game launch.
        /// <summary>
        public bool onStart = false;

        /// <summary>
        /// Whether this object should walk to the first waypoint or spawn there.
        /// <summary>
        public bool moveToPath = false;

        /// <summary>
        /// Option for closing the path on the "loop" looptype.
        /// <summary>
        public bool closeLoop = false;

        /// <summary>
        /// Whether rotation should be overridden by the NavMesh agent.
        /// <summary>
        public bool updateRotation = true;

        /// <summary>
        /// Array storing delay values per waypoint.
        /// <summary>
        [HideInInspector]
        public float[] delays;

        /// <summary>
        /// Message class storing message names and values per waypoint.
        /// <summary>
        [HideInInspector]
        public Messages messages = new Messages();

        /// <summary>
        /// Supported movement looptypes when moving on the path. 
        /// <summary>
        public enum LoopType
        {
            none,
            loop,
            pingPong,
            random
        }
        public LoopType loopType = LoopType.none;

        /// <summary>
        /// Waypoint array references of the requested path.
        /// <summary>
        [HideInInspector]
        public Transform[] waypoints;

        /// <summary>
        /// Used on loopType "pingPong" for determining forward or backward movement.
        /// <summary>
        [HideInInspector]
        public bool repeat = false;

        //reference to the agent component
        private UnityEngine.AI.NavMeshAgent agent;
        //looptype random generator
        private System.Random rand = new System.Random();
        //looptype random current waypoint index
        private int rndIndex = 0;
        //whether the tween was paused
        private bool waiting = false;


        //check for automatic initialization
        void Start()
        {
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

            if (onStart)
                StartMove();
        }


        /// <summary>
        /// Starts movement. Can be called from other scripts to allow start delay.
        /// <summary>
        public void StartMove()
        {
            //don't continue without path container
            if (pathContainer == null)
            {
                Debug.LogWarning(gameObject.name + " has no path! Please set Path Container.");
                return;
            }

            //get Transform array with waypoint positions
            waypoints = new Transform[pathContainer.waypoints.Length];
            Array.Copy(pathContainer.waypoints, waypoints, pathContainer.waypoints.Length);

            //check if delay array wasn't set in the editor, then we have to initialize it
            if (delays == null)
                delays = new float[waypoints.Length];
            else if (delays.Length < waypoints.Length)
            {
                //else if the delay array is smaller than the waypoint array,
                //that means we have added waypoints to the path but haven't modified the
                //delay settings, here we need to resize it again while keeping old values
                float[] tempDelay = new float[delays.Length];
                Array.Copy(delays, tempDelay, delays.Length);
                delays = new float[waypoints.Length];
                Array.Copy(tempDelay, delays, tempDelay.Length);
            }

            //check for message count and reinitialize if necessary
            if (messages.list.Count > 0)
                messages.Initialize(waypoints.Length);


            Stop(false);
            //start movement
            StartCoroutine(Move());
        }


        //constructs the tween and starts movement
        private IEnumerator Move()
        {
            //enable agent updates
            agent.Resume();
            agent.updateRotation = updateRotation;

            //if move to path is enabled,
            //set an additional destination to the first waypoint
            if (moveToPath)
            {
                agent.SetDestination(waypoints[currentPoint].position);
                yield return StartCoroutine(WaitForDestination());
                moveToPath = false;
            }
            else
            {
                //set the transform's position to the first waypoint
                agent.Warp(waypoints[currentPoint].position);
            }

            //we're now at the first waypoint position, so directly call the next waypoint.
            //on looptype random we have to initialize a random order of waypoints first.
            if (loopType == LoopType.random)
                StartCoroutine(ReachedEnd());
            else
            {
                StartCoroutine(NextWaypoint());
            }
        }


        //this method moves us one by one to the next waypoint
        //and executes all delay or tweening interaction
        private IEnumerator NextWaypoint()
        {
            //execute all messages for this waypoint
            messages.Execute(this, currentPoint);
            yield return new WaitForEndOfFrame();

            //execute waypoint delay
            if (delays[currentPoint] > 0)
                yield return StartCoroutine(WaitDelay());

            //check for pausing and wait until unpaused again
            while (waiting) yield return null;
            Transform next = null;

            //repeating mode is on: moving backwards
            if (loopType == LoopType.pingPong && repeat)
                currentPoint--;
            else if (loopType == LoopType.random)
            {
                //parse currentPoint value from waypoint
                rndIndex++;
                currentPoint = int.Parse(waypoints[rndIndex].name.Replace("Waypoint ", ""));
                next = waypoints[rndIndex];
            }
            else
                //default mode: move forwards
                currentPoint++;

            //just to make sure we don't run into an out of bounds exception
            currentPoint = Mathf.Clamp(currentPoint, 0, waypoints.Length - 1);
            //set the next waypoint based on calculated current point
            if (next == null) next = waypoints[currentPoint];

            //set destination to the next waypoint
            agent.SetDestination(next.position);
            yield return StartCoroutine(WaitForDestination());

            //determine if the agent reached the path's end
            if (loopType != LoopType.random && currentPoint == waypoints.Length - 1
                || rndIndex == waypoints.Length - 1 || repeat && currentPoint == 0)
                StartCoroutine(ReachedEnd());
            else
                StartCoroutine(NextWaypoint());
        }


        //wait until the agent reached its destination
        private IEnumerator WaitForDestination()
        {
            while (agent.pathPending)
                yield return null;

            float remain = agent.remainingDistance;
            while (remain == Mathf.Infinity || remain - agent.stoppingDistance > float.Epsilon
            || agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
            {
                remain = agent.remainingDistance;
                yield return null;
            }
        }


        //wait a predefined amount of time
        private IEnumerator WaitDelay()
        {
            //wait seconds defined in delays at current waypoint position
            //own implementation of a WaitForSeconds() coroutine,
            //with an additional check for pausing/unpausing (waiting)
            float timer = Time.time + delays[currentPoint];
            while (!waiting  && Time.time < timer)
                yield return null;
        }


        //object reached the end of its path
        private IEnumerator ReachedEnd()
        {
            //each looptype has specific properties
            switch (loopType)
            {
                //LoopType.none means there will be no repeat,
                //so we just execute the final messages and delay
                case LoopType.none:

                    messages.Execute(this, currentPoint);
                    if (delays[currentPoint] > 0)
                        yield return StartCoroutine(WaitDelay());
                    yield break;

                //in a loop we set our position indicator back to zero,
                //also executing messages and delay 
                case LoopType.loop:

                    messages.Execute(this, currentPoint);
                    if (delays[currentPoint] > 0)
                        yield return StartCoroutine(WaitDelay());

                    //additional option: if the path was closed, we move our object
                    //from the last to the first waypoint instead of just "appearing" there
                    if (closeLoop)
                    {
                        agent.SetDestination(waypoints[0].position);
                        yield return StartCoroutine(WaitForDestination());
                    }
                    else
                        agent.Warp(waypoints[0].position);

                    currentPoint = 0;
                    break;

                //on LoopType.pingPong, we have to invert currentPoint updates
                case LoopType.pingPong:

                    repeat = !repeat;
                    break;

                //on LoopType.random, we calculate a random order between all waypoints
                //and loop through them, for this case we use the Fisher-Yates algorithm
                case LoopType.random:

                    //reinitialize original waypoint positions
                    Array.Copy(pathContainer.waypoints, waypoints, pathContainer.waypoints.Length);
                    int n = waypoints.Length;

                    //shuffle waypoints array
                    while (n > 1)
                    {
                        int k = rand.Next(n--);
                        Transform temp = waypoints[n];
                        waypoints[n] = waypoints[k];
                        waypoints[k] = temp;
                    }

                    //since all waypoints are shuffled the first waypoint does not
                    //correspond with the actual current position, so we have to
                    //swap the first waypoint with the actual waypoint
                    Transform first = pathContainer.waypoints[currentPoint];
                    for (int i = 0; i < waypoints.Length; i++)
                    {
                        if (waypoints[i] == first)
                        {
                            Transform temp = waypoints[0];
                            waypoints[0] = waypoints[i];
                            waypoints[i] = temp;
                            break;
                        }
                    }

                    //reset random loop index
                    rndIndex = 0;
                    break;
            }

            //start moving to the next iteration
            StartCoroutine(NextWaypoint());
        }


        /// <summary>
        /// Changes the current path of this walker object and starts movement.
        /// <summary>
        public void SetPath(PathManager newPath)
        {
            //disable any running movement methods
            Stop(true);
            //set new path container
            pathContainer = newPath;
            //restart movement
            StartMove();
        }


        /// <summary>
        /// Disables any running movement routines.
        /// <summary>
        public void Stop() { Stop(false); }


        /// <summary>
        /// Overloaded method with an optional parameter for toggling agent updates.
        /// <summary>
        public void Stop(bool stopUpdates)
        {
            StopAllCoroutines();
            //reset current waypoint index to zero
            currentPoint = 0;
            agent.Stop(stopUpdates);
        }


        /// <summary>
        /// Stops movement and resets to the first waypoint.
        /// <summary>
        public void ResetMove()
        {
            Stop(true);
            if (pathContainer)
                agent.Warp(pathContainer.waypoints[currentPoint].position);
        }


        /// <summary>
        /// Pauses the current movement routine.
        /// <summary>
        public void Pause() { Pause(false); }


        /// <summary>
        /// Overloaded method with an optional parameter for toggling agent updates.
        /// <summary>
        public void Pause(bool stopUpdates)
        {
            waiting = true;
            agent.Stop(stopUpdates);
        }


        /// <summary>
        /// Resumes the current movement routine.
        /// <summary>
        public void Resume()
        {
            waiting = false;
            agent.Resume();
        }


        /// <summary>
        /// Wrapper to change agent speed.
        /// <summary>
        public void ChangeSpeed(float value)
        {
            agent.speed = value;
        }


        /// <summary>
        /// Sets delay at a specific waypoint.
        /// <summary>
        public void SetDelay(int index, float value)
        {
            if (delays == null)
                delays = new float[waypoints.Length];

            delays[index] = value;
        }
    }
}