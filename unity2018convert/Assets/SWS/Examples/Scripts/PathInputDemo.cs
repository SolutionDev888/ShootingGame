/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Collections;
using SWS;

/// <summary>
/// Example: object controlled by user input along the path
/// <summary>
public class PathInputDemo : MonoBehaviour
{
    /// <summary>
    /// Speed value to multiply the input speed with. 
    /// <summary>
    public float speedMultiplier = 10f;

    /// <summary>
    /// Object progress on the path, should be read only.
    /// <summary>
    public float progress = 0f;

    //references
    private minimalMove move;
    private Animator animator;


    //get references at start
    //initialize movement but don't start it yet
    void Start()
    {
        animator = GetComponent<Animator>();
        move = GetComponent<minimalMove>();
        move.StartMove();
        move.Pause();
        progress = 0f;
    }


    //listens to user input
    void Update()
    {
        float speed = speedMultiplier / 100f;

        //right arrow key
        if (Input.GetKey("right"))
        {
            //add a value based on time and speed to the progress to start moving right
            //also clamp it between 0-1 and get its position on the path 
            progress += Time.deltaTime * speed;
            progress = Mathf.Clamp01(progress);
            transform.position = move.tween.GetPointOnPath(progress);

            //if the object should orient to the path, we calculate another progress value
            //on the path with some offset in the correct direction and look at that position
            if (move.orientToPath)
            {
                float lookAhead = move.lookAhead > 0.01f ? move.lookAhead : 0.01f;
                transform.LookAt(move.tween.GetPointOnPath(Mathf.Clamp01(progress + lookAhead)));
            }
        }

        //left arrow key
        //same as above, but here we invert the progress direction
        if (Input.GetKey("left"))
        {
            progress -= Time.deltaTime * speed;
            progress = Mathf.Clamp01(progress);
            transform.position = move.tween.GetPointOnPath(progress);

            if (move.orientToPath)
            {
                float lookAhead = move.lookAhead > 0.01f ? move.lookAhead : 0.01f;
                lookAhead = -lookAhead;
                transform.LookAt(move.tween.GetPointOnPath(Mathf.Clamp01(progress + lookAhead)));
            }
        }

        //let Mecanim animate our object when moving,
        //otherwise set speed to zero
        if ((Input.GetKey("right") || Input.GetKey("left"))
            && progress != 0 && progress != 1)
            animator.SetFloat("Speed", move.speed);
        else
            animator.SetFloat("Speed", 0f);
    }
}
