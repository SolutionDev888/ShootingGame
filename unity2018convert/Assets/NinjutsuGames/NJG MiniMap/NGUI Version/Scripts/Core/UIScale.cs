using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UIScale : MonoBehaviour 
{
	public UIWidget target;

	Vector2 mScale = Vector3.zero;
	Transform mTrans;

	// Use this for initialization
	void Awake () 
	{
		mTrans = transform;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (target == null) return;
		if (mScale.x != target.width || mScale.y != target.height)
		{
			mScale.x = target.width;
			mScale.y = target.height;
			mTrans.localScale = mScale;
		}
	}
}
