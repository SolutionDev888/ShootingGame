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
    /// Adds a new Portal Manager gameobject to the scene.
    /// <summary>
    [CustomEditor(typeof(NavMeshManager))]
    public class NavMeshManagerEditor : Editor
    {
        //manager reference
        private NavMeshManager script;


        void OnEnable()
        {
            script = (NavMeshManager)target;
        }


        /// <summary>
        /// Custom inspector override for buttons.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (GUILayout.Button("New NavMesh"))
            {
                CreateNewNavMesh();
                GetSceneView().Focus();
            }

            if (GUILayout.Button("Toggle Renderers"))
            {
                //invert boolean and toggle all renderers
                script.rendererToggle = !script.rendererToggle;
                MeshRenderer[] renderers = script.GetComponentsInChildren<MeshRenderer>(true);

                for (int i = 0; i < renderers.Length; i++)
                    renderers[i].enabled = script.rendererToggle;
            }

            if (GUILayout.Button("Bake NavMesh"))
            {
                BakeNavMesh();
            }
        }


        /// <summary>
        /// Creates a new gameobject to use it as NavMeshObject.
        /// </summary>
        public void CreateNewNavMesh()
        {
            //create gameobject
            GameObject navGO = new GameObject("New NavMesh");
            navGO.transform.parent = script.transform;
            navGO.isStatic = true;
            navGO.AddComponent<NavMeshObject>();

            //modify renderer to ignore shadows
            MeshRenderer mRenderer = navGO.GetComponent<MeshRenderer>();
            #if UNITY_4_6 || UNITY_4_7
            mRenderer.castShadows = false;
            #else
            mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            #endif
            mRenderer.receiveShadows = false;
            if (script.meshMaterial)
                mRenderer.sharedMaterial = script.meshMaterial;
            else
                mRenderer.enabled = false;

            Undo.RegisterCreatedObjectUndo(navGO, "Created NavMesh");
            Selection.activeGameObject = navGO;
        }


        /// <summary>
        /// Bakes the Unity NavMesh on created NavMeshObjects.
        /// </summary>
        public void BakeNavMesh()
        {
            //loop over renderers and enable them for the baking process,
            //as otherwise the NavMeshBuilder will ignore them 
            List<Renderer> disabledObjects = new List<Renderer>();
            foreach (Renderer item in Object.FindObjectsOfType(typeof(Renderer))) 
            {
                if (GameObjectUtility.AreStaticEditorFlagsSet(item.gameObject, StaticEditorFlags.NavigationStatic)
                    && !item.enabled)
                {
                    disabledObjects.Add(item);
                    item.GetComponent<Renderer>().enabled = true;
                }
            }

            //trigger navmesh builder
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            //re-enable disabled renderers
            disabledObjects.ForEach(obj => obj.enabled = false);

            ShowNotification("NavMesh successfully built.");
        }


        /// <summary>
        /// Shows a SceneView notification.
        /// </summary>
        public static void ShowNotification(string text)
        {
            GetSceneView().ShowNotification(new GUIContent(text));
        }


        /// <summary>
        /// Gets the active SceneView or creates one.
        /// </summary>
        public static SceneView GetSceneView()
        {
            SceneView view = SceneView.currentDrawingSceneView;
            if (view == null)
                view = EditorWindow.GetWindow<SceneView>();

            return view;
        }
    }
}
