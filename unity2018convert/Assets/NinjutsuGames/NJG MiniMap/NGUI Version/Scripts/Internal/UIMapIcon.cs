//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Game map can have icons on it -- this class takes care of animating them when needed.
/// </summary>

[ExecuteInEditMode]
public class UIMapIcon : NJG.UIMapIconBase
{
	public UISprite sprite;
	public UISprite border;

	Color mColor;
	TweenColor mTweenColor;
	//Vector3 mHideScale = new Vector3(0.001f, 0.001f, 1);

	/// <summary>
	/// Triggered when the icon is visible on the map.
	/// </summary>

	protected override void Start()
	{
		UnSelect();
		if (item.fadeOutAfterDelay == 0)
			sprite.alpha = 1;

		base.Start();
	}

	/// <summary>
	/// Display a tooltip with the appropiate content.
	/// </summary>

	protected virtual void OnTooltip(bool show)
	{
		if (!string.IsNullOrEmpty(item.content))
		{
			if (show)
				UICustomTooltip.Show(item.content);
			else
				UICustomTooltip.Hide();
		}
	}

	/// <summary>
	/// Triggered when mouse is over this icon.
	/// </summary>
	
	protected virtual void OnHover(bool isOver)
	{
		if (isOver)
		{
			if (!isLooping)
			{
				//tweenParms.Prop("localScale", onHoverScale).Ease(EaseType.EaseOutExpo);
				//HOTween.To(cachedTransform, 0.1f, tweenParms);
				TweenScale.Begin(sprite.cachedGameObject, 0.1f, onHoverScale);
			}
			
		}
		else
		{
			if (!isLooping)
			{
				//tweenParms.Prop("localScale", Vector3.one).Ease(EaseType.EaseOutExpo);
				//HOTween.To(cachedTransform, 0.3f, tweenParms);
				TweenScale.Begin(sprite.cachedGameObject, 0.3f, Vector3.one);
			}			
		}
	}

	public override void Select()
	{
		base.Select();
		if (border != null) border.enabled = true;
	}

	public override void UnSelect()
	{
		base.UnSelect();
		if (border != null) border.enabled = false;
	}

	void OnClick() { Select(); }

	void OnSelect(bool isSelected) 
	{
		if (isSelected) Select();
		else
		{
			if (!Input.GetKey(KeyCode.LeftShift) && !item.forceSelection) UnSelectAll();
		}
	}	

	/// <summary>
	/// React to key-based input.
	/// </summary>

	void OnKey(KeyCode key)
	{
		if (enabled && NGUITools.GetActive(gameObject))
		{
			if (key == KeyCode.Escape)
			{
				OnSelect(false);
			}
		}
	}

	/*public void Disable()
	{
		if (collider != null) collider.enabled = false;
		if (mLoop != null) mLoop.enabled = false;
		enabled = false;
		if (sprite != null) sprite.cachedTransform.localScale = mHideScale;
		if (border != null) border.cachedTransform.localScale = mHideScale;
	}

	public void Enable()
	{
		if (collider != null) collider.enabled = true;
		if (mLoop != null) mLoop.enabled = true;
		enabled = true;
		if (sprite != null) sprite.cachedTransform.localScale = Vector3.one;
		if (border != null) border.cachedTransform.localScale = Vector3.one;
	}*/

	protected override void OnVisible()
	{
		if (!isVisible)
		{
			if (item.fadeOutAfterDelay > 0)
			{
				if (!mFadingOut)
				{
					mFadingOut = true;
					StartCoroutine(DelayedFadeOut());
				}
			}

			TweenAlpha ta = TweenAlpha.Begin(sprite.cachedGameObject, 0.3f, 1f);
			ta.from = 0;
			ta.method = UITweener.Method.Linear;

			if (!item.loopAnimation)
			{
				sprite.cachedTransform.localScale = Vector3.zero;
				TweenScale ts = TweenScale.Begin(sprite.cachedGameObject, 0.5f, Vector3.one);
				//ts.from = new Vector3(0.01f, 0.01f, 0.01f);
				ts.method = UITweener.Method.BounceIn;
			}
			isVisible = true;
		}
	}

	TweenScale mLoop;

	protected override void OnLoop()
	{
		if (item.loopAnimation)
		{
			isLooping = true;

			if (mLoop == null)
			{
				mLoop = TweenScale.Begin(sprite.cachedGameObject, 1, Vector3.one);
				mLoop.from = Vector3.one * 1.5f;
				mLoop.style = UITweener.Style.PingPong;
				mLoop.method = UITweener.Method.Linear;
			}
		}
		//if(mLoop.
	}

	protected override void OnFadeOut()
	{
		TweenAlpha ta = TweenAlpha.Begin(sprite.cachedGameObject, 1f, 0f);
		//ta.from = 0;
		ta.method = UITweener.Method.Linear;
		/*if (mTweenColor == null)
		{
			mColor.a = 0;
			mTweenColor = TweenColor.Begin(sprite.cachedGameObject, 1, mColor);
			mColor.a = 1;
			mTweenColor.from = mColor;
			mTweenColor.method = UITweener.Method.Linear;
		}
		else
		{
			mTweenColor.Play(true);
		}*/
		mFadingOut = false;
	}
}