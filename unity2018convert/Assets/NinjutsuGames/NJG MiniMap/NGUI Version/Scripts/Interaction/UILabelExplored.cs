//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Label Explored")]
[RequireComponent(typeof(UILabel))]
public class UILabelExplored : MonoBehaviour
{
	public string format = "Explored:{0}";
	UILabel label;
	string mContent;

	void Awake() { label = GetComponent<UILabel>(); }

	void Update()
	{
		if (UIMiniMap.instance == null)
			return;

		if (UIMiniMap.instance.target)
		{
			mContent = string.Format(format, (int)(NJGFOW.instance.exploredRatio * 100));
			if (label.text != mContent) label.text = mContent;
		}
	}
}
