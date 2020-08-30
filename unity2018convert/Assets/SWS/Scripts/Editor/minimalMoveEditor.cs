/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace SWS
{
    /// <summary>
    /// Custom inspector for minimalMove.
    /// <summary>
    [CustomEditor(typeof(minimalMove))]
    public class minimalMoveEditor : moveEditor
    {
        //called whenever this inspector window is loaded 
        public override void OnEnable()
        {
            //we create a reference to our script object by passing in the target
            m_Object = new SerializedObject(target);
        }


        //called whenever the inspector gui gets rendered
        public override void OnInspectorGUI()
        {
            //this pulls the relative variables from unity runtime and stores them in the object
            m_Object.Update();
            //DrawDefaultInspector();

            //draw custom variable properties by using serialized properties
            EditorGUILayout.PropertyField(m_Object.FindProperty("pathContainer"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("pathType"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("onStart"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("moveToPath"));

            SerializedProperty orientToPath = m_Object.FindProperty("orientToPath");
            EditorGUILayout.PropertyField(orientToPath);
            if (orientToPath.boolValue)
            {
                EditorGUILayout.PropertyField(m_Object.FindProperty("lookAhead"));
                EditorGUILayout.PropertyField(m_Object.FindProperty("lockAxis"));
            }

            SerializedProperty timeValue = m_Object.FindProperty("timeValue");
            EditorGUILayout.PropertyField(timeValue);
            if (timeValue.enumValueIndex == 0)
            {
                SerializedProperty easeType = m_Object.FindProperty("easeType");
                EditorGUILayout.PropertyField(easeType);
                if (easeType.enumValueIndex == 31)
                    EditorGUILayout.PropertyField(m_Object.FindProperty("animEaseType"));
            }
            EditorGUILayout.PropertyField(m_Object.FindProperty("speed"));

            SerializedProperty loopType = m_Object.FindProperty("loopType");
            EditorGUILayout.PropertyField(loopType);
            if (loopType.enumValueIndex == 1)
                EditorGUILayout.PropertyField(m_Object.FindProperty("closeLoop"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("lockPosition"));

            //we push our modified variables back to our serialized object
            m_Object.ApplyModifiedProperties();
        }
    }
}