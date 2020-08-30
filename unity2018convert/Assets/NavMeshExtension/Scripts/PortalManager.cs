/*  This file is part of the "NavMesh Extension" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NavMeshExtension
{
    /// <summary>
    /// Stores portal path trees and provides methods for getting the shortest path.
    /// </summary>
    public class PortalManager : MonoBehaviour
    {
        //manager reference
        private static PortalManager instance;

        /// <summary>
        /// Dictionary for portal containers and its children, the actual portal transforms.
        /// </summary>
        public static readonly Dictionary<Transform, List<Transform>> portals = new Dictionary<Transform, List<Transform>>();


        //initializes the portal tree
        void Awake()
        {
            instance = this;
            Init();
        }


        //static dictionaries keep their variables between scenes,
        //we don't want that to happen - clear the path dictionary
        //whenever this object gets destroyed (e.g. on scene change)
        void OnDestroy()
        {
            portals.Clear();
        }


        /// <summary>
        /// Populate portal dictionary with transforms.
        /// </summary>
        public static void Init()
        {
            foreach (Transform parent in instance.transform)
            {
                if (!portals.ContainsKey(parent))
                {
                    portals.Add(parent, new List<Transform>());

                    foreach (Transform child in parent)
                    {
                        portals[parent].Add(child);
                    }
                }
            }
        }


        //returns the complement position of a portal 
        static Vector3 GetComplement(Transform parent, Vector3 portal)
        {
            Vector3 pos;

            if (portals[parent][0].position == portal)
                pos = portals[parent][1].position;
            else
                pos = portals[parent][0].position;

            return pos;
        }


        //returns the nearest portal of a container, based on the position 
        static Vector3? FindNearest(Transform parent, Vector3 pos)
        {
            //initialize variables
            UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
            Vector3? nearest = null;
            float distance = Mathf.Infinity;

            //don't continue if the parent does not exist
            if (!portals.ContainsKey(parent)) return null;

            //loop over portals of this portal container,
            //find the shortest NavMesh path to a portal
            for (int i = 0; i < portals[parent].Count; i++)
            {
                Vector3 portal = portals[parent][i].position;
                float length = Mathf.Infinity;
                //let Unity calculate the path and set length, if valid
                if (UnityEngine.AI.NavMesh.CalculatePath(pos, portal, -1, path)
                    && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    length = PathLength(path);

                if (length < distance)
                {
                    distance = length;
                    nearest = portal;
                }
            }

            return nearest;
        }


        //returns the length of a Unity-constructed NavMesh path
        static float PathLength(UnityEngine.AI.NavMeshPath path)
        {
            //passed in path is too short
            if (path.corners.Length < 2)
                return 0f;

            //initialize variables
            Vector3 previousCorner = path.corners[0];
            float lengthSoFar = 0.0f;
            int i = 1;

            //iterate over corners for the full path length
            while (i < path.corners.Length)
            {
                Vector3 currentCorner = path.corners[i];
                lengthSoFar += Vector3.Distance(previousCorner, currentCorner);
                previousCorner = currentCorner;
                i++;
            }

            return lengthSoFar;
        }


        /// <summary>
        /// Returns the shortest path based on all portals available.
        /// </summary>
        public static Vector3[] GetPath(Vector3 start, Vector3 target)
        {
            PortalPath path = new PortalPath();
            path.pos = start;
            path.final = target;
            return path.GetShortestPath();
        }


        /// <summary>
        /// Class that receives start and end points to build path nodes.
        /// </summary>
        [System.Serializable]
        public class PortalPath
        {
            /// <summary>
            /// Starting point of the path.
            /// </summary>
            public Vector3 pos;

            /// <summary>
            /// Desired target destination of the path.
            /// </summary>
            public Vector3 final;


            /// <summary>
            /// Calculates the shortest path of all portal tree branches.
            /// </summary>
            public Vector3[] GetShortestPath()
            {
                //get list of all portal transforms
                List<Transform> myNodes = new List<Transform>();
                foreach (Transform portal in portals.Keys)
                    myNodes.Add(portal);

                //get the full tree of possible path branches
                List<PortalNode> tree = new List<PortalNode>();
                tree.Add(new PortalNode(this, pos));
                for (int i = 0; i < myNodes.Count; i++)
                    tree.Add(new PortalNode(pos, myNodes, i));

                //for (int i = 0; i < tree.Count; i++)
                //    Debug.Log("Tree " + i + ": " + tree[i].PrintPath());

                //let path branches calculate its length
                for (int i = 0; i < tree.Count; i++)
                    tree[i].CalculateLength(0f);

                //get the shortest path length of all branches,
                //then cache the best index of the total path 
                float min = Mathf.Infinity;
                int minIndex = 0;
                for (int i = 0; i < tree.Count; i++)
                {
                    float len = tree[i].GetShortestLength();
                    if (len < min)
                    {
                        min = len;
                        minIndex = i;
                    }

                    //Debug.Log("Shortest of " + i + ": " + len);
                }

                return tree[minIndex].GetShortestPath().ToArray();
            }


            /// <summary>
            /// Class for branch length calculation of the full path.
            /// </summary>
            [System.Serializable]
            public class PortalNode
            {
                //path instance reference
                private static PortalPath path;

                /// <summary>
                /// Starting point of the node.
                /// </summary>
                public Vector3 start;

                /// <summary>
                /// Desired target destination of the node.
                /// </summary>
                public Vector3? target;

                /// <summary>
                /// Calculated node length.
                /// </summary>
                public float length = 0f;

                /// <summary>
                /// Children nodes of this branch/node.
                /// </summary>
                public List<PortalNode> childs = new List<PortalNode>();


                /// <summary>
                /// Constructor for setting up initial starting point and final destination.
                /// </summary>
                public PortalNode(PortalPath instance, Vector3 start)
                {
                    path = instance;
                    this.start = start;

                    //initialize variables
                    UnityEngine.AI.NavMeshPath p = new UnityEngine.AI.NavMeshPath();
                    //let Unity calculate this path and set target, if valid
                    if (UnityEngine.AI.NavMesh.CalculatePath(start, instance.final, -1, p)
                        && p.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                    {
                        this.target = instance.final;
                    }
                }


                /// <summary>
                /// Constructor for setting up deeper branches of this branch.
                /// </summary>
                public PortalNode(Vector3 tar, List<Transform> nodes, int index)
                {
                    start = tar;

                    //receive portal parents from the upper branch
                    List<Transform> childNodes = new List<Transform>(nodes);
                    Transform portalParent = childNodes[index];

                    //set the nearest target to the portal destination,
                    //then find the complement and remove this portal node
                    target = FindNearest(portalParent, start);
                    if (!target.HasValue) return;

                    Vector3 com = GetComplement(portalParent, target.Value);
                    childNodes.Remove(portalParent);

                    //add direct path to the complement as a new node
                    //set up deeper branches without the previous portal container 
                    childs.Add(new PortalNode(path, com));
                    for (int i = 0; i < childNodes.Count; i++)
                        childs.Add(new PortalNode(com, childNodes, i));
                }


                /// <summary>
                /// Recursively logs node positions of this branch.
                /// </summary>
                public string PrintPath()
                {
                    string str = " " + start + "->" + target;

                    for (int i = 0; i < childs.Count; i++)
                    {
                        str += childs[i].PrintPath();
                    }

                    return str;
                }


                /// <summary>
                /// Sets the length of this branch and invokes it for all childs.
                /// </summary>
                public void CalculateLength(float parLength)
                {
                    //initialize variables
                    UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
                    float myLength = Mathf.Infinity;
                    
                    //let Unity calculate this path
                    if (target.HasValue &&
                        UnityEngine.AI.NavMesh.CalculatePath(start, target.Value, -1, path))
                    {
                        myLength = parLength + PathLength(path);
                    }

                    length = myLength;

                    //loop over children to do the same
                    for (int i = 0; i < childs.Count; i++)
                    {
                        childs[i].CalculateLength(length);
                    }
                }


                /// <summary>
                /// Loops over all child branches to find the total shortest path length.
                /// </summary>
                public float GetShortestLength()
                {
                    //don't continue at the last node/leaf
                    if (childs.Count == 0)
                        return length;

                    float min = Mathf.Infinity;
                    for (int i = 0; i < childs.Count; i++)
                    {
                        float len = childs[i].GetShortestLength();
                        if (len < min)
                            min = len;
                    }

                    return min;
                }


                /// <summary>
                /// Recursively gets node positions of the shortest branch.
                /// </summary>
                public List<Vector3> GetShortestPath()
                {
                    //initialize list with positions of this node
                    List<Vector3> nodes = new List<Vector3>();
                    if (target.HasValue)
                    {
                        nodes.Add(start);
                        nodes.Add(target.Value);
                    }

                    //we're done without target or children
                    if (!target.HasValue || childs.Count == 0)
                        return nodes;

                    //do the same as in GetShortestLength(),
                    //but here we also keep the shortest node index
                    float min = Mathf.Infinity;
                    int index = 0;
                    for (int i = 0; i < childs.Count; i++)
                    {
                        float len = childs[i].GetShortestLength();
                        if (len < min)
                        {
                            min = len;
                            index = i;
                        }
                    }

                    //add best/shortest branch to the final list
                    nodes.AddRange(childs[index].GetShortestPath());
                    return nodes;
                }
            }
        }
    }
}