//----------------------------------------------
//            NJG MiniMap (NGUI)
// Copyright © 2014 Ninjutsu Games LTD.
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Game miniMap can have icons on it -- this class takes care of animating them when needed.
/// </summary>

namespace NJG
{
	public class UIMapArrowBase : MonoBehaviour
	{
		[SerializeField]
		public NJGMapItem item;

		public Transform child;

		public bool isValid;

		/// <summary>
		/// Cache transform for speed.
		/// </summary>

		private Transform mTrans;
		public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

		protected float rotationOffset = 0.0f;

		Vector3 mRot = Vector3.zero;
		Vector3 mArrowRot = Vector3.zero;
		Vector3 mFrom = Vector3.zero;

		/// <summary>
		/// Triggered when the icon is visible on the miniMap.
		/// </summary>

		public void UpdateRotation(Vector3 fromTarget)
		{
			mFrom = fromTarget - item.cachedTransform.position;

			float angle = 0;

			if (NJGMapBase.instance.orientation == NJGMapBase.Orientation.XZDefault)
			{
				mFrom.y = 0;
				angle = Vector3.Angle(Vector3.forward, mFrom);
			}
			else
			{
				mFrom.z = 0;
				angle = Vector3.Angle(Vector3.up, mFrom);
			}

			if (Vector3.Dot(Vector3.right, mFrom) < 0)
				angle = 360 - angle;

			angle += 180;

			mRot = Vector3.zero;

			if (NJGMapBase.instance.orientation == NJGMapBase.Orientation.XZDefault)
			{
				mRot.z = angle;
				mRot.y = 180;
			}
			else
			{
				mRot.z = -angle;
				mRot.y = mRot.x = 0;
			}

			if(!cachedTransform.localEulerAngles.Equals(mRot)) cachedTransform.localEulerAngles = mRot;

			if (!item.arrowRotate)
			{
				mArrowRot.x = 0;
				mArrowRot.y = 180;
				mArrowRot.z = UIMiniMapBase.inst.rotateWithPlayer ? (UIMiniMapBase.inst.iconRoot.localEulerAngles.z - cachedTransform.localEulerAngles.z) : -cachedTransform.localEulerAngles.z;
				if (child.localEulerAngles != mArrowRot) child.localEulerAngles = mArrowRot;
			}
		}
	}
}