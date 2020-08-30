
using System;
using UnityEngine;
using System.Collections;

public class UIMapAlignChanger : MonoBehaviour 
{
	public KeyCode changeKey = KeyCode.P;

	NJG.UIMiniMapBase map;
	string[] mPivots;

	void Start () 
	{
		map = NJG.UIMiniMapBase.inst;
		mPivots = Enum.GetNames(typeof(NJG.UIMapBase.Pivot));
	}
	
	void Update () 
	{
		if (map == null) return;

		if (Input.GetKeyDown(changeKey))
		{
			map.pivot = (NJG.UIMiniMapBase.Pivot)Enum.Parse(typeof(NJG.UIMiniMapBase.Pivot), mPivots[UnityEngine.Random.Range(0, mPivots.Length - 1)]);
		}
	}
}
