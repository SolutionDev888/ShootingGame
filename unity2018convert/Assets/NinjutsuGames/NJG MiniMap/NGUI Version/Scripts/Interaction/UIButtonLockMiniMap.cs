//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Button Lock MiniMap")]
public class UIButtonLockMiniMap : MonoBehaviour 
{
	public bool toggle;

	void OnClick()
	{
		if (UIMiniMap.instance)
		{
			toggle = !toggle;
			UIMiniMap.instance.rotateWithPlayer = toggle;
		}
	}
}
