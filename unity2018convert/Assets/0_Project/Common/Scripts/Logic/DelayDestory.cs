using UnityEngine;
using System.Collections;

public class DelayDestory : MonoBehaviour {
    public float time;
    private void Awake()
    {
        GameObject.Destroy(gameObject, time);
    }
}
