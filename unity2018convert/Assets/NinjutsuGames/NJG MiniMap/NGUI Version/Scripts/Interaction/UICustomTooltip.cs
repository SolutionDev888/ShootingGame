//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example script that can be used to show tooltips.
/// </summary>

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Tooltip")]
public class UICustomTooltip : MonoBehaviour
{
	static UICustomTooltip mInstance;

	public Camera uiCamera;

	public UILabel text;
	public UISprite background;
	//public UISprite arrow;	

	public float appearSpeed = 10f;
	public bool scalingTransitions = true;
	public bool followMousePosition = true;
	public Vector2 backgroundPadding = new Vector2(4f, 4f);
	public Vector2 positionOffset = new Vector2(0f, 30f);	

	Transform mTrans;
	float mTarget = 0f;
	float mCurrent = 0f;
	Vector3 mPos;
	Vector3 mSize;

	UIWidget[] mWidgets;

	void Awake() { mInstance = this; }
	void OnDestroy() { mInstance = null; }

	/// <summary>
	/// Get a list of widgets underneath the tooltip.
	/// </summary>

	void Start()
	{
		mTrans = transform;
		NGUITools.SetActive(gameObject, true);
		mWidgets = GetComponentsInChildren<UIWidget>();
		mPos = mTrans.localPosition;
		mSize = mTrans.localScale;
		if (uiCamera == null) uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
		SetAlpha(0f);
	}

	/// <summary>
	/// Update the tooltip's alpha based on the target value.
	/// </summary>

	void Update()
	{		
		if (mCurrent != mTarget)
		{
			mCurrent = Mathf.Lerp(mCurrent, mTarget, Time.deltaTime * appearSpeed);
			if (Mathf.Abs(mCurrent - mTarget) < 0.001f) mCurrent = mTarget;
			SetAlpha(mCurrent * mCurrent);

			if (scalingTransitions)
			{
				Vector3 offset = mSize * 0.25f;
				offset.y = -offset.y;

				Vector3 size = Vector3.one * (1.5f - mCurrent * 0.5f);
				Vector3 pos = Vector3.Lerp(mPos - offset, mPos, mCurrent);

				mTrans.localPosition = pos;
				mTrans.localScale = size;
			}
		}
		if (followMousePosition && mCurrent > 0) UpdatePosition();
	}

	/// <summary>
	/// Set the alpha of all widgets.
	/// </summary>

	void SetAlpha(float val)
	{
		for (int i = 0, imax = mWidgets.Length; i < imax; ++i)
		{
			UIWidget w = mWidgets[i];
			Color c = w.color;
			c.a = val;
			w.color = c;
		}
	}

	/// <summary>
	/// Set the tooltip's text to the specified string.
	/// </summary>

	void SetText(string content, Color bgColor)
	{
		if (content != null && !string.IsNullOrEmpty(content))
		{
			mTarget = 1f;
			text.text = content;			
			//background.color = bgColor;

			if (background != null)
			{
				//Transform backgroundTrans = background.transform;
				Transform textTrans = text.transform;
				Vector3 textScale = textTrans.localScale;

				// Calculate the dimensions of the printed text
				mSize = text.printedSize;

				// Scale by the transform and adjust by the backgroundPadding offset
				mSize.x *= textScale.x;
				mSize.y *= textScale.y;
				mSize.x += background.border.x + background.border.z + backgroundPadding.x;
				mSize.y += background.border.y + background.border.w + backgroundPadding.y;
				mSize.z = 1f;

				textTrans.localPosition = new Vector3(textTrans.localPosition.x, backgroundPadding.y *2, textTrans.localPosition.z);

				background.width = (int)mSize.x;
				background.height = (int)mSize.y;

				/*if(arrow != null){
					Transform arrowTrans = arrow.transform;
					arrowTrans.localPosition = new Vector3(0, mSize.y / 2f, backgroundTrans.localPosition.z);
				}*/
			}

			UpdatePosition();
		}
		else mTarget = 0f;
		SetAlpha(mCurrent * mCurrent);
	}

	/// <summary>
	/// Update Tooltip position.
	/// </summary>

	void UpdatePosition()
	{
		// Orthographic camera positioning is trivial
		mPos = Input.mousePosition;

		if (uiCamera != null)
		{
			// Since the screen can be of different than expected size, we want to convert
			// mouse coordinates to view space, then convert that to world position.
			mPos.x = Mathf.Clamp01(mPos.x / Screen.width);
			mPos.y = Mathf.Clamp01(mPos.y / Screen.height);

			// Calculate the ratio of the camera's target orthographic size to current screen size
			float activeSize = uiCamera.orthographicSize / mTrans.parent.lossyScale.y;
			float ratio = (Screen.height * 0.5f) / activeSize;

			// Calculate the maximum on-screen size of the tooltip window
			Vector2 max = new Vector2(ratio * mSize.x / Screen.width, ratio * mSize.y / Screen.height);

			// Limit the tooltip to always be visible
			mPos.x = Mathf.Min(mPos.x, 1f - max.x);
			mPos.y = Mathf.Max(mPos.y, max.y);

			// Update the absolute position and save the local one
			mTrans.position = uiCamera.ViewportToWorldPoint(mPos);
			mPos = mTrans.localPosition;
			mPos.x = (int)(mPos.x + positionOffset.x);
			mPos.y = (int)(mPos.y + positionOffset.y);
			mTrans.localPosition = mPos;
		}
		else
		{
			// Don't let the tooltip leave the screen area
			if (mPos.x + mSize.x > Screen.width) mPos.x = Screen.width - mSize.x;
			if (mPos.y - mSize.y < 0f) mPos.y = mSize.y;

			// Simple calculation that assumes that the camera is of fixed size
			mPos.x -= Screen.width * 0.5f;
			mPos.y -= Screen.height * 0.5f;
		}
	}

	/// <summary>
	/// Hide the tooltip.
	/// </summary>

	static public void Hide() { if (mInstance != null) mInstance.mTarget = 0f; }

	/// <summary>
	/// Show a tooltip with the specified text.
	/// </summary>

	static public void Show(string contentText)
	{
		if (mInstance != null)
		{
			mInstance.SetText(contentText, Color.white);
		}
	}

	/// <summary>
	/// Show a tooltip with the specified text.
	/// </summary>

	static public void Show(string contentText, Color color)
	{
		if (mInstance != null)
		{
			mInstance.SetText(contentText, color);
		}
	}
}