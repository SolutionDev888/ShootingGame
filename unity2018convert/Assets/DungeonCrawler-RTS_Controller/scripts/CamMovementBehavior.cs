/// \file
/// Handles Input Relating Specifically Towards Concepts Around the Main Camera.
/// @author: Chase Hutchens

using UnityEngine;
using System.Collections;

/// <summary>
/// Enumerator for whether this is a DungeonCrawlerToggle or a RealTimeStratToggle
/// </summary>
public enum ControllerType
{
    DungeonCrawlerToggle = 0x0,
    RealTimeStratToggle = 0x1
}

/// <summary>
/// This Class is for Handling Movement and Key Inputs for the Camera's Behavior
/// </summary>
public class CamMovementBehavior : MonoBehaviour
{
    private SelectionBehavior selectionBehavior;

    private bool follow_toggle = false;
    private bool KeyMovementOverride = false;
    private bool MouseMovement = false;
    private bool BetweenAngleCalculated = false;    
    private bool TargetPositionCalculated = false;
    private bool ActiveScrolling = false;

    private RaycastHit TargetCastInfo;
    private Vector3 TargetPosition = Vector3.zero;
    private Quaternion look_rotation = Quaternion.identity;

    private float zoom_distance = 320f;
    private float angle_between = 3 * Mathf.PI / 2;
    private float cam_range_time = 0;
    private float followcam_delay_counter = 0;
    private float TargetDistance = 0;

    /// <summary>
    /// The Key that will Initiate Camera Following when Selecting a ControlObjHandler.
    /// </summary>
    public KeyCode FollowToggleKey = KeyCode.Space;

    /// <summary>
    /// The LayerMask to Exclude when Casting Rays.
    /// </summary>
    public LayerMask IgnoreMask;

    /// <summary>
    /// Toggles between a Steeper Angle - Either 35.264 Degrees or 45 Degrees
    /// </summary>
    public bool SteeperAngle = true;

    /// <summary>
    /// Is Mouse Scrolling Enabled?
    /// </summary>
    /// <remarks>RealTimeStratToggle must also be enabled</remarks>
    public bool EnableMouseScroll = false;

    /// <summary>
    /// Is this a Real Time Strategy System or a Dungeon Crawler System?
    /// </summary>
    /// <remarks>The Difference Between this and the Dungeon Crawler System is the Movement Behavior
    /// and the Amount of Control Objects that can be Controlled at One Time. More will change with time.</remarks>
    public ControllerType ControllerToggleType = ControllerType.RealTimeStratToggle;

    /// <summary>
    /// The Amount of Pixels before being able to Move with the Mouse.
    /// </summary>
    public float ScreenMovementBuffer = 5;

    /// <summary>
    /// The Minimum Zoomed In Distance.
    /// </summary>
    public float MinZoomDistance = 20;

    /// <summary>
    /// The Maximum Zoomed Out Distance.
    /// </summary>
    public float MaxZoomDistance = 400;

    /// <summary>
    /// The Speed at which the Camera Moves when using the W, A, S, and D Keys.
    /// </summary>
    public float MoveToSpeed = 3;

    /// <summary>
    /// The Zooming In and Out Speed.
    /// </summary>
    public float ZoomSpeed = 3;

    /// <summary>
    /// The Camera Rotation Speed when using the Q and E Keys.
    /// </summary>
    public float RotateSpeed = 3;

    void Start()
    {
        selectionBehavior = transform.GetComponent<SelectionBehavior>();

        ActiveScrolling = ControllerToggleType.Equals(ControllerType.RealTimeStratToggle) && EnableMouseScroll;
    }

	void Update()
    {
        Transform selectedTarget = selectionBehavior.getSelectedTarget();
        Transform activeTarget = selectionBehavior.getActiveTarget();

        DetermineKeyPressEvent(selectedTarget, activeTarget);

        if (selectedTarget == null)
            DoDefaultCameraBehavior();
        else
        {
            if (follow_toggle)
            {
                if (!selectionBehavior.SimilarSelected)
                {
                    this.transform.position = CalculateNewMoveToPosition(transform.position, selectedTarget.position, (MaxZoomDistance - zoom_distance));

                    // look towards the selected target
                    look_rotation = Quaternion.LookRotation(selectedTarget.position - transform.position);

                    if (!SteeperAngle)
                        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(35.264f, look_rotation.eulerAngles.y, 0), RotateSpeed);
                    else
                        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(45, look_rotation.eulerAngles.y, 0), RotateSpeed);
                }
                else
                    followcam_delay_counter += Time.deltaTime;

                if (followcam_delay_counter > Time.deltaTime * 10)
                {
                    selectionBehavior.SimilarSelected = false;
                    followcam_delay_counter = 0.0f;
                }
            }
            else // this happens only when a ControlObject is selected but not actively being followed
            {
                DoDefaultCameraBehavior();
            }
        }
	}

    /// <summary>
    /// Executes proper Key and Mouse Events based off the selected Control Object and NPC or Creature Object
    /// </summary>
    private void DetermineKeyPressEvent(Transform selected, Transform active)
    {
        // forward scroll
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (follow_toggle && selected != null)// selected_target != null)
            {
                if ((selected.position - GetComponent<Camera>().transform.position).magnitude > MinZoomDistance)
                {
                    zoom_distance += (ZoomSpeed * Time.deltaTime * 60);
                    TargetPositionCalculated = false;
                }
            }
            else if (!follow_toggle)
            {
                if ((TargetPosition - GetComponent<Camera>().transform.position).magnitude > MinZoomDistance)
                {
                    //zoom_distance += (ZoomSpeed * Time.deltaTime * 60);
                    GetComponent<Camera>().transform.position += GetComponent<Camera>().transform.forward * (ZoomSpeed * Time.deltaTime * 60);
                    TargetPositionCalculated = false;
                }
            }
        }
        //  backward scroll
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            // target selected
            if (follow_toggle && selected != null)
            {
                if ((selected.position - GetComponent<Camera>().transform.position).magnitude < MaxZoomDistance)
                {
                    zoom_distance -= (ZoomSpeed * Time.deltaTime * 60);
                    TargetPositionCalculated = false;
                }
            }
            else if (!follow_toggle)
            {
                if ((TargetPosition - GetComponent<Camera>().transform.position).magnitude < MaxZoomDistance)
                {
                    //zoom_distance -= (ZoomSpeed * Time.deltaTime * 60);
                    GetComponent<Camera>().transform.position += GetComponent<Camera>().transform.forward * -(ZoomSpeed * Time.deltaTime * 60);
                    TargetPositionCalculated = false;
                }
            }
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
            KeyMovementOverride = true;
        else if (KeyMovementOverride)
            KeyMovementOverride = false;

        if (ActiveScrolling && (!KeyMovementOverride && (Input.mousePosition.x < ScreenMovementBuffer || Input.mousePosition.x > Screen.width - ScreenMovementBuffer ||
                                    Input.mousePosition.y > Screen.height - ScreenMovementBuffer || Input.mousePosition.y < ScreenMovementBuffer)))
            MouseMovement = true;
        else if (MouseMovement)
            MouseMovement = false;

        if (Input.GetKey(KeyCode.A) || (ActiveScrolling && (!KeyMovementOverride && Input.mousePosition.x < ScreenMovementBuffer)))
        {
            this.transform.position += transform.right * -(MoveToSpeed * Time.deltaTime * 60);
            follow_toggle = false;
            TargetPositionCalculated = false;

            if (!KeyMovementOverride && selectionBehavior.MouseDirections != null && Input.mousePosition.x < ScreenMovementBuffer)
                selectionBehavior.setActiveCursorScrollValues(3, -8, -16);
        }
        else if (Input.GetKey(KeyCode.D) || (ActiveScrolling && (!KeyMovementOverride && Input.mousePosition.x > Screen.width - ScreenMovementBuffer)))
        {
            this.transform.position += transform.right * (MoveToSpeed * Time.deltaTime * 60);
            follow_toggle = false;
            TargetPositionCalculated = false;

            if (!KeyMovementOverride && selectionBehavior.MouseDirections != null && Input.mousePosition.x > Screen.width - ScreenMovementBuffer)
                selectionBehavior.setActiveCursorScrollValues(1, -24, -16);
        }

        if (Input.GetKey(KeyCode.W) || (ActiveScrolling && (!KeyMovementOverride && Input.mousePosition.y > Screen.height - ScreenMovementBuffer)))
        {
            this.transform.position += (Vector3.Cross(Vector3.up, -transform.right)) * (MoveToSpeed * Time.deltaTime * 60);
            follow_toggle = false;
            TargetPositionCalculated = false;

            if (!KeyMovementOverride && selectionBehavior.MouseDirections != null && Input.mousePosition.y > Screen.height - ScreenMovementBuffer)
            {
                if (Input.mousePosition.x < ScreenMovementBuffer)
                    selectionBehavior.setActiveCursorScrollValues(6, -8, -8);
                else if (Input.mousePosition.x > Screen.width - ScreenMovementBuffer)
                    selectionBehavior.setActiveCursorScrollValues(7, -24, -8);
                else
                    selectionBehavior.setActiveCursorScrollValues(0, -16, -8);
            }
        }
        else if (Input.GetKey(KeyCode.S) || (ActiveScrolling && (!KeyMovementOverride && Input.mousePosition.y < ScreenMovementBuffer)))
        {
            //this.transform.position += Vector3.forward * -( Time.deltaTime);
            this.transform.position += (Vector3.Cross(Vector3.up, -transform.right)) * -(MoveToSpeed * Time.deltaTime * 60);
            follow_toggle = false;
            TargetPositionCalculated = false;

            if (!KeyMovementOverride && selectionBehavior.MouseDirections != null && Input.mousePosition.y < ScreenMovementBuffer)
            {
                if (Input.mousePosition.x < ScreenMovementBuffer)
                    selectionBehavior.setActiveCursorScrollValues(4, -8, -24);
                else if (Input.mousePosition.x > Screen.width - ScreenMovementBuffer)
                    selectionBehavior.setActiveCursorScrollValues(5, -24, -24);
                else
                    selectionBehavior.setActiveCursorScrollValues(2, -16, -24);
            }
        }

        if (Input.GetKey(KeyCode.Q))
        {
            angle_between -= RotateSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            angle_between += RotateSpeed * Time.deltaTime;
        }

        if (selected != null && (Input.GetKeyDown(FollowToggleKey) && !follow_toggle))
        {
            follow_toggle = true;
            BetweenAngleCalculated = false;
            cam_range_time = 0;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            selectionBehavior.SendMessage("ExecuteEscapeSequence");
    }

    /// <summary>
    /// The Default Camera Behavior is when the Camera is not Following a selected_target.
    /// </summary>
    private void DoDefaultCameraBehavior()
    {
        if (!TargetPositionCalculated)
        {
            Ray cam_ray = new Ray(GetComponent<Camera>().transform.position, GetComponent<Camera>().transform.forward);

            if (Physics.Raycast(cam_ray, out TargetCastInfo, 5 * MaxZoomDistance, ~IgnoreMask.value))
            {
                TargetPosition = TargetCastInfo.point;
                TargetDistance = TargetCastInfo.distance;
            }

            TargetPositionCalculated = true;
        }

        //Debug.DrawLine(camera.transform.position, TargetPosition, Color.red, 10);
        //this.transform.position = CalculateNewMoveToPosition(transform.position, TargetPosition, (MaxZoomDistance - zoom_distance));
        this.transform.position = TargetPosition + (TargetDistance * (CalculateNewMoveToPosition(transform.position, TargetPosition, TargetDistance) - TargetPosition).normalized);

        look_rotation = Quaternion.LookRotation(TargetPosition - transform.position);

        if (!SteeperAngle)
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(35.264f, look_rotation.eulerAngles.y, 0), RotateSpeed);
        else
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.Euler(45, look_rotation.eulerAngles.y, 0), RotateSpeed);
    }

    /// <summary>
    /// Calculates a New Move To Position Around a Circle from the Selected Position to a MaxDistance Away. With a 45 Degree Angle a long the y-axis.
    /// </summary>
    /// <param name="Cam">The Camera World Position.</param>
    /// <param name="Selected">The Selected World Position.</param>
    /// <param name="MaxDistance">The Distance Away.</param>
    /// <returns>The calculated Camera Position</returns>
    private Vector3 CalculateNewMoveToPosition(Vector3 Cam, Vector3 Selected, float MaxDistance)
    {
        Vector3 calculated = Vector3.zero;

        if (cam_range_time < .06 && !BetweenAngleCalculated)
            CalculateRadiansAngle(Cam, Selected);

        calculated.x = Selected.x + MaxDistance * Mathf.Cos(angle_between);
        calculated.z = Selected.z + MaxDistance * Mathf.Sin(angle_between);

        if (!SteeperAngle)
            calculated.y = Selected.y + (MaxDistance * Mathf.Sin(Mathf.PI / 4));
        else
            calculated.y = Selected.y + MaxDistance;

        if ((cam_range_time < .06) && Vector3.Distance(Cam, Selected) < calculated.magnitude)
            cam_range_time += Time.deltaTime;

        return calculated;
    }

    /// <summary>
    /// Calculates the Radians Between Two Vector3s. Where Zero is Facing a long Vector3.Right.
    /// </summary>
    /// <param name="Cam">The Camera World Position.</param>
    /// <param name="Selected">The Selected World Position.</param>
    private void CalculateRadiansAngle(Vector3 Cam, Vector3 Selected)
    {
        float angle_mod = 0;
        float dx = 0;
        float dz = 0;

        if (Cam.x >= Selected.x)
            dx = Cam.x - Selected.x;
        else
            dx = Selected.x - Cam.x;

        if (Cam.z >= Selected.z)
            dz = Cam.z - Selected.z;
        else
            dz = Selected.z - Cam.z;

        if (Cam.x < Selected.x)
            angle_mod = Mathf.PI;
        if (Cam.z < Selected.z)
            angle_mod = -Mathf.PI;
        if (Cam.x > Selected.x && Cam.z < Selected.z)
            angle_mod = 2 * Mathf.PI;

        angle_between = (Mathf.Abs(angle_mod - Mathf.Atan(dz / dx)));

        BetweenAngleCalculated = true;
    }

    /// <summary>
    /// Called after Escape is pressed
    /// </summary>
    private void ExecuteEscapeSequence()
    {
        follow_toggle = false;
        TargetPositionCalculated = false;
    }

    /// <summary>
    /// Is there Mouse Scrolling Movement?
    /// </summary>
    public bool isMouseMovement()
    {
        return MouseMovement;
    }

    /// <summary>
    /// Is the camera following a Control Object?
    /// </summary>
    public bool isFollowToggle()
    {
        return follow_toggle;
    }
}
