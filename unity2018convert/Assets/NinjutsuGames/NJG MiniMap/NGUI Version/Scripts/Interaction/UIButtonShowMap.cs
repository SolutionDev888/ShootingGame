//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Button Show World Map")]
public class UIButtonShowMap : MonoBehaviour 
{
	void OnClick()
	{
		if (UIWorldMap.instance)
			UIWorldMap.instance.Show();
	}
}
