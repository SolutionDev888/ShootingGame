/*  This file is part of the "Simple Waypoint System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SWS
{
    /// <summary>
    /// Stores waypoints, accessed by walker objects.
    /// Provides gizmo visualization in the editor.
    /// <summary>
    public class PathManager : MonoBehaviour
    {
        /// <summary>
        /// Waypoint array creating the path.
        /// <summary>
        public Transform[] waypoints;

        /// <summary>
        /// Toggles drawing of linear or curved gizmo lines.
        /// <summary>
        public bool drawCurved = true;

        /// <summary>
        /// Gizmo color for path ends.
        /// <summary>
        public Color color1 = new Color(1, 0, 1, 0.5f);

        /// <summary>
        /// Gizmo color for lines and waypoints.
        /// <summary>
        public Color color2 = new Color(1, 235 / 255f, 4 / 255f, 0.5f);

        /// <summary>
        /// Gizmo size for path ends.
        /// <summary>
        public Vector3 size = new Vector3(.7f, .7f, .7f);

        /// <summary>
        /// Gizmo radius for waypoints.
        /// <summary>
        public float radius = .4f;

        /// <summary>
        /// Gameobject for replacing waypoints.
        /// <summary>
        public GameObject replaceObject;


        //editor visualization
        void OnDrawGizmos()
        {
            if (waypoints.Length <= 0) return;

            //get positions
            Vector3[] wpPositions = GetPathPoints();

            //assign path ends color
            Gizmos.color = color1;
            Gizmos.DrawWireCube(wpPositions[0], size);
            Gizmos.DrawWireCube(wpPositions[wpPositions.Length - 1], size);

            //assign line and waypoints color
            Gizmos.color = color2;
            for (int i = 1; i < wpPositions.Length - 1; i++)
                Gizmos.DrawWireSphere(wpPositions[i], radius);

            //draw linear or curved lines with the same color
            if (drawCurved && wpPositions.Length >= 2)
                WaypointManager.DrawCurved(wpPositions);
            else
                WaypointManager.DrawStraight(wpPositions);
        }


        /// <summary>
        /// Returns waypoint positions (path positions) as Vector3 array.
        /// <summary>
        public virtual Vector3[] GetPathPoints()
        {
            Vector3[] pathPoints = new Vector3[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
                pathPoints[i] = waypoints[i].position;
            return pathPoints;
        }
    }
}


