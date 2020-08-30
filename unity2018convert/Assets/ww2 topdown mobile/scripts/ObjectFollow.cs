using UnityEngine;

[AddComponentMenu("Standard/ObjectFollow")]

class ObjectFollow : MonoBehaviour
{
	public string m_sTagToFollow = "MainCamera";
	public Vector3 m_vOffset = Vector3.zero;

	GameObject Go = null;
	Transform m_tTheirTrans = null;
	Transform m_tMyTrans = null;

	void Start()
	{
		Go = GameObject.FindGameObjectWithTag(m_sTagToFollow);
		if (Go == null)
		{
			Debug.LogError("GO with tag: " + m_sTagToFollow);
		}
		else
		{
			m_tTheirTrans = Go.transform;
		}

		m_tMyTrans = this.transform;
	}

	void Update()
	{
		if (Go != null)
		{
			m_tMyTrans.position = m_tTheirTrans.position + m_vOffset;
		}
	}
}
