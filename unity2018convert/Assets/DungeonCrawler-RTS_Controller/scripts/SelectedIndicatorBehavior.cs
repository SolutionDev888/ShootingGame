/// \file
/// Handles Behavior of the SelectedIndicator.
/// @author: Chase Hutchens

using UnityEngine;
using System.Collections;

/// <summary>
/// This class is used for Controlling the SelectedIndicator Plane Behavior
/// bellow the ControlObjHandler, NPCHandler or CreatureHandler Objects.
/// </summary>
/// <remarks>This can of course be extended to work on any object that requires a SelectedIndicator.</remarks>
public class SelectedIndicatorBehavior : MonoBehaviour
{
    private RaycastHit target;

    /// <summary>
    /// The LayerMask to Ignore when Raycasting.
    /// </summary>
    public LayerMask IgnoreLayerMask;

	void Update()
    {
        Ray selected_ind_down_ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(selected_ind_down_ray, out target, 10, ~IgnoreLayerMask))
        {
            if (target.transform.name.Contains("Terrain") || target.transform.name.Contains("PlatformSloped"))
                transform.rotation = Quaternion.FromToRotation(Vector3.up, target.normal);
        }
	}
}
