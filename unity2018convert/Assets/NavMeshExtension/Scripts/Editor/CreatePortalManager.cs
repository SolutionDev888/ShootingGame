/*  This file is part of the "NavMesh Extension" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using UnityEditor;

namespace NavMeshExtension
{
    /// <summary>
    /// Adds a new Portal Manager gameobject to the scene.
    /// <summary>
    public class CreatePortalManager : EditorWindow
    {
        [MenuItem("Window/NavMesh Extension/Portal Manager")]
        //initialize method
        static void Init()
        {
            string managerName = "Portal Manager";
            //search for a manager object within current scene
            GameObject manager = GameObject.Find(managerName);

            //if no manager object was found
            if (manager == null)
            {
                //create a new gameobject with that name
                manager = new GameObject(managerName);
                manager.AddComponent<PortalManager>();

                Undo.RegisterCreatedObjectUndo(manager, "Created Manager");
            }

            //in both cases, select the gameobject
            Selection.activeGameObject = manager;
        }
    }
}
