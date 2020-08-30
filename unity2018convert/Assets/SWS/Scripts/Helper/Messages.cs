/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SWS
{
    /// <summary>
    /// Contains messages, their options and methods for manipulation or execution.
    /// <summary>
    [System.Serializable]
    public class Messages
    {
        /// <summary>
        /// List of messages (names, values) per waypoint.
        /// <summary>
        public List<MessageOptions> list = new List<MessageOptions>();


        /// <summary>
        /// If the message count is not equal to the desired path length,
        /// this method re-initializes its message slots to the corresponding size.
        /// </summary>
        public void Initialize(int size)
        {
            //message count is smaller than waypoint count,
            //add empty message per waypoint and refill with default values
            for (int i = list.Count; i <= size; i++)
                list.Add(AddEmptyToOption(new MessageOptions()));
        }


        /// <summary>
        /// Adds a new message slot with default values to an existing message option.
        /// <summary>
        public MessageOptions AddEmptyToOption(MessageOptions opt)
        {
            opt.message.Add("");
            opt.type.Add(MessageOptions.ValueType.None);
            opt.obj.Add(null);
            opt.text.Add(null);
            opt.num.Add(0);
            opt.vect2.Add(Vector2.zero);
            opt.vect3.Add(Vector3.zero);
            return opt;
        }


        /// <summary>
        /// Fills unused message slots of an option with default values.
        /// <summary>
        public void FillOptionWithValues(MessageOptions opt)
        {
            int size = opt.message.Count;
            if (opt.type.Count < size) opt.type.Add(MessageOptions.ValueType.None);
            if (opt.obj.Count< size) opt.obj.Add(null);
            if (opt.text.Count < size) opt.text.Add(null);
            if (opt.num.Count < size) opt.num.Add(0);
            if (opt.vect2.Count < size) opt.vect2.Add(Vector2.zero);
            if (opt.vect3.Count < size) opt.vect3.Add(Vector3.zero);
        }


        /// <summary>
        /// Gets the message option at a specific waypoint.
        /// <summary>
        public MessageOptions GetMessageOption(int waypoint)
        {
            //in case message options weren't used before 
            Initialize(waypoint);

            //get message option at waypoint
            MessageOptions opt = list[waypoint];

            //returns message option
            return opt;
        }


        /// <summary>
        /// Executes all messages at a specific waypoint index on a movement script.
        /// <summary>
        public void Execute(MonoBehaviour mono, int index)
        {
            //skip execution if no messages were set
            if (list == null || list.Count - 1 < index
                || list[index].message == null)
                return;

            //loop through messages for this waypoint
            for (int i = 0; i < list[index].message.Count; i++)
            {
                //if no message name was defined, skip further execution
                if (list[index].message[i] == "")
                    continue;

                //else store MessageOption at this waypoint
                MessageOptions mess = list[index];
                //differ between various data types and pass in the corresponding value
                switch (mess.type[i])
                {
                    case MessageOptions.ValueType.None:
                        mono.SendMessage(mess.message[i], SendMessageOptions.DontRequireReceiver);
                        break;
                    case MessageOptions.ValueType.Object:
                        mono.SendMessage(mess.message[i], mess.obj[i], SendMessageOptions.DontRequireReceiver);
                        break;
                    case MessageOptions.ValueType.Text:
                        mono.SendMessage(mess.message[i], mess.text[i], SendMessageOptions.DontRequireReceiver);
                        break;
                    case MessageOptions.ValueType.Numeric:
                        mono.SendMessage(mess.message[i], mess.num[i], SendMessageOptions.DontRequireReceiver);
                        break;
                    case MessageOptions.ValueType.Vector2:
                        mono.SendMessage(mess.message[i], mess.vect2[i], SendMessageOptions.DontRequireReceiver);
                        break;
                    case MessageOptions.ValueType.Vector3:
                        mono.SendMessage(mess.message[i], mess.vect3[i], SendMessageOptions.DontRequireReceiver);
                        break;
                }
            }
        }
    }


    /// <summary>
    /// Available message types to be called.
    /// Stores multiple messages along with their values for one waypoint.
    /// <summary>
    [System.Serializable]
    public class MessageOptions
    {
        /// <summary>
        /// Name of the method to call (message).
        /// <summary>
        public List<string> message = new List<string>();
        /// <summary>
        /// Object value.
        /// <summary>
        public List<UnityEngine.Object> obj = new List<UnityEngine.Object>();
        /// <summary>
        /// Text value.
        /// <summary>
        public List<string> text = new List<string>();
        /// <summary>
        /// Numerical value.
        /// <summary>
        public List<float> num = new List<float>();
        /// <summary>
        /// Vector2 value.
        /// <summary>
        public List<Vector2> vect2 = new List<Vector2>();
        /// <summary>
        /// Vector3 value.
        /// <summary>
        public List<Vector3> vect3 = new List<Vector3>();
        /// <summary>
        /// Available data types for message parameters.
        /// <summary>
        public enum ValueType
        {
            None,
            Object,
            Text,
            Numeric,
            Vector2,
            Vector3
        }
        public List<ValueType> type = new List<ValueType>();
        /// <summary>
        /// Path position (only used for bezier paths).
        /// <summary>
        public float pos;
    }
}
