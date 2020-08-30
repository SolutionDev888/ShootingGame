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
    /// Movement script: minimal version of splineMove. Underlying tween library: HOTween.
    /// <summary>
    [AddComponentMenu("Simple Waypoint System/minimalMove")]
    public class minimalMove : MonoBehaviour
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
        /// Value to look ahead on the path when orientToPath is enabled (0-1).
        /// <summary>
        public float lookAhead = 0;

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
        }
        public LoopType loopType = LoopType.none;

        /// <summary>
        /// Waypoint array references of the requested path.
        /// <summary>
        [HideInInspector]
        public Vector3[] waypoints;

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
        //parameters for the tween
        private TweenParms tParms;
        //path plugin property
        private PlugVector3Path plugPath;
        //original speed when changing the tween's speed
        private float originSpeed;


        //check for automatic initialization
        void Start()
        {
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

            //get array with waypoint positions
            waypoints = pathContainer.GetPathPoints();

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
                //set the transform's position to the first waypoint
                transform.position = waypoints[0];
            }

            //create the tween and start moving
            CreateTween();
        }


        //constructs a smooth curve to the first waypoint
        private IEnumerator MoveToPath()
        {
            //we dont need more than 4 waypoints for calculating a curve to the first waypoint
            int max = waypoints.Length > 4 ? 4 : waypoints.Length;
            Vector3[] wpPos = pathContainer.GetPathPoints();
            waypoints = new Vector3[max];

            //fill array with positions
            for (int i = 1; i < max; i++)
                waypoints[i] = wpPos[i - 1];

            //set the first slot to the current position
            waypoints[0] = transform.position;

            //create HOTween tweener
            CreateTween();
            //resume tweener if paused
            if (tween.isPaused)
                tween.Play();

            //reinitialize original waypoint positions
            waypoints = pathContainer.GetPathPoints();
            //wait until we're at the first waypoint
            yield return StartCoroutine(tween.UsePartialPath(-1, 1).WaitForCompletion());
            moveToPath = false;

            //discard tweener because it was only used for this option
            if (tween != null) 
                tween.Kill();
            tween = null;
        }


        //creates a new HOTween tween with give arguments that moves along the path
        private void CreateTween()
        {
            //prepare HOTween's parameters, you can look them up here
            //http://www.holoville.com/hotween/documentation.html

            //create new plugin for curved paths
            //pass in array of Vector3 waypoint positions, relative = true
            plugPath = new PlugVector3Path(waypoints, true, pathType);

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
            tParms.Prop("position", plugPath);

            //additional tween parameters
            tParms.AutoKill(false);
            tParms.Loops(1);
            if(!moveToPath)
                tParms.OnComplete(ReachedEnd);

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


        //object reached the end of its path
        private void ReachedEnd()
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
                    return;

                //in a loop we start from the beginning
                case LoopType.loop:

                    tween.Restart();
                    break;

                //on LoopType.pingPong, we toggle the "repeat" boolean and reverse waypoints
                case LoopType.pingPong:

                    //discard tweener to construct a new one
                    if (tween != null)
                        tween.Kill();
                    tween = null;

                    //reverse waypoints
                    repeat = !repeat;
                    Array.Reverse(waypoints);

                    //create tweener for next iteration
                    CreateTween();
                    break;
            }
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
                transform.position = pathContainer.waypoints[0].position;
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
    }
}