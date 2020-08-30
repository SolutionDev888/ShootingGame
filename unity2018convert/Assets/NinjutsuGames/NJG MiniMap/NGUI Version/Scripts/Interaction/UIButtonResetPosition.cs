//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------


using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Button Reset Panning Position")]
public class UIButtonResetPosition : MonoBehaviour 
{
	public bool isMinimap = true;

	void OnClick()
	{
		if (isMinimap)
		{
			if (UIMiniMap.instance)
			{
				UIMiniMap.instance.ResetPanning();
			}
		}
		else
		{
			if (UIWorldMap.instance)
			{
				UIWorldMap.instance.ResetPanning();
			}
		}
	}
}
