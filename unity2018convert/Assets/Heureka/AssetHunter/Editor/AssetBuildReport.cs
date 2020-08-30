using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HeurekaGames
{
    [System.Serializable]
    public class AssetBuildReport
    {
        [SerializeField]
        private List<string> m_includedDependencies = new List<string>();
        [SerializeField]
        public List<BuildReportAsset> m_BuildSizeList = new List<BuildReportAsset>();

        internal void AddDependency(string assemblyName)
        {
            m_includedDependencies.Add(assemblyName);
        }

        internal bool IsEmpty()
        {
            return m_BuildSizeList.Count == 0;
        }

        public List<string> IncludedDependencies
        {
            get { return m_includedDependencies; }
            set { m_includedDependencies = value; }
        }

        internal void AddAsset(BuildReportAsset asset)
        {
            m_BuildSizeList.Add(asset);
        }

        public int AssetCount
        {
            get { return m_BuildSizeList.Count; }
        }

        internal BuildReportAsset GetAssetAtIndex(int i)
        {
            return m_BuildSizeList[i];
        }

        internal void AddPrefabs(List<string> usedPrefabsInScenes)
        {
            foreach (string path in usedPrefabsInScenes)
            {
                EditorUtility.DisplayProgressBar(
                                "Adding prefabs",
                                "Analyzing scenes to get prefabs",
                                (float)usedPrefabsInScenes.IndexOf(path) / (float)usedPrefabsInScenes.Count);
                //Early out
                if (m_BuildSizeList.Exists(val => val.Path == path))
                    continue;

                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

                if (obj != null)
                {
                    BuildReportAsset newAsset = new BuildReportAsset();

                    newAsset.SetAssetInfo(obj, path);
                    newAsset.SetSize(0.0f, "--");
                    m_BuildSizeList.Add(newAsset);
                }
                else
                {
                    Debug.LogWarning(path + " is not a valid asset");
                }
            }

            EditorUtility.ClearProgressBar();
        }
    }

    [System.Serializable]
    public class BuildReportAsset : IEquatable<BuildReportAsset>
    {
        [SerializeField]
        private string m_name;
        [SerializeField]
        private string m_path;
        [SerializeField]
        private string m_assetGUID;
        [SerializeField]
        private SerializableSystemType m_type;
        [SerializeField]
        private float m_assetSize;
        [SerializeField]
        private string m_sizePostFix;
        [SerializeField]
        private bool m_bShowSceneDependency;
        [SerializeField]
        private UnityEngine.Object[] m_sceneDependencies;
        [SerializeField]
        private bool m_bFoldOut;

        internal void SetAssetInfo(UnityEngine.Object obj, string path)
        {
            this.m_path = path;
            string[] parts = path.Split('/');
            this.m_name = parts[parts.Length - 1];

            m_assetGUID = UnityEditor.AssetDatabase.AssetPathToGUID(path);

            //TODO Make sure we are doing proper garbage collection
            m_type = new SerializableSystemType(obj.GetType());

            //TODO Memory leak management (Perhaps should be a toggle value in window, or automatically detect if needed
            EditorUtility.UnloadUnusedAssets();
        }

        internal void SetSize(float assetSize, string postFix)
        {
            m_assetSize = assetSize;
            m_sizePostFix = postFix;
        }

        public bool Equals(BuildReportAsset other)
        {
            // Would still want to check for null etc. first.
            return this.m_assetGUID == other.m_assetGUID &&
                   this.m_name == other.m_name &&
                   this.m_path == other.m_path;
        }

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        public SerializableSystemType Type
        {
            get { return m_type; }
        }

        public string GUID
        {
            get { return m_assetGUID; }
        }

        public string Path
        {
            get { return m_path; }
        }

        public float Size
        {
            get { return m_assetSize; }
        }

        public string SizePostFix
        {
            get { return m_sizePostFix; }
        }

        internal void ToggleShowSceneDependency()
        {
            m_bShowSceneDependency = !m_bShowSceneDependency;
            m_bFoldOut = m_bShowSceneDependency;

            if (m_bShowSceneDependency == false)
                m_sceneDependencies = null;
        }

        internal void SetSceneDependencies(UnityEngine.Object[] scenes)
        {
            m_sceneDependencies = scenes;
        }

        public bool ShouldShowDependencies
        {
            get { return m_bShowSceneDependency; }
        }

        public bool FoldOut
        {
            get { return m_bFoldOut; }
            set { m_bFoldOut = value; }
        }

        internal UnityEngine.Object[] GetSceneDependencies()
        {
            return m_sceneDependencies;
        }
    }
}
