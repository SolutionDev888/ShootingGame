//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections;

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Label World Name (Dynamic)")]
[RequireComponent(typeof(UILabel))]
public class UILabelDynamicWorldName : MonoBehaviour 
{
	public UITweener.Method method = UITweener.Method.Linear;
	public bool showOnStart = true;
	public float fadeInDuration = 0.5f;
	public float fadeOutDuration = 1f;
	public float hideDelay = 3;

	UILabel label;
	TweenColor tc;
	Color oc;

	void Awake() 
	{ 
		label = GetComponent<UILabel>();
		oc = label.color;		
	}

	/// <summary>
	/// Start with fade in effect.
	/// </summary>

	void Start()
	{
		if(NJGMap.instance != null) NJGMap.instance.onWorldNameChanged += OnNameChanged;
		if(showOnStart) StartCoroutine(FadeIn());
	}

	/// <summary>
	/// Fades in the label wait few seconds then fade out.
	/// </summary>

	IEnumerator FadeIn()
	{
		Color ec = oc;
		ec.a = 1;

		Color sc = oc;
		sc.a = 0;

		tc = TweenColor.Begin(gameObject, fadeInDuration, ec);
		tc.from = sc;
		tc.method = method;

		yield return new WaitForSeconds(hideDelay);
		FadeOut();
	}

	/// <summary>
	/// Fades out the label.
	/// </summary>

	void FadeOut()
	{
		Color ec = oc;
		ec.a = 0;

		Color sc = oc;
		sc.a = 1;

		tc = TweenColor.Begin(gameObject, fadeOutDuration, ec);
		tc.from = sc;
		tc.method = method;
	}

	/// <summary>
	/// The name changed so lets fade in and change the label. 
	/// </summary>

	void OnNameChanged(string worldName)
	{
		oc = NJGMap.instance.zoneColor;
		StopAllCoroutines();
		StartCoroutine(FadeIn());
		label.text = worldName;
	}
}
