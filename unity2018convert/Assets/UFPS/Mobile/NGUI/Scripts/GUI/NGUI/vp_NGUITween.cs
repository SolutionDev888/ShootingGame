/////////////////////////////////////////////////////////////////////////////////
//
//	vp_NGUITween.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	extends vp_UITween to add tween functionality for NGUI Widgets
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class vp_NGUITween : vp_UITween
{

	[Serializable]
	public class TweenNGUI : Tween
	{
		
		public UIWidget Sprite = null;
		
		public TweenNGUI( object obj, params object[] parameters ) : base( obj, parameters )
		{
			
			Sprite = (UIWidget)obj;
			Name = Sprite.name;
			Transform = Sprite.transform;
			StartScale = Transform.localScale;
			
		}
		
		
		public override void ColorUpdate(){ Sprite.color = CurrentColor; }
	
	}
	
	
	/// <summary>
	/// Creates or updates a Color Tween and fades to the alpha specified
	/// </summary>
	public static new void FadeTo( object obj, float alpha, float duration ){ vp_NGUITween.ColorTo( obj, vp_UITween.Hash("color", alpha, "duration", duration, "handle", null, "onComplete", null) ); }
	public static new void FadeTo(object obj, float alpha, float duration, vp_UITween.Handle handle) { vp_NGUITween.ColorTo(obj, vp_UITween.Hash("color", alpha, "duration", duration, "handle", handle, "onComplete", null)); }
	public static new void FadeTo(object obj, float alpha, float duration, vp_UITween.Handle handle, Action onComplete) { vp_NGUITween.ColorTo(obj, vp_UITween.Hash("color", alpha, "duration", duration, "handle", handle, "onComplete", onComplete)); }
	

	/// <summary>
	/// Creates or updates a Color Tween
	/// </summary>
	public static new void ColorTo(object obj, Color color, float duration) { vp_NGUITween.ColorTo(obj, vp_UITween.Hash("color", color, "duration", duration, "handle", null, "onComplete", null)); }
	public static new void ColorTo(object obj, Color color, float duration, vp_UITween.Handle handle) { vp_NGUITween.ColorTo(obj, vp_UITween.Hash("color", color, "duration", duration, "handle", handle, "onComplete", null)); }
	public static new void ColorTo(object obj, Color color, float duration, vp_UITween.Handle handle, Action onComplete) { vp_NGUITween.ColorTo(obj, vp_UITween.Hash("color", color, "duration", duration, "handle", handle, "onComplete", onComplete)); }
	public static new void ColorTo(object obj, Hashtable ht)
	{

		if (obj.GetType().BaseType != typeof(UIWidget) && obj.GetType().BaseType != typeof(UIBasicSprite))
			return;
		
		UIWidget widget = (UIWidget)obj;
		
		Color alpha = widget.color;
		alpha.a = ht["color"].GetType() == typeof(float) ? (float)ht["color"] : alpha.a;
		Color color = ht["color"].GetType() == typeof(float) ? alpha : (Color)ht["color"];
		
		vp_UITween.Tween tween = null;
		if(!Instance.Tweens.TryGetValue((vp_UITween.Handle)ht["handle"], out tween))
		{
			tween = new vp_NGUITween.TweenNGUI( obj, "startColor", widget.color, "endColor", color, "duration", ht["duration"], "handle", ht["handle"], "onComplete", ht["onComplete"], "type", vp_UITween.vp_UITweenType.Color );
			Instance.Tweens.Add((vp_UITween.Handle)ht["handle"], tween);
		}
		
		tween.ColorCheck(obj, widget.color, color, ht["duration"], ht["onComplete"]);
	
	}
	
}
