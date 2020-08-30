using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UIStick : MonoBehaviour 
{
	public Transform target;

	Vector3 mLastPosition;
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
		if (mLastPosition != target.localPosition)
		{
			mLastPosition = target.localPosition;
			mTrans.localPosition = mLastPosition;
		}
	}
}
