/// \file
/// Used for Calculating A* PathFinding.
/// @author: Chase Hutchens

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// This Structure is Used for Storing Information about a Particular PathCalculator that is Used in the PathFinder Queue.
/// </summary>
/// <remarks>The Transform: trans Stores the Unity3D Transform.
/// The GridGenerator.GridSquare: i Stores the Initial GridSquare.
/// The GridGenerator.GridSquare: f Stores the Final GridSquare.</remarks>
struct PathStructQueue
{
    public Transform trans;
    public GridGenerator.GridSquare i;
    public GridGenerator.GridSquare f;
}

/// <summary>
/// This is an Implementation of the A* Pathfinding System, that takes in GridSquares from the GridGenerator Class. The Path is Generated within a Separate Thread for Increased Performance.
/// </summary>
public class PathFinder
{
    //List<GridGenerator.GridSquare> CalculatedTiles = new List<GridGenerator.GridSquare>();
    GridGenerator mainGrid;
    PathCalculator newPath;
    Thread pathCalcThread;

    Queue<PathStructQueue> calcQueue = new Queue<PathStructQueue>();

    /// <summary>
    /// Creates a new PathFinder object for a particular GridGenerator.
    /// </summary>
    /// <param name="grid">The GridGenerator pertaining to this PathFinder</param>
    public PathFinder(GridGenerator grid)
    {
        mainGrid = grid;
    }

    /// <summary>
    /// Initiates the generation of a NewPath given an Initial GridSquare and Final GridSquare.
    /// </summary>
    /// <param name="init">Initial GridSquare</param>
    /// <param name="final">Final GridSquare</param>
    public void CalculateNewPath(GridGenerator.GridSquare init, GridGenerator.GridSquare final)
    {
        InitiateThreadCalculation(new PathCalculator(init, final));
    }

    /// <summary>
    /// Begins Calculation of a New Path with a Unity3D Transform Movement Object, from it's Initial GridGenerator.GridSquare to a Final GridGenerator.GridSquare.
    /// </summary>
    /// <param name="moveobj">The Unity3D Transform Object to be Used and Updated. This Requires the Function ReceiveCalculatedPath as seen in ControlObjHander.ReceiveCalculatedPath.</param>
    /// <param name="init">The Initial GridGenerator.GridSquare.</param>
    /// <param name="final">The Final GridGenerator.GridSquare.</param>
    public void CalculateNewPath(Transform moveobj, GridGenerator.GridSquare init, GridGenerator.GridSquare final)
    {
        PathStructQueue newPathQueue = new PathStructQueue();
        newPathQueue.trans = moveobj;
        newPathQueue.i = init;
        newPathQueue.f = final;

        calcQueue.Enqueue(newPathQueue);

        if (calcQueue.Count == 1)
        {
            InitiateThreadCalculation(new PathCalculator(newPathQueue.trans, newPathQueue.i, newPathQueue.f));
        }
    }

    /// <summary>
    /// Initiates a New Thread Calculation for a PathCalculator.
    /// </summary>
    /// <param name="d_path">The New PathCalculator to be Calculated.</param>
    private void InitiateThreadCalculation(PathCalculator d_path)
    {
        newPath = d_path;
        pathCalcThread = new Thread(new ThreadStart(newPath.ThreadRun));

        try
        {
            pathCalcThread.Start();
        }
        catch (ThreadStateException e) { Debug.Log(e.Message); }
    }

    /// <summary>
    /// Resets the GridGenerator if the Path has been Generated and Calculated, and Starts Calculation of the Next PathCalculator in the PathFinder Queue if there is One.
    /// </summary>
    public void CheckPathCalculation()
    {
        if (newPath != null) // only execute if the newPath has been generated
        {
            if (newPath.isCalculated() && pathCalcThread != null) // one time function calls
            {                
                pathCalcThread.Join();
                pathCalcThread = null;

                //Debug.Log("Path Calculation Time: " + newPath.ElapsedTime + " ms.");
                mainGrid.ResetGrid();
                newPath.SendTransformCalculatedPath(); // send calculated path to the transform if there was one then set Calculated = false

                calcQueue.Dequeue();

                if (calcQueue.Count > 0)
                {
                    PathStructQueue newPathQueue = calcQueue.Peek();
                    InitiateThreadCalculation(new PathCalculator(newPathQueue.trans, newPathQueue.i, newPathQueue.f));
                }
            }
        }
    }

    /// <summary>
    /// Wrapper Around PathCalculator.DrawPath for use Elsewhere.
    /// </summary>
    public void DrawPath()
    {
        if (newPath != null && newPath.isActiveDraw())
            newPath.DrawPath(mainGrid, 10); // Draw path for 10 seconds then turns ActiveDraw = false
    }

    /// <summary>
    /// Retrieves the Calculated Path Tiles for Use Elsewhere.
    /// </summary>
    /// <returns>The Calculated Path Tiles</returns>
    public List<GridGenerator.GridSquare> getCalculatedTiles()
    {
        return newPath.getCalculatedPathTiles();
    }

    /// <summary>
    /// Used for Calculation of a Desired Path from an Initial GridGenerator.GridSquare to a Final GridGenerator.GridSquare.
    /// </summary>
    private class PathCalculator
    {
        Transform moveObject = null;
        GridGenerator.GridSquare init;
        GridGenerator.GridSquare final;

        bool ActiveDraw = false;
        float drawTime = 0;

        List<GridGenerator.GridSquare> pathTiles = new List<GridGenerator.GridSquare>();
        bool Calculated = false;
        float elapsedTime = 0;

        /// <summary>
        /// Create a new PathCalculator Object for Calculation between an Initial GridGenerator.GridSquare and Final GridGenerator.GridSquare.
        /// </summary>
        /// <param name="i">Initial GridSquare</param>
        /// <param name="f">Final GridSquare</param>
        public PathCalculator(GridGenerator.GridSquare i, GridGenerator.GridSquare f)
        {
            init = i;
            final = f;
        }

        /// <summary>
        /// Takes in a Desired Transform Movement Object for Calculation at it's Initial GridGenerator.GridSquare and Final GridGenerator.GridSquare.
        /// </summary>
        /// <param name="t">The Desired Moving Object.</param>
        /// <param name="i">The Initial GridGenerator.GridSquare</param>
        /// <param name="f">The Final GridGenerator.GridSquare</param>
        public PathCalculator(Transform t, GridGenerator.GridSquare i, GridGenerator.GridSquare f)
        {
            moveObject = t;
            init = i;
            final = f;
        }

        /// <summary>
        /// Initiate the CalculatePath function in it's own thread. This is Called from PathFinder.InitiateThreadCalculation.
        /// </summary>
        public void ThreadRun()
        {
            CalculatePath(init, final);
        }

        /// <summary>
        /// Has the Path been Calculated?
        /// </summary>
        /// <returns>Whether the Path has been Calculated</returns>
        public bool isCalculated()
        {
            return Calculated;
        }

        /// <summary>
        /// Is this PathCalculator Drawing the Calculated Path?
        /// </summary>
        /// <returns>Whether ActiveDraw is T or F</returns>
        public bool isActiveDraw()
        {
            return ActiveDraw;
        }

        /// <summary>
        /// (Read-only) Retrieves the Elapsed Calculation Time in Milliseconds.
        /// </summary>
        public float ElapsedTime
        {
            get { return elapsedTime; }
        }

        /// <summary>
        /// Retrieves the Calculated Path Tiles for Use Elsewhere.
        /// </summary>
        /// <returns>The Calculated Path Tiles</returns>
        public List<GridGenerator.GridSquare> getCalculatedPathTiles()
        {
            List<GridGenerator.GridSquare> moveTiles = pathTiles.GetRange(0, pathTiles.Count);

            return moveTiles;
        }

        /// <summary>
        /// Sends the moveObject Transform the Calculated PathTiles.
        /// </summary>
        public void SendTransformCalculatedPath()
        {
            if (moveObject != null)
                moveObject.SendMessage("ReceiveCalculatedPath", getCalculatedPathTiles());

            Calculated = false;
        }
        
        /// <summary>
        /// Renders the Calculated A* Path for a Given Amount of Time in Seconds with GL Functions. Used in PathFinder.CheckPathCalculation().
        /// </summary>
        /// <param name="gridGen">The GridGenerator</param>
        /// <param name="sTime">The Draw Time in Seconds.</param>
        public void DrawPath(GridGenerator gridGen, float sTime)
        {
            if (drawTime > sTime)
            {
                drawTime = 0;
                ActiveDraw = false;
            }

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

            drawTime += Time.deltaTime;
        }

        /// <summary>
        /// Calculates the best path between the starting grid square and ending grid square.
        /// </summary>
        /// <param name="start">Initial GridSquare</param>
        /// <param name="end">Ending GridSquare</param>
        public void CalculatePath(GridGenerator.GridSquare start, GridGenerator.GridSquare end)
        {            
            BinaryHeap<GridGenerator.GridSquare> openedTiles = new BinaryHeap<GridGenerator.GridSquare>(new GridGenerator.GridComparer());//(mainGrid.getRows() * mainGrid.getColumns());
            List<GridGenerator.GridSquare> closedTiles = new List<GridGenerator.GridSquare>();
            GridGenerator.GridSquare currentSquare = start;
            GridGenerator.GridSquare lowestDistEndSquare = null;

            float previousTime = System.DateTime.Now.Millisecond;
            Calculated = false;
            pathTiles.Clear();

            currentSquare.setGMovementCost(0);
            currentSquare.setHMovementCost((Mathf.Abs(end.getRow() - start.getRow()) + Mathf.Abs(end.getColumn() - start.getColumn())) * 10);
            currentSquare.setFMovementCost(currentSquare.getGMovementCost() + currentSquare.getHMovementCost());
            openedTiles.Insert(currentSquare); // add initial square to open list

            while (openedTiles.Count > 0)
            {
                currentSquare = openedTiles.RemoveRoot();
                currentSquare.isOnOpenList = false;

                if (currentSquare == end)
                    break;

                currentSquare.isOnCloseList = true;
                closedTiles.Add(currentSquare);

                List<GridGenerator.GridSquare> adjacentList = new List<GridGenerator.GridSquare>(currentSquare.getAdjacentSquares());

                for (int i = 0; i < adjacentList.Count; i++)
                {
                    if (!adjacentList[i].isOnCloseList)
                    {
                        int dist_between = 12;

                        if ((Mathf.Abs(currentSquare.getRow() - adjacentList[i].getRow()) + Mathf.Abs(currentSquare.getColumn() - adjacentList[i].getColumn())) == 2)
                            dist_between = 14;
                        
                        int heuristic = (Mathf.Abs(end.getRow() - adjacentList[i].getRow()) + Mathf.Abs(end.getColumn() - adjacentList[i].getColumn())) * 10;
                        int tentativeGValue = (currentSquare.getGMovementCost() + dist_between);
                        bool alreadyContains = adjacentList[i].isOnOpenList;

                        if ((!alreadyContains || tentativeGValue <= adjacentList[i].getGMovementCost()))
                        {
                            adjacentList[i].ParentObject = currentSquare; // set the adjacentSquare's parent to our currentSquare
                            adjacentList[i].setGMovementCost(tentativeGValue);
                            adjacentList[i].setHMovementCost(heuristic);
                            adjacentList[i].setFMovementCost(adjacentList[i].getGMovementCost() + adjacentList[i].getHMovementCost());

                            if (lowestDistEndSquare == null || lowestDistEndSquare.getHMovementCost() > currentSquare.getHMovementCost())
                                lowestDistEndSquare = currentSquare;

                            if (!alreadyContains)
                            {
                                adjacentList[i].isOnOpenList = true;
                                openedTiles.Insert(adjacentList[i]);                                
                            }
                        }
                    }
                }
            }

            if (currentSquare == end)
            {
                // Calculate the pathTiles
                pathTiles.Add(currentSquare);

                while (currentSquare != start)
                {
                    pathTiles.Add(currentSquare.ParentObject);
                    currentSquare = currentSquare.ParentObject;
                }
            }
            else
            {
                pathTiles.Add(lowestDistEndSquare);

                while (lowestDistEndSquare != start)
                {
                    pathTiles.Add(lowestDistEndSquare.ParentObject);
                    lowestDistEndSquare = lowestDistEndSquare.ParentObject;
                }
            }

            pathTiles.Reverse();
            elapsedTime = System.DateTime.Now.Millisecond - previousTime;            
            Calculated = true;
            ActiveDraw = true;
        }
    }
}

/// <summary>
/// BinaryHeap used from: http://www.informit.com/guides/content.aspx?g=dotnet&seqNum=789
/// </summary>
/// <remarks>
/// Selects and Bubbles Smallest Object on Top of Heap.
/// </remarks>
/// <typeparam name="T">Any Object that can be Sorted.</typeparam>
public class BinaryHeap<T> : IEnumerable<T>
{
    private IComparer<T> Comparer;
    private List<T> Items = new List<T>(); // This would likely be faster with an Array

    /// <summary>
    /// Creates a BinaryHeap with a Default IComparer.
    /// </summary>
    public BinaryHeap()
        : this(Comparer<T>.Default)
    {
    }

    /// <summary>
    /// Creates a BinaryHeap with a Custom IComparer. 
    /// </summary>
    /// <param name="comp"></param>
    public BinaryHeap(IComparer<T> comp)
    {
        Comparer = comp;
    }

    /// <summary>
    /// Get a Count of the Number of Items in the Collection.
    /// </summary>
    public int Count
    {
        get { return Items.Count; }
    }

    /// <summary>
    /// Removes All Items from the Collection.
    /// </summary>
    public void Clear()
    {
        Items.Clear();
    }

    /// <summary>
    /// Sets the Capacity to the Actual Number of Elements in the BinaryHeap,
    /// if that Number is Less Than a Threshold Value.
    /// </summary>
    /// <remarks>
    /// The current threshold value is 90% (.NET 3.5), but might change in a future release.
    /// </remarks>
    public void TrimExcess()
    {
        Items.TrimExcess();
    }

    /// <summary>
    /// Inserts an Item onto the Heap.
    /// </summary>
    /// <param name="newItem">The Item to be Inserted.</param>
    public void Insert(T newItem)
    {
        int i = Count;
        Items.Add(newItem);

        while (i > 0 && Comparer.Compare(Items[(i - 1) / 2], newItem) > 0)
        {
            Items[i] = Items[(i - 1) / 2];
            i = (i - 1) / 2;
        }

        Items[i] = newItem;
    }

    /// <summary>
    /// Return the Root Item from the Collection, without Removing it.
    /// </summary>
    /// <returns>Returns the Item at the Root of the Heap.</returns>
    public bool Contains(T item)
    {
        T[] listArray = Items.ToArray();
        for (int i = 0; i < listArray.Length; i++)
        {
            if (listArray[i].Equals(item))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the Item at the Top of the BinaryHeap.
    /// </summary>
    public T Peek()
    {
        if (Items.Count == 0)
        {
            throw new InvalidOperationException("The heap is empty.");
        }

        return Items[0];
    }

    /// <summary>
    /// Removes and Returns the Root Item from the Collection.
    /// </summary>
    /// <returns>Returns the Item at the Root of the Heap.</returns>
    public T RemoveRoot()
    {
        if (Items.Count == 0)
        {
            throw new InvalidOperationException("The heap is empty.");
        }

        // Get the first item
        T rslt = Items[0];
        // Get the last item and bubble it down.
        T tmp = Items[Items.Count - 1];
        Items.RemoveAt(Items.Count - 1);

        if (Items.Count > 0)
        {
            int i = 0;
            while (i < Items.Count / 2)
            {
                int j = (2 * i) + 1;

                if ((j < Items.Count - 1) && (Comparer.Compare(Items[j], Items[j + 1]) > 0))
                {
                    ++j;
                }

                if (Comparer.Compare(Items[j], tmp) >= 0)
                {
                    break;
                }

                Items[i] = Items[j];
                i = j;
            }

            Items[i] = tmp;
        }

        return rslt;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        foreach (var i in Items)
        {
            yield return i;
        }
    }

    public IEnumerator GetEnumerator()
    {
        return GetEnumerator();
    }
}