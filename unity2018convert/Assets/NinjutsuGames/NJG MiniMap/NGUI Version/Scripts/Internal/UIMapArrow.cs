//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Game map can have icons on it -- this class takes care of animating them when needed.
/// </summary>

public class UIMapArrow : NJG.UIMapArrowBase
{
	[HideInInspector] public UISprite sprite;

	/*Vector3 mHideScale = new Vector3(0.001f, 0.001f, 1);

	public void Disable()
	{
		if (enabled)
		{
			enabled = false;
			if (sprite != null) sprite.cachedTransform.localScale = mHideScale;
		}
	}

	public void Enable()
	{
		if (!enabled)
		{
			enabled = true;
			if (sprite != null) sprite.cachedTransform.localScale = Vector3.one;
		}
	}*/
}