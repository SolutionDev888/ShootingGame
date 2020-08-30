//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------


using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Button Full Screen")]
public class UIButtonFullscreen : MonoBehaviour 
{
	public UIWidget normalState;
	public UIWidget exitState;
	public UIStretch stretch;
	public UIWidget widget;
	public UIDragObject drag;
	public UIDragResize resize;
	public float speed = 1;
	public UITweener.Method ease = UITweener.Method.BounceOut;
	public int defaultWidth = 750;
	public int defaultHeight = 400;

	bool mToggle;

	void Awake()
	{
		NGUITools.SetActive(normalState.gameObject, true);
		NGUITools.SetActive(exitState.gameObject, false);
	}

	void OnClick()
	{
		mToggle = !mToggle;

		if (mToggle)
		{
			widget.cachedTransform.localPosition = Vector3.zero;
			NGUITools.SetActive(normalState.gameObject, false);
			NGUITools.SetActive(exitState.gameObject, true);
			//stretch.enabled = true;
			//stretch.style = UIStretch.Style.Both;
			TweenWidth tw = TweenWidth.Begin(widget, speed, Screen.width);
			tw.method = ease;
			EventDelegate.Add(tw.onFinished, OnFullScreen, true);

			TweenHeight th = TweenHeight.Begin(widget, speed, Screen.height);
			th.method = ease;

			if (drag != null) NGUITools.SetActive(drag.gameObject, false);
			if (resize != null) NGUITools.SetActive(resize.gameObject, false);
		}
		else
		{
			NGUITools.SetActive(normalState.gameObject, true);
			NGUITools.SetActive(exitState.gameObject, false);
			stretch.style = UIStretch.Style.None;
			stretch.enabled = false;
			//widget.width = defaultWidth;
			//widget.height = defaultHeight;

			TweenWidth tw = TweenWidth.Begin(widget, speed, defaultWidth);
			tw.method = ease;

			TweenHeight th = TweenHeight.Begin(widget, speed, defaultHeight);
			th.method = ease;

			if (drag != null) NGUITools.SetActive(drag.gameObject, true);
			if (resize != null) NGUITools.SetActive(resize.gameObject, true);
		}
	}

	void OnFullScreen()
	{
		stretch.enabled = true;
		stretch.style = UIStretch.Style.Both;
	}
}
