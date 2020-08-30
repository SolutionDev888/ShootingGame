//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NJG;

//[ExecuteInEditMode]
[AddComponentMenu("NJG MiniMap/Map Item")]
public class NJGMapItem : MonoBehaviour 
{
	static public List<NJGMapItem> list = new List<NJGMapItem>();

	#region Public Properties

	public bool isRevealed = false;
	public bool revealFOW = false;
	public bool drawDirection = false;
	public string content = "";
	public int type = 0;
	public int revealDistance = 0;
	public UIMapArrowBase arrow;

	public Action<bool> onSelect;
	public Color color { get { if (!mColorSet) mColor = NJGMapBase.instance.GetColor(type); mColorSet = true; return mColor; } }
	public bool rotate { get { if (!mRotateSet) { mRotateSet = true; mRotate = NJGMapBase.instance.GetRotate(type); } return mRotate; } }
	public bool interaction { get { if (!mInteractionSet) { mInteractionSet = true; mInteraction = NJGMapBase.instance.GetInteraction(type); } return mInteraction; } }
	public bool arrowRotate { get { if (!mArrowRotateSet) { mArrowRotateSet = true; mArrowRotate = NJGMapBase.instance.GetArrowRotate(type); } return mArrowRotate; } }
	public bool updatePosition { get { if (!mUpdatePosSet) { mUpdatePosSet = true; mUpdatePos = NJGMapBase.instance.GetUpdatePosition(type); } return mUpdatePos; } }
	public bool animateOnVisible { get { if (!mAnimOnVisibleSet) { mAnimOnVisibleSet = true; mAnimOnVisible = NJGMapBase.instance.GetAnimateOnVisible(type); } return mAnimOnVisible; } }
	public bool showOnAction { get { if (!mAnimOnActionSet) { mAnimOnActionSet = true; mAnimOnAction = NJGMapBase.instance.GetAnimateOnAction(type); } return mAnimOnAction; } }
	public bool loopAnimation { get { if (!mLoopSet) { mLoopSet = true; mLoop = NJGMapBase.instance.GetLoopAnimation(type); } return mLoop; } }
	public bool haveArrow { get { if (!mArrowSet) { mArrowSet = true; mArrow = NJGMapBase.instance.GetHaveArrow(type); } return mArrow; } }
	//public bool revealFOW { get { if (!mFOWSet) { mFOWSet = true; mFOW = NJGMapBase.instance.GetRevealFOW(type); } return mFOW; } }
	public float fadeOutAfterDelay { get { if (mFadeOut == -1) mFadeOut = NJGMapBase.instance.GetFadeOutAfter(type); return mFadeOut; } }
	public int size 
	{ 
		get 
		{
			if (NJGMapBase.instance.GetCustom(type))			
				mSize = NJGMapBase.instance.GetSize(type);
			else mSize = NJGMapBase.instance.iconSize; 
			return mSize; 
		} 
	}
	public int borderSize
	{
		get
		{
			if (NJGMapBase.instance.GetCustomBorder(type))
				mBSize = NJGMapBase.instance.GetBorderSize(type);
			else mBSize = NJGMapBase.instance.borderSize;
			return mBSize;
		}
	}
	public virtual Vector3 iconScale
	{
		get
		{
			if (mIconSize != size)
			{
				mIconSize = size;
				mIconScale.x = mIconScale.y = size;
			}
			return mIconScale;
		}
	}
	public virtual Vector3 borderScale
	{
		get
		{
			if (mBorderSize != borderSize)
			{
				mBorderSize = borderSize;
				mBorderScale.x = mBorderScale.y = borderSize;
			}
			return mBorderScale;
		}
	}
	public int depth { get { if (mDepth == int.MaxValue) mDepth = NJGMapBase.instance.GetDepth(type); return mDepth; } }
	public int arrowDepth { get { if (mArrowDepth == int.MaxValue) mArrowDepth = NJGMapBase.instance.GetArrowDepth(type); return mArrowDepth; } }
	public int arrowOffset { get { if (mArrowOffset == int.MaxValue) mArrowOffset = NJGMapBase.instance.GetArrowOffset(type); return mArrowOffset; } }

	public bool isSelected { get { return mSelected; } set { mSelected = value; if (onSelect != null) onSelect(mSelected); } }
	public bool forceSelection { get { return mForceSelect; } set { mForceSelect = value; } }

	public bool showIcon;

	/// <summary>
	/// Whether the revealer is actually active or not. If you wanted additional checks such as "is the unit dead?",
	/// then simply derive from this class and change the "isActive" value accordingly.
	/// </summary>

	public bool isActive = true;

	/// <summary>
	/// Cache transform for speed.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	#endregion

	#region Private Properties

	Color mColor = Color.clear;
	bool mInteraction;
	bool mRotate;
	bool mArrowRotate;
	bool mUpdatePos;
	bool mAnimOnVisible;
	bool mAnimOnAction;
	bool mLoop;
	bool mArrow;
	bool mFOW;
	float mFadeOut = -1;
	Vector2 mIconScale;
	Vector2 mBorderScale;
	int mIconSize = int.MaxValue;
	int mSize = int.MaxValue;
	int mBSize = int.MaxValue;
	int mBorderSize = int.MaxValue;
	int mDepth = int.MaxValue;
	int mArrowDepth = int.MaxValue;
	int mArrowOffset = int.MaxValue;

	bool mInteractionSet;
	bool mColorSet;
	bool mRotateSet;
	bool mArrowRotateSet;
	bool mUpdatePosSet;
	bool mAnimOnVisibleSet;
	bool mAnimOnActionSet;
	bool mLoopSet;
	bool mArrowSet;
	bool mFOWSet;

	bool mForceSelect;
	bool mSelected;

	Transform mTrans;
	NJGFOW.Revealer mRevealer;

	#endregion

	#region Monobehaviour Methods

	void Start()
	{
		if (NJGMapBase.instance != null)
		{
			if (revealFOW && NJGMapBase.instance.fow.enabled)
				mRevealer = NJGFOW.CreateRevealer();
		}

		//enabled = revealFOW;
	}

	/// <summary>
	/// Add this unit to the list of in-game units.
	/// </summary>

	void OnEnable() { list.Add(this); }

	/// <summary>
	/// Remove this unit from the list.
	/// </summary>

	void OnDestroy()
	{
		if (Application.isPlaying)
		{
			if(NJGMapBase.instance != null) if(NJGMapBase.instance.fow.enabled) NJGFOW.DeleteRevealer(mRevealer);
			mRevealer = null;			
			if (arrow != null) UIMiniMapBase.inst.DeleteArrow(arrow);
			arrow = null;
		}
		list.Remove(this);
	}

	void OnDisable() 
	{
		if (Application.isPlaying)
		{
			if (mRevealer != null) mRevealer.isActive = false;
			if (arrow != null) UIMiniMapBase.inst.DeleteArrow(arrow);
			arrow = null;
		}
		list.Remove(this);
	}	

	void Update()
	{
		if (revealFOW)
		{
			if (NJGMapBase.instance == null || NJG.UIMiniMapBase.inst == null) return;

			if (mRevealer == null) mRevealer = NJGFOW.CreateRevealer();

			if (isActive)
			{
				mRevealer.pos = NJG.UIMiniMapBase.inst.WorldToMap(cachedTransform.position, false);
				mRevealer.revealDistance = revealDistance > 0 ? revealDistance : NJGMapBase.instance.fow.revealDistance;
				mRevealer.isActive = true;
			}
			else
			{
				mRevealer.isActive = false;
			}			
		}
	}

	void OnDrawGizmosSelected()
	{
		if (revealFOW)
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(transform.position, revealDistance);
		}
	}
	#endregion

	#region Public Methods

	/// <summary>
	/// Manually select current icon.
	/// </summary>

	public void Select() { mSelected = true; }

	/// <summary>
	/// Manually force icon selection.
	/// </summary>

	public void Select(bool forceSelect) { mSelected = true; mForceSelect = forceSelect; }

	/// <summary>
	/// Unselects the current icon.
	/// </summary>

	public void UnSelect() { mSelected = false; }

	/// <summary>
	/// Shows the icon.
	/// </summary>

	public void Show() { showIcon = true; }

	#endregion
}
