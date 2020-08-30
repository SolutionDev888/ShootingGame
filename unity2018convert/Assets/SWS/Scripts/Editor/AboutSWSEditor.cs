/*  This file is part of the "3D Tower Defense Starter Kit" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// Rebound Games about/help/support window.
/// <summary>
public class AboutSWSEditor : EditorWindow
{
    [MenuItem("Window/Simple Waypoint System/About")]
    static void Init()
    {
        AboutSWSEditor aboutWindow = (AboutSWSEditor)EditorWindow.GetWindowWithRect
                (typeof(AboutSWSEditor), new Rect(0, 0, 300, 250), false, "About");
        aboutWindow.Show();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect((Screen.width / 2) - 80, 20, 300, 100));
        GUILayout.Label("Simple Waypoint System", EditorStyles.boldLabel);
        GUILayout.Label("         Rebound Games");
        GUILayout.EndArea();
        GUILayout.Space(70);


        GUILayout.Label("Info", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Homepage");
        if (GUILayout.Button("Visit", GUILayout.Width(100)))
        {
            Help.BrowseURL("www.rebound-games.com");
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        GUILayout.Label("Support", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Support Forum");
        if (GUILayout.Button("Visit", GUILayout.Width(100)))
        {
            Help.BrowseURL("http://www.rebound-games.com/forum/");
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Unity Forum");
        if (GUILayout.Button("Visit", GUILayout.Width(100)))
        {
            Help.BrowseURL("http://forum.unity3d.com/threads/115086-Simple-Waypoint-System-%28SWS%29-RELEASED");
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        GUILayout.Label("Support us!", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Rate/Review");
        if (GUILayout.Button("Visit", GUILayout.Width(100)))
        {
            UnityEditorInternal.AssetStore.Open("content/2506");
        }
        GUILayout.EndHorizontal();
    }
}