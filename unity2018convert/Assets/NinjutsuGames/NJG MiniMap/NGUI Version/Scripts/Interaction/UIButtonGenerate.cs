//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Button Generate Map")]
public class UIButtonGenerate : MonoBehaviour 
{
	void OnClick()
	{
		NJGMap.instance.GenerateMap();	
	}
}
