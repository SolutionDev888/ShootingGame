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
    /// Portal Object Editor for moving portals and drawing connections between them.
    /// <summary>
    [CustomEditor(typeof(PortalObject))]
    public class PortalObjectEditor : Editor
    {
        //portal object reference
        private PortalObject script;


        void OnEnable()
        {
            script = (PortalObject)target;
        }


        /// <summary>
        /// Custom inspector override for portal properties.
        /// </summary>
        public override void OnInspectorGUI()
        {
            //color field for portal gizmos and connections
            script.color = EditorGUILayout.ColorField("Color", script.color);
            SceneView.RepaintAll();

            //sum up and display distance between portals
            int childs = script.transform.childCount;
            float dist = 0f;
            for (int i = 0; i < childs - 1; i++)
                dist += Vector3.Distance(script.transform.GetChild(i).position,
                                         script.transform.GetChild(i + 1).position);
            EditorGUILayout.FloatField("Distance", dist);
        }


        /// <summary>
        /// Draw Scene GUI PositionHandles for child portals.
        /// <summary>
        public void OnSceneGUI()
        {
            //get child count
            int childs = script.transform.childCount;
            Handles.color = script.color;

            //loop over childs and draw handle at their position
            for (int i = 0; i < childs; i++)
            {
                Transform child = script.transform.GetChild(i);
                Vector3 pos = child.position;
                Handles.DrawWireDisc(pos, Vector3.up,
                        HandleUtility.GetHandleSize(pos) * 0.2f);

                child.position = Handles.PositionHandle(pos, Quaternion.identity);
            }

            //draw connections with lines
            Handles.DrawLine(script.transform.GetChild(0).position,
                             script.transform.GetChild(1).position);
        }
    }
}
