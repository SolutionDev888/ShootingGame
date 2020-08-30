/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

/// <summary>
/// Simple helper script to rotate an object over a period of time.
/// <summary>
public class RotationHelper : MonoBehaviour
{
    /// <summary>
    /// Total rotation duration.
    /// <summary>
    public float duration;

    /// <summary>
    /// Rotation value for rotating the object.
    /// <summary>
    public int rotation;


    void Start()
    {
        HOTween.To(transform, duration,
            new TweenParms()
            .Prop("rotation", new Vector3(rotation,0,0))
            .Ease(EaseType.Linear)
            .Loops(-1, LoopType.Incremental));   
    }
}
