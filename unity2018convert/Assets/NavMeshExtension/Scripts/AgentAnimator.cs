/*  This file is part of the "NavMesh Extension" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Collections;

namespace NavMeshExtension
{
    /// <summary>
    /// Mecanim motion animator for movement scripts.
    /// Passes speed and direction to the Mecanim controller. 
    /// <summary>
    public class AgentAnimator : MonoBehaviour
    {
        //movement script references
        private UnityEngine.AI.NavMeshAgent nAgent;
        //Mecanim animator reference
        private Animator animator;


        //getting component references
        void Start()
        {
            animator = GetComponentInChildren<Animator>();
            nAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        }


        //method override for root motion on the animator
        void OnAnimatorMove()
        {
            //init variables
            float speed = 0f;
            float angle = 0f;

            //calculate variables based on movement script:
            //get the agent's speed and calculate the rotation difference to the last frame
            speed = nAgent.velocity.magnitude;
            Vector3 velocity = Quaternion.Inverse(transform.rotation) * nAgent.desiredVelocity;
            angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;

            //push variables to the animator with some optional damping
            animator.SetFloat("Speed", speed);
            animator.SetFloat("Direction", angle, 0.15f, Time.deltaTime);
        }
    }
}