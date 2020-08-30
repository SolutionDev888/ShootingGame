//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------


using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Button Zoom Mini Map")]
public class UIButtonZoom : MonoBehaviour 
{
	public bool zoomIn;
	public bool isMinimap = true;
	public float amount = 0.5f;

	void OnClick()
	{
		if (isMinimap)
		{
			if (UIMiniMap.instance)
			{
				if (zoomIn)
					UIMiniMap.instance.ZoomIn(amount);
				else
					UIMiniMap.instance.ZoomOut(amount);
			}
		}
		else
		{
			if (UIWorldMap.instance)
			{
				if (zoomIn)
					UIWorldMap.instance.ZoomIn(amount);
				else
					UIWorldMap.instance.ZoomOut(amount);
			}
		}
	}
}
