/// \file
/// Used for Handling Concepts Tied Specifically to Creature Objects.
/// @author: Chase Hutchens

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Creates a Unity3D object to Inherit the Functionality of a DC/RTS CreatureHandler within
/// the Unity3D Environment.
/// </summary>
public class CreatureHandler : MonoBehaviour 
{
    static List<Transform> creature_handlers = new List<Transform>(); // Used for keeping track of Creature Objects

    /// <summary>
    /// The Name of this Creature Object.
    /// </summary>
    public string Creature_Name = "NEWCREATURE";

    /// <summary>
    /// The Color of this Creature's SelectedIndicator.
    /// </summary>
    public Color ActiveColor = Color.magenta;
    bool active_target = false;

    private GUIStyle handler_style = new GUIStyle();

    List<GridGenerator.GridSquare> pathTiles = new List<GridGenerator.GridSquare>();
    private bool MovementToggle = false;
    private int indexMovetoCount = 1;

    private int ID = -1;

	/// <summary>
	/// Unity3D MonoBehavior.Start() override.
	/// </summary>
	void Start()
    {
        transform.Find("SelectedIndicator").gameObject.SetActive(false);
        transform.Find("SelectedIndicator").GetComponent<Renderer>().material.SetColor("_Color", new Color(ActiveColor.r + (ActiveColor.r / 2), ActiveColor.g + (ActiveColor.g / 2), ActiveColor.b + (ActiveColor.b / 2)));
        transform.Find("SelectedIndicator").GetComponent<Renderer>().material.SetColor("_Emission", ActiveColor);
        transform.Find("SelectedIndicator").localScale = new Vector3(.1f + transform.localScale.x * .03f, 1, .1f + transform.localScale.z * .03f);

        creature_handlers.Add(this.transform);
	}

    void OnDestroy()
    {
        creature_handlers.Remove(this.transform);
    }

    /// <summary>
    /// Unity3D MonoBehavior.OnGUI() override.
    /// </summary>
    void OnGUI()
    {
        if (active_target)
        {
            Vector3 creature_screen_pos = Camera.main.WorldToScreenPoint(this.transform.position + new Vector3(0, transform.localScale.y, 0));
            Vector2 handler_text_dimens = handler_style.CalcSize(new GUIContent(Creature_Name));

            GUI.Label(new Rect(creature_screen_pos.x - (handler_text_dimens.x) / 2, (Screen.height - creature_screen_pos.y) - handler_text_dimens.y, handler_text_dimens.x, handler_text_dimens.y * 2), Creature_Name);
        }
    }

    void Update()
    {
        if (indexMovetoCount < pathTiles.Count && Mathf.Sqrt(Mathf.Pow((transform.position.x - pathTiles[indexMovetoCount].getGridSquareCentralOrigin().x), 2) + Mathf.Pow((transform.position.z - pathTiles[indexMovetoCount].getGridSquareCentralOrigin().z), 2)) < .5f)
        {
            indexMovetoCount++; // increment indexMovetoCount for next tile to move to
        }
        else if (indexMovetoCount == pathTiles.Count && Moveable) // gets here once reached last point on pathTiles
        {
            indexMovetoCount = 1;
            Moveable = false;
        }

        if (ID != creature_handlers.IndexOf(this.transform))
        {
            ID = creature_handlers.IndexOf(this.transform);
            this.name = "CreatureObj" + ID;
        }
    }

    /// <summary>
    /// Receives a List of GridSquares that Represents this Creature's Move To Path.
    /// </summary>
    /// <param name="path">The List of GridSquares to be Received.</param>
    private void ReceiveCalculatedPath(List<GridGenerator.GridSquare> path)
    {
        pathTiles = path;
        indexMovetoCount = 1;
        Moveable = true;
    }

    /// <summary>
    /// Displays the Creature Object's Calculated Path Tiles using GL Functions.
    /// </summary>
    /// <param name="gridGen">The GridGenerator to Display the Calculated Path Tiles.</param>
    private void DisplayPath(GridGenerator gridGen)
    {
        for (int i = 0; i < pathTiles.Count; i++)
        {
            GL.Color(Color.yellow);
            GL.Vertex3(pathTiles[i].getGridSquareCentralOrigin().x, pathTiles[i].getGridSquareCentralOrigin().y, pathTiles[i].getGridSquareCentralOrigin().z);
            GL.Vertex3(pathTiles[i].getGridSquareCentralOrigin().x, pathTiles[i].getGridSquareCentralOrigin().y + gridGen.GridHeight / 2, pathTiles[i].getGridSquareCentralOrigin().z);

            GL.Color(Color.green);
            GL.Vertex3(pathTiles[i].getGridSquareCentralOrigin().x - 0.5f, pathTiles[i].getGridSquareCentralOrigin().y, pathTiles[i].getGridSquareCentralOrigin().z);
            GL.Vertex3(pathTiles[i].getGridSquareCentralOrigin().x - 0.5f, pathTiles[i].getGridSquareCentralOrigin().y + gridGen.GridHeight / 2, pathTiles[i].getGridSquareCentralOrigin().z);

            GL.Color(Color.blue);
            GL.Vertex3(pathTiles[i].getGridSquareCentralOrigin().x - 0.25f, pathTiles[i].getGridSquareCentralOrigin().y, pathTiles[i].getGridSquareCentralOrigin().z - 0.5f);
            GL.Vertex3(pathTiles[i].getGridSquareCentralOrigin().x - 0.25f, pathTiles[i].getGridSquareCentralOrigin().y + gridGen.GridHeight / 2, pathTiles[i].getGridSquareCentralOrigin().z - 0.5f);
        }
    }

    /// <summary>
    /// Is the Current Move To Path Tile not the Last Tile?
    /// </summary>
    public bool isValidMovePathTileCount()
    {
        if (indexMovetoCount < pathTiles.Count)
            return true;

        return false;
    }

    /// <summary>
    /// Gets the Path Tile the Creature Object is Moving Towards.
    /// </summary>
    /// <returns>The GridGenerator.GridSquare that the Creature Object is Moving Towards.</returns>
    public GridGenerator.GridSquare getMovePathTile()
    {
        GridGenerator.GridSquare movetoTile = pathTiles[indexMovetoCount];

        return movetoTile;
    }

    /// <summary>
    /// Does the Creature Object have a Path to Traverse?
    /// </summary>
    public bool Moveable
    {
        get
        {
            return MovementToggle;
        }

        set
        {
            MovementToggle = value;
        }
    }

    /// <summary>
    /// Sets whether the Creature Object is an ActiveTarget.
    /// </summary>
    /// <remarks>This is Called for when the Creature Object is Selected.</remarks>
    /// <param name="val">True or False.</param>
    public void setActiveTarget(bool val)
    {
        active_target = val;
    }

    /// <summary>
    /// Is the Creature Object an Active Target?
    /// </summary>
    public bool isActiveTarget()
    {
        return active_target;
    }

    /// <summary>
    /// Returns the Name of this Creature Object.
    /// </summary>
    public override string ToString()
    {
        return Creature_Name.ToString();
    }
}
