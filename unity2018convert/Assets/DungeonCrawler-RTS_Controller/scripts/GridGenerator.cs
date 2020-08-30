/// \file
/// Handles Data Representing the GridGenerator. Generating GridSquareQuadrants that are made up of GridSquares.
/// @author: Chase Hutchens

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class generates a grid a long the xz plane. 
/// The grid is made up of GridQuadrants that are made up of GridSquares,
/// where the grid is determined based off raycasts.
/// The calculated grids are stored in a 2D array of
/// GridSquares, which can be acessed by: getDesiredGridSquare(int r, int c) function.
/// This GridGenerator is utilized within the PathFinder.cs object, with the
/// A* pathfinding implementation.
/// </summary>
/// <remarks>For the grid to generate properly, at this time, slopes need to be parallel to either the x or z axis.</remarks>
public class GridGenerator : MonoBehaviour
{
    GridSquareQuadrant[,] gridQuadrants;
    PathFinder pathFinder;

    RaycastHit raycastInfo = new RaycastHit();

    /// <summary>
    /// The Layermask this GridGenerator will Ignore (Control Objects, NPC Objects, Creature Objects, etc)
    /// </summary>
    public LayerMask IgnoreMask;

    /// <summary>
    /// The Number of Rows this GridGenerator is Made Up of.
    /// </summary>
    public int Rows = 16;

    /// <summary>
    /// The Number of Columns this GridGenerator is Made Up of.
    /// </summary>
    public int Columns = 16;

    /// <summary>
    /// The Width of an Individual GridSquare.
    /// </summary>
    public int GridWidth = 10;

    /// <summary>
    /// The Height of an Individual GridSquare.
    /// </summary>
    /// <remarks>This Height is also Used for Calculating from the Top Down, it's a Very Important Factor in the Calculation.</remarks>
    public int GridHeight = 20;

    /// <summary>
    /// The Depth of an Individual GridSquare.
    /// </summary>
    public int GridDepth = 10;

    /// <summary>
    /// Used for Dividing the Grid Into Individual Quadrants Made Up of GridQuadSize GridSquares.
    /// </summary>
    public int GridQuadSize = 8; // divides the grid into individual quadrants

    /// <summary>
    /// The Minimum Slope Angle to Determine.
    /// </summary>
    public float MinSlopeAngle = 5.0f;

    /// <summary>
    /// The Maximum Slope Angle to Determine.
    /// </summary>
    public float MaxSlopeAngle = 30.0f;

    /// <summary>
    /// Linear Rigidity, in Terms of How Bumpy the GridGenerator will Allow a GridSquare Along it's GridSquare Generation.
    /// </summary>
    public float Rigidity = .1f;

    /// <summary>
    /// Angular Rigidity, in Terms of the Factor of how Smooth a Slope is.
    /// </summary>
    public float AngularRigidity = 0.01f;

	void Start()
    {
        gridQuadrants = new GridSquareQuadrant[Mathf.FloorToInt(Rows / GridQuadSize) + 1, Mathf.FloorToInt(Columns / GridQuadSize) + 1];

        Vector3 lowerLeft = new Vector3(GridWidth, 0, GridDepth);
        Vector3 lowerRight = new Vector3(lowerLeft.x + GridWidth, 0, lowerLeft.z);
        Vector3 upperLeft = new Vector3(lowerLeft.x, 0, lowerLeft.z + GridDepth);
        Vector3 upperRight = new Vector3(lowerRight.x, 0, lowerRight.z + GridDepth);

        int[] activeGridQuad;

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                lowerLeft = new Vector3(i * GridWidth, 0, j * GridDepth);
                lowerRight = new Vector3(lowerLeft.x + GridWidth, 0, lowerLeft.z);
                upperLeft = new Vector3(lowerLeft.x, 0, lowerLeft.z + GridDepth);
                upperRight = new Vector3(lowerRight.x, 0, lowerRight.z + GridDepth);

                activeGridQuad = DetermineActiveGridQuad(i, j);

                try
                {
                    if (gridQuadrants[activeGridQuad[0], activeGridQuad[1]] == null)
                    {
                        if (gridQuadrants[0, 0] == null) // Initialization
                            gridQuadrants[activeGridQuad[0], activeGridQuad[1]] = new GridSquareQuadrant(GridQuadSize, Rows, Columns, activeGridQuad[0], activeGridQuad[1]);
                        else
                            gridQuadrants[activeGridQuad[0], activeGridQuad[1]] = new GridSquareQuadrant(activeGridQuad[0], activeGridQuad[1]);
                    }
                }
                catch (System.IndexOutOfRangeException) { }

                // if the corner points of the grid ray casted from the GridHeight downwards collide with something that isn't a ControlObject
                if (Physics.Raycast(new Ray(transform.position + lowerLeft + new Vector3(0, GridHeight, 0), -transform.up), GridHeight + (GridHeight / 2), ~IgnoreMask.value)
                    && Physics.Raycast(new Ray(transform.position + lowerRight + new Vector3(0, GridHeight, 0), -transform.up), GridHeight + (GridHeight / 2), ~IgnoreMask.value)
                    && Physics.Raycast(new Ray(transform.position + upperLeft + new Vector3(0, GridHeight, 0), -transform.up), GridHeight + (GridHeight / 2), ~IgnoreMask.value)
                    && Physics.Raycast(new Ray(transform.position + upperRight + new Vector3(0, GridHeight, 0), -transform.up), GridHeight + (GridHeight / 2), ~IgnoreMask.value))
                {
                    // these are the main raycasts that determine a grid areas behavior
                    RaycastHit[] c_points = new RaycastHit[4];

                    Physics.Raycast(new Ray(transform.position + lowerLeft + new Vector3(0, GridHeight, 0), -transform.up), out raycastInfo, GridHeight + (GridHeight / 2), ~IgnoreMask.value);
                    c_points[0] = raycastInfo;

                    Physics.Raycast(new Ray(transform.position + lowerRight + new Vector3(0, GridHeight, 0), -transform.up), out raycastInfo, GridHeight + (GridHeight / 2), ~IgnoreMask.value);
                    c_points[1] = raycastInfo;

                    Physics.Raycast(new Ray(transform.position + upperLeft + new Vector3(0, GridHeight, 0), -transform.up), out raycastInfo, GridHeight + (GridHeight / 2), ~IgnoreMask.value);
                    c_points[2] = raycastInfo;

                    Physics.Raycast(new Ray(transform.position + upperRight + new Vector3(0, GridHeight, 0), -transform.up), out raycastInfo, GridHeight + (GridHeight / 2), ~IgnoreMask.value);
                    c_points[3] = raycastInfo;
                    
                    // [0] - determines angle between lowerLeft and upperLeft
                    // [1] - determines angle between lowerLeft and lowerRight
                    float[] slopedAngle = { Mathf.Atan((c_points[2].point.y - c_points[0].point.y) / (c_points[2].point.z - c_points[0].point.z)),
                                            Mathf.Atan((c_points[1].point.y - c_points[0].point.y) / (c_points[1].point.x - c_points[0].point.x)) };

                    // Rigidity is the minimum difference between the raycast points
                    if (Mathf.Abs(c_points[0].distance - c_points[1].distance) <= Rigidity && Mathf.Abs(c_points[0].distance - c_points[2].distance) <= Rigidity && Mathf.Abs(c_points[0].distance - c_points[3].distance) <= Rigidity)
                    {
                        // if the previous grid behind this one exists
                        if (j - 1 >= 0 && GridSquareQuadrant.retrieveSpecificSquare(i, j - 1) != null)
                        {
                            upperLeft = new Vector3(GridSquareQuadrant.retrieveSpecificSquare(i, j - 1).getGridSquareLowerLeft().x - transform.position.x, lowerLeft.y + (c_points[2].point.y - lowerLeft.y), lowerLeft.z + GridDepth);
                            upperRight = new Vector3(GridSquareQuadrant.retrieveSpecificSquare(i, j - 1).getGridSquareUpperRight().x - transform.position.x, upperLeft.y, lowerRight.z + GridDepth);
                            lowerLeft = new Vector3(GridSquareQuadrant.retrieveSpecificSquare(i, j - 1).getGridSquareUpperLeft().x - transform.position.x, GridSquareQuadrant.retrieveSpecificSquare(i, j - 1).getGridSquareUpperLeft().y - transform.position.y, GridSquareQuadrant.retrieveSpecificSquare(i, j - 1).getGridSquareUpperLeft().z - transform.position.z);
                            lowerRight = new Vector3(lowerLeft.x + GridWidth, lowerLeft.y, lowerLeft.z);
                        }
                        else
                        {
                            if (c_points[2].distance < c_points[0].distance) // going up a slope
                                lowerLeft = new Vector3(c_points[0].point.x - transform.position.x, lowerLeft.y + (c_points[2].point.y - lowerLeft.y), c_points[0].point.z - transform.position.z);
                            else if (c_points[0].distance < c_points[2].distance) // going down a slope
                                lowerLeft = new Vector3(c_points[0].point.x - transform.position.x, lowerLeft.y + (c_points[0].point.y - lowerLeft.y), c_points[0].point.z - transform.position.z);
                            else // default
                                lowerLeft = new Vector3(c_points[0].point.x - transform.position.x, lowerLeft.y + (c_points[2].point.y - lowerLeft.y), c_points[0].point.z - transform.position.z);

                            lowerRight = new Vector3(lowerLeft.x + GridWidth, lowerLeft.y, lowerLeft.z);
                            upperLeft = new Vector3(lowerLeft.x, lowerLeft.y, lowerLeft.z + GridDepth);
                            upperRight = new Vector3(lowerRight.x, lowerLeft.y, lowerRight.z + GridDepth);
                        }

                        // lowerLeft && lowerRight up isn't colliding, and lowerLeft right && lowerRight left isn't colliding && lowerLeft && lowerRight forward isn't colliding
                        if (!Physics.Raycast(new Ray(transform.position + lowerLeft, transform.up), GridHeight, ~IgnoreMask.value)
                            && !Physics.Raycast(new Ray(transform.position + lowerRight, transform.up), GridHeight, ~IgnoreMask.value)
                            && !Physics.Raycast(new Ray(transform.position + lowerLeft, transform.right), GridWidth, ~IgnoreMask.value)
                            && !Physics.Raycast(new Ray(transform.position + lowerRight, -transform.right), GridWidth, ~IgnoreMask.value)
                            && !Physics.Raycast(new Ray(transform.position + lowerLeft, (upperLeft - lowerLeft).normalized), GridDepth, ~IgnoreMask.value)
                            && !Physics.Raycast(new Ray(transform.position + lowerRight, (upperRight - lowerRight).normalized), GridDepth, ~IgnoreMask.value))
                        {                            
                            GridSquare newGridSquare = new GridSquare(this, i, j, 0.0f, activeGridQuad,lowerLeft, lowerRight, upperLeft, upperRight);
                            GridSquareQuadrant.addSquaretoQuad(newGridSquare, i, j);
                        }
                    }
                    // slopedAngled[0] represents the angle between the c_points[2] (Top Left) and the c_points[0] (Lower Left)
                    // this also represents the slope going from backward to forward
                    else if ((slopedAngle[0] >= Mathf.Deg2Rad * MinSlopeAngle && slopedAngle[0] <= Mathf.Deg2Rad * MaxSlopeAngle)
                            || (slopedAngle[0] >= -Mathf.Deg2Rad * MaxSlopeAngle && slopedAngle[0] <= -Mathf.Deg2Rad * MinSlopeAngle))
                    {
                        float lowerLeft_toForward_angle = Mathf.Atan((c_points[2].point.y - c_points[0].point.y) / (c_points[2].point.z - c_points[0].point.z));
                        float lowerRight_toForward_angle = Mathf.Atan((c_points[3].point.y - c_points[1].point.y) / (c_points[3].point.z - c_points[1].point.z));

                        if (Mathf.Abs(lowerLeft_toForward_angle - lowerRight_toForward_angle) <= AngularRigidity) // 0.01f is a good angular rigidity value for smooth slopes
                        {
                            if ((j - 1 >= 0 && GridSquareQuadrant.retrieveSpecificSquare(i, j - 1) != null))
                            {
                                lowerLeft = new Vector3(lowerLeft.x, GridSquareQuadrant.retrieveSpecificSquare(i, j - 1).getGridSquareUpperLeft().y - transform.position.y, GridSquareQuadrant.retrieveSpecificSquare(i, j - 1).getGridSquareUpperLeft().z - transform.position.z);
                                lowerRight = new Vector3(lowerRight.x, GridSquareQuadrant.retrieveSpecificSquare(i, j - 1).getGridSquareUpperRight().y - transform.position.y, GridSquareQuadrant.retrieveSpecificSquare(i, j - 1).getGridSquareUpperRight().z - transform.position.z);
                            }
                            else
                            {
                                lowerLeft = new Vector3(c_points[0].point.x - transform.position.x, c_points[0].point.y - transform.position.y, c_points[0].point.z - transform.position.z);
                                lowerRight = new Vector3(c_points[1].point.x - transform.position.x, lowerRight.y + (c_points[1].point.y - lowerRight.y), lowerRight.z);
                            }

                            upperLeft = new Vector3(lowerLeft.x, upperLeft.y + (c_points[2].point.y - upperLeft.y), c_points[2].point.z - transform.position.z);
                            upperRight = new Vector3(lowerRight.x, upperRight.y + (c_points[3].point.y - upperRight.y), c_points[3].point.z - transform.position.z);

                            // lowerLeft and lowerRight up not colliding, lowerLeft right and lowerRight left not colliding, lowerLeft and lowerRight foward colliding, lowerLeft and lowerRight forward angled not colliding
                            if (!Physics.Raycast(new Ray(transform.position + lowerLeft, transform.up), GridHeight, ~IgnoreMask.value)
                                && !Physics.Raycast(new Ray(transform.position + lowerRight, transform.up), GridHeight, ~IgnoreMask.value)
                                && !Physics.Raycast(new Ray(transform.position + lowerLeft, transform.right), GridWidth, ~IgnoreMask.value)
                                && !Physics.Raycast(new Ray(transform.position + lowerRight, -transform.right), GridWidth, ~IgnoreMask.value)
                                && !Physics.Raycast(new Ray(transform.position + lowerLeft, (upperLeft - lowerLeft).normalized), GridDepth, ~IgnoreMask.value)
                                && !Physics.Raycast(new Ray(transform.position + lowerRight, (upperRight - lowerRight).normalized), GridDepth, ~IgnoreMask.value))
                            {                                
                                GridSquare newGridSquare = new GridSquare(this, i, j, slopedAngle[0], activeGridQuad, lowerLeft, lowerRight, upperLeft, upperRight);
                                GridSquareQuadrant.addSquaretoQuad(newGridSquare, i, j);
                            }
                        }
                    }
                    // slopedAngled[1] represents the angle between the c_points[1] (Lower Left) and the c_points[0] (Lower Right)
                    // this also represents the slope going from left to right
                    else if ((slopedAngle[1] >= Mathf.Deg2Rad * MinSlopeAngle && slopedAngle[1] <= Mathf.Deg2Rad * MaxSlopeAngle)
                            || (slopedAngle[1] >= -Mathf.Deg2Rad * MaxSlopeAngle && slopedAngle[1] <= -Mathf.Deg2Rad * MinSlopeAngle))
                    {
                        float lowerLeft_toRight_angle = Mathf.Atan((c_points[1].point.y - c_points[0].point.y) / (c_points[1].point.x - c_points[0].point.x));
                        float upperLeft_toRight_angle = Mathf.Atan((c_points[3].point.y - c_points[2].point.y) / (c_points[3].point.x - c_points[2].point.x));

                        if (Mathf.Abs(lowerLeft_toRight_angle - upperLeft_toRight_angle) <= AngularRigidity) // 0.01f is a good angular rigidity value for smooth slopes
                        {
                            if (i - 1 > 0 && GridSquareQuadrant.retrieveSpecificSquare(i - 1, j) != null)
                            {
                                lowerLeft = new Vector3(c_points[0].point.x - transform.position.x, GridSquareQuadrant.retrieveSpecificSquare(i - 1, j).getGridSquareLowerRight().y - transform.position.y, GridSquareQuadrant.retrieveSpecificSquare(i - 1, j).getGridSquareLowerRight().z - transform.position.z);
                                upperLeft = new Vector3(c_points[2].point.x - transform.position.x, GridSquareQuadrant.retrieveSpecificSquare(i - 1, j).getGridSquareUpperRight().y - transform.position.y, GridSquareQuadrant.retrieveSpecificSquare(i - 1, j).getGridSquareUpperRight().z - transform.position.z);
                            }
                            else
                            {
                                lowerLeft = new Vector3(c_points[0].point.x - transform.position.x, c_points[0].point.y - transform.position.y, c_points[0].point.z - transform.position.z);
                            }

                            lowerRight = new Vector3(c_points[1].point.x - transform.position.x, lowerRight.y + (c_points[1].point.y - lowerRight.y), lowerRight.z);
                            upperRight = new Vector3(c_points[3].point.x - transform.position.x, upperRight.y + (c_points[3].point.y - upperRight.y), upperRight.z);

                            // lowerLeft and upperLeft up not colliding, lowerLeft forward and lowerLeft backward not colliding, lowerLeft toward lowerRight not colliding, and upperLeft toward upperRight not colliding
                            if (!Physics.Raycast(new Ray(transform.position + lowerLeft, transform.up), GridHeight, ~IgnoreMask.value)
                                && !Physics.Raycast(new Ray(transform.position + upperLeft, transform.up), GridHeight, ~IgnoreMask.value)
                                && !Physics.Raycast(new Ray(transform.position + lowerLeft, transform.forward), GridWidth, ~IgnoreMask.value)
                                && !Physics.Raycast(new Ray(transform.position + upperLeft, -transform.forward), GridWidth, ~IgnoreMask.value)
                                && !Physics.Raycast(new Ray(transform.position + lowerLeft, (lowerRight - lowerLeft).normalized), GridDepth, ~IgnoreMask.value)
                                && !Physics.Raycast(new Ray(transform.position + upperLeft, (upperRight - upperLeft).normalized), GridDepth, ~IgnoreMask.value))
                            {
                                GridSquare newGridSquare = new GridSquare(this, i, j, slopedAngle[1], activeGridQuad, lowerLeft, lowerRight, upperLeft, upperRight);
                                GridSquareQuadrant.addSquaretoQuad(newGridSquare, i, j);
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i < Mathf.FloorToInt(Rows / GridQuadSize) + 1; i++)
        {
            for (int j = 0; j < Mathf.FloorToInt(Columns / GridQuadSize) + 1; j++)
            {
                if (gridQuadrants[i, j] != null)
                    gridQuadrants[i, j].determineAdjacentQuadrants(gridQuadrants);
            }
        }

        // this loop trims off grid squares that aren't connected to the proper amount of adjacent squares (7 default)
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (GridSquareQuadrant.retrieveSpecificSquare(i, j) != null)
                {
                    if (DetermineEmptyAdjacentCount(i, j) >= 7)
                        GridSquareQuadrant.removeSpecificSquare(i, j);
                    else
                        GridSquareQuadrant.retrieveSpecificSquare(i, j).calculateAdjacentSquares();
                }
            }
        }

        pathFinder = new PathFinder(this);
        //previousSquare = getDesiredGridSquare(0, 0);
        //DetermineGridCheckPoints(new Vector3(560, 0, 795));
	}

    private void Update()
    {
        pathFinder.CheckPathCalculation();
    }

    /// <summary>
    /// Determines the amount of empty adjacent GridSquares to a specific Row and Column.
    /// </summary>
    /// <param name="row">The Row</param>
    /// <param name="col">The Column</param>
    /// <returns>Amount of empty adjacent GridSquares</returns>
    private int DetermineEmptyAdjacentCount(int row, int col)
    {
        int[] rows = { row - 1, row, row + 1 };
        int[] cols = { col - 1, col, col + 1 };
        int emptyCount = 0;

        for (int i = 0; i < rows.Length; i++)
        {
            for (int j = 0; j < cols.Length; j++)
            {               
                try
                {
                    if (GridSquareQuadrant.retrieveSpecificSquare(rows[i], cols[j]) == null)
                        emptyCount++;
                }
                catch (System.IndexOutOfRangeException) { emptyCount++; }
            }
        }

        return emptyCount;
    }

    /// <summary>
    /// Determines the Grid Coordinates of a Desired Vector3 within the GridGenerator.
    /// </summary>
    /// <param name="moveToPoint">The Desired Move To Point</param>
    /// <returns>A Two Element Array containing GridSquare Coordinates</returns>
    private int[] DetermineGridCheckPoints(Vector3 moveToPoint)
    {
        // starts at -1 due to if statements in while loops
        // as well as not to exceed the max GridWidth or GridDepth
        int[] grid_coords = { -1, -1 };

        // these are the initial coordinates of this GridGenerator
        int moveFrom_x = (int)transform.position.x;
        int moveFrom_z = (int)transform.position.z;

        while (moveFrom_x < moveToPoint.x) // while moveFrom_x doesn't exceed moveToPoint.x
        {
            if (moveToPoint.x > transform.position.x + GridWidth)
            {
                moveFrom_x += GridWidth; // incriment in x dir until found row
                grid_coords[0]++;
            }
            else
            {
                grid_coords[0] = 0;
                break;
            }
        }

        while (moveFrom_z < moveToPoint.z) // while moveFrom_z doesn't exceed moveToPoint.z
        {
            if (moveToPoint.z > transform.position.z + GridDepth)
            {
                moveFrom_z += GridDepth; // incriment in z dir until found col
                grid_coords[1]++;
            }
            else
            {
                grid_coords[1] = 0;
                break;
            }
        }

        if (GridSquareQuadrant.retrieveSpecificSquare(grid_coords[0], grid_coords[1]) == null)
        {
            Debug.Log("Grid Coord: (" + grid_coords[0] + ", " + grid_coords[1] + ") Does Not Exist");
            grid_coords[0] = -1;
            grid_coords[1] = -1;
        }

        return grid_coords;
    }

    /// <summary>
    /// Determines the GridQuadrant coordinates based off a Row and Column value.
    /// </summary>
    /// <param name="r">The Row</param>
    /// <param name="c">The Column</param>
    /// <returns>A Two Element Array containing GridQuadrant coordinates</returns>
    private int[] DetermineActiveGridQuad(int r, int c)
    {
        int[] quadVals = new int[2];

        quadVals[0] = Mathf.FloorToInt(r / GridQuadSize);
        quadVals[1] = Mathf.FloorToInt(c / GridQuadSize);

        return quadVals;
    }

    /// <summary>
    /// Utility function for Collecting Information about a Raycast.
    /// </summary>
    /// <param name="ray">The Ray to Cast</param>
    /// <param name="dist">The Distance</param>
    /// <param name="layerMask">The Layer Mask to Ignore</param>
    /// <returns>The RaycastHit Information</returns>
    private RaycastHit CollectedRaycastInfo(Ray ray, float dist, int layerMask)
    {
        RaycastHit collectedRay = new RaycastHit();

        Physics.Raycast(ray, out collectedRay, dist, layerMask);

        return collectedRay;
    }

    /// <summary>
    /// Wrapper Around PathFinder.CalculateNewPath for Calculating the Movement Path Given an Initial and Final World Position.
    /// </summary>
    /// <param name="init">Initial Position.</param>
    /// <param name="final">Final Position.</param>
    public void CalculateNewPath(Vector3 init, Vector3 final)
    {
        int[] initsquare = DetermineGridCheckPoints(init);
        int[] finalsquare = DetermineGridCheckPoints(final);

        if (initsquare[0] != -1 && initsquare[1] != -1 && finalsquare[0] != -1 && finalsquare[1] != -1)
            pathFinder.CalculateNewPath(getDesiredGridSquare(initsquare[0], initsquare[1]), getDesiredGridSquare(finalsquare[0], finalsquare[1]));
    }

    /// <summary>
    /// Wrapper Around PathFinder.CalculateNewPath for Calculating the Movement Path Given a Unity3D Transform Object and a Final World Position.
    /// </summary>
    /// <param name="moveObj">The Unity3D Transform Object to Move.</param>
    /// <param name="final">The Final Position.</param>
    public void CalculateNewPath(Transform moveObj, Vector3 final)
    {
        int[] initsquare = DetermineGridCheckPoints(moveObj.position);
        int[] finalsquare = DetermineGridCheckPoints(final);

        if (initsquare[0] != -1 && initsquare[1] != -1 && finalsquare[0] != -1 && finalsquare[1] != -1)
            pathFinder.CalculateNewPath(moveObj, getDesiredGridSquare(initsquare[0], initsquare[1]), getDesiredGridSquare(finalsquare[0], finalsquare[1]));
    }

    /// <summary>
    /// Retrieves a GridSquare from a specific Row and Column.
    /// </summary>
    /// <param name="r">The Row</param>
    /// <param name="c">The Column</param>
    /// <returns>The GridSquare</returns>
    public GridSquare getDesiredGridSquare(int r, int c)
    {
        GridSquare selectedSquare = GridSquareQuadrant.retrieveSpecificSquare(r, c);

        return selectedSquare;
    }

    /// <summary>
    /// Retrieves a GridQuadrant from a specific Quad Row and Quad Column.
    /// </summary>
    /// <param name="qr">The Quadrant Row</param>
    /// <param name="qc">The Quadrant Column</param>
    /// <returns>The Selected GridSquareQuadrant</returns>
    public GridSquareQuadrant getDesiredQuadrant(int qr, int qc)
    {
        GridSquareQuadrant selectedQuad = gridQuadrants[qr, qc];

        return selectedQuad;
    }

    /// <summary>
    /// Allows for Retrieval of the Paths Calculated Path Tiles.
    /// </summary>
    /// <returns>List of Grid Squares</returns>
    public List<GridSquare> getCalculatedPathTiles()
    {
        return pathFinder.getCalculatedTiles();
    }

    /// <summary>
    /// Resets both the GridQuandrants and GridSquares that are attached to the GridQuadrants.
    /// </summary>
    public void ResetGrid()
    {
        for (int i = 0; i < Mathf.FloorToInt(Rows / GridQuadSize) + 1; i++)
        {
            for (int j = 0; j < Mathf.FloorToInt(Columns / GridQuadSize) + 1; j++)
            {
                if (gridQuadrants[i, j] != null)
                    gridQuadrants[i, j].resetQuadrant();
            }
        }

        GridSquareQuadrant.resetQuadrantSquares(Rows, Columns);
    }

    /// <summary>
    /// Returns the total number of grid rows.
    /// </summary>
    /// <returns>Number of Rows</returns>
    public int getRows()
    {
        return Rows;
    }

    /// <summary>
    /// Returns the total number of grid columns.
    /// </summary>
    /// <returns>Number of Columns</returns>
    public int getColumns()
    {
        return Columns;
    }

    /// <summary>
    /// Used for Sorting GridSquares within a Data Structure that Contains an IComparer.
    /// </summary>
    public class GridComparer : IComparer<GridSquare>
    {
        /// <summary>
        /// Compares Two GridSquares Together, Determining which has the Smallest GridSquare.FMovementCost
        /// </summary>
        /// <returns>-1: Smaller, 0: Equal, 1: Larger</returns>
        public int Compare(GridSquare a, GridSquare b)
        {
            if (a.getFMovementCost() < b.getFMovementCost())
                return -1;
            else if (a.getFMovementCost() > b.getFMovementCost())
                return 1;
            else
                return 0;
        }
    }

    /// <summary>
    /// Used for Being Able to Easily keep Track of GridSquares Tied to Specific Quadrants.
    /// </summary>
    public class GridSquareQuadrant
    {
        static GridSquare[,] gridSquares;
        static int QuadSize = -1;
        List<GridSquareQuadrant> adjacentQuadrants = new List<GridSquareQuadrant>();
        int quad_row, quad_col = -1;

        /// <summary>
        /// Creates a new GridSquareQuadrant and initializes Static Values.
        /// </summary>
        /// <param name="quad_size">The Size of the Quadrant</param>
        /// <param name="t_rows">The Total Rows of all GridSquares</param>
        /// <param name="t_cols">The Total Columns of all GridSquares</param>
        /// <param name="q_row">This Quadrant's Row</param>
        /// <param name="q_col">This Quadrant's Column</param>
        public GridSquareQuadrant(int quad_size, int t_rows, int t_cols, int q_row, int q_col)
        {
            if (gridSquares == null)
            {
                gridSquares = new GridSquare[t_rows, t_cols];
                QuadSize = quad_size;
            }

            quad_row = q_row;
            quad_col = q_col;
        }

        /// <summary>
        /// Creates a new GridSquareQuadrant.
        /// </summary>
        /// <param name="q_row">This Quadrant's Row</param>
        /// <param name="q_col">This Quadrant's Column</param>
        public GridSquareQuadrant(int q_row, int q_col)
        {
            quad_row = q_row;
            quad_col = q_col;
        }

        /// <summary>
        /// Resets all the GridSquares attached to the Static GridSquares Array.
        /// </summary>
        /// <param name="t_r">The Total Rows</param>
        /// <param name="t_c">The Total Columns</param>
        public static void resetQuadrantSquares(int t_r, int t_c)
        {
            for (int i = 0; i < t_r; i++)
            {
                for (int j = 0; j < t_c; j++)
                {
                    if (gridSquares[i, j] != null)
                    {
                        gridSquares[i, j].isOnOpenList = false;
                        gridSquares[i, j].isOnCloseList = false;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a GridSquare from a Particular Row and Column.
        /// </summary>
        /// <param name="r">The Row</param>
        /// <param name="c">The Column</param>
        /// <returns></returns>
        public static GridSquare retrieveSpecificSquare(int r, int c)
        {
            GridSquare selectedSquare = gridSquares[r, c];

            return selectedSquare;
        }

        /// <summary>
        /// Removes a square from the Static GridSquares Array by Setting it to null.
        /// </summary>
        /// <param name="r">The Row</param>
        /// <param name="c">The Column</param>
        public static void removeSpecificSquare(int r, int c)
        {
            gridSquares[r, c] = null;
        }

        /// <summary>
        /// Adds a GridSquare to the Static GridSquares Array at a Particular Location.
        /// </summary>
        /// <param name="newSquare">The new GridSquare to add</param>
        /// <param name="r">The Row</param>
        /// <param name="c">The Column</param>
        public static void addSquaretoQuad(GridSquare newSquare, int r, int c)
        {
            gridSquares[r, c] = newSquare;
        }

        /// <summary>
        /// Determines Adjacent GridQuadrants to this GridQuadrant.
        /// </summary>
        /// <param name="quads">Two dimensional array of GridQuadrants</param>
        public void determineAdjacentQuadrants(GridSquareQuadrant[,] quads)
        {
            // Lower Left to Lower Right
            for (int i = 0; i < QuadSize; i++)
            {
                try
                {
                    if (gridSquares[(quad_row * QuadSize) + i, quad_col * QuadSize] != null && gridSquares[(quad_row * QuadSize) + i, quad_col * QuadSize - 1] != null)
                    {
                        adjacentQuadrants.Add(quads[quad_row, quad_col - 1]);
                        break;
                    }
                }
                catch (System.IndexOutOfRangeException) { }
            }

            // Lower Left to Upper Left
            for (int i = 0; i < QuadSize; i++)
            {
                try
                {
                    if (gridSquares[quad_row * QuadSize, (quad_col * QuadSize) + i] != null && gridSquares[quad_row * QuadSize - 1, (quad_col * QuadSize) + i] != null)
                    {
                        adjacentQuadrants.Add(quads[quad_row - 1, quad_col]);
                        break;
                    }
                }
                catch (System.IndexOutOfRangeException) { }
            }

            // Upper Left to Upper Right
            for (int i = 0; i < QuadSize; i++)
            {
                try
                {
                    if (gridSquares[(quad_row * QuadSize) + i, (quad_col * QuadSize) + QuadSize - 1] != null && gridSquares[(quad_row * QuadSize) + i, (quad_col * QuadSize) + QuadSize - 1] != null)
                    {
                        adjacentQuadrants.Add(quads[quad_row, quad_col + 1]);
                        break;
                    }
                }
                catch (System.IndexOutOfRangeException) { }
            }

            // Lower Right to Upper Right
            for (int i = 0; i < QuadSize; i++)
            {
                try
                {
                    if (gridSquares[(quad_row * QuadSize) + QuadSize - 1, (quad_col * QuadSize) + i] != null && gridSquares[(quad_row * QuadSize) + QuadSize, (quad_col * QuadSize) + i] != null)
                    {
                        adjacentQuadrants.Add(quads[quad_row + 1, quad_col]);
                        break;
                    }
                }
                catch (System.IndexOutOfRangeException) { }
            }

            try
            {
                // check lowerleft corner adjacent
                if (gridSquares[quad_row * QuadSize, quad_col * QuadSize] != null && gridSquares[quad_row * QuadSize - 1, quad_col * QuadSize - 1] != null)
                    adjacentQuadrants.Add(quads[quad_row - 1, quad_col - 1]);
            }
            catch (System.IndexOutOfRangeException) { }

            try
            {
                // check upperleft corner adjacent
                if (gridSquares[quad_row * QuadSize, (quad_col * QuadSize) + QuadSize - 1] != null && gridSquares[quad_row * QuadSize - 1, (quad_col * QuadSize) + QuadSize] != null)
                    adjacentQuadrants.Add(quads[quad_row - 1, quad_col + 1]);
            }
            catch (System.IndexOutOfRangeException) { }

            try
            {
                // check lowerright corner adjacent
                if (gridSquares[(quad_row * QuadSize) + QuadSize - 1, quad_col * QuadSize] != null && gridSquares[(quad_row * QuadSize) + QuadSize, quad_col * QuadSize - 1] != null)
                    adjacentQuadrants.Add(quads[quad_row + 1, quad_col - 1]);
            }
            catch (System.IndexOutOfRangeException) { }

            try
            {
                // check upperright corner adjacent
                if (gridSquares[(quad_row * QuadSize) + QuadSize - 1, (quad_col * QuadSize) + QuadSize - 1] != null && gridSquares[(quad_row * QuadSize) + QuadSize, (quad_col * QuadSize) + QuadSize] != null)
                    adjacentQuadrants.Add(quads[quad_row + 1, quad_col + 1]);
            }
            catch (System.IndexOutOfRangeException) { }
        }

        /// <summary>
        /// Retrieves the GridSquares Attached to this GridQuadrant.
        /// </summary>
        /// <returns>Array of GridSquares</returns>
        public GridSquare[] retrieveQuadrantSquares()
        {
            List<GridSquare> quadSquares = new List<GridSquare>();

            for (int i = quad_row * QuadSize; i < (quad_row * QuadSize) + QuadSize; i++)
            {
                for (int j = quad_col * QuadSize; j < (quad_col * QuadSize) + QuadSize; j++)
                {
                    try
                    {
                        if (gridSquares[i, j] != null)
                            quadSquares.Add(gridSquares[i, j]);
                    }
                    catch (System.IndexOutOfRangeException) { }
                }
            }

            return quadSquares.ToArray();
        }

        /// <summary>
        /// Used for Reseting any Information Tied to this Specific Quadrant.
        /// </summary>
        public void resetQuadrant()
        {
            // implement reset information here if need be.
        }

        /// <summary>
        /// Retrieves the Row Value of this GridQuadrant.
        /// </summary>
        /// <returns>Quad Row Value</returns>
        public int getRow()
        {
            return quad_row;
        }

        /// <summary>
        /// Retrieves the Column Value of this GridQuadrant.
        /// </summary>
        /// <returns>Quad Column Value</returns>
        public int getColumn()
        {
            return quad_col;
        }

        /// <summary>
        /// Retrieves this GridQuadrant's Adjacent GridQuadrants
        /// </summary>
        /// <returns>List of adjacent GridQuadrants</returns>
        public List<GridSquareQuadrant> getAdjacentQuadrants()
        {
            return adjacentQuadrants;
        }
    }

    /// <summary>
    /// Represents a 3D GridSquare within the xz-plane.
    /// </summary>
    /// <remarks>
    /// <pre>
    /// xz-plane
    /// upperLeft ---- w ---- upperRight
    ///           |         |
    ///          d|         |d
    ///           |         |
    ///           |         |
    /// lowerLeft ---- w ---- lowerRight
    /// </pre> 
    /// Each GridSquare is Attached to a GridSquareQuadrant.
    ///  </remarks>
    public class GridSquare
    {        
        static GridGenerator parentGenerator;
        int row, col = -1;
        float slopedAngle = 0.0f;
        int[] quadrant;
        Vector3[] positions = new Vector3[4]; // { LowerLeft, LowerRight, UpperLeft, UpperRight }
        List<GridSquare> adjacentSquares = new List<GridSquare>();
        
        int f_val, g_val, h_val = 0;
        GridSquare parentSquare = null;
        bool onOpenList, onCloseList = false;

        /// <summary>
        /// Generates a GridSquare.
        /// </summary>
        /// <param name="parent">Parent GridGenerator</param>
        /// <param name="r">The Row of this GridSquare</param>
        /// <param name="c">The Column of this GridSquare</param>
        /// <param name="slope">The Slope of this GridSquare</param>
        /// <param name="quad">The Two Element integer array for the Quadrant coordinates this GridSquare lies in</param>
        /// <param name="LL">This GridSquare's LowerLeft corner</param>
        /// <param name="LR">This GridSquare's LowerRight corner</param>
        /// <param name="UL">This GridSquare's UpperLeft corner</param>
        /// <param name="UR">This GridSquare's UpperRight corner</param>
        public GridSquare(GridGenerator parent, int r, int c, float slope, int[] quad, Vector3 LL, Vector3 LR, Vector3 UL, Vector3 UR)
        {
            parentGenerator = parent;

            row = r;
            col = c;

            slopedAngle = slope; // in radians

            quadrant = quad;

            positions[0] = LL;
            positions[1] = LR;
            positions[2] = UL;
            positions[3] = UR;
        }

        /// <summary>
        /// Determines whether a particular row and column value contains a GridSquare.
        /// </summary>
        /// <param name="r">Desired Row</param>
        /// <param name="c">Desired Column</param>
        /// <returns>Either a GridSquare or null</returns>
        private GridSquare DeterminedValidAdjacent(int r, int c)
        {
            if ((r <= parentGenerator.Rows - 1 && c <= parentGenerator.Columns - 1) && (r >= 0 && c >= 0))
                return parentGenerator.getDesiredGridSquare(r, c);
            else
                return null;
        }

        /// <summary>
        /// Calculates the Adjacent Squares to this GridSquare.
        /// </summary>
        public void calculateAdjacentSquares()
        {
            int[] rows = { row - 1, row, row + 1 };
            int[] cols = { col - 1, col, col + 1 };

            for (int i = 0; i < rows.Length; i++)
            {
                for (int j = 0; j < cols.Length; j++)
                {
                    if (i != 1 | j != 1)
                    {
                        GridSquare adjacent = DeterminedValidAdjacent(rows[i], cols[j]);
                        if (adjacent != null)
                            adjacentSquares.Add(adjacent);
                    }
                }
            }
        }

        /// <summary>
        /// Allows retrieval and setting of the ParentSquare of this GridSquare.
        /// </summary>
        public GridSquare ParentObject
        {
            get { return parentSquare; }
            set { parentSquare = value; }
        }

        /// <summary>
        /// Allows retrieval and setting of whether this GridSquare lies on a OpenList.
        /// </summary>
        public bool isOnOpenList
        {
            get { return onOpenList; }
            set { onOpenList = value; }
        }

        /// <summary>
        /// Allows retrieval and setting of whether this GridSquare lies on a CloseList.
        /// </summary>
        public bool isOnCloseList
        {
            get { return onCloseList; }
            set { onCloseList = value; }
        }

        /// <summary>
        /// Retrieves the Quadrant this GridSquare lies in.
        /// </summary>
        public GridSquareQuadrant getQuadrant()
        {
            return parentGenerator.getDesiredQuadrant(quadrant[0], quadrant[1]);
        }

        /// <summary>
        /// Retrieves the Lower Left vector of this GridSquare.
        /// </summary>
        public Vector3 getGridSquareLowerLeft()
        {
            return parentGenerator.transform.position + positions[0];
        }

        /// <summary>
        /// Retrieves the Lower Right vector of this GridSquare.
        /// </summary>
        public Vector3 getGridSquareLowerRight()
        {
            return parentGenerator.transform.position + positions[1];
        }

        /// <summary>
        /// Retrieves the Upper Left vector of this GridSquare.
        /// </summary>
        public Vector3 getGridSquareUpperLeft()
        {
            return parentGenerator.transform.position + positions[2];
        }

        /// <summary>
        /// Retrieves the Upper Right vector of this GridSquare.
        /// </summary>
        public Vector3 getGridSquareUpperRight()
        {
            return parentGenerator.transform.position + positions[3];
        }

        /// <summary>
        /// Retrieves the Center vector of this GridSquare.
        /// </summary>
        public Vector3 getGridSquareCentralOrigin()
        {
            return parentGenerator.transform.position + (positions[0] + (positions[3] - positions[0]) / 2);
        }

        /// <summary>
        /// Retrieves this GridSquare's Sloped Value.
        /// </summary>
        public float getSlopeValue()
        {
            return slopedAngle;
        }

        /// <summary>
        /// Sets this GridSquare's F Score. (F = G + H).
        /// </summary>
        public void setFMovementCost(int val)
        {
            f_val = val;
        }

        /// <summary>
        /// Sets this GridSquare's G Score.
        /// </summary>
        public void setGMovementCost(int val)
        {
            g_val = val;
        }

        /// <summary>
        /// Sets this GridSquare's Heuristic value.
        /// </summary>
        public void setHMovementCost(int val)
        {
            h_val = val;
        }

        /// <summary>
        /// Retrieves this GridSquare's F Score (F = G + H).
        /// </summary>
        /// <returns>F Score Value</returns>
        public int getFMovementCost()
        {
            return f_val;
        }

        /// <summary>
        /// Retrieves this GridSquare's G Score (Squares between this square and the initial square).
        /// </summary>
        /// <returns>G Score Value</returns>
        public int getGMovementCost()
        {
            return g_val;
        }

        /// <summary>
        /// Retrieves this GridSquare's Heuristic value.
        /// </summary>
        /// <returns>Heuristic Value</returns>
        public int getHMovementCost()
        {
            return h_val;
        }

        /// <summary>
        /// Retrieves the Row Value of this GridSquare.
        /// </summary>
        /// <returns>Row Value</returns>
        public int getRow()
        {
            return row;
        }

        /// <summary>
        /// Retrieves the Column Value of this GridSquare.
        /// </summary>
        /// <returns>Column Value</returns>
        public int getColumn()
        {
            return col;
        }

        /// <summary>
        /// Retrieves this GridSquare's Adjacent GridSquares.
        /// </summary>
        /// <returns>List of Adjacent GridSquares</returns>
        public List<GridSquare> getAdjacentSquares()
        {
            return adjacentSquares;
        }
    }
}