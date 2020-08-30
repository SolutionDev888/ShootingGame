/*  This file is part of the "NavMesh Extension" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace NavMeshExtension
{
    /// <summary>
    /// Stores a NavMesh mesh object and lists to manipulate it.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class NavMeshObject : MonoBehaviour
    {
        /// <summary>
        /// Whether new submeshes should be created automatically.
        /// </summary>
        public bool autoSplit = false;

        /// <summary>
        /// The vertex count where new submeshes should be created, if autoSplit is true.
        /// </summary>
        public int splitAt = 4;

        /// <summary>
        /// Offset on the y-axis when adding new vertices to the mesh.
        /// </summary>
        public float yOffset = 0.015f;

        /// <summary>
        /// List of relative vertex positions for this mesh.
        /// </summary>
        [HideInInspector]
        public List<Vector3> list = new List<Vector3>();

        /// <summary>
        /// List of indices placed in the current submesh, pointing to the list of vertex positions.
        /// </summary>
        [HideInInspector]
        public List<int> current = new List<int>();

        /// <summary>
        /// List of indices for each submesh, pointing to the list of vertex positions.
        /// </summary>
        [HideInInspector]
        public List<SubPoints> subPoints = new List<SubPoints>();

        /// <summary>
        /// Reference to the mesh component of the current submesh.
        /// </summary>
        [HideInInspector]
        public Mesh subMesh;

        /// <summary>
        /// Wrapper class storing references to vertex positions for each submesh.
        /// </summary>
        [System.Serializable]
        public class SubPoints
        {
            public List<int> list = new List<int>();
        }

        /// <summary>
        /// Combines all submeshes into the mesh on this object.
        /// </summary>
        public void Combine()
        {
            //get all mesh filters, but don't continue if there are no submeshes
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
            
            //get the mesh filter on this object
            MeshFilter myFilter = meshFilters[0];
            List<CombineInstance> combine = new List<CombineInstance>();

            //add meshes to the combine instances
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].sharedMesh == null)
                    continue;

                CombineInstance c = new CombineInstance();
                c.mesh = meshFilters[i].sharedMesh;
                c.transform = meshFilters[i].transform.localToWorldMatrix;
                combine.Add(c);
            }

            //rename mesh to a more appropriate name or keep it
            string meshName = "NavMesh";
            if (myFilter.sharedMesh != null)
                meshName = myFilter.sharedMesh.name;

            //create new shared mesh from combined meshes
            myFilter.sharedMesh = new Mesh();
            myFilter.sharedMesh.name = meshName;
            myFilter.sharedMesh.CombineMeshes(combine.ToArray());
            current.Clear();

            //list of vertices and triangles
            List<Vector3> vertices = new List<Vector3>(myFilter.sharedMesh.vertices);
            List<int> triangles = new List<int>(myFilter.sharedMesh.triangles);
            //convert vertex positions into relative positions
            for (int i = 0; i < vertices.Count; i++)
                vertices[i] = transform.InverseTransformPoint(vertices[i]);

            /*
            string str = "";
            for (int i = 0; i < triangles.Count; i++)
                str += triangles[i] + " ";
            
            Debug.Log("BEFORE tris: " + str);
            */

            //find duplicated vertex positions
            List<int> dupIndices = new List<int>();
            List<Vector3> duplicates = vertices.GroupBy(x => x)
                                       .Where(x => x.Count() > 1)
                                       .Select(x => x.Key)
                                       .ToList();

            //Debug.Log("duplicates: " + duplicates.Count);

            //loop over duplicates to find vertex indices,
            //also overwrite indices with the first occurence in the triangle array
            for (int i = 0; i < duplicates.Count; i++)
            {
                //get all occurences of duplicated indices
                List<int> indices = vertices.Select((value, index) => new { value, index })
                          .Where(a => Vector3.Equals(a.value, duplicates[i]))
                          .Select(a => a.index).ToList();

                //get first occurence
                int unique = indices[0];
                indices.RemoveAt(0);

                //loop over duplicated indices
                for (int j = 0; j < indices.Count; j++)
                {
                    //get this duplicate
                    int dupIndex = indices[j];
                    //get all matches in the triangle array
                    List<int> matches = Enumerable.Range(0, triangles.Count)
                                        .Where(v => triangles[v] == dupIndex)
                                        .ToList();

                    //overwrite duplicated matches with the unique index
                    for (int k = 0; k < matches.Count; k++)
                    {
                        //Debug.Log("overwriting index: " + matches[j] + " with: " + first);
                        triangles[matches[k]] = unique;
                    }

                    //remember for later, when we are merging vertices
                    dupIndices.Add(dupIndex);
                }
            }

            //sort duplicated indices in a descending order
            dupIndices = dupIndices.OrderByDescending(x => x).ToList();

            //loop over indices
            for (int i = 0; i < dupIndices.Count; i++)
            {
                //remove the vertex
                int dupIndex = dupIndices[i];
                vertices.RemoveAt(dupIndex);

                //decrease indices starting after this vertex,
                //since we removed it and the array is smaller now
                for (int j = dupIndex; j < triangles.Count; j++)
                {
                    if (triangles[j] >= dupIndex)
                        triangles[j] = triangles[j] - 1;
                }
            }

            /*
            str = "";
            for (int i = 0; i < triangles.Count; i++)
                str += triangles[i] + " ";
            
            Debug.Log("AFTER tris: " + str);
            */
            //Debug.Log("COUNTS: " + vertices.Count + " " + triangles.Count);
            
            //assign merged vertices and triangles to the new mesh
            myFilter.sharedMesh.triangles = triangles.ToArray();
            myFilter.sharedMesh.vertices = vertices.ToArray();

            //recalculate and optimize
            myFilter.sharedMesh.RecalculateNormals();
            myFilter.sharedMesh.RecalculateBounds();
            var o_197_12_636835518921457596 = myFilter.sharedMesh;
        }


        /// <summary>
        /// Creates a new submesh gameobject, which will be merged into the existing one later on.
        /// </summary>
        public GameObject CreateSubMesh()
        {
            //add new entry for vertex references
            subPoints.Add(new SubPoints());

            //create new submesh gameobject
            GameObject obj = new GameObject("New SubMesh");
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            
            //get important components
            MeshFilter subFilter = obj.AddComponent<MeshFilter>();
            MeshRenderer subRenderer = obj.AddComponent<MeshRenderer>();
            
            //modify material and create actual submesh
            subRenderer.sharedMaterial = GetComponent<Renderer>().sharedMaterial;
            subRenderer.enabled = GetComponent<Renderer>().enabled;
            subFilter.mesh = subMesh = new Mesh();
            subMesh.name = "SubMesh";
            current.Clear();
            
            return obj;
        }


        /// <summary>
        /// Updates the mesh with new vertex positions.
        /// </summary>
        public void UpdateMesh(Vector3[] verts)
        {
            //convert passed in vertices to relative positioning
            MeshFilter myFilter = GetComponent<MeshFilter>();
            for (int i = 0; i < verts.Length; i++)
                verts[i] = transform.InverseTransformPoint(verts[i]);
            
            //assign vertices
            myFilter.sharedMesh.vertices = verts;
        }


        /// <summary>
        /// Adds a new vertex to the current submesh.
        /// </summary>
        public void AddPoint(Vector3 point)
        {
            //modify point to take offset into account
            point = point + new Vector3(0, yOffset, 0);

            //re-position this object to the first point
            if (list.Count == 0)
                transform.position = point;

            //add new point to the list of vertices
            list.Add(transform.InverseTransformPoint(point));

            //get the current index,
            //then add it to the current and actual submesh list
            int index = list.Count - 1;
            current.Add(index);
            subPoints[subPoints.Count - 1].list.Add(index);
        }


        /// <summary>
        /// Adds a reference to an existing vertex to the current submesh.
        /// </summary>
        public void AddPoint(int point)
        {
            //just add the index to the lists
            current.Add(point);
            subPoints[subPoints.Count - 1].list.Add(point);
        }


        /// <summary>
        /// Creates the double-sided submesh based on vertices and triangles.
        /// </summary>
        public void CreateMesh()
        {
            //clear mesh definitions
            if (subMesh) subMesh.Clear();
            //get components
            MeshFilter subFilter = null;
            MeshFilter[] subFilters = GetComponentsInChildren<MeshFilter>(true);

            //find corresponding MeshFilter
            for (int i = 0; i < subFilters.Length; i++)
            {
                if (subFilters[i].sharedMesh == subMesh)
                {
                    subFilter = subFilters[i];
                    break;
                }
            }

            //get vertex positions of current submesh
            Vector3[] vertex = new Vector3[current.Count];
            for (int i = 0; i < current.Count; i++)
                vertex[i] = list[current[i]];

            //don't continue without meshfilter or not enough points
            if (!subFilter || vertex.Length < 3) return;

            //set uvs of vertices
            Vector2[] uvs = new Vector2[vertex.Length];
            for (int i = 0; i < vertex.Length; i++)
            {
                if ((i % 2) == 0)
                    uvs[i] = new Vector2(0, 0);
                else
                    uvs[i] = new Vector2(1, 1);
            }

            //assign data to mesh
            subMesh.vertices = vertex;
            subMesh.uv = uvs;
            subMesh.triangles = RecalculateTriangles(null);

            //recalculate and optimize
            subMesh.RecalculateNormals();
            subMesh.RecalculateBounds();
            ;

            //assign mesh to filter
            subFilter.mesh = subMesh;
        }


        /// <summary>
        /// Recalculates a triangle array for a given list of vertex indices.
        /// </summary>
        public int[] RecalculateTriangles(List<int> list)
        {
            //create triangles array
            //3 verts per triangle * num triangles
            int triLength = list == null ? current.Count - 2
                                         : list.Count - 2;
            int[] tris = new int[3 * triLength * 2];

            //triangle indices (forwards)
            int C1 = list == null ? 0 : list[0];
            int C2 = list == null ? 1 : list[1];
            int C3 = list == null ? 2 : list[2];

            //assign triangles clockwise
            for (int j = 0; j < tris.Length / 2; j += 3)
            {
                tris[j] = C1;
                tris[j + 1] = C2;
                tris[j + 2] = C3;

                C2++;
                C3++;
            }

            //assign triangles counterclockwise
            for (int j = tris.Length / 2; j < tris.Length; j += 3)
            {
                int index = (j - tris.Length / 2) * 2;
                tris[j] = C1;
                tris[j + 1] = tris[j - index - 1];
                tris[j + 2] = tris[j - index - 2];
            }
          
            /*
            string str = "";
            for (int i = 0; i < tris.Length; i++)
                str += tris[i] + " ";
            Debug.Log("Recalculated Tris: " + str);
            */

            return tris;
        }
    }
}