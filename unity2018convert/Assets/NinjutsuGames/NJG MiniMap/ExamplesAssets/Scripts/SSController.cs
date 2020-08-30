using UnityEngine;
using System.Collections;

public class SSController : MonoBehaviour
{
	public float velocity = 200.0f;
	public int jump = 15;
	public float runMultiplier = 3f;
	Rigidbody mRBody = null;
	bool mGrounded = true;
	float mVelocity = 0;

	void Awake()
	{
		mRBody = GetComponent<Rigidbody>();
		mVelocity = velocity;
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		velocity = Input.GetKey(KeyCode.LeftShift) ? mVelocity * runMultiplier : mVelocity;

		if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
			mRBody.AddForce(-velocity, 0.0f, 0.0f, ForceMode.Force);
		else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
			mRBody.AddForce(velocity, 0f, 0f, ForceMode.Force);

		if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space) && mGrounded)
		{
			GetComponent<Rigidbody>().AddForce(0f, jump * 100f, 0f);
			mGrounded = false;
		}

		if (mRBody.velocity.x > 5f)
			mRBody.velocity = new Vector3(5f, mRBody.velocity.y, 0f);
		else if (mRBody.velocity.x < -5f)
			mRBody.velocity = new Vector3(-5f, mRBody.velocity.y, 0f);

		mRBody.AddForce(0f, -50f, 0f);

		if (transform.position.y < -10f)
			mRBody.MovePosition(new Vector3(0, 20f, 0f));
	}

	public void OnCollisionEnter(Collision theCollision)
	{
		mGrounded = true;
	}
}
