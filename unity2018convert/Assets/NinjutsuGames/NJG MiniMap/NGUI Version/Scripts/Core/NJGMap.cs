//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NJG MiniMap/NGUI/Map")]
public class NJGMap : NJG.NJGMapBase 
{	
	/// <summary>
	/// Get instance.
	/// </summary>

	static NJGMap mInst;
	new static public NJGMap instance { get { if (mInst == null) mInst = GameObject.FindObjectOfType(typeof(NJGMap)) as NJGMap; return mInst; } }

	/// <summary>
	/// Atlas we are going to use for icon sprites.
	/// </summary>

	public UIAtlas atlas;

	/// <summary>
	/// Default sprite.
	/// </summary>

	public UISpriteData defaultSprite;

	/// <summary>
	/// Use this to check when mouse is over the minimap UI.
	/// </summary>

	public override bool isMouseOver
	{
		get
		{
			return (UICamera.hoveredObject != null || UICamera.inputHasFocus) || base.isMouseOver;
		}
	}
	
	public UICamera uiCam;

	#region Getters

	/// <summary>
	/// Get sprite from type.
	/// </summary>

	public UISpriteData GetSprite(int type)
	{
		if (atlas == null)
		{
			Debug.LogWarning("You need to assign an atlas", this);
			return null;
		}
		return Get(type) == null ? defaultSprite : atlas.GetSprite(Get(type).sprite);
	}

	/// <summary>
	/// Get sprite border from type.
	/// </summary>

	public UISpriteData GetSpriteBorder(int type)
	{
		if (atlas == null)
		{
			Debug.LogWarning("You need to assign an atlas", this);
			return null;
		}
		return Get(type) == null ? null : atlas.GetSprite(Get(type).selectedSprite);
	}

	/// <summary>
	/// Get arrow sprite from type.
	/// </summary>

	public UISpriteData GetArrowSprite(int type)
	{
		if (atlas == null)
		{
			Debug.LogWarning("You need to assign an atlas", this);
			return null;
		}
		return Get(type) == null ? defaultSprite : atlas.GetSprite(Get(type).arrowSprite);
	}
	#endregion

	/// <summary>
	/// Clean up.
	/// </summary>

	void OnDestroy()
	{
		//if (mapRenderer != null)
		//	if (mapRenderer.gameObject != null)
		//		NJGTools.Destroy(mapRenderer.gameObject);

		if (UIMiniMap.instance != null) UIMiniMap.instance.material.mainTexture = null;
		if (UIWorldMap.instance != null) UIWorldMap.instance.material.mainTexture = null;

		if (mapTexture != null) NJGTools.Destroy(mapTexture);
		mapTexture = null;

		//base.OnDestroy();
	}

	#if UNITY_EDITOR

	/// <summary>
	/// Update layer mask for UICamera and Camera inside
	/// </summary>
	
	/*override protected void Update()
	{
		if (!Application.isPlaying)
		{
			

			/*if (generateMapTexture)
			{
				if (UIMiniMap.instance != null) if (UIMiniMap.instance.uiTexture != null) UIMiniMap.instance.uiTexture.material.mainTexture = null;
				if (UIWorldMap.instance != null) if(UIWorldMap.instance.uiTexture != null) UIWorldMap.instance.uiTexture.material.mainTexture = null;
			} 
			else if (userMapTexture != null)
			{
				if (UIMiniMap.instance != null) if (UIMiniMap.instance.uiTexture.material.mainTexture != userMapTexture) UIMiniMap.instance.uiTexture.material.mainTexture = userMapTexture;
				if (UIWorldMap.instance != null) if (UIWorldMap.instance.uiTexture.material.mainTexture != userMapTexture) UIWorldMap.instance.uiTexture.material.mainTexture = userMapTexture;
			}	*	
		}
		base.Update();
	}*/
	#endif
}
