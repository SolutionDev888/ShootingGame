/// \file
/// Handles Input and Data for the Multi Selection Behavior in MovementSelectionBehavior.RealTimeStratToggle Mode.
/// @author: Chase Hutchens

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is used for rendering the
/// multiselect rectangle when in RTS mode.
/// This class also keeps track of the selected ControlObjects.
/// </summary>
public class MultiSelectToggle : MonoBehaviour
{
    float xStartPos;
    float yStartPos;
    float xEndPos;
    float yEndPos;

    Material line_material;
    ControllerType controllerType;

    bool movementAllowed = false;

    List<Transform> currently_selected = new List<Transform>();

    void OnGUI()
    {
        if (controllerType.Equals(ControllerType.RealTimeStratToggle))
        {
            if (Input.GetMouseButtonDown(0))
            {
                xStartPos = Event.current.mousePosition.x;
                yStartPos = Screen.height - Event.current.mousePosition.y;
            }

            if (Input.GetMouseButton(0))
            {
                xEndPos = Event.current.mousePosition.x;
                yEndPos = Screen.height - Event.current.mousePosition.y;

                if (Mathf.Abs(xEndPos - xStartPos) > 10 && Mathf.Abs(yEndPos - yStartPos) > 10)
                {
                    movementAllowed = false;

                    GL.PushMatrix();
                    line_material.SetPass(0);
                    GL.LoadPixelMatrix();
                    GL.Begin(GL.LINES);
                    GL.Color(line_material.color);
                    GL.Vertex3(xStartPos, yStartPos, 0);
                    GL.Vertex3(xEndPos, yStartPos, 0);

                    GL.Vertex3(xStartPos, yStartPos - 1, 0);
                    GL.Vertex3(xStartPos, yEndPos, 0);

                    GL.Vertex3(xStartPos, yEndPos, 0);
                    GL.Vertex3(xEndPos, yEndPos, 0);

                    GL.Vertex3(xEndPos, yStartPos, 0);
                    GL.Vertex3(xEndPos, yEndPos, 0);
                    GL.End();
                    GL.PopMatrix();
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            List<Transform> temp_list = ControlObjHandler.DetermineInMultiSelect(new Vector2(xStartPos, Screen.height - yStartPos), new Vector2(xEndPos, Screen.height - yEndPos));
            if (temp_list.Count > 0)
            {                
                ClearSelectedTargets();

                foreach (Transform trans in temp_list)
                {
                    trans.Find("SelectedIndicator").gameObject.SetActive(true);
                    currently_selected.Add(trans);
                }

                if (currently_selected.Count > 0)
                    this.SendMessage("ChangeSelectedTarget", currently_selected[(int)(currently_selected.Count / 2)]);
            }

            movementAllowed = true;
        }
    }

    /// <summary>
    /// Sets Initial Options of the MultiSelectToggle. Setting the Material and the Game Type.
    /// </summary>
    /// <param name="material">The Material of the Box Selection. (This Needs to contain VertexColorUnlit Shader of some sort)</param>
    /// <param name="gametype">The Game Type of the System.</param>
    public void SetInitialOptions(Material material, ControllerType gametype)
    {
        line_material = material;
        controllerType = gametype;
    }

    /// <summary>
    /// Disables and Clears the Selected Objects.
    /// </summary>
    public void ClearSelectedTargets()
    {
        if (currently_selected.Count > 0)
        {
            foreach (Transform trans in currently_selected)
            {
                trans.Find("SelectedIndicator").gameObject.SetActive(false);
            }

            currently_selected.Clear();
        }
    }

    /// <summary>
    /// Returns the List of Unity3D Transform Objects that are Selected.
    /// </summary>
    public List<Transform> getCurrentlySelected()
    {
        return currently_selected;
    }

    /// <summary>
    /// Is Movement Allowed for a selected Control Object?
    /// </summary>
    public bool isMovementAllowed()
    {
        return movementAllowed;
    }
}
