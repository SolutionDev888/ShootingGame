//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Button Toggle World Map")]
public class UIButtonToggleMap : MonoBehaviour 
{
	void OnClick()
	{
		UIMiniMap.instance.ToggleWorldMap();
	}
}
