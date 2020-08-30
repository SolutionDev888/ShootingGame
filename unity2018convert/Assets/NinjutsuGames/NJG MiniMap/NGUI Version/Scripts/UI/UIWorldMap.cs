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
[AddComponentMenu("NJG MiniMap/NGUI/World Map")]
public class UIWorldMap : NJG.UIWorldMapBase
{
	static UIWorldMap mInst;
	static public UIWorldMap instance 
	{ 
		get 
		{
			if (mInst == null)
			{
				//if (NJGMap.instance.worldMap != null) mInst = NJGMap.instance.worldMap as UIWorldMap;
				//else 
				mInst = GameObject.FindObjectOfType(typeof(UIWorldMap)) as UIWorldMap;
			}
			return mInst; 
		} 
	}

    public void OnHoverOver() { isMouseOver = true; }
    public void OnHoverOut() { isMouseOver = false; }

	protected override void OnStart()
	{
		if (NJGMap.instance == null) return;

		if (mInst == null) mInst = this;

		base.OnStart();

		if (planeRenderer == null) return;


		BoxCollider col = planeRenderer.GetComponent<BoxCollider>();
		if (col == null)
			NGUITools.AddWidgetCollider(planeRenderer.gameObject);

		UIForwardEvents fe = planeRenderer.GetComponent<UIForwardEvents>();
		if (fe == null)
			fe = planeRenderer.gameObject.AddComponent<UIForwardEvents>();

		fe.onClick = true;
		fe.onHover = true;
		fe.target = gameObject;
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
			if (ic == null) continue;
			if (ic.item.Equals(item))
			{
				string spr = NJGMap.instance.GetSprite(item.type).name;
				if (ic.sprite.spriteName != spr) ic.sprite.spriteName = spr;
                /*if (ic.sprite.depth != item.depth) ic.sprite.depth = item.depth;
                if (!ic.sprite.color.Equals(item.color)) ic.sprite.color = item.color;
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
                            if (ic.collider != null) ic.collider.size = item.borderScale;
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
			ent.sprite.depth = 1 + NGUITools.CalculateNextDepth(ent.sprite.gameObject) + item.depth;
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
					ent.border.depth = 1 + NGUITools.CalculateNextDepth(ent.border.gameObject) + item.depth + 1;
					ent.border.color = item.color;
					if (ent.border.localSize != (Vector2)item.borderScale)
					{
						ent.border.width = (int)item.borderScale.x;
						ent.border.height = (int)item.borderScale.y;
					}
				}
			}
			mUnused.RemoveAt(mUnused.Count - 1);
			NGUITools.SetActive(ent.gameObject, true);
			mList.Add(ent);
			return ent;
		}

		// Create this new icon
		GameObject go = NGUITools.AddChild(iconRoot.gameObject);
		go.name = "Icon" + mCount;
		 
		UISprite sprite = NGUITools.AddWidget<UISprite>(go);
		sprite.name = "Icon";
		sprite.depth = 1 + NGUITools.CalculateNextDepth(sprite.gameObject) + item.depth;
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

			UISprite border = null;
			s = NJGMap.instance.GetSpriteBorder(item.type);
			if (s != null)
			{
				border = NGUITools.AddWidget<UISprite>(go);
				border.name = "Selection";
				border.depth = 1 + NGUITools.CalculateNextDepth(border.gameObject) + item.depth + 1;
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
}