using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour
{
    private FormationSwitches formationHandler = new FormationSwitches();

	void Start()
    {
        formationHandler.Init();
	}

    void OnGUI()
    {
        formationHandler.OnGUI();
    }
}
