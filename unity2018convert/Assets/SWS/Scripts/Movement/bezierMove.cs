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
    /// Movement script: bezier. Underlying tween library: HOTween.
    /// <summary>
    [AddComponentMenu("Simple Waypoint System/bezierMove")]
    public class bezierMove : MonoBehaviour
    {
        /// <summary>
        /// Path component to use for movement.
        /// <summary>
        public BezierPathManager pathContainer;

        /// <summary>
        /// Animation path type, linear or curved.
        /// <summary>
        public PathType pathType = Holoville.HOTween.PathType.Curved;

        /// <summary>
        /// Whether this object should start its movement at game launch.
        /// <summary>
        public bool onStart = false;

        /// <summary>
        /// Whether this object should walk to the first waypoint or spawn there.
        /// <summary>
        public bool moveToPath = false;

        /// <summary>
        /// Whether this object should orient itself to the path.
        /// <summary>
        public bool orientToPath = false;

        /// <summary>
        /// Value to look ahead on the path when orientToPath is enabled (0-1).
        /// <summary>
        public float lookAhead = 0;

        /// <summary>
        /// Additional units to add on the y-axis.
        /// <summary>
        public float sizeToAdd = 0;

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
        //enum to choose from available looptypes
        public enum LoopType
        {
            none,
            loop,
            pingPong,
        }
        public LoopType loopType = LoopType.none;

        /// <summary>
        /// Waypoint array references of the requested path.
        /// <summary>
        private Vector3[] waypoints;

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
        //current path position (0-1)
        private float positionOnPath = -1f;
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
                wpPos[i] = waypoints[i] + new Vector3(0, sizeToAdd, 0);
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
            waypoints = pathContainer.pathPoints;

            //cache original speed for future speed changes
            originSpeed = speed;

            Stop();
            //start movement
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
                transform.position = waypoints[0] + new Vector3(0, sizeToAdd, 0);
            }

            //we create the tween and start moving
            CreateTween();
        }


        //constructs a smooth curve to the first waypoint
        private IEnumerator MoveToPath()
        {
            //we need 7 waypoints for calculating a curve to the first waypoint
            wpPos = new Vector3[7];
            wpPos[0] = transform.position;
            wpPos[1] = 2 * waypoints[0] - waypoints[1] + new Vector3(0, sizeToAdd, 0);
            wpPos[2] = 2 * waypoints[0] - waypoints[2] + new Vector3(0, sizeToAdd, 0);
            wpPos[3] = waypoints[0] + new Vector3(0, sizeToAdd, 0);

            //now we smooth out the way to the first waypoint
            List<Vector3> unsmoothedList = new List<Vector3>();
            for (int i = 0; i < 4; i++)
                unsmoothedList.Add(wpPos[i]);
            //store smoothed array
            Vector3[] smoothed = WaypointManager.SmoothCurve(unsmoothedList, 1).ToArray();

            //copy smoothed positions to the tween array
            for (int i = 0; i < 4; i++)
                wpPos[i] = smoothed[i];

            //add a few other positions to the path, in order to
            //speed up the movement to the first path point and avoiding speed issues
            wpPos[4] = waypoints[1] + new Vector3(0, sizeToAdd, 0);
            wpPos[5] = pathContainer.bPoints[1].wp.position + new Vector3(0, sizeToAdd, 0);

            if (pathContainer.bPoints.Count > 2)
                wpPos[6] = pathContainer.bPoints[2].wp.position + new Vector3(0, sizeToAdd, 0);
            else
                wpPos[6] = wpPos[5];

            //create HOTween tweener
            CreateTween();

            //wait until we're at the first waypoint
            yield return StartCoroutine(tween.UsePartialPath(-1, 3).WaitForCompletion());
            moveToPath = false;

            //discard tweener because it was only used for this option
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
            plugPath = null;
            plugPath = new PlugVector3Path(wpPos, true, pathType);

            //orients the tween target along the path
            if (orientToPath)
                plugPath.OrientToPath(lookAhead, lockAxis);

            //lock position axis, if set
            if (lockPosition != Holoville.HOTween.Axis.None)
                plugPath.LockPosition(lockPosition);

            //create TweenParms for storing HOTween's parameters
            tParms = new TweenParms();
            //sets the path plugin as tween position property
            tParms.Prop("position", plugPath);

            //additional tween parameters for partial tweens
            tParms.AutoKill(false);
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

            //if we're on the original tween,
            //attach methods to the tween
            if (!moveToPath)
            {
                tParms.OnUpdate(CheckPoint);
                tParms.OnComplete(ReachedEnd);
            }

            //finally create the tween
            tween = HOTween.To(transform, originSpeed, tParms);

            //continue new tween with adjusted speed if it was changed before
            if (originSpeed != speed)
                ChangeSpeed(speed);
        }


        //invokes messages at the defined positions
        private void CheckPoint()
        {
            //store current position for later comparison
            float oldPosition = positionOnPath;
            //set new position (percentage value)
            positionOnPath = tween.fullElapsed / tween.fullDuration;

            //check messages, trigger events for them
            for (int i = 0; i < messages.list.Count; i++)
            {
                //if this gameobject has passed a message position, trigger message
                if (oldPosition < messages.list[i].pos && positionOnPath >= messages.list[i].pos
                    && oldPosition != positionOnPath)
                {
                    messages.Execute(this, i);
                }
            }
        }


        /// <summary>
        /// Wait a predefined amount of time. Complements delay settings.
        /// <summary>
        public IEnumerator Wait(float value)
        {
            //pause tween while waiting
            tween.Pause();

            //wait at the current waypoint position
            //own implementation of a WaitForSeconds() coroutine
            float timer = Time.time + value;
            while (Time.time < timer)
                yield return null;

            //resume in between path points
            if (positionOnPath < 1 && positionOnPath != -1)
                Resume();
        }


        //object reached the end of its path
        private void ReachedEnd()
        {
            //reset calculated position used for invoking messages
            positionOnPath = -1;

            //each looptype has specific properties
            switch (loopType)
            {
                //LoopType.none means there will be no repeat,
                //so we just discard the tweener and return
                case LoopType.none:

                    if (tween != null)
                        tween.Kill();
                    tween = null;
                    break;

                //in a loop we start from the beginning
                case LoopType.loop:

                    Stop();
                    StartMove();
                    break;

                //on LoopType.pingPong, we revert the tween,
                //let it run through (backwards) and then restart from the beginning (forwards)
                //so our object basically moves back and forth
                case LoopType.pingPong:

                    //discard tweener to construct a new one
                    if (tween != null)
                        tween.Kill();
                    tween = null;

                    //cache old waypoint positions
                    Vector3[] cachePos = new Vector3[wpPos.Length];
                    System.Array.Copy(wpPos, cachePos, wpPos.Length);
                    //invert waypoint positions
                    for (int i = 0; i < wpPos.Length; i++)
                    {
                        wpPos[i] = cachePos[wpPos.Length - 1 - i];
                    }

                    //cache old messages and their positions
                    MessageOptions[] cacheOpt = new MessageOptions[messages.list.Count];
                    messages.list.CopyTo(cacheOpt);
                    //invert messages
                    for (int i = 0; i < messages.list.Count; i++)
                    {
                        messages.list[i].pos = 1 - messages.list[i].pos;
                        messages.list[i] = cacheOpt[cacheOpt.Length - 1 - i];
                    }

                    //create a new reversed tween
                    CreateTween();
                    break;
            }
        }


        /// <summary>
        /// Changes the current path of this walker object and starts movement.
        /// <summary>
        public void SetPath(BezierPathManager newPath)
        {
            //disable any running movement methods
            Stop();
            //set new path container
            pathContainer = newPath;
            //restart movement
            StartMove();
        }


        /// <summary>
        /// Disables any running movement routines.
        /// <summary>
        public void Stop()
        {
            StopAllCoroutines();
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
                transform.position = waypoints[0] + new Vector3(0, sizeToAdd, 0);
        }


        /// <summary>
        /// Pauses the current movement routine.
        /// <summary>
        public void Pause()
        {
            if (tween != null)
                tween.Pause();
        }


        /// <summary>
        /// Resumes the current movement routine.
        /// <summary>
        public void Resume()
        {
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


        //draw spheres for each message on the path for visibility
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            if (tween == null || moveToPath)
                return;

            for (int i = 0; i < messages.list.Count; i++)
                Gizmos.DrawSphere(tween.GetPointOnPath(messages.list[i].pos), 0.2f);
        }
    }
}