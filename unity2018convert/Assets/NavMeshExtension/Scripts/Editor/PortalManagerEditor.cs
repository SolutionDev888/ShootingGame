/*  This file is part of the "NavMesh Extension" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace NavMeshExtension
{
    /// <summary>
    /// Portal Manager Editor for creating new portal objects.
    /// <summary>
    [CustomEditor(typeof(PortalManager))]
    public class PortalManagerEditor : Editor
    {
        //manager reference
        private PortalManager script;


        void OnEnable()
        {
            script = (PortalManager)target;
        }


        /// <summary>
        /// Custom inspector override for buttons.
        /// </summary>
        public override void OnInspectorGUI()
        {
            //create new portal button
            if (GUILayout.Button("New Portal"))
            {
                CreateNewPortal();
            }
        }


        /// <summary>
        /// Creates a portal container gameobject with portals as its children.
        /// </summary>
        public void CreateNewPortal()
        {
            //get the current unix timestamp for a unique naming scheme
            var epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            string timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds.ToString("F0");

            //create portal container with unique name
            GameObject portalGO = new GameObject();
            portalGO.transform.parent = script.transform;
            portalGO.name = "Portal " + timestamp;
            portalGO.AddComponent<PortalObject>();

            //try to get a position in the center of our scene view,
            //one near position directly at the camera and a position 10 units far away
            Camera sceneCam = NavMeshManagerEditor.GetSceneView().camera;
            Vector3 camNear = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
            Vector3 camFar = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            //calculate direction from near to far position
            Vector3 dir = (camFar - camNear).normalized;
            //set the portal container to the far position as default
            portalGO.transform.position = camFar;

            //raycast in the center of the scene view,
            //if the ray hits something the portal container gets positioned there
            RaycastHit hit;
            if (Physics.Raycast(camNear, dir, out hit))
            {
                portalGO.transform.position = hit.point;
            }

            //create child objects/portals
            for (int i = 0; i < 2; i++)
            {
                GameObject child = new GameObject("" + i);
                child.transform.parent = portalGO.transform;
            }

            //reposition them around the portal container
            portalGO.transform.GetChild(0).position = portalGO.transform.position + Vector3.left;
            portalGO.transform.GetChild(1).position = portalGO.transform.position + Vector3.right;

            Undo.RegisterCreatedObjectUndo(portalGO, "Created Portal");
            Selection.activeGameObject = portalGO;
        }
    }
}
