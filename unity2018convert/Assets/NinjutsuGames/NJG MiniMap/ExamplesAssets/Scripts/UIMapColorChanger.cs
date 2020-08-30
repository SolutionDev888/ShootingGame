using NJG;
using UnityEngine;
using System.Collections;

public class UIMapColorChanger : MonoBehaviour {

	public KeyCode changeKey = KeyCode.C;
	public KeyCode resetKey = KeyCode.R;

	Color mColor = Color.white;
	Color mMinimapColor = Color.white;
	Color mWorldmapColor = Color.white;
	
	void Start () 
	{
		if (NJG.UIMiniMapBase.inst != null)
		{
			if (NJG.UIMiniMapBase.inst.material != null)
			{
				mMinimapColor = NJG.UIMiniMapBase.inst.material.color;
			}
		}
		if (NJG.UIWorldMapBase.inst != null)
		{
			if (NJG.UIWorldMapBase.inst.material != null)
			{
				mWorldmapColor = NJG.UIWorldMapBase.inst.material.color;
			}
		}
	}
	
	void Update () 
	{
		if (Input.GetKeyDown(changeKey))
		{
			mColor = ColorHSV.GetRandomColor(Random.Range(0.0f, 360f), 1, 1);
			if (NJG.UIMiniMapBase.inst != null)
			{
				if (NJG.UIMiniMapBase.inst.material != null)
				{
					NJG.UIMiniMapBase.inst.material.color = mColor;
					NJG.UIMiniMapBase.inst.material.SetColor("_Color", mColor);
				}
			}
			if (NJG.UIWorldMapBase.inst != null)
			{
				if (NJG.UIWorldMapBase.inst.material != null)
				{
					NJG.UIWorldMapBase.inst.material.color = mColor;
					NJG.UIWorldMapBase.inst.material.SetColor("_Color", mColor);
				}
			}
		}

		if (Input.GetKeyDown(resetKey))
		{
			if (NJG.UIMiniMapBase.inst != null)
			{
				if (NJG.UIMiniMapBase.inst.material != null)
				{
					NJG.UIMiniMapBase.inst.material.color = mMinimapColor;
					NJG.UIMiniMapBase.inst.material.SetColor("_Color", mMinimapColor);
				}
			}
			if (NJG.UIWorldMapBase.inst != null)
			{
				if (NJG.UIWorldMapBase.inst.material != null)
				{
					NJG.UIWorldMapBase.inst.material.color = mWorldmapColor;
					NJG.UIWorldMapBase.inst.material.SetColor("_Color", mWorldmapColor);
				}
			}
		}
	}
}
