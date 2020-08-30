using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
    public Transform target;
    [HideInInspector]
    public Transform source;
    public float Yangle;
    public float Xangle;
    // Use this for initialization
    void Start () {
        source = transform;

    }
	
	// Update is called once per frame
	void Update () {
        if (target == null) return;
        Yangle = source.forward.deltaYAngle(target.position - source.position);
        Xangle = source.forward.deltaXAngle(target.position - source.position);

    }
    public void Damage(float value)
    {
        Debug.Log("Damage = " + value);
    }
}
