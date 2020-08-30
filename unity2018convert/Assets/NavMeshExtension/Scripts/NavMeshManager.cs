/*  This file is part of the "NavMesh Extension" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System.Collections;

namespace NavMeshExtension
{
    /// <summary>
    /// NavMesh Manager class storing NavMesh properties.
    /// </summary>
    public class NavMeshManager : MonoBehaviour
    {
        /// <summary>
        /// Material for newly created meshes.
        /// </summary>
        public Material meshMaterial;

        /// <summary>
        /// Boolean used when toggling mesh renderers.
        /// </summary>
        [HideInInspector]
        public bool rendererToggle = true;
    }
}