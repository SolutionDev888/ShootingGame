//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Label Coords")]
[RequireComponent(typeof(UILabel))]
public class UILabelCoords : MonoBehaviour
{
	public string format = "X:{0},Y:{1}";
	UILabel label;
	string mContent;

	void Awake() { label = GetComponent<UILabel>(); }

	void Update()
	{
		if (UIMiniMap.instance == null)
			return;

		if (UIMiniMap.instance.target)
		{
			int x = (int)UIMiniMap.instance.target.position.x;
			int y = (int)(NJGMap.instance.orientation == NJG.NJGMapBase.Orientation.XZDefault ? UIMiniMap.instance.target.position.z : UIMiniMap.instance.target.position.y);

			mContent = string.Format(format, x, y);
			if (label.text != mContent) label.text = mContent;
		}
	}
}
