/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

namespace SWS
{
    /// <summary>
    /// Movement script: linear or curved splines. Underlying tween library: HOTween.
    /// <summary>
    [AddComponentMenu("Simple Waypoint System/splineMove")]
    public class splineMove : MonoBehaviour
    {
        /// <summary>
        /// Path component to use for movement.
        /// <summary>
        public PathManager pathContainer;

        /// <summary>
        /// Animation path type, linear or curved.
        /// <summary>
        public PathType pathType = Holoville.HOTween.PathType.Curved;

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
        /// Whether this object should orient itself to the path.
        /// <summary>
        public bool orientToPath = false;

        /// <summary>
        /// Whether local positioning should be used when tweening this object.
        /// <summary>
        public bool local = false;

        /// <summary>
        /// Value to look ahead on the path when orientToPath is enabled (0-1).
        /// <summary>
        public float lookAhead = 0;

        /// <summary>
        /// Additional units to add on the y-axis.
        /// <summary>
        public float sizeToAdd = 0;

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
        /// Selection for speed-based movement or time in seconds per segment. 
        /// <summary>
        public enum TimeValue
        {
            time,
            speed
        }
        public TimeValue timeValue = TimeValue.speed;

        /// <summary>
        /// Speed or time value depending on the selected TimeValue type.
        /// <summary>
        public float speed = 5;

        /// <summary>
        /// Animation easetype on TimeValue type time.
        /// <summary>
        public Holoville.HOTween.EaseType easeType = Holoville.HOTween.EaseType.Linear;

        /// <summary>
        /// Custom curve when AnimationCurve has been selected as easeType.
        /// <summary>
        public AnimationCurve animEaseType;

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

        /// <summary>
        /// Option for locking a rotation axis with orientToPath enabled.
        /// <summary>
        public Holoville.HOTween.Axis lockAxis = Holoville.HOTween.Axis.X;

        /// <summary>
        /// Option for locking a position axis.
        /// <summary>
        public Holoville.HOTween.Axis lockPosition = Holoville.HOTween.Axis.None;

        //---HOTween animation helper variables---
        /// <summary>
        /// Tween initialized by HOTween for movement along waypoints.
        /// <summary>
        public Tweener tween;
        //array of modified waypoint positions for the tween
        private Vector3[] wpPos;
        //parameters for the tween
        private TweenParms tParms;
        //path plugin property
        private PlugVector3Path plugPath;
        //looptype random generator
        private System.Random rand = new System.Random();
        //looptype random waypoint index array
        private int[] rndArray;
        //looptype random current waypoint index
        private int rndIndex = 0;
        //whether the tween was paused
        private bool waiting = false;
        //original speed when changing the tween's speed
        private float originSpeed;


        //check for automatic initialization
        void Start()
        {
            if (onStart)
                StartMove();
        }


        //initialize or update modified waypoint positions
        //fills array with original positions and adds custom height
        private void InitWaypoints()
        {
            wpPos = new Vector3[waypoints.Length];
            for (int i = 0; i < wpPos.Length; i++)
                wpPos[i] = waypoints[i].position + new Vector3(0, sizeToAdd, 0);
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

            //get array with waypoint positions
            waypoints = pathContainer.waypoints;

            //cache original speed for future speed changes
            originSpeed = speed;

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

            Stop();
            //start movement
            if (currentPoint > 0)
                Teleport(currentPoint);
            else
                StartCoroutine(Move());
        }


        //constructs the tween and starts movement
        private IEnumerator Move()
        {
            //if move to path is enabled,
            //start an additional tween to the first waypoint
            if (moveToPath)
                yield return StartCoroutine(MoveToPath());
            else
            {
                //initialize waypoint positions
                InitWaypoints();
                //set the transform's position to the first waypoint
                if (currentPoint == waypoints.Length - 1)
                    transform.position = wpPos[currentPoint];
                else
                    transform.position = wpPos[0];
            }

            //we're now at the first waypoint position, so directly call the next waypoint.
            //on looptype random we have to initialize a random order of waypoints first.
            if (loopType == LoopType.random)
                StartCoroutine(ReachedEnd());
            else
            {
                CreateTween();
                StartCoroutine(NextWaypoint());
            }
        }


        //constructs a smooth curve to the first waypoint
        private IEnumerator MoveToPath()
        {
            //we dont need more than 4 waypoints for calculating a curve to the first waypoint
            int max = waypoints.Length > 4 ? 4 : waypoints.Length;
            wpPos = new Vector3[max];
            
            //set and add custom height to the waypoints 
            for (int i = 1; i < max; i++)
                wpPos[i] = waypoints[i - 1].position + new Vector3(0, sizeToAdd, 0);

            //set the first slot to the current position
            wpPos[0] = transform.position;
            
            //create HOTween tweener
            CreateTween();
            //resume tweener if paused
            if (tween.isPaused)
                tween.Play();

            //wait until we're at the first waypoint
            yield return StartCoroutine(tween.UsePartialPath(-1, 1).WaitForCompletion());
            moveToPath = false;

            //discard tweener because it was only used for this option
            if (tween != null) 
                tween.Kill();
            tween = null;

            //reinitialize original waypoint positions
            InitWaypoints();
        }


        //creates a new HOTween tween with give arguments that moves along the path
        private void CreateTween()
        {
            //prepare HOTween's parameters, you can look them up here
            //http://www.holoville.com/hotween/documentation.html
            
            //create new plugin for curved paths
            //pass in array of Vector3 waypoint positions, relative = true
            plugPath = new PlugVector3Path(wpPos, true, pathType);

            //orients the tween target along the path
            if (orientToPath)
                plugPath.OrientToPath(lookAhead, lockAxis);

            //lock position axis, if set
            if (lockPosition != Holoville.HOTween.Axis.None)
                plugPath.LockPosition(lockPosition);

            //create a closed loop, if set
            if (loopType == LoopType.loop && closeLoop)
                plugPath.ClosePath(true);

            //create TweenParms for storing HOTween's parameters
            tParms = new TweenParms();
            //sets the path plugin as tween position property
            if (local)
                tParms.Prop("localPosition", plugPath);
            else
                tParms.Prop("position", plugPath);

            //additional tween parameters for partial tweens
            tParms.AutoKill(false);
            tParms.Pause(true);
            tParms.Loops(1);

            //differ between TimeValue, use speed with linear easing
            //or time based tweening with an animation easetype
            if (timeValue == TimeValue.speed)
            {
                tParms.SpeedBased();
                tParms.Ease(EaseType.Linear);
            }
            else
            {
                //use time in seconds and the chosen easetype
                if (easeType == Holoville.HOTween.EaseType.AnimationCurve)
                    tParms.Ease(animEaseType);
                else
                    tParms.Ease(easeType);
            }

            //finally create the tween
            tween = HOTween.To(transform, originSpeed, tParms);

            //continue new tween with adjusted speed if it was changed before
            if (originSpeed != speed)
                ChangeSpeed(speed);
        }


        //loop index while moving
        int startingPoint = 0;
        //this method moves us one by one to the next waypoint
        //and executes all delay or tweening interaction
        private IEnumerator NextWaypoint()
        {
            //loop through modified waypoint positions
            for (int point = startingPoint; point < wpPos.Length - 1; point++)
            {
                //execute all messages for this waypoint
                messages.Execute(this, currentPoint);

                //execute waypoint delay
                if (delays[currentPoint] > 0)
                    yield return StartCoroutine(WaitDelay());

                //the tween could be destroyed with a message,
                //here we have to check if it still exists before continuing
                if (tween == null)
                    yield break;

                //check for pausing and wait until unpaused again
                while (waiting) yield return null;

                //continue tween
                tween.Play();
                //tween from current point to the next point
                yield return StartCoroutine(tween.UsePartialPath(point, point + 1).WaitForCompletion());

                //repeating mode is on: moving backwards
                if (loopType == LoopType.pingPong && repeat)                    
                    currentPoint--;
                else if (loopType == LoopType.random)
                {
                    //count up random index after each waypoint
                    rndIndex++;
                    //assign next waypoint of our shuffled array
                    currentPoint = rndArray[rndIndex];
                }
                else
                    //default mode: move forwards
                    currentPoint++;
            }

            //avoid additional message/delay at last waypoint on these looptypes
            if (loopType != LoopType.pingPong && loopType != LoopType.random)
            {
                messages.Execute(this, currentPoint);
                if (delays[currentPoint] > 0)
                    yield return StartCoroutine(WaitDelay());
            }

            //reset loop index
            startingPoint = 0;
            //differ between tween options at the end of the path
            StartCoroutine(ReachedEnd());
        }


        //wait a predefined amount of time
        private IEnumerator WaitDelay()
        {
            //pause tween while waiting
            tween.Pause();

            //wait seconds defined in delays at current waypoint position
            //own implementation of a WaitForSeconds() coroutine,
            //with an additional check for pausing/unpausing (waiting)
            float timer = Time.time + delays[currentPoint];
            while (!waiting && Time.time < timer)
                yield return null;
        }


        //object reached the end of its path
        private IEnumerator ReachedEnd()
        {
            //each looptype has specific properties
            switch (loopType)
            {
                //LoopType.none means there will be no repeat,
                //so we just discard the tweener and return
                case LoopType.none:

                    if (tween != null)
                        tween.Kill();
                    tween = null;
                    yield break;

                //in a loop we set our position indicator back to zero
                case LoopType.loop:

                    //additional option: if the path was closed, we move our object
                    //from the last to the first waypoint instead of just "appearing" there
                    if (closeLoop)
                    {
                        tween.Play();
                        yield return StartCoroutine(tween.UsePartialPath(currentPoint, -1).WaitForCompletion());
                    }
                    currentPoint = 0;
                    break;

                //on LoopType.pingPong, we decrease our location indicator till it reaches zero again
                //to start from the beginning - to achieve that, and differ between back and forth,
                //we use the boolean "repeat"
                case LoopType.pingPong:

                    //discard tweener to construct a new one
                    if (tween != null)
                        tween.Kill();
                    tween = null;

                    if (!repeat)
                    {
                        //enable repeat mode
                        repeat = true;
                        //update waypoint positions backwards
                        for (int i = 0; i < wpPos.Length; i++)
                        {
                            wpPos[i] = waypoints[waypoints.Length - 1 - i].position + new Vector3(0, sizeToAdd, 0);
                        }
                    }
                    else
                    {
                        //we are at the first waypoint again,
                        //reinitialize original waypoint positions
                        //and disable repeating mode
                        InitWaypoints();
                        repeat = false;
                    }

                    //create tween for next iteration
                    CreateTween();
                    break;

                //on LoopType.random, we calculate a random order between all waypoints
                //and loop through them, for this case we use the Fisher-Yates algorithm
                case LoopType.random:
                    //reset random index, because we calculate a new order
                    rndIndex = 0;
                    //reinitialize original waypoint positions
                    InitWaypoints();

                    //discard tweener for new order
                    if (tween != null)
                        tween.Kill();
                    tween = null;

                    //create array with ongoing index numbers to keep them in mind,
                    //this gets shuffled with all waypoint positions at the next step 
                    rndArray = new int[wpPos.Length];
                    for (int i = 0; i < rndArray.Length; i++)
                    {
                        rndArray[i] = i;
                    }

                    //get total array length
                    int n = wpPos.Length;
                    //shuffle wpPos and rndArray
                    while (n > 1)
                    {
                        int k = rand.Next(n--);
                        Vector3 temp = wpPos[n];
                        wpPos[n] = wpPos[k];
                        wpPos[k] = temp;

                        int tmpI = rndArray[n];
                        rndArray[n] = rndArray[k];
                        rndArray[k] = tmpI;
                    }

                    //since all waypoints are shuffled the first waypoint does not
                    //correspond with the actual current position, so we have to
                    //swap the first waypoint with the actual waypoint.
                    //start by caching the first waypoint position and number
                    Vector3 first = wpPos[0];
                    int rndFirst = rndArray[0];
                    //loop through wpPos array and find corresponding waypoint
                    for (int i = 0; i < wpPos.Length; i++)
                    {
                        //currentPoint is equal to this waypoint number
                        if (rndArray[i] == currentPoint)
                        {
                            //swap rnd index number and waypoint positions
                            rndArray[i] = rndFirst;
                            wpPos[0] = wpPos[i];
                            wpPos[i] = first;
                        }
                    }
                    //set current rnd index number to the actual current point
                    rndArray[0] = currentPoint;

                    //create tween with random order
                    CreateTween();
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
            Stop();
            //set new path container
            pathContainer = newPath;
            //restart movement
            StartMove();
        }


        /// <summary>
        /// Teleports to a path waypoint and starts movement.
        /// <summary>
        public void Teleport(int index)
        {
            if (loopType == LoopType.random)
            {
                Debug.LogWarning("Teleporting doesn't work with looptype set to 'random'. Resetting.");
                index = 0;
            }
            //don't exceed the waypoint count
            index = Mathf.Clamp(index, 0, waypoints.Length - 1);

            //cancel movement
            Resume();
            Stop();
            moveToPath = false;

            //don't use the last waypoint on loop, just reset it instead
            if (loopType == LoopType.loop && index == (waypoints.Length - 1))
                index = 0;

            //start movement at the teleported position
            currentPoint = startingPoint = index;
            StartCoroutine(Move());
        }


        /// <summary>
        /// Disables any running movement routines.
        /// <summary>
        public void Stop()
        {
            StopAllCoroutines();
            //reset current waypoint index to zero
            currentPoint = 0;

            if (tween != null)
                tween.Kill();
            plugPath = null;
            tween = null;
        }


        /// <summary>
        /// Stops movement and resets to the first waypoint.
        /// <summary>
        public void ResetMove()
        {
            Stop();
            if (pathContainer)
                transform.position = pathContainer.waypoints[currentPoint].position
                                     + new Vector3(0, sizeToAdd, 0);
        }


        /// <summary>
        /// Pauses the current movement routine.
        /// <summary>
        public void Pause()
        {
            //block further tween execution
            waiting = true;
            if (tween != null)
                tween.Pause();
        }


        /// <summary>
        /// Resumes the current movement routine.
        /// <summary>
        public void Resume()
        {
            //unblock further tween execution
            waiting = false;
            if (tween != null)
                tween.Play();
        }


        /// <summary>
        /// Change running tween speed by manipulating its timeScale.
        /// <summary>
        public void ChangeSpeed(float value)
        {
            //calulate new timeScale value based on original speed
            float newValue;
            if (timeValue == TimeValue.speed)
                newValue = value / originSpeed;
            else
                newValue = originSpeed / value;

            //set speed, change timeScale percentually
            speed = value;
            if (tween != null)
                tween.timeScale = newValue;
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