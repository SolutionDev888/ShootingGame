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
    /// Serves as template for movement script editor inspectors.
    /// <summary>
    //[CustomEditor(typeof(...))]
    public class moveEditor : Editor
    {
        //define Serialized Objects we want to use/control
        //this will be our serialized reference to the inspected script
        public SerializedObject m_Object;

        //delay array size, define path to know where to lookup for this variable
        //(we expect an array, so it's "name_of_array.data_type.size")
        public string spArraySize = "delays.Array.size";
        //.data gives us the data of the array,
        //we replace this {0} token with an index we want to get
        public string spArrayData = "delays.Array.data[{0}]";

        //local delays array
        private float[] delayArray;
        //inspector scrollbar x/y position, modified by mouse input
        private Vector2 scrollPosDelay;
        //whether delay settings menu should be visible
        public bool showDelaySetup = false;
        //variable to set all delay slots to this value
        private float delayAll = 0;

        //serialized message list property
        public SerializedProperty m_List;
        //local MessageOptions list
        public List<MessageOptions> mList;
        //inspector scrollbar x/y position, modified by mouse input
        public Vector2 scrollPosMessage;
        //whether message settings menu should be visible
        public bool showMessageSetup = false;


        //called whenever this inspector window is loaded 
        public virtual void OnEnable()
        {
            //we create a reference to our script object by passing in the target
            m_Object = new SerializedObject(target);
            //get reference to message list
            m_List = m_Object.FindProperty("messages").FindPropertyRelative("list");
        }


        //returns PathManager component for later use
        public virtual PathManager GetPathTransform()
        {
            //get pathContainer from serialized property and return its PathManager component
            return m_Object.FindProperty("pathContainer").objectReferenceValue as PathManager;
        }


        private float[] GetDelayArray()
        {
            //get array count from Path Manager component by accessing waypoints length,
            //and store value into var arrayCount. why length+1?
            //here we do a little trick: when modifying the last waypoint delay value
            //(e.g. to 5) and switching the path of this walker, all later waypoints would
            //have a 5 second delay too. So we increase the length by 1 which will stay empty.
            var arrayCount = GetPathTransform().waypoints.Length + 1;
            //create new float array with size of arrayCount
            var floatArray = new float[arrayCount];

            //get delay array count from serialized property
            //and store its int value into var array
            var array = m_Object.FindProperty(spArraySize);
            //resize array to waypoint array count
            array.intValue = arrayCount;

            //loop over waypoints
            for (var i = 0; i < arrayCount; i++)
            {
                //for each one use "FindProperty" to get the associated object reference
                //of the delays array, string.Format replaces {0} token with index i
                //and store the object reference value as type of float in floatArray[i]
                floatArray[i] = m_Object.FindProperty(string.Format(spArrayData, i)).floatValue;
            }
            //finally return that array copy for modification purposes
            return floatArray;
        }


        //similiar to GetDelay(), find serialized property which belongs to an index
        //and set this value to parameter float "value" directly
        private void SetDelay(int index, float value)
        {
            m_Object.FindProperty(string.Format(spArrayData, index)).floatValue = value;
        }


        //similiar to SetDelay(), this will return the delay value at an index
        public virtual float GetDelay(int index)
        {
            return m_Object.FindProperty(string.Format(spArrayData, index)).floatValue;
        }


        //return message list
        //we create a new list and fill it with all given values for serialization
        public List<MessageOptions> GetMessageList()
        {
            //create new list for returning
            List<MessageOptions> msgOpt = new List<MessageOptions>();

            //loop through whole list
            for (int i = 0; i < m_List.arraySize; i++)
            {
                //get serialized MessageOption slot
                SerializedProperty slot = m_List.GetArrayElementAtIndex(i);
                //create new MessageOption to store values
                MessageOptions opt = new MessageOptions();
                //store values of serialized MessageOption properties
                SerializedProperty msgList = slot.FindPropertyRelative("message");
                SerializedProperty typeList = slot.FindPropertyRelative("type");
                SerializedProperty objList = slot.FindPropertyRelative("obj");
                SerializedProperty textList = slot.FindPropertyRelative("text");
                SerializedProperty numList = slot.FindPropertyRelative("num");
                SerializedProperty vect2List = slot.FindPropertyRelative("vect2");
                SerializedProperty vect3List = slot.FindPropertyRelative("vect3");

                //loop through values of this MessageOption
                for (int j = 0; j < msgList.arraySize; j++)
                {
                    //fill created MessageOption with serialized values
                    opt.message.Add(msgList.GetArrayElementAtIndex(j).stringValue);

                    //cancel if opened at runtime without specification of message settings before
                    if (typeList.arraySize != msgList.arraySize)
                    {
                        Debug.LogWarning("Resized Message List at runtime! Please open Message Settings in the editor first.");
                        return null;
                    }

                    opt.type.Add((MessageOptions.ValueType)typeList.GetArrayElementAtIndex(j).enumValueIndex);
                    opt.obj.Add(objList.GetArrayElementAtIndex(j).objectReferenceValue);
                    opt.text.Add(textList.GetArrayElementAtIndex(j).stringValue);
                    opt.num.Add(numList.GetArrayElementAtIndex(j).floatValue);
                    opt.vect2.Add(vect2List.GetArrayElementAtIndex(j).vector2Value);
                    opt.vect3.Add(vect3List.GetArrayElementAtIndex(j).vector3Value);
                }
                //add filled MessageOption to the list
                msgOpt.Add(opt);
            }
            //return final list of messages
            return msgOpt;
        }


        //add new MessageOption at the end of waypoint's message list 
        public void AddMessageOption(int index)
        {
            //get MessageOption list at index and extend every value by one
            SerializedProperty slot = m_List.GetArrayElementAtIndex(index);
            slot.FindPropertyRelative("message").arraySize++;
            slot.FindPropertyRelative("type").arraySize++;
            slot.FindPropertyRelative("obj").arraySize++;
            slot.FindPropertyRelative("text").arraySize++;
            slot.FindPropertyRelative("num").arraySize++;
            slot.FindPropertyRelative("vect2").arraySize++;
            slot.FindPropertyRelative("vect3").arraySize++;
        }


        //remove MessageOption at the end of waypoint's message list 
        public void RemoveMessageOption(int index)
        {
            //get MessageOption list of index and decrease every value by one
            SerializedProperty slot = m_List.GetArrayElementAtIndex(index);
            slot.FindPropertyRelative("message").arraySize--;
            slot.FindPropertyRelative("type").arraySize--;
            slot.FindPropertyRelative("obj").arraySize--;
            slot.FindPropertyRelative("text").arraySize--;
            slot.FindPropertyRelative("num").arraySize--;
            slot.FindPropertyRelative("vect2").arraySize--;
            slot.FindPropertyRelative("vect3").arraySize--;
        }


        //methods to set a MessageOption value of a given waypoint
        //these are overwritten for every available data type value
        //parameters are list index, property, data type slot, type value
        public void SetMessageOption(int index, string field, int slot, string value)
        { m_List.GetArrayElementAtIndex(index).FindPropertyRelative(field).GetArrayElementAtIndex(slot).stringValue = value; }

        public void SetMessageOption(int index, string field, int slot, MessageOptions.ValueType value)
        { m_List.GetArrayElementAtIndex(index).FindPropertyRelative(field).GetArrayElementAtIndex(slot).enumValueIndex = (int)value; }

        public void SetMessageOption(int index, string field, int slot, Object value)
        { m_List.GetArrayElementAtIndex(index).FindPropertyRelative(field).GetArrayElementAtIndex(slot).objectReferenceValue = value; }

        public void SetMessageOption(int index, string field, int slot, float value)
        { m_List.GetArrayElementAtIndex(index).FindPropertyRelative(field).GetArrayElementAtIndex(slot).floatValue = value; }

        public void SetMessageOption(int index, string field, int slot, Vector2 value)
        { m_List.GetArrayElementAtIndex(index).FindPropertyRelative(field).GetArrayElementAtIndex(slot).vector2Value = value; }

        public void SetMessageOption(int index, string field, int slot, Vector3 value)
        { m_List.GetArrayElementAtIndex(index).FindPropertyRelative(field).GetArrayElementAtIndex(slot).vector3Value = value; }


        //called whenever the inspector gui gets rendered
        public override void OnInspectorGUI()
        {
            //this pulls the relative variables from unity runtime and stores them in the object
            m_Object.Update();

            //show default variables in inspector
            DrawDefaultInspector();

            //get Path Manager component
            var path = GetPathTransform();
            EditorGUILayout.Space();
            EditorGUIUtility.LookLikeControls();

            //draw bold delay settings label
            GUILayout.Label("Settings:", EditorStyles.boldLabel);

            //check whether a Path Manager component is set, if not display a label
            if (path == null)
            {
                GUILayout.Label("No path set.");

                //get delay array count from serialized property and resize it to zero
                //(in case of previously defined delay settings, clear old data)
                //do the same with message list properties
                m_Object.FindProperty(spArraySize).intValue = 0;
                m_List.arraySize = 0;
            }
            else
            {
                //draw delay options
                DelaySettings();
                EditorGUILayout.Separator();
                //draw message options
                MessageSettings();
            }

            //we push our modified variables back to our serialized object
            m_Object.ApplyModifiedProperties();
        }


        public void DelaySettings()
        {
            //path is set and boolean for displaying delay settings is true
            //(button below was clicked)
            if (showDelaySetup)
            {
                //get delays array reference
                delayArray = GetDelayArray();

                EditorGUILayout.BeginHorizontal();
                //begin a scrolling view inside GUI, pass in Vector2 scroll position 
                scrollPosDelay = EditorGUILayout.BeginScrollView(scrollPosDelay, GUILayout.Height(105));

                //loop through waypoint array
                for (int i = 0; i < GetPathTransform().waypoints.Length; i++)
                {
                    GUILayout.BeginHorizontal();
                    //draw label with waypoint index,
                    GUILayout.Label(i + ".", GUILayout.Width(20));
                    //create a float field for every waypoint delay slot
                    var result = EditorGUILayout.FloatField(delayArray[i], GUILayout.Width(50));

                    //if the float field has changed, set waypoint delay to new input
                    //(within serialized array property)
                    if (GUI.changed)
                        SetDelay(i, result);

                    GUILayout.EndHorizontal();
                }
                //ends the scrollview defined above
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginVertical();

                //draw button for hiding of delay settings
                if (GUILayout.Button("Hide Delay Settings"))
                    showDelaySetup = false;

                //draw button to set all delay value slots
                if (GUILayout.Button("Set All:"))
                {
                    for (int i = 0; i < delayArray.Length; i++)
                        SetDelay(i, delayAll);
                }

                //create a float field for being able to change variable delayAll
                delayAll = EditorGUILayout.FloatField(delayAll, GUILayout.Width(50));

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                //if path is set but delay settings are not shown,
                //draw button to toggle showDelaySetup
                if (GUILayout.Button("Show Delay Settings"))
                    showDelaySetup = true;
            }
        }


        public virtual void MessageSettings()
        {
            //path is set and boolean for displaying message settings is true
            if (showMessageSetup)
            {
                //draw button for hiding message settings
                if (GUILayout.Button("Hide Message Settings"))
                    showMessageSetup = false;

                EditorGUILayout.BeginHorizontal();

                //clear message values
                if (GUILayout.Button("Clear"))
                {
                    //display custom dialog and wait for user input to clear all message values
                    if (EditorUtility.DisplayDialog("Are you sure?",
                        "Do you really want to reset all messages and their values?",
                        "Continue",
                        "Cancel"))
                    {
                        m_List.arraySize = 0;
                    }
                }

                //delete all message values and disable settings so they are not recreated again
                if (GUILayout.Button("Deactivate"))
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

                //begin a scrolling view inside GUI, pass in Vector2 scroll position 
                scrollPosMessage = EditorGUILayout.BeginScrollView(scrollPosMessage, GUILayout.Height(245));
                //get modifiable list of MessageProperties
                mList = GetMessageList();

                //loop through list
                for (int i = 0; i < mList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    //draw label with waypoint index, display total count of messages for this waypoint
                    EditorGUILayout.HelpBox(i + ". Waypoint - " + "Messages: " + mList[i].message.Count, MessageType.None);

                    //add new message to this waypoint
                    if (GUILayout.Button("+"))
                        AddMessageOption(i);

                    //remove last message from this waypoint
                    if (mList[i].message.Count > 1 && GUILayout.Button("-"))
                        RemoveMessageOption(i);

                    EditorGUILayout.EndHorizontal();

                    //loop through messages
                    for (int j = 0; j < mList[i].message.Count; j++)
                    {
                        //display text box and message name input field
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox("Method: ", MessageType.None);
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
                        //set the message name and enum type of this MessageOption
                        if (GUI.changed)
                        {
                            SetMessageOption(i, "message", j, messageField);
                            SetMessageOption(i, "type", j, typeField);
                        }
                    }
                }
                //ends the scrollview defined above
                EditorGUILayout.EndScrollView();

                //here we check for the last GUI pass, where the Inspector gets drawn
                //the first pass is responsible for the GUI layout of all fields,
                //and if we resize the list in the first pass it throws an error in the second pass
                //this is because the first and the second pass must have the same values on redraw
                if (Event.current.type == EventType.Repaint)
                {
                    //get total list size and set it to the waypoint size,
                    //so each waypoint has one MessageOption value
                    m_List.arraySize = GetPathTransform().waypoints.Length;
                    //loop through messages per waypoint
                    for (int i = 0; i < mList.Count; i++)
                    {
                        //if the waypoint does not contain a message option, add a new slot
                        if (mList[i].message.Count == 0)
                        {
                            AddMessageOption(i);
                            break;
                        }
                    }
                }
            }
            else
            {
                //if path is set but delay settings are not shown,
                //draw button to toggle showDelaySetup
                if (GUILayout.Button("Show Message Settings"))
                    showMessageSetup = true;
            }
        }
    }
}