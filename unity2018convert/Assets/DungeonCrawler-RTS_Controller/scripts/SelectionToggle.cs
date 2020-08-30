/// \file
/// Handles the Behavior of the SelectionToggle.
/// @author: Chase Hutchens

using UnityEngine;
using System.Collections;

/// <summary>
/// This class is used for displaying the selected
/// light object when a position is chosen within the World.
/// </summary>
public class SelectionToggle : MonoBehaviour 
{
    private GameObject main_selection_toggle;
    private GameObject selection_toggle = null;
    private bool new_toggle_created = false;
    private float time_counter = 0;
    
	void Start ()
    {
        main_selection_toggle = (GameObject)Resources.Load("SelectionToggle");
	}
	
	void Update()
    {
        if (new_toggle_created)
        {
            time_counter += Time.deltaTime;

            if (time_counter >= .06)
            {
                time_counter = 0;
                Light selection_toggle_light = selection_toggle.transform.Find("toggle_light").GetComponent<Light>();
                selection_toggle_light.intensity -= 1;

                if (selection_toggle_light.intensity <= 0)
                    new_toggle_created = false;
            }
        }
	}

    /// <summary>
    /// Destroys the old selection_toggle if it Exists and Creates a New selection_toggle at the given Position and Rotation.
    /// </summary>
    /// <param name="position">The New Position.</param>
    /// <param name="rotation">The New Rotation.</param>
    public void createSelectionToggle(Vector3 position, Quaternion rotation)
    {
        if (selection_toggle != null)
            GameObject.DestroyImmediate(selection_toggle);

        selection_toggle = (GameObject)GameObject.Instantiate(main_selection_toggle, position, rotation);
        new_toggle_created = true;

        if (time_counter != 0)
            time_counter = 0;
    }
}
