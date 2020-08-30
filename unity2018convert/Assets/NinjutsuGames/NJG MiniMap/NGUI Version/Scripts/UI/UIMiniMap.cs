//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NJG;

/// <summary>
/// A game mini map that display icons and scroll UITexture when target moves.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NJG MiniMap/NGUI/Minimap")]
[RequireComponent(typeof(UIAnchor))]
public class UIMiniMap : UIMiniMapBase
{
	static UIMiniMap mInst;
	static public UIMiniMap instance
	{
		get
		{
			if (mInst == null)
				mInst = GameObject.FindObjectOfType(typeof(UIMiniMap)) as UIMiniMap;

			return mInst;
		}
	}
	/// <summary>
	/// Sprite used for the background or overlay, if any.
	/// </summary>

	public UISprite overlay;

	/*public override bool isMouseOver
	{
		get
		{
			mMOver = UICamera.hoveredObject != null;
			return mMOver;
		}
		set
		{
			mMOver = value;
		}
	}*/

	UIAnchor mAnchor;
	//bool mMOver;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		if (map == null) return;

		mAnchor = GetComponent<UIAnchor>();

		base.Start();
		UpdateAlignment();
	}

	protected override void OnStart()
	{
		BoxCollider col = rendererTransform.GetComponent<BoxCollider>();
		if (col == null)
		{
			NGUITools.AddWidgetCollider(rendererTransform.gameObject);
		}

		base.OnStart();
	}

	protected override void UpdateZoomKeys()
	{
		if (UIWorldMap.instance != null)
		{
			if (!UIWorldMap.instance.isVisible)
			{
				base.UpdateZoomKeys();
			}
		}
	}

	protected override void UpdateKeys()
	{
		if (!UICamera.inputHasFocus)
		{
			if (Input.GetKeyDown(mapKey))
				ToggleWorldMap();

			if (Input.GetKeyDown(lockKey))
				rotateWithPlayer = !rotateWithPlayer;
		}
	}	

	/// <summary>
	/// Get the map icon entry associated with the specified unit.
	/// </summary>

	protected override UIMapIconBase GetEntry(NJGMapItem item)
	{
		UISpriteData s = null;

		// Try to find an existing entry
		for (int i = 0, imax = mList.Count; i < imax; ++i)
		{
			UIMapIcon ic = (UIMapIcon)mList[i];
			if (ic.item.Equals(item))
			{
				string spr = NJGMap.instance.GetSprite(item.type).name;
				if(ic.sprite.spriteName != spr) ic.sprite.spriteName = spr;
				if(ic.sprite.depth != item.depth) ic.sprite.depth = item.depth;
                /*if (!ic.sprite.color.Equals(item.color)) ic.sprite.color = item.color;
                if (ic.sprite.cachedTransform.localScale != item.iconScale)
                {
                    ic.collider.size = item.iconScale;
                    ic.sprite.cachedTransform.localScale = item.iconScale;
                }

                s = NJGMap.instance.GetSpriteBorder(item.type);
                if (s != null)
                {
                    string brd = s.name;
                    if (ic.border != null)
                    {
                        if (ic.border.spriteName != brd) ic.border.spriteName = brd;
                        if (ic.border.depth != (item.depth + 1)) ic.border.depth = item.depth + 1;
                        if (!ic.border.color.Equals(item.color)) ic.border.color = item.color;
                        if (ic.border.cachedTransform.localScale != item.borderScale)
                        {
                            ic.border.cachedTransform.localScale = item.borderScale;
                        }
                    }
                }*/
				return ic;
			}
		}

		// See if an unused entry can be reused
		if (mUnused.Count > 0)
		{
			UIMapIcon ent = (UIMapIcon)mUnused[mUnused.Count - 1];
			ent.item = item;
			ent.sprite.spriteName = NJGMap.instance.GetSprite(item.type).name;
			ent.sprite.depth = 1 + item.depth;
			ent.sprite.color = item.color;
			if (ent.sprite.localSize != (Vector2)item.iconScale)
			{
				if (ent.collider != null)
				{
					if (ent.collider is BoxCollider)
					{
						BoxCollider col = ent.collider as BoxCollider;
						col.size = item.iconScale;
					}

					if (ent.collider is BoxCollider2D)
					{
						BoxCollider2D col = ent.collider as BoxCollider2D;
						col.size = item.iconScale;
					}
				}
				ent.sprite.width = (int)item.iconScale.x;
				ent.sprite.height = (int)item.iconScale.y;
			}

			s = NJGMap.instance.GetSpriteBorder(item.type);
			if (s != null)
			{
				if (ent.border != null)
				{
					ent.border.spriteName = s.name;
					ent.border.depth = 1 + item.depth;
					ent.border.color = item.color;
					if (ent.border.localSize != (Vector2)item.borderScale)
					{
						ent.border.width = (int)item.borderScale.x;
						ent.border.height = (int)item.borderScale.y;
					}
				}
			}
			mUnused.RemoveAt(mUnused.Count - 1);
			//ent.Enable();
			NGUITools.SetActive(ent.gameObject, true);
			mList.Add(ent);
			return ent;
		}

		// Create this new icon
		GameObject go = NGUITools.AddChild(iconRoot.gameObject);
		go.name = "Icon" + mCount;

		UISprite sprite = NGUITools.AddWidget<UISprite>(go);
		sprite.name = "Icon";
		sprite.depth = 1 + item.depth;
		sprite.atlas = NJGMap.instance.atlas;
		sprite.spriteName = NJGMap.instance.GetSprite(item.type).name;
		sprite.color = item.color;
		sprite.width = (int)item.iconScale.x;
		sprite.height = (int)item.iconScale.y;

		UIMapIcon mi = go.AddComponent<UIMapIcon>();
		mi.item = item;
		mi.sprite = sprite;
		if (item.interaction)
		{
			if (go.GetComponent<Collider>() == null)
			{
				NGUITools.AddWidgetCollider(go);
				mi.collider = go.GetComponent<Collider>();

				if (mi.collider is BoxCollider)
				{
					BoxCollider col = mi.collider as BoxCollider;
					col.size = item.iconScale;
				}

				if (mi.collider is BoxCollider2D)
				{
					BoxCollider2D col = mi.collider as BoxCollider2D;
					col.size = item.iconScale;
				}
			}

			s = NJGMap.instance.GetSpriteBorder(item.type);
			if (s != null)
			{
				UISprite border = NGUITools.AddWidget<UISprite>(go);
				border.name = "Selection";
				border.depth = item.depth + 2;
				border.atlas = NJGMap.instance.atlas;
				border.spriteName = s.name;
				border.color = item.color;
				border.width = (int)item.borderScale.x;
				border.height = (int)item.borderScale.y;
				mi.border = border;
			}
		}

		if (mi == null)
		{
			Debug.LogError("Expected to find a Game Map Icon on the prefab to work with", this);
			Destroy(go);
		}
		else
		{
			mCount++;
			mi.item = item;
			mList.Add(mi);
		}
		return mi;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (shaderType == NJG.NJGMapBase.ShaderType.TextureMask)
		{
			if (mMask != maskTexture)
			{
				mMask = maskTexture;
				if (material != null) material.SetTexture("_Mask", mMask);
			}
		}
		else
		{
			if (material != null)
				material.SetColor("_MaskColor", NJG.NJGMapBase.instance.cameraBackgroundColor);
		}
	}

	/// <summary>
	/// Update the map's alignment.
	/// </summary>

	public override void UpdateAlignment()
	{
		base.UpdateAlignment();

		if (mAnchor == null) mAnchor = GetComponent<UIAnchor>();
		if (mAnchor != null) mAnchor.side = (UIAnchor.Side)pivot;
		if (iconRoot != null) iconRoot.localPosition = new Vector3(rendererTransform.localPosition.x, rendererTransform.localPosition.y, 1);

		if (overlay != null && NJGMap.instance.atlas != null)
		{
			UISpriteData sp = NJGMap.instance.atlas.GetSprite(overlay.spriteName);

			if (sp != null)
			{
				Vector4 border = overlay.border;
				overlay.width = (int)mapScale.x + (int)(border.x + border.z + overlayBorderOffset);
				overlay.height = (int)mapScale.y + (int)(border.y + border.w + overlayBorderOffset);
			}
		}
	}

	static public System.Action<Vector3> onMapDoubleClick;
	Vector3 mClickOffset;


	public void OnDoubleClick()
	{
		// Early out
		//if (MouseManager.instance.currentGameMode == MouseManager.GameMode.GUIOVERLAYMODAL) return;

		mClickOffset = (UICamera.currentTouch.pos - (Vector2)(cachedTransform.position));
		mClickOffset.x = Mathf.Abs(mClickOffset.x);
		mClickOffset.y = -Mathf.Abs(mClickOffset.y);

		//Debug.Log(mClickOffset + " / " + UICamera.currentTouch.pos);
		//Debug.Log ( ScreenCoordsToRadar ( mClickOffset ) ) ;

		//mClickPos = MapToWorld(mClickOffset);

		//CameraManager.instance.SetCameraPosition(mClickPos);

		if (onMapDoubleClick != null) onMapDoubleClick(MapToWorld(mClickOffset));
	}

    public void OnHoverOver() { isMouseOver = true; }
    public void OnHoverOut() { isMouseOver = false; }

	#region Arrows

	/// <summary>
	/// Get the map icon entry associated with the specified unit.
	/// </summary>

	protected override UIMapArrowBase GetArrow(Object o)
	{
		NJGMapItem item = (NJGMapItem)o;
		// Try to find an existing entry
		for (int i = 0, imax = mListArrow.Count; i < imax; ++i)
		{
			if (mListArrow[i].item == item)
			{
				UIMapArrow ic = (UIMapArrow)mListArrow[i];
				/*string spr = NJGMap.instance.GetArrowSprite(item.type).name;
				if (ic.sprite.spriteName != spr) ic.sprite.spriteName = spr;*/
				/*if(ic.sprite.depth != item.depth) ic.sprite.depth = item.arrowDepth;
				if(!ic.sprite.color.Equals(item.color)) ic.sprite.color = item.color;
				if(ic.sprite.cachedTransform.localScale != arrowScale) ic.sprite.cachedTransform.localScale = arrowScale;
				Vector3 offset = new Vector3(0, mapScale.y / 2 - item.arrowOffset, 0);
				if(ic.sprite.cachedTransform.localPosition != offset) ic.sprite.cachedTransform.localPosition = offset;*/
				return ic;
			}
		}

		// See if an unused entry can be reused
		if (mUnusedArrow.Count > 0)
		{
			UIMapArrow ent = (UIMapArrow)mUnusedArrow[mUnusedArrow.Count - 1];
			ent.item = item;
			ent.child = ent.sprite.cachedTransform;
			ent.sprite.spriteName = NJGMap.instance.GetArrowSprite(item.type).name;
			ent.sprite.depth = 1 + item.arrowDepth;
			ent.sprite.color = item.color;
			ent.sprite.width = (int)arrowScale.x;
			ent.sprite.height = (int)arrowScale.y;
			ent.sprite.cachedTransform.localPosition = new Vector3(0, mapScale.y / 2 - item.arrowOffset, 0);
			mUnusedArrow.RemoveAt(mUnusedArrow.Count - 1);
			NGUITools.SetActive(ent.gameObject, true);
			mListArrow.Add(ent);
			return ent;
		}

		// Create this new icon
		GameObject go = NGUITools.AddChild(rendererTransform.parent.gameObject);
		go.name = "Arrow" + mArrowCount;
		go.transform.parent = UIMiniMap.instance.arrowRoot.transform;
		go.transform.localPosition = Vector3.zero;
		go.transform.localScale = Vector3.one;

		UISprite sprite = NGUITools.AddWidget<UISprite>(go);
		//sprite.type = UISprite.Type.Sliced;
		sprite.depth = 1 + item.arrowDepth;
		sprite.atlas = NJGMap.instance.atlas;
		sprite.spriteName = NJGMap.instance.GetArrowSprite(item.type).name;
		sprite.color = item.color;
		sprite.width = (int)arrowScale.x;
		sprite.height = (int)arrowScale.y;
		sprite.cachedTransform.localPosition = new Vector3(0, rendererTransform.localScale.y / 2 - item.arrowOffset, 0);

		UIMapArrow mi = go.AddComponent<UIMapArrow>();
		mi.child = sprite.cachedTransform;
		mi.child.localEulerAngles = new Vector3(0, 180f, 0);
		mi.item = item;
		mi.sprite = sprite;

		if (mi == null)
		{
			Debug.LogError("Expected to find a UIMapArrow on the prefab to work with");
			Destroy(go);
		}
		else
		{
			mArrowCount++;
			mi.item = item;
			mListArrow.Add(mi);
		}
		return mi;
	}

	#endregion
}