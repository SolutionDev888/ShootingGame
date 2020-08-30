/// \file
/// Handles Input Relating Specifically Towards Concepts Around Selecting, Deselecting, etc, of Control, NPC, and Creature Objects.
/// @author: Chase Hutchens

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class handles inputs for selecting the objects through the game world
/// </summary>
public class SelectionBehavior : MonoBehaviour
{
    private CamMovementBehavior camMovementBehavior;
    private MultiSelectToggle multiselect_toggle;
    private SelectionToggle selection_toggle;

    private RaycastHit TargetCastInfo;
    private Transform selectedTarget = null;
    private Transform activeTarget = null;

    private Vector3 moveto_pos = Vector3.zero;
    private Vector3 previous_moveto_pos = Vector3.zero;

    private Texture2D[] mouseCursor = new Texture2D[3];
    private Texture2D[] mouseDirection = new Texture2D[8];
    private Texture2D activeCursorGfx = null;
    private Vector2 CursorPosMod = Vector2.zero;

    // Set these three strings to the name that your Object's prefab contains
    private string ControlAssetName = "ControlObj";
    private string NPCAssetName = "NPCObj";
    private string CreatureAssetName = "CreatureObj";

    private bool similar_selected = false;

    /// <summary>
    /// The Main GridGenerator to use for Calculating Paths with the PathFinder.
    /// </summary>
    public GridGenerator MainGridGenerator;

    /// <summary>
    /// The Key that will Add Additional Control Objects when RealTimeStratToggle is Toggled.
    /// </summary>
    public KeyCode AddAdditionalKey = KeyCode.LeftAlt;

    /// <summary>
    /// The Mouse Cursor Texture to use. 3 frames each 32x32 -> total: 96x32
    /// </summary>
    public Texture2D MouseCursor = null;

    /// <summary>
    /// The 8 framed 32x32 Texture Sheet for the Mouse Scroll Movement Directions
    /// </summary>
    public Texture2D MouseDirections = null;

    /// <summary>
    /// The Material that is used for Coloring the Multi Selection Mouse Drag Box.
    /// </summary>
    public Material MultiSelectMaterial = null;

    void Start()
    {
        if (MouseCursor != null)
        {
            Cursor.visible = false;

            for (int i = 0; i < mouseCursor.Length; i++)
            {
                Color[] sPixels = MouseCursor.GetPixels(32 * i, 0, 32, 32);
                mouseCursor[i] = new Texture2D(32, 32, TextureFormat.ARGB32, true);
                mouseCursor[i].SetPixels(0, 0, 32, 32, sPixels);
                mouseCursor[i].Apply();
            }

            activeCursorGfx = mouseCursor[0];
        }

        if (MouseDirections != null)
        {
            for (int i = 0; i < mouseDirection.Length; i++)
            {
                Color[] sPixels = MouseDirections.GetPixels(32 * i, 0, 32, 32);
                mouseDirection[i] = new Texture2D(32, 32, TextureFormat.ARGB32, true);
                mouseDirection[i].SetPixels(0, 0, 32, 32, sPixels);
                mouseDirection[i].Apply();
            }
        }

        camMovementBehavior = transform.GetComponent<CamMovementBehavior>();
        multiselect_toggle = (MultiSelectToggle)gameObject.AddComponent<MultiSelectToggle>();
        selection_toggle = (SelectionToggle)gameObject.AddComponent<SelectionToggle>();

        multiselect_toggle.SetInitialOptions(MultiSelectMaterial, camMovementBehavior.ControllerToggleType);
    }

    void OnGUI()
    {
        if (activeCursorGfx != null)
            GUI.DrawTexture(new Rect(Event.current.mousePosition.x + (activeCursorGfx == mouseCursor[0] ? 0 : CursorPosMod.x), Event.current.mousePosition.y + (activeCursorGfx == mouseCursor[0] ? 0 : CursorPosMod.y), 32, 32), activeCursorGfx);
        
        if (TargetCastInfo.transform != null)
        {
            GUI.Label(new Rect(0, 0, Screen.width / 2, 20), "Target: " + TargetCastInfo.transform.name);
        }

        if (selectedTarget != null)
        {
            GUI.color = Color.white;

            GUI.Label(new Rect(0, 20, Screen.width / 2, 20), "Selected: " + selectedTarget.transform.name);
            GUI.Label(new Rect(0, 40, Screen.width / 2, 20), "Count: " + multiselect_toggle.getCurrentlySelected().Count);
        }

        if (activeTarget != null)
            GUI.Label(new Rect(0, 60, Screen.width / 2, 20), "Active: " + activeTarget.transform.name);
    }

    void Update()
    {
        if (!camMovementBehavior.isMouseMovement() && activeCursorGfx != mouseCursor[0])
        {
            if (mouseCursor[0] != null)
                activeCursorGfx = mouseCursor[0];
            else
                activeCursorGfx = null;
        }

        Ray ray = GetComponent<Camera>().ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        if (Physics.Raycast(ray, out TargetCastInfo))
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (TargetCastInfo.transform != null)
                {
                    if (TargetCastInfo.transform.name.Contains(ControlAssetName))
                    {
                        if (camMovementBehavior.ControllerToggleType.Equals(ControllerType.RealTimeStratToggle) && !Input.GetKey(AddAdditionalKey))
                            multiselect_toggle.ClearSelectedTargets();

                        ChangeSelectedTarget(TargetCastInfo.transform);
                    }

                    if (TargetCastInfo.transform.name.Contains(CreatureAssetName) || TargetCastInfo.transform.name.Contains(NPCAssetName))
                    {
                        // de-init last active_target's data
                        if (activeTarget != null)
                        {
                            if (activeTarget.name.Contains(NPCAssetName))
                            {
                                NPCHandler npc_handler = activeTarget.GetComponent<NPCHandler>();
                                npc_handler.setActiveTarget(false);
                            }
                            else if (activeTarget.name.Contains(CreatureAssetName))
                            {
                                CreatureHandler creature_handler = activeTarget.GetComponent<CreatureHandler>();
                                creature_handler.setActiveTarget(false);
                            }

                            activeTarget.Find("SelectedIndicator").gameObject.SetActive(false);
                        }

                        activeTarget = TargetCastInfo.transform;

                        activeTarget.Find("SelectedIndicator").gameObject.SetActive(true);

                        if (activeTarget.name.Contains(NPCAssetName))
                        {
                            NPCHandler npc_handler = activeTarget.GetComponent<NPCHandler>();
                            npc_handler.setActiveTarget(true);
                        }
                        else
                        {
                            CreatureHandler creature_handler = activeTarget.GetComponent<CreatureHandler>();
                            creature_handler.setActiveTarget(true);
                        }
                    }
                }
            }
        }

        // a target is selected
        if (selectedTarget != null)
        {
            if (TargetCastInfo.transform != null)
            {
                // left mouse button pressed
                if ((multiselect_toggle.isMovementAllowed() && (Input.GetMouseButtonUp(0) && camMovementBehavior.ControllerToggleType.Equals(ControllerType.RealTimeStratToggle)) || (Input.GetMouseButton(0) && camMovementBehavior.ControllerToggleType.Equals(ControllerType.DungeonCrawlerToggle))))
                {
                    if (selectedTarget.name.Contains(ControlAssetName) && !TargetCastInfo.transform.name.Contains(ControlAssetName))
                    {
                        moveto_pos = TargetCastInfo.point;
                        moveto_pos.y += selectedTarget.transform.localScale.y;

                        selection_toggle.createSelectionToggle(moveto_pos, Quaternion.FromToRotation(Vector3.up, TargetCastInfo.normal));

                        if (camMovementBehavior.ControllerToggleType.Equals(ControllerType.RealTimeStratToggle))
                        {
                            int counter = 0;
                            foreach (Transform trans in multiselect_toggle.getCurrentlySelected())
                            {
                                // Movement Formations created in this location
                                if (moveto_pos != previous_moveto_pos)
                                    MainGridGenerator.CalculateNewPath(trans, moveto_pos);

                                UpdatePositionToggle.AddUpdaterToggle(new UpdatePositionToggle(trans, counter++));
                            }
                        }
                        else
                        {
                            if (moveto_pos != previous_moveto_pos)
                                MainGridGenerator.CalculateNewPath(selectedTarget, moveto_pos);

                            UpdatePositionToggle.AddUpdaterToggle(new UpdatePositionToggle(selectedTarget, 1));
                        }

                        previous_moveto_pos = moveto_pos;
                    }
                }
            }
        }

        UpdatePositionToggle.UpdateTogglePositions();
    }

    /// <summary>
    /// Changes the Selected Target to a Different Unity3D Transform Object, Deactivating Old and Activating New.
    /// </summary>
    /// <remarks>The selected_target Needs to Contain a ControlObjHandler.</remarks>
    /// <param name="trans">The Unity3D Transform Containing a ControlObjHandler.</param>
    private void ChangeSelectedTarget(Transform trans)
    {
        if (!camMovementBehavior.isFollowToggle() && selectedTarget != null)
        {
            ControlObjHandler old_control_handler = selectedTarget.GetComponent<ControlObjHandler>();

            if (selectedTarget.name.Contains(ControlAssetName))
                similar_selected = true;

            if (multiselect_toggle.getCurrentlySelected().Count == 0)
                selectedTarget.Find("SelectedIndicator").gameObject.SetActive(false);

            old_control_handler.setActiveTarget(false);
        }
        else if (camMovementBehavior.isFollowToggle() && selectedTarget != null)
        {
            ControlObjHandler old_control_handler = selectedTarget.GetComponent<ControlObjHandler>();

            if (multiselect_toggle.getCurrentlySelected().Count == 0)
                selectedTarget.Find("SelectedIndicator").gameObject.SetActive(false);

            old_control_handler.setActiveTarget(false);
        }

        // now selected_target != null
        selectedTarget = trans;

        if (camMovementBehavior.ControllerToggleType.Equals(ControllerType.RealTimeStratToggle) && !multiselect_toggle.getCurrentlySelected().Contains(selectedTarget))
            multiselect_toggle.getCurrentlySelected().Add(selectedTarget);

        selectedTarget.Find("SelectedIndicator").gameObject.SetActive(true);

        ControlObjHandler controler_handler = selectedTarget.GetComponent<ControlObjHandler>();
        controler_handler.setActiveTarget(true);
    }

    /// <summary>
    /// Activated when Escape is pressed within CamMovementBehavior.cs
    /// </summary>
    private void ExecuteEscapeSequence()
    {
        // only clear when no active_target is selected onEscape
        if (camMovementBehavior.ControllerToggleType.Equals(ControllerType.RealTimeStratToggle) && (activeTarget == null && multiselect_toggle.getCurrentlySelected().Count > 0))
            multiselect_toggle.ClearSelectedTargets();

        if (activeTarget != null)
        {
            if (activeTarget.name.Contains(NPCAssetName))
                activeTarget.GetComponent<NPCHandler>().setActiveTarget(false);
            else
                activeTarget.GetComponent<CreatureHandler>().setActiveTarget(false);

            activeTarget.Find("SelectedIndicator").gameObject.SetActive(false);
            activeTarget = null;
        }
        else if (selectedTarget != null)
        {
            selectedTarget.Find("SelectedIndicator").gameObject.SetActive(false);
            selectedTarget.GetComponent<ControlObjHandler>().setActiveTarget(false);
            selectedTarget = null;

            camMovementBehavior.SendMessage("ExecuteEscapeSequence");
        }
    }

    /// <summary>
    /// Sets the Scroll Cursor to a particular frame, with a position modifier for placing the cursor in the right location.
    /// </summary>
    /// <remarks>
    /// Called from CamMovementBehavior.cs
    /// </remarks>
    /// <param name="c_index">The Index of the valid Mouse Scroll Direction texture array</param>
    /// <param name="dx">The x position modifier</param>
    /// <param name="dy">The y position modifier</param>
    public void setActiveCursorScrollValues(int c_index, float dx, float dy)
    {
        activeCursorGfx = mouseDirection[c_index];
        CursorPosMod = new Vector2(dx, dy);
    }

    /// <summary>
    /// Retrieves the selected Control Object
    /// </summary>
    /// <returns>Transform of selected Control Object</returns>
    public Transform getSelectedTarget()
    {
        return selectedTarget;
    }

    /// <summary>
    /// Retrieves the selected NPC Object or Creature Object
    /// </summary>
    /// <returns>Transform of selected NPC or Creature Object</returns>
    public Transform getActiveTarget()
    {
        return activeTarget;
    }

    /// <summary>
    /// Allows for setting and retrieval of whether a similar Control Object has been selected
    /// </summary>
    public bool SimilarSelected
    {
        get
        {
            return similar_selected;
        }
        set
        {
            similar_selected = value;
        }
    }

    /// <summary>
    /// This Class is used for Continually Calculating and Updating the Position of a Unity3D Transform Object. Specifically ControlObjHandler Objects (At this time).
    /// </summary>
    private class UpdatePositionToggle
    {
        private static List<UpdatePositionToggle> updater_list = new List<UpdatePositionToggle>();
        private Transform update_target;
        //private Vector3 update_position = Vector3.zero;
        private bool toggled_movement = false;
        //private bool angle_adjusted = false;
        private int toggle_count = 0;

        /// <summary>
        /// Creates a New UpdatePositionToggle with a Unity3D Transform Object and the toggle_count.
        /// </summary>
        /// <param name="ref_obj">The Unity3D Transform Object.</param>
        /// <param name="c">The Count of this UpdatePositionToggle (This will be used for Formations).</param>
        public UpdatePositionToggle(Transform ref_obj, int c)
        {
            update_target = ref_obj;
            //update_position = new_pos;
            toggle_count = c;

            toggled_movement = true;
            //angle_adjusted = true;
        }

        /// <summary>
        /// Adds a New UpdatePositionToggle if one isn't Attached to the update_target. Else, Add a New one to the updater_list.
        /// </summary>
        /// <param name="new_toggle">The New UpdatePositionToggle to Change or Add.</param>
        public static void AddUpdaterToggle(UpdatePositionToggle new_toggle)
        {
            new_toggle.MovementToggle = true;

            if (updater_list.Count > 0)
            {
                for (int i = 0; i < updater_list.Count; i++)
                {
                    UpdatePositionToggle selected_toggle = (UpdatePositionToggle)updater_list[i];

                    if (selected_toggle.getName().Equals(new_toggle.getName()))
                    {
                        updater_list[i] = new_toggle;

                        break;
                    }

                    if (i == updater_list.Count - 1)
                        updater_list.Add(new_toggle);
                }
            }
            else
                updater_list.Add(new_toggle);
        }

        /// <summary>
        /// Updates the updater_list of UpdatePositionToggles. Moving it's Position and Changing it's Rotation of the update_target Only if it is Moveable.
        /// </summary>
        public static void UpdateTogglePositions()
        {
            // this is for updating the objects chosen position and rotation
            if (updater_list.Count > 0)
            {
                for (int i = 0; i < updater_list.Count; i++)
                {
                    UpdatePositionToggle selected = (UpdatePositionToggle)updater_list[i];

                    if (selected.isMoveable())
                    {
                        selected.Position = selected.Position + (selected.destinationPosition() - selected.Position).normalized * (30 * Time.deltaTime);

                        //if (selected.angle_adjusted)
                        //{
                        float new_angle = 0.0f;

                        if (selected.destinationPosition().z <= selected.Position.z)
                        {
                            new_angle = (selected.destinationPosition().z - selected.Position.z);

                            if (new_angle != 0)
                                new_angle = Mathf.Rad2Deg * (Mathf.PI + Mathf.Atan((selected.destinationPosition().x - selected.Position.x) / new_angle));
                            else
                                new_angle = selected.Rotation.y;
                        }
                        else
                        {
                            new_angle = (selected.Position.z - selected.destinationPosition().z);

                            if (new_angle != 0)
                                new_angle = Mathf.Rad2Deg * Mathf.Atan((selected.Position.x - selected.destinationPosition().x) / new_angle);
                            else
                                new_angle = selected.Rotation.y;
                        }

                        //Debug.Log("Angle: " + new_angle);
                        selected.Rotation = new Vector3(selected.Rotation.x, new_angle, selected.Rotation.z);

                        //selected.angle_adjusted = false;
                        //}

                        if (!selected.MovementToggle)
                            updater_list.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Returns whether the update_target ControlObjHandler.isActiveCollision().
        /// </summary>
        /// <returns>ControlObjHandler is Colliding with something.</returns>
        private bool ActiveCollision()
        {
            return update_target.GetComponent<ControlObjHandler>().isActiveCollision();
        }

        /// <summary>
        /// Returns or Sets whether this is an Active MovementToggle.
        /// </summary>
        public bool MovementToggle
        {
            get { return toggled_movement; }

            set { toggled_movement = value; }
        }

        /// <summary>
        /// Returns whether the update_target is able to Move.
        /// </summary>
        /// <remarks>This is True when the update_target has Calculated Path Tiles.</remarks>
        public bool isMoveable()
        {
            return update_target.GetComponent<ControlObjHandler>().Moveable;
        }

        /// <summary>
        /// The Current Position of the update_target.
        /// </summary>
        public Vector3 Position
        {
            get { return update_target.position; }
            set { update_target.position = value; }
        }

        /// <summary>
        /// The Current Rotation of the update_target.
        /// </summary>
        public Vector3 Rotation
        {
            get { return update_target.localEulerAngles; }

            set { update_target.localEulerAngles = value; }
        }

        /// <summary>
        /// Returns the Destination Position of the update_target based on the Calculated Path Tiles.
        /// </summary>
        /// <returns>The Vector3 Relating to the Position.</returns>
        public Vector3 destinationPosition()
        {
            if (update_target.GetComponent<ControlObjHandler>().isValidMovePathTileCount())
            {
                Vector3 movetoPos = update_target.GetComponent<ControlObjHandler>().getMovePathTile().getGridSquareCentralOrigin();
                movetoPos.y += update_target.localScale.y;

                return movetoPos;
            }
            else // gets here once the final point has been reached
            {
                MovementToggle = false;
                return update_target.position;
            }
        }

        /// <summary>
        /// The Name of the update_target.
        /// </summary>
        public string getName()
        {
            return update_target.name;
        }
    }
}
