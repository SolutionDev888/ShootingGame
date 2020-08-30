/*  This file is part of the "NavMesh Extension" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace NavMeshExtension
{
    /// <summary>
    /// Custom Editor for editing vertices and exporting the mesh.
    /// </summary>
    [CustomEditor(typeof(NavMeshObject))]
    public class NavMeshObjectEditor : Editor
    {
        //navmesh object reference
        private NavMeshObject script;
        //export path
        private const string assetPath = "Assets/NavMeshExtension/Prefabs/";
        //scene view help texts
        private const string editOnText = "Click: Place Vertices\n" +
                                          "Ctrl+Click: Submesh\n" +
                                          "Always exit Edit Mode!";
        private const string editOffText = "Click vertices: Select/Deselect\n" +
                                           "Right-click: Deselect All\n" +
                                           "Backspace: Delete Selection";

        //converted array of vertex positions
        private Vector3[] allPoints;
        //indices of selected vertices
        private List<int> selected = new List<int>();
        //whether or not placement mode is active
        private static bool placing = false;
        //undo/redo flag for eventually disabling placing
        private static bool undoRedo = false;

        //clicked mouse position in the scene
        private Vector3 mousePosition;
        //vertex index near mouse position
        private int dragIndex;


        void OnEnable()
        {
            script = (NavMeshObject)target;
            if(script.subMesh == null)
                EnterEditMode();
        }


        void OnDisable()
        {
            //if undo or redo was performed,
            //don't toggle placing - and vice versa
            if (undoRedo)
                PerformUndoRedo();
            else if (placing)
                LeaveEditMode();
        }


        /// <summary>
        /// Custom inspector override for buttons.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            //display current count of placed vertices in the active submesh
            EditorGUILayout.LabelField("Current Vertices: " + script.current.Count);

            //enter placement mode
            if (!placing && GUILayout.Button("Edit Mode: Off"))
            {
                EnterEditMode();
            }

            GUI.color = Color.yellow;

            //leave placement mode and try to combine submeshes
            if (placing && GUILayout.Button("Edit Mode: On"))
            {
                LeaveEditMode();
                return;
            }

            GUI.color = Color.white;

            //export mesh to asset and prefab
            if (GUILayout.Button("Save as Prefab"))
            {
                //get gameobject and its mesh filter
                GameObject gObj = script.gameObject;
                Mesh mesh = gObj.GetComponent<MeshFilter>().sharedMesh;

                if (!mesh)
                {
                    Debug.LogWarning("Could not save as prefab, asset does not have a Mesh.");
                    return;
                }

                if (AssetDatabase.Contains(mesh))
                {
                    Debug.Log("Mesh asset already exists. Cancelling.");
                    return;
                }

                //get the current unix timestamp for a unique naming scheme
                var epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
                string timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds.ToString("F0");
                string assetName = "NavMesh_";

                //check that the folder does exist
                string dir = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    AssetDatabase.ImportAsset(dir);
                }

                //create the mesh asset at the path specified, with unique name
                AssetDatabase.CreateAsset(mesh, assetPath + assetName + timestamp + ".asset");
                AssetDatabase.SaveAssets();

                //create the prefab of this gameobject at the path specified, with the same name
                PrefabUtility.CreatePrefab(assetPath + assetName + timestamp + ".prefab", gObj,
                                           ReplacePrefabOptions.ConnectToPrefab);
                //rename instance to prefab name
                gObj.name = assetName + timestamp;
            }
        }
    

        /// <summary>
        /// Draw Scene GUI handles, circles and outlines for submesh vertices.
        /// <summary>
        public void OnSceneGUI()
        {
            //get world positions of vertices
            allPoints = ConvertAllPoints();

            //create a ray to get where we clicked in the scene view and pass in mouse position
            Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hitInfo;
            Event e = Event.current;

            //this prevents selecting other objects in the scene
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);
            //find index of closest vertex, if any
            dragIndex = FindClosest();

            if (!placing && e.type == EventType.KeyDown && e.keyCode == KeyCode.Backspace)
            {
                e.Use();
                Undo.RegisterCompleteObjectUndo(script, "Delete Vertex");
                Undo.RecordObject(script.GetComponent<MeshFilter>().sharedMesh, "Delete Vertex");
                DeleteSelected();
                return;
            }

            //in the edit mode, ray hit something
            if (placing)
            {
                Tools.current = Tool.None;
                Handles.BeginGUI();
                GUILayout.Window(2, new Rect(Screen.width - 157, Screen.height - 100, 100, 50), (id) =>
                {
                    GUILayout.Label(editOnText);
                }, "Control Info Box");
                Handles.EndGUI();

                if (Physics.Raycast(worldRay, out hitInfo))
                {
                    //the actual hit position
                    mousePosition = hitInfo.point;

                    //place new point if the left mouse button was clicked
                    if (e.type == EventType.MouseUp && e.button == 0 && !e.alt)
                    {
                        Undo.RegisterCompleteObjectUndo(script, "Add Vertex");

                        //get current submesh vertex count
                        int currentCount = script.current.Count;
                        //create a new submesh, if control was hold in addition to the mouse click
                        //or if autoSplit is true and the current count exceeds splitAt
                        if ((e.control && currentCount >= 3) ||
                           (script.autoSplit && currentCount >= script.splitAt))
                        {
                            GameObject newObj = script.CreateSubMesh();
                            Undo.RegisterCreatedObjectUndo(newObj, "Add Vertex");
                        }

                        //call this method when you've used an event.
                        //the event's type will be set to EventType.Used,
                        //causing other GUI elements to ignore it
                        e.Use();

                        //add point to existing vertex, if near any
                        //but don't add it to the same submesh (closed mesh)
                        //otherwise just add a new vertex position
                        if (script.current.Contains(dragIndex))
                            script.AddPoint(script.transform.TransformPoint(script.list[dragIndex]));
                        else if (dragIndex >= 0)
                            script.AddPoint(dragIndex);
                        else
                            script.AddPoint(mousePosition);

                        //invoke new mesh calculation
                        Undo.RegisterCompleteObjectUndo(script.subMesh, "Add Vertex");
                        script.CreateMesh();
                        return;
                    }
                }
            }

            //not in edit mode
            if (!placing)
            {
                Handles.BeginGUI();
                GUILayout.Window(2, new Rect(Screen.width - 200, Screen.height - 100, 100, 50), (id) =>
                {
                    GUILayout.Label(editOffText);
                }, "Control Info Box");
                Handles.EndGUI();

                //clicking near vertices will select them and show handles
                if (e.type == EventType.MouseUp && e.button == 0 && !e.alt)
                {
                    //select/unselect vertex point
                    if (dragIndex >= 0)
                    {
                        if (!selected.Contains(dragIndex))
                            selected.Add(dragIndex);
                        else
                            selected.Remove(dragIndex);

                        SceneView.RepaintAll();
                    }
                    else if(selected.Count == 0)
                        Selection.activeObject = null;
                }
                //unselect all vertices
                else if (e.type == EventType.MouseUp && e.button == 1 && !e.alt)
                {
                    selected.Clear();
                }
            }

            //draw scene gizmos
            DrawSelectedHandles();
            DrawPolygonOutline();

            //handle undo of vertex modifications (redraw mesh)
            if (e.type == EventType.ValidateCommand && e.commandName == "UndoRedoPerformed")
                undoRedo = true;

            HandleUtility.AddDefaultControl(-1);
            HandleUtility.Repaint();
        }


        private void EnterEditMode()
        {
            Undo.RegisterCompleteObjectUndo(script, "Edit On");

            //if possible, create new submesh
            GameObject newObj = script.CreateSubMesh();
            Undo.RegisterCreatedObjectUndo(newObj, "Edit On");

            //clear handle selections
            selected.Clear();
            placing = true;
        }


        private void LeaveEditMode()
        {
            Undo.RegisterCompleteObjectUndo(script, "Edit Off");

            //if possible, combine submeshes
            CheckCombine();

            //get all mesh filters
            MeshFilter[] meshFilters = script.GetComponentsInChildren<MeshFilter>();
            Undo.RecordObject(meshFilters[0], "Edit Off");

            //let the script combine them 
            script.Combine();

            for (int i = 1; i < meshFilters.Length; i++)
            {
                Undo.DestroyObjectImmediate(meshFilters[i].gameObject);
            }

            placing = false;
        }


        private void PerformUndoRedo()
        {
            undoRedo = false;
            if (script.current.Count > 1 && script.subMesh)
            {
                script.subMesh.triangles = script.RecalculateTriangles(null);
                OptimizeMesh(script.subMesh);
                return;
            }

            Mesh sharedMesh = script.GetComponent<MeshFilter>().sharedMesh;
            if (!sharedMesh) return;

            List<int> triangles = new List<int>();
            for (int i = 0; i < script.subPoints.Count; i++)
                triangles.AddRange(script.RecalculateTriangles(script.subPoints[i].list));

            sharedMesh.triangles = new int[sharedMesh.vertexCount * 3];
            script.UpdateMesh(ConvertAllPoints());
            sharedMesh.triangles = triangles.ToArray();
            if (!placing) script.Combine();
        }


        //check if a combine of submeshes is possible
        private void CheckCombine()
        {
            NavMeshManagerEditor.GetSceneView().Focus();

            //get count of all submeshes
            int subPointsCount = script.subPoints.Count;
            if (script.subPoints.Count == 0) return;
            NavMeshObject.SubPoints lastPoints = script.subPoints[subPointsCount - 1];

            if (lastPoints.list.Count <= 2)
            {
                selected.Clear();
                for (int i = 0; i < lastPoints.list.Count; i++)
                {
                    selected.Add(lastPoints.list[i]);
                }

                DeleteSelected();
            }
        }


        //find closest vertex to the mouse position
        private int FindClosest()
        {
            //initialize variables
            List<int> closest = new List<int>();
            Vector2 mousePos = Event.current.mousePosition;
            int near = -1;

            //loop over vertices to find the nearest ones
            for (int i = 0; i < allPoints.Length; i++)
            {
                Vector2 screenPoint = HandleUtility.WorldToGUIPoint(allPoints[i]);
                if (Vector2.Distance(screenPoint, mousePos) < 10)
                    closest.Add(i);
            }

            //don't do further calculation in some cases
            if (closest.Count == 0)
                return near;
            else if (closest.Count == 1)
                return closest[0];
            else
            {
                //there are more than a few vertices near the mouse position,
                //here only the closest vertex to the camera should matter
                Vector3 camPos = Camera.current.transform.position;
                float nearDist = float.MaxValue;

                //loop over all vertices and get the one near to the camera
                for (int i = 0; i < closest.Count; i++)
                {
                    float dist = Vector3.Distance(allPoints[closest[i]], camPos);
                    if (dist < nearDist)
                    {
                        nearDist = dist;
                        near = closest[i];
                    }
                }
            }

            closest.Clear();
            return near;
        }


        //convert relative vertex positions to world positions
        private Vector3[] ConvertAllPoints()
        {
            int count = script.list.Count;
            List<Vector3> all = new List<Vector3>();
            for (int i = 0; i < count; i++)
                all.Add(script.transform.TransformPoint(script.list[i]));

            return all.ToArray();
        }


        //delete previously selected vertices and rebuild mesh
        private void DeleteSelected(bool auto = false)
        {
            //get mesh references
            MeshFilter filter = script.GetComponent<MeshFilter>();
            List<Vector3> vertices = null;
            if(filter.sharedMesh != null)
                vertices = new List<Vector3>(filter.sharedMesh.vertices);
            //filter selected list for unique entries
            selected = selected.Distinct().ToList();
            selected = selected.OrderByDescending(x => x).ToList();

            //loop over selected vertex indices
            for (int i = 0; i < selected.Count; i++)
            {
                //remove index from mesh vertices
                int index = selected[i];
                if (vertices != null)
                {
                    if (vertices.Contains(allPoints[index])) vertices.Remove(allPoints[index]);
                    else try { vertices.RemoveAt(index); } catch { };
                }
                script.list.RemoveAt(index);
                
                //loop over submeshes and remove it there too
                for (int j = 0; j < script.subPoints.Count; j++)
                {
                    script.subPoints[j].list.Remove(index);
                    //decrease higher entries, as the array is smaller now
                    for (int k = 0; k < script.subPoints[j].list.Count; k++)
                    {
                        if (script.subPoints[j].list[k] >= index)
                            script.subPoints[j].list[k] -= 1;
                    }
                }
            }

            //clear selection
            selected.Clear();

            //loop over submeshes to remove obsolete indices,
            //e.g. if a submesh has only 2 vertices after removal
            for (int i = script.subPoints.Count - 1; i >= 0; i--)
            {
                //check for vertex count
                if (script.subPoints[i].list.Count <= 2)
                {
                    //construct a combined list with all indices
                    List<int> allIndices = new List<int>();
                    for (int j = 0; j < script.subPoints.Count; j++)
                        allIndices.AddRange(script.subPoints[j].list);
                    
                    //check whether an index occurs more than once
                    List<int> duplicates = allIndices.GroupBy(x => x)
                                           .Where(x => x.Count() > 1)
                                           .Select(x => x.Key)
                                           .ToList();

                    //if an index in this submesh is not being used in other
                    //submeshes anymore, this means that we can remove it too
                    for (int j = 0; j < script.subPoints[i].list.Count; j++)
                        if (!duplicates.Contains(script.subPoints[i].list[j]))
                            selected.Add(script.subPoints[i].list[j]);
                   
                    //delete this submesh entry
                    script.subPoints.RemoveAt(i);
                }
            }

            //recalculate triangles for complete mesh
            if (filter.sharedMesh == null) return;
            List<int> triangles = new List<int>();
            for (int i = 0; i < script.subPoints.Count; i++)
                triangles.AddRange(script.RecalculateTriangles(script.subPoints[i].list));

            //assign triangles and update vertices
            filter.sharedMesh.triangles = triangles.ToArray();
            script.list = vertices;
            script.UpdateMesh(ConvertAllPoints());

            //recursively delete the remaining obsolete indices
            //which were found by looking through all submeshes
            if (selected.Count > 0)
            {
                DeleteSelected();
                return;
            }

            //deletion done - optimize mesh
            OptimizeMesh(filter.sharedMesh);
        }


        //rebuild mesh properties
        private void OptimizeMesh(Mesh mesh)
        {
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            ;
        }


        //draw selection and move handles
        private void DrawSelectedHandles()
        {
            //don't draw anything without mesh
            if (!script.GetComponent<MeshFilter>().sharedMesh)
                return;

            Handles.color = new Color(1, 0, 0, 0.2f);

            //get current vertex count of the active submesh
            int currentCount = script.current.Count;

            //draw a solid disc at the lastest position we clicked in edit mode
            if (placing && currentCount > 0 && (!script.autoSplit
               || (script.autoSplit && currentCount != script.splitAt)))
            {
                Vector3 pos = allPoints[script.current[currentCount - 1]];
                Handles.DrawSolidDisc(pos, Vector3.up,
                                      HandleUtility.GetHandleSize(pos) * 0.1f);
            }

            //don't continue without selected vertices
            if (selected.Count == 0)
                return;

            //draw a solid disc for each vertex we selected
            Vector3[] selectedHandles = new Vector3[selected.Count];
            for (int i = 0; i < selected.Count; i++)
            {
                Vector3 pos = allPoints[selected[i]];
                selectedHandles[i] = pos;
                Handles.DrawSolidDisc(pos, Vector3.up,
                                      HandleUtility.GetHandleSize(pos) * 0.1f);
            }

            //get the center position of selected vertices and draw a handle there
            Vector3 center = GetCenterOfVector3(selectedHandles);
            Vector3 handle = Handles.PositionHandle(center, Quaternion.identity);
            Vector3 diff = handle - center;

            //if the handle moved
            if (diff != Vector3.zero)
            {
                Undo.RegisterCompleteObjectUndo(script, "Handle Moved");
                Mesh myMesh = script.gameObject.GetComponent<MeshFilter>().sharedMesh;
                if (myMesh) Undo.RecordObject(myMesh, "Handle Moved");

                //adjust corresponding mesh vertices
                for (int i = 0; i < selected.Count; i++)
                    script.list[selected[i]] += diff;

                //update mesh with new vertex positions
                script.UpdateMesh(ConvertAllPoints());
            }
        }


        //draws submesh outlines
        private void DrawPolygonOutline()
        {
            List<NavMeshObject.SubPoints> sub = script.subPoints;
            Handles.color = Color.yellow;

            //for each submesh
            for (int i = 0; i < sub.Count; i++)
            {
                //get actual vertex positions from indices
                Vector3[] points = new Vector3[sub[i].list.Count];
                for (int j = 0; j < points.Length; j++)
                    points[j] = allPoints[sub[i].list[j]];

                //draw polyline with these points
                Handles.DrawPolyLine(points);

                //connect the first and last point to a loop
                if (points.Length >= 2)
                {
                    Vector3[] p = { points[0], points[points.Length - 1] };
                    Handles.DrawPolyLine((p));
                }
            }

            Handles.color = Color.red;

            //draw a circle at each vertex position
            for (int i = 0; i < allPoints.Length; i++)
            {
                Handles.CircleCap(0, allPoints[i],
                            Quaternion.LookRotation(Vector3.up, Vector3.forward),
                            HandleUtility.GetHandleSize(allPoints[i]) * 0.1f);
            }
        }

        
        //returns the center point of an array of Vector3s
        private Vector3 GetCenterOfVector3(Vector3[] points)
        {
            Vector3 sum = Vector3.zero;

            if (points.Length == 0) return sum;

            for (int i = 0; i < points.Length; i++)
                sum += points[i];

            return sum / points.Length;
        }
    }
}