/// \file
/// Handles Rendering of a GridGenerator GridSquares and Calculated Path Tiles.
/// @author: Chase Hutchens

using UnityEngine;
using System.Collections;

/// <summary>
/// This class is used for rendering the grid and calculated grid paths to the world.
/// Which, this script must be attached to the camera if you wish to render the grid, due to the OnPostRender function.
/// </summary>
public class GridRenderer : MonoBehaviour
{
    private GridGenerator gridGenerator;
    private Matrix4x4 gridTransformation;

    /// <summary>
    /// The Unity3D GameObject that Contains a GridGenerator.cs Object.
    /// </summary>
    public GameObject Grid;
    
    /// <summary>
    /// Draw the Grid Squares?
    /// </summary>
    public bool ActiveGridDraw = false;

    /// <summary>
    /// Draw the Calculated Path Tiles?
    /// </summary>
    public bool ActivePathDraw = false;

    private void Start()
    {
        gridGenerator = Grid.GetComponent<GridGenerator>();
        gridTransformation = gridGenerator.transform.worldToLocalMatrix * Matrix4x4.TRS(gridGenerator.transform.position, gridGenerator.transform.rotation, gridGenerator.transform.localScale);
    }

    private void OnPostRender()
    {
        GL.PushMatrix();
        gridGenerator.GetComponent<Renderer>().material.SetPass(0);
        GL.MultMatrix(gridTransformation);
        GL.Begin(GL.LINES);
        GL.Color(gridGenerator.GetComponent<Renderer>().material.color); // change the color in the GridGenerator material within the Inspector

        if (ActiveGridDraw)
        {
            for (int i = 0; i < gridGenerator.getRows(); i++)
            {
                for (int j = 0; j < gridGenerator.getColumns(); j++)
                {
                    if (gridGenerator.getDesiredGridSquare(i, j) != null && isInFOV(gridGenerator.getDesiredGridSquare(i, j).getGridSquareCentralOrigin()))
                    {
                        Vector3 lowerLeft = gridGenerator.getDesiredGridSquare(i, j).getGridSquareLowerLeft();
                        Vector3 lowerRight = gridGenerator.getDesiredGridSquare(i, j).getGridSquareLowerRight();
                        Vector3 upperLeft = gridGenerator.getDesiredGridSquare(i, j).getGridSquareUpperLeft();
                        Vector3 upperRight = gridGenerator.getDesiredGridSquare(i, j).getGridSquareUpperRight();

                        GL.Vertex3(lowerLeft.x, lowerLeft.y, lowerLeft.z);
                        GL.Vertex3(upperLeft.x, upperLeft.y, upperLeft.z);

                        GL.Vertex3(lowerLeft.x, lowerLeft.y, lowerLeft.z);
                        GL.Vertex3(lowerRight.x, lowerRight.y, lowerRight.z);

                        GL.Vertex3(lowerRight.x, lowerRight.y, lowerRight.z);
                        GL.Vertex3(upperRight.x, upperRight.y, upperRight.z);

                        GL.Vertex3(upperLeft.x, upperLeft.y, upperLeft.z);
                        GL.Vertex3(upperRight.x, upperRight.y, upperRight.z);
                    }
                }
            }
        }

        if (ActivePathDraw)
        {
            foreach (ControlObjHandler obj in FindObjectsOfType(typeof(ControlObjHandler)))
            {
                if (obj.Moveable)
                    obj.SendMessage("DisplayPath", gridGenerator);
            }
        }

        GL.End();
        GL.PopMatrix();
    }

    /// <summary>
    /// Determines if the Camera is in the Field of View of a Particular Vector3.
    /// </summary>
    /// <param name="worldPos">The World Position of a Particular Vector3.</param>
    /// <returns>In View or Not In View</returns>
    private bool isInFOV(Vector3 worldPos)
    {
        worldPos = GetComponent<Camera>().WorldToScreenPoint(worldPos);

        if (worldPos.x >= 0 && worldPos.x <= Screen.width && worldPos.y >= 0 && worldPos.y <= Screen.height)
            return true;

        return false;
    }
}