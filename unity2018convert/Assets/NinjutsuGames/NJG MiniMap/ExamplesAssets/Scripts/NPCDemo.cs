using UnityEngine;
using System.Collections;

public class NPCDemo : MonoBehaviour 
{
	public float damping = 6.0f;
	public float interval = 1f;
	public float walkSpeed = 3f;
	public LayerMask walkableLayers;

	//Vector3 mLastPos;
	Vector3 mPos;
	float mTime;
	Transform mTrans;
	//float mSpeed;
	//Tweener mTween;
	//TweenParms mParms;

	void Awake() { mTrans = transform;/* mParms = new TweenParms().Prop("position", mPos);*/ }
	void OnCollisionEnter(Collision col) { Move(); }
	void OnTriggerEnter(Collider col) { Move(); }
	
	void Update () 
	{
		if (Time.time > mTime)
		{
			mTime = Time.time + interval;
			Move();
		}

		Vector3 lookRot = mPos - mTrans.position;

		if (lookRot != Vector3.zero)
		{
			Quaternion rotation = Quaternion.LookRotation(lookRot);
			mTrans.rotation = Quaternion.Slerp(mTrans.rotation, rotation, Time.deltaTime * damping);
		}
		mTrans.position = Vector3.Lerp(mTrans.position, mPos, Time.deltaTime * 0.06f);
	}

	void Move()
	{
		if (NJG.NJGMapBase.instance != null)
		{
			mPos.x = Random.Range(NJG.NJGMapBase.instance.bounds.min.x, NJG.NJGMapBase.instance.bounds.max.x);
			mPos.y = mTrans.position.y;
			mPos.z = Random.Range(NJG.NJGMapBase.instance.bounds.min.y, NJG.NJGMapBase.instance.bounds.max.z);

            //NavMeshHit hit;
            //NavMesh.SamplePosition(mPos, out hit, 10, 1);
            //mPos = hit.position;
		}
	}
}
