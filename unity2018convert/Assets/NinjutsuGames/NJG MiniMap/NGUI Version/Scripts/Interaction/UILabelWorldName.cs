//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Label World Name")]
[RequireComponent(typeof(UILabel))]
public class UILabelWorldName : MonoBehaviour 
{
	UILabel label;

	void Awake()
	{
		label = GetComponent<UILabel>();
		if (NJGMap.instance != null) NJGMap.instance.onWorldNameChanged += OnNameChanged;
	}

	void OnNameChanged(string worldName)
	{
		label.color = NJGMap.instance.zoneColor;
		label.text = worldName;
	}
}
