/// \file
/// Handles Concepts Relating to Formations. (Incomplete)
/// @author: Chase Hutchens

using UnityEngine;
using System.Collections;

/// <summary>
/// This class will be Used for Handling
/// Different Formations. Such as Activating Them,
/// Determining Them and Maybe Even Calculating Them. (Incomplete)
/// </summary>
public class FormationSwitches
{
    public int FormationCount = 9;

    Texture2D baseFormations;
    Texture2D[] inactiveBaseFormations;
    Texture2D[] activeBaseFormations;

    public void Init()
    {
        inactiveBaseFormations = new Texture2D[FormationCount];
        activeBaseFormations = new Texture2D[FormationCount];

        baseFormations = (Texture2D)Resources.Load("gfx/Formations_296x32");
        activeBaseFormations = new Texture2D[FormationCount];

        for (int i = 0; i < FormationCount; i++)
        {
            Color[] formation_gfx = baseFormations.GetPixels(32 * i + i, 32, 32, 32);
            inactiveBaseFormations[i] = new Texture2D(32, 32, TextureFormat.ARGB32, true);
            inactiveBaseFormations[i].SetPixels(formation_gfx);
            inactiveBaseFormations[i].Apply();

            formation_gfx = baseFormations.GetPixels(32 * i + i, 0, 32, 32);
            activeBaseFormations[i] = new Texture2D(32, 32, TextureFormat.ARGB32, true);
            activeBaseFormations[i].SetPixels(formation_gfx);
            activeBaseFormations[i].Apply();
        }
    }

    public void OnGUI()
    {
        for (int i = 0; i < FormationCount; i++)
        {
            GUI.DrawTexture(new Rect(Mathf.Floor(baseFormations.width / FormationCount) * i + i, Screen.height - baseFormations.height / 2, 32, 32), inactiveBaseFormations[i]);
            GUI.DrawTexture(new Rect(Mathf.Floor(baseFormations.width / FormationCount) * i + i, Screen.height - baseFormations.height, 32, 32), activeBaseFormations[i]);
        }
    }
}
