/// \file
/// Used for Handling Concepts Tied Specifically to Control Objects.
/// @author: Chase Hutchens

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Creates a Unity3D Object to Inherit the Functionality of a DC/RTS ControlObjHandler within
/// the Unity3D Environment.
/// </summary>
public class ControlObjHandler : MonoBehaviour 
{
    static List<Transform> control_handlers = new List<Transform>(); // Used for keeping track of Control Objects

    private bool active_target = false;
    private bool active_collision = false;

    private List<GridGenerator.GridSquare> pathTiles = new List<GridGenerator.GridSquare>();
    private bool MovementToggle = false;
    private int indexMovetoCount = 1;

    private int ID = -1;

    private HealthSystem healthSystem;
    private int lineWidth = 2;

    /// <summary>
    /// The Name of this Control Object.
    /// </summary>
    public string Controller_Name = "NEWCONTROLLER";

    /// <summary>
    /// The Color of this Control Object's SelectedIndicator.
    /// </summary>
    public Color ActiveColor = Color.white;

    /// <summary>
    /// Will this Control Object use the Toggle Indicator?
    /// </summary>
    /// <remarks>This is the selected outline around the Control Object</remarks>
    public bool ToggleIndicatorSelect = true;

    /// <summary>
    /// Will this Control Object display health above it's head?
    /// </summary>
    public bool DisplayHealth = true;

    /// <summary>
    /// The Indicator Material to use for drawing the Toggle Indicator around the Control Object
    /// </summary>
    public Material IndicatorMaterial;

    /// <summary>
    /// The Dimensions of the Health Bar above the Control Object's head
    /// </summary>
    public Rect HealthBarDimens;

    /// <summary>
    /// Is this a Verticle Health Bar?
    /// </summary>
    public bool VerticleHealthBar;

    /// <summary>
    /// The Texture of an individual health bubble
    /// </summary>
    public Texture HealthBubbleTexture;

    /// <summary>
    /// The Texture to use to indicate health
    /// </summary>
    public Texture HealthTexture;

    /// <summary>
    /// The rotation of this health bar
    /// </summary>
    public float BarRotationVal;

	void Start()
    {
        transform.Find("SelectedIndicator").gameObject.SetActive(false);
        transform.Find("SelectedIndicator").GetComponent<Renderer>().material.SetColor("_Color", new Color(ActiveColor.r + (ActiveColor.r / 2), ActiveColor.g + (ActiveColor.g / 2), ActiveColor.b + (ActiveColor.b / 2)));
        transform.Find("SelectedIndicator").GetComponent<Renderer>().material.SetColor("_Emission", ActiveColor);
        transform.Find("SelectedIndicator").localScale = new Vector3(.1f + transform.localScale.x * .03f, 1, .1f + transform.localScale.z * .03f);

        control_handlers.Add(this.transform);

        healthSystem = new HealthSystem(HealthBarDimens, VerticleHealthBar, HealthBubbleTexture, HealthTexture, BarRotationVal);
        healthSystem.Initialize();
    }

    void OnDestroy()
    {
        control_handlers.Remove(this.transform);
    }

    void OnGUI()
    {
        if (transform.Find("SelectedIndicator").gameObject.activeSelf)
        {
            if (DisplayHealth)
                healthSystem.DrawBar();

            if (ToggleIndicatorSelect)
            {
                Vector3 LLparentPos = Camera.main.WorldToScreenPoint(transform.position + (transform.localScale.magnitude / 2 * -Camera.main.transform.right - new Vector3(0, transform.localScale.y + transform.localScale.y / 2, 0)));
                Vector3 ULparentPos = Camera.main.WorldToScreenPoint(transform.position + (transform.localScale.magnitude / 2 * -Camera.main.transform.right + new Vector3(0, transform.localScale.y + transform.localScale.y / 2, 0)));
                Vector3 LRparentPos = Camera.main.WorldToScreenPoint(transform.position + (transform.localScale.magnitude / 2 * Camera.main.transform.right - new Vector3(0, transform.localScale.y + transform.localScale.y / 2, 0)));
                Vector3 URparentPos = Camera.main.WorldToScreenPoint(transform.position + (transform.localScale.magnitude / 2 * Camera.main.transform.right + new Vector3(0, transform.localScale.y + transform.localScale.y / 2, 0)));

                if (isInFOV(LLparentPos) || isInFOV(ULparentPos) || isInFOV(LRparentPos) || isInFOV(URparentPos))
                {
                    GL.PushMatrix();
                    IndicatorMaterial.SetPass(0);
                    GL.LoadPixelMatrix();
                    GL.Begin(GL.LINES);
                    GL.Color(IndicatorMaterial.color);

                    for (int i = 0; i < lineWidth; i++)
                    {
                        // left vert line
                        GL.Vertex3(LLparentPos.x - i, LLparentPos.y, 0);
                        GL.Vertex3(ULparentPos.x - i, ULparentPos.y, 0);

                        // right vert line
                        GL.Vertex3(LRparentPos.x + i, LRparentPos.y, 0);
                        GL.Vertex3(URparentPos.x + i, URparentPos.y, 0);

                        // upper left and upper right horiz line
                        GL.Vertex3(ULparentPos.x, ULparentPos.y - i, 0);
                        GL.Vertex3(ULparentPos.x + (URparentPos.x - ULparentPos.x) / 4, ULparentPos.y - i, 0);
                        GL.Vertex3(URparentPos.x, URparentPos.y - i, 0);
                        GL.Vertex3(URparentPos.x + (ULparentPos.x - URparentPos.x) / 4, URparentPos.y - i, 0);

                        // lower left and lower right horiz line
                        GL.Vertex3(LLparentPos.x, LLparentPos.y + i, 0);
                        GL.Vertex3(LLparentPos.x + (LRparentPos.x - LLparentPos.x) / 4, LLparentPos.y + i, 0);
                        GL.Vertex3(LRparentPos.x, LRparentPos.y + i, 0);
                        GL.Vertex3(LRparentPos.x + (LLparentPos.x - LRparentPos.x) / 4, LRparentPos.y + i, 0);
                    }

                    GL.End();
                    GL.PopMatrix();
                }
            }
        }
    }

    void Update()
    {
        // This statement is used for incrementing the path move to square
        if (indexMovetoCount < pathTiles.Count && Mathf.Sqrt(Mathf.Pow((transform.position.x - pathTiles[indexMovetoCount].getGridSquareCentralOrigin().x), 2) + Mathf.Pow((transform.position.z - pathTiles[indexMovetoCount].getGridSquareCentralOrigin().z), 2)) < .5f)
        {
            indexMovetoCount++; // increment indexMovetoCount for next tile to move to
        }
        else if (indexMovetoCount == pathTiles.Count && Moveable) // gets here once reached last point on pathTiles
        {            
            indexMovetoCount = 1;
            Moveable = false;
        }

        if (ID != control_handlers.IndexOf(this.transform))
        {
            ID = control_handlers.IndexOf(this.transform);
            this.name = "ControlObj" + ID;
        }

        Vector3 controller_screen_pos = Camera.main.WorldToScreenPoint(this.transform.position + new Vector3(0, transform.localScale.y + transform.localScale.y / 2, 0));
        controller_screen_pos.y = Screen.height - controller_screen_pos.y;

        if (healthSystem.getScrollBarRect().x != controller_screen_pos.x && healthSystem.getScrollBarRect().y != controller_screen_pos.y)
            healthSystem.Update((int)controller_screen_pos.x, (int)controller_screen_pos.y);
    }

    void OnCollisionEnter(Collision collision)
    {
        active_collision = true;
    }

    void OnCollisionExit(Collision collision)
    {
        active_collision = false;
    }

    /// <summary>
    /// Used for Sending a Control Object the Calculated Path Tiles from the PathFinder class.
    /// </summary>
    /// <param name="path">The Calculated Path Tiles.</param>
    private void ReceiveCalculatedPath(List<GridGenerator.GridSquare> path)
    {
        pathTiles = path;
        indexMovetoCount = 1;
        Moveable = true;
    }

    /// <summary>
    /// Displays the Control Object's Calculated Path Tiles using GL Functions.
    /// </summary>
    /// <remarks>This is Used in GridRender, but can be Utilized elsewhere.</remarks>
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
    /// Determines if the worldPos is in the Field of View of the Screen.
    /// </summary>
    /// <param name="worldPos">The World Position of a Particular Vector3.</param>
    /// <returns>In View or Not In View</returns>
    private bool isInFOV(Vector3 worldPos)
    {
        if (worldPos.x >= 0 && worldPos.x <= Screen.width && worldPos.y >= 0 && worldPos.y <= Screen.height)
            return true;

        return false;
    }

    /// <summary>
    /// Determines if Control Objects are within the MultiSelectToggle based on Screen Coordinates.
    /// </summary>
    /// <param name="init">The Initial Screen Coordinate.</param>
    /// <param name="end">The Final Screen Coordinate.</param>
    /// <returns>List of Unity3D Transform Objects within the Bounds of the init and end Vector2.</returns>
    public static List<Transform> DetermineInMultiSelect(Vector2 init, Vector2 end)
    {
        List<Transform> toggled_controllers = new List<Transform>();

        foreach (Transform trans in control_handlers)
        {
            Vector2 trans2screen = Camera.main.WorldToScreenPoint(trans.position);
            trans2screen.y = (Screen.height - trans2screen.y);
            
            if ((trans2screen.x >= init.x && trans2screen.x <= end.x) && (trans2screen.y >= init.y && trans2screen.y <= end.y) ||
                (trans2screen.x <= init.x && trans2screen.x >= end.x) && (trans2screen.y <= init.y && trans2screen.y >= end.y) ||
                (trans2screen.x >= end.x && trans2screen.y <= end.y) && (trans2screen.x <= init.x && trans2screen.y >= init.y) ||
                (trans2screen.x >= init.x && trans2screen.y <= init.y) && (trans2screen.x <= end.x && trans2screen.y >= end.y))
            {
                if (!toggled_controllers.Contains(trans))
                {
                    toggled_controllers.Add(trans);
                }
            }
        }

        return toggled_controllers;
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
    /// Gets the Path Tile the Control Object is Moving Towards.
    /// </summary>
    /// <returns>The GridGenerator.GridSquare that the Control Object is Moving Towards.</returns>
    public GridGenerator.GridSquare getMovePathTile()
    {
        GridGenerator.GridSquare movetoTile = pathTiles[indexMovetoCount];

        return movetoTile;
    }

    /// <summary>
    /// Does the Control Object have a Path to Traverse?
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
    /// Sets whether the Control Object is an ActiveTarget.
    /// </summary>
    /// <remarks>This is Called for when the Control Object is Selected.</remarks>
    /// <param name="val">True or False.</param>
    public void setActiveTarget(bool val)
    {
        active_target = val;
    }

    /// <summary>
    /// Is the Control Object an Active Target?
    /// </summary>
    public bool isActiveTarget()
    {
        return active_target;
    }

    /// <summary>
    /// Is the Control Object Colliding with anything?
    /// </summary>
    /// <returns></returns>
    public bool isActiveCollision()
    {
        return active_collision;
    }

    /// <summary>
    /// Returns the Name of this Control Object.
    /// </summary>
    public override string ToString()
    {
        return Controller_Name.ToString();
    }
}
