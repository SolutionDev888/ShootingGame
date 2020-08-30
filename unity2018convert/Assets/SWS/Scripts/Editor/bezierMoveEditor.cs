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
    /// Custom inspector for bezierMove.
    /// <summary>
    [CustomEditor(typeof(bezierMove))]
    public class bezierMoveEditor : moveEditor
    {
        //returns BezierPathManager component for later use
        public new BezierPathManager GetPathTransform()
        {
            return m_Object.FindProperty("pathContainer").objectReferenceValue as BezierPathManager;
        }

        //method to set the message path progress position - where to call the message
        private void SetMessageKeyPos(int index, float value)
        { m_List.GetArrayElementAtIndex(index).FindPropertyRelative("pos").floatValue = value; }


        //override inspector gui of moveEditor
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
            EditorGUILayout.PropertyField(m_Object.FindProperty("sizeToAdd"));

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
            EditorGUILayout.PropertyField(m_Object.FindProperty("loopType"));
            EditorGUILayout.PropertyField(m_Object.FindProperty("lockPosition"));

            //get Path Manager component
            var path = GetPathTransform();
            EditorGUILayout.Space();
            EditorGUIUtility.LookLikeControls();

            //draw bold delay settings label
            GUILayout.Label("Settings:", EditorStyles.boldLabel);

            //check whether a Bezier Path Manager component is set, if not display a label
            if (path == null)
                GUILayout.Label("No path set.");
            else
            {
                //draw message options
                MessageSettings();
            }

            //we push our modified variables back to our serialized object
            m_Object.ApplyModifiedProperties();
        }


        //override message settings of moveEditor
        public override void MessageSettings()
        {
            //path is set and display is enabled
            if (showMessageSetup)
            {
                //draw button for hiding message settings
                if (GUILayout.Button("Hide Message Settings"))
                    showMessageSetup = false;

                EditorGUILayout.BeginHorizontal();

                //delete all message values and disable settings
                if (GUILayout.Button("Deactivate all Messages"))
                {
                    //display custom dialog and wait for user input to delete all message values
                    if (EditorUtility.DisplayDialog("Are you sure?",
                        "This will delete all message slots to reduce memory usage.",
                        "Continue",
                        "Cancel"))
                    {
                        m_List.arraySize = 0;
                        showMessageSetup = false;
                        return;
                    }
                }

                EditorGUILayout.EndHorizontal();

                //button to insert a new message at the end
                if (GUILayout.Button("+ Add new Message +"))
                {
                    //increase the message count by one
                    m_List.arraySize++;
                    mList = GetMessageList();
                    //insert a new callable message,
                    //only if the message has no slot already
                    if (mList[mList.Count - 1].message.Count == 0)
                        AddMessageOption(mList.Count - 1);

                    //when adding a new message, the values from the latest message
                    //gets copied to the new one. We want a fresh message instead,
                    //so we remove all other slots until one slot remains
                    for (int i = mList[mList.Count - 1].message.Count - 1; i > 0; i--)
                        RemoveMessageOption(mList.Count - 1);
                }

                //begin a scrolling view inside GUI, pass in Vector2 scroll position 
                scrollPosMessage = EditorGUILayout.BeginScrollView(scrollPosMessage, GUILayout.Height(245));
                //get modifiable list of MessageProperties
                mList = GetMessageList();

                //loop through list
                for (int i = 0; i < mList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    //draw label with waypoint index, display total count of messages for this waypoint
                    EditorGUILayout.HelpBox("Message " + i + " - Message Count: " + mList[i].message.Count, MessageType.None);

                    //button to add new message to this waypoint
                    if (GUILayout.Button("+"))
                        AddMessageOption(i);

                    //button to remove last message from this waypoint
                    if (GUILayout.Button("-"))
                    {
                        RemoveMessageOption(i);
                        //delete the message if no slots are left
                        if (mList[i].message.Count == 1)
                            m_List.DeleteArrayElementAtIndex(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();

                    //display text box with path progress input field (call position),
                    //clamped between 0 (start) and 1 (end)
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Call at path position in %:", MessageType.None);
                    float callAtPosField = EditorGUILayout.FloatField(mList[i].pos, GUILayout.Width(60));
                    callAtPosField = Mathf.Clamp01(callAtPosField);
                    EditorGUILayout.EndHorizontal();

                    //loop through messages
                    for (int j = 0; j < mList[i].message.Count; j++)
                    {
                        //display text box and message name input field
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("Method:", MessageType.None);
                        string messageField = EditorGUILayout.TextField(mList[i].message[j]);
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();

                        //display enum for selectable data types
                        //declare variables for each data type for storing their input value
                        MessageOptions.ValueType typeField = (MessageOptions.ValueType)EditorGUILayout.EnumPopup(mList[i].type[j]);
                        Object objField; string textField; float numField; Vector2 vect2Field; Vector3 vect3Field;

                        //draw corresponding data type field for selected enum type
                        //store input in the values above. if the field has changed,
                        //set the corresponding type value for the current MessageOption 
                        switch (typeField)
                        {
                            case MessageOptions.ValueType.None:
                                break;
                            case MessageOptions.ValueType.Object:
                                objField = EditorGUILayout.ObjectField(mList[i].obj[j], typeof(Object), true);
                                if (GUI.changed) SetMessageOption(i, "obj", j, objField);
                                break;
                            case MessageOptions.ValueType.Text:
                                textField = EditorGUILayout.TextField(mList[i].text[j]);
                                if (GUI.changed) SetMessageOption(i, "text", j, textField);
                                break;
                            case MessageOptions.ValueType.Numeric:
                                numField = EditorGUILayout.FloatField(mList[i].num[j]);
                                if (GUI.changed) SetMessageOption(i, "num", j, numField);
                                break;
                            case MessageOptions.ValueType.Vector2:
                                vect2Field = EditorGUILayout.Vector2Field("", mList[i].vect2[j]);
                                if (GUI.changed) SetMessageOption(i, "vect2", j, vect2Field);
                                break;
                            case MessageOptions.ValueType.Vector3:
                                vect3Field = EditorGUILayout.Vector3Field("", mList[i].vect3[j]);
                                if (GUI.changed) SetMessageOption(i, "vect3", j, vect3Field);
                                break;
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Separator();

                        //regardless of the data type,
                        //set the call position, message name and enum type of this MessageOption
                        if (GUI.changed)
                        {
                            SetMessageKeyPos(i, callAtPosField);
                            SetMessageOption(i, "message", j, messageField);
                            SetMessageOption(i, "type", j, typeField);
                        }
                    }
                }
                //ends the scrollview defined above
                EditorGUILayout.EndScrollView();
            }
            else
            {
                //draw button to toggle showDelaySetup
                if (GUILayout.Button("Show Message Settings"))
                    showMessageSetup = true;
            }
        }


        //if this path is selected, display small info boxes
        //-above all waypoint positions
        //-at the approximate message position on the path
        void OnSceneGUI()
        {
            //get Bezier Path Manager component
            var path = GetPathTransform();

            //do not execute further code if we have no path defined
            if (path == null) return;
            mList = GetMessageList();

            //begin GUI block
            Handles.BeginGUI();
            //loop through waypoint array
            for (int i = 0; i < path.bPoints.Count; i++)
            {
                //translate waypoint vector3 position in world space into a position on the screen
                var guiPoint = HandleUtility.WorldToGUIPoint(path.bPoints[i].wp.position);
                //create rectangle with that positions and do some offset
                var rect = new Rect(guiPoint.x - 50.0f, guiPoint.y - 40, 100, 20);
                //draw box at rect position with current waypoint name
                GUI.Box(rect, "Waypoint: " + i);
            }
            Handles.EndGUI(); //end GUI block
        }
    }
}