//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using NJG;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A game mini map that display icons and scroll UITexture when target moves.
/// </summary>

namespace NJG
{
	public abstract class UIWorldMapBase : NJG.UIMapBase
	{
		static UIWorldMapBase mInst;
		static public UIWorldMapBase inst
		{
			get
			{
				//if (mInst == null && NJGMapBase.instance.worldMap != null)
				//	mInst = NJGMapBase.instance.worldMap;

				if (mInst == null) 
					mInst = GameObject.FindObjectOfType(typeof(UIWorldMapBase)) as UIWorldMapBase; 
				
				return mInst;
			}
		}

		protected override void Awake()
		{
			inst.enabled = true;
			limitBounds = true;
			base.Awake();
		}

		protected override void OnStart()
		{
			base.OnStart();			
			//if (Application.isPlaying) NJGTools.SetActive(gameObject, false);
			if (mChild == null) mChild = cachedTransform.GetChild(0);
			if (mChild == null) mChild = transform;
			if (Application.isPlaying) NJGTools.SetActive(mChild.gameObject, false);

			if (calculateBorder)
				mapBorderRadius = (rendererTransform.localScale.x / 2f) / 4f;
		}		

		/// <summary>
		/// Update the icon icon for the specified unit, assuming it's visible.
		/// </summary>

		protected override void UpdateIcon(NJGMapItem item, float x, float y)
		{
			// If the unit is not visible, don't consider it			
			bool isVisible = (((x - mapBorderRadius) >= -mapHalfScale.x) &&
				((x + mapBorderRadius) <= mapHalfScale.x)) &&
				(((y - mapBorderRadius) >= -mapHalfScale.y) &&
				((y + mapBorderRadius) <= mapHalfScale.y));

			Vector3 newPos = new Vector3(x, y, 0f);

			if (!isVisible) return;

			UIMapIconBase icon = GetEntry(item);

			if (icon != null) icon.isMapIcon = true;

			if (icon != null && !icon.isValid)
			{
				icon.isValid = true;
				Transform t = icon.cachedTransform;
				if (item.updatePosition) if (t.localPosition != newPos) t.localPosition = newPos;

				if (item.rotate)
				{
					float angle = ((Vector3.Dot(item.cachedTransform.forward, Vector3.Cross(Vector3.up, Vector3.forward)) <= 0f) ? 1f : -1f) * Vector3.Angle(item.cachedTransform.forward, Vector3.forward);
					t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, angle);
				}
				else
				{
					if (t.localEulerAngles != Vector3.zero) 
						t.localEulerAngles = Vector3.zero;
				}
			}
		}
	}
}