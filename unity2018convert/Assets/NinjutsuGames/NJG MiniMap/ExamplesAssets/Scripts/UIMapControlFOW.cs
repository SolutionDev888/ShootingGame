using UnityEngine;
using System.Collections;

public class UIMapControlFOW : MonoBehaviour {

	public KeyCode enableKey = KeyCode.F;
	public KeyCode resetKey = KeyCode.Z;

	NJGFOW fow;
	NJG.NJGMapBase map;

	void Start () {
		map = NJG.NJGMapBase.instance;
		fow = NJGFOW.instance;
	}
	
	// Update is called once per frame
	void Update () {
		if (fow == null || map == null) return;

		if (Input.GetKeyDown(enableKey))
		{
			map.fow.enabled = !map.fow.enabled;

			if (map.fow.enabled)
			{
				//map.fow.enabled = true;
				NJGFOW.instance.Init();
			}
		}

		if (Input.GetKeyDown(resetKey))
		{
			fow.ResetFOW();
		}
	}
}
