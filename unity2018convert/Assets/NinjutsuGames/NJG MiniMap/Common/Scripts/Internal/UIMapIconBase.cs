//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game map can have icons on it -- this class takes care of animating them when needed.
/// </summary>

namespace NJG
{
	public class UIMapIconBase : MonoBehaviour
	{
		static List<UIMapIconBase> selected = new List<UIMapIconBase>();

		public NJGMapItem item;
		public bool isValid = false;
		public bool isMapIcon = true;
		public bool isVisible = false;
		new public UnityEngine.Object collider;

		/// <summary>
		/// Cache transform for speed.
		/// </summary>

		private Transform mTrans;
		public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }
		//public Transform dynamicTransform { get { Debug.Log("isActive " + gameObject.activeInHierarchy); return cachedTransform; } }

		public float alpha { get { return mAlpha; } set { mAlpha = value; } }

		protected bool isLooping;
		protected bool isScaling;
		protected Vector3 onHoverScale = new Vector3(1.3f, 1.3f, 1.3f);
		//protected TweenParms tweenParms = new TweenParms();

		//Tweener mLoop;
		float mAlpha = 1;
		protected bool mFadingOut;

		protected bool mSelected;

		/// <summary>
		/// Triggered when the icon is visible on the map.
		/// </summary>

		protected virtual void Start() { if (Application.isPlaying) { CheckAnimations(); } }

		protected virtual void Update() 
		{
			if (mSelected != item.isSelected)
			{
				mSelected = item.isSelected;
				if (mSelected) Select();
				else UnSelect();
			}

			if (item.showIcon && item.showOnAction)
			{
				OnVisible();
				item.showIcon = false;
			}
		}

		public virtual void Select()
		{
			if (!Input.GetKey(KeyCode.LeftShift) && !item.forceSelection)
				UnSelectAll();

			item.isSelected = true;			
			if (!selected.Contains(this)) selected.Add(this);
		}

		public virtual void UnSelect() { }

		protected void UnSelectAll()
		{
			for (int i = 0, imax = selected.Count; i < imax; i++)
			{
				UIMapIconBase ic = selected[i];
				ic.UnSelect();
			}
			selected.Clear();
		}

		protected void CheckAnimations()
		{
			alpha = 1;
			if (item != null)
			{				
				if (item.showOnAction)
					cachedTransform.localScale = Vector3.zero;
				else if (item.fadeOutAfterDelay > 0)
				{
					if (!mFadingOut)
					{
						mFadingOut = true;
						StartCoroutine(DelayedFadeOut());
					}
				}
				else if (item.loopAnimation)
					OnLoop();
				else if (item.animateOnVisible && !isMapIcon && item.fadeOutAfterDelay == 0)
					OnVisible();				
			}
		}

		/// <summary>
		/// Add this unit to the list of in-game units.
		/// </summary>

		void OnEnable()
		{
			if (Application.isPlaying)
			{				
				//if (mLoop != null && !item.loopAnimation) mLoop.Kill();
				cachedTransform.localScale = Vector3.one;
				CheckAnimations();
			}
		}

		/// <summary>
		/// Remove this unit from the list.
		/// </summary>

		void OnDisable()
		{
			if (mFadingOut)
			{
				mFadingOut = false;
				StopAllCoroutines();
			}
			/*if (Application.isPlaying)
			{
				if (item != null)
				{
					if (item.loopAnimation)
					{
						if (mLoop != null)
							if (!mLoop.isPaused)
								mLoop.Pause();
					}
					else
					{
						if (mLoop != null) mLoop.Kill();
					}
				}
			}*/
			if (isVisible) isVisible = false;
		}

		/// <summary>
		/// Triggered when the icon is visible on the map.
		/// </summary>

		protected virtual void OnVisible()
		{
			if (!isVisible)
			{
				//alpha = 0;
				//HOTween.To(this, 0.9f, "alpha", 1).easeType = EaseType.Linear;

				if (item.fadeOutAfterDelay > 0)
				{
					if (!mFadingOut)
					{
						mFadingOut = true;
						StartCoroutine(DelayedFadeOut());
					}
				}

				if (!item.loopAnimation)
				{
					cachedTransform.localScale = Vector3.one * 0.01f;
                    LeanTween.scale(cachedTransform.gameObject, Vector3.one, 1).setEase(LeanTweenType.easeOutExpo);
					//tweenParms.Prop("localScale", Vector3.one).Ease(EaseType.EaseInOutElastic);
					//HOTween.To(cachedTransform, 1, tweenParms);
				}

				isVisible = true;
			}			
		}

		protected virtual void OnLoop()
		{
			if (item.loopAnimation)
			{
				isLooping = true;
                LeanTween.scale(cachedTransform.gameObject, Vector3.one, 0.5f).setLoopType(LeanTweenType.pingPong).setFrom(Vector3.one * 1.5f);
                
				/*if (mLoop == null)
				{
					cachedTransform.localScale = Vector3.one * 1.5f;
					mLoop = HOTween.To(cachedTransform, 0.5f, new TweenParms().Prop("localScale", Vector3.one).Ease(EaseType.Linear).Loops(-1, LoopType.Yoyo).IntId(999));
				}
				else
				{
					if (mLoop.isPaused)
						mLoop.Play();
				}*/
			}
		}

		protected IEnumerator DelayedFadeOut()
		{
			yield return new WaitForSeconds(item.fadeOutAfterDelay);

			OnFadeOut();
		}

		protected virtual void OnFadeOut()
		{
            LeanTween.alpha(gameObject, 0, 0.9f);
			/*Tweener t = HOTween.To(this, 0.9f, "alpha", 0);
			t.easeType = EaseType.Linear;*/
			mFadingOut = false;
		}
	}
}