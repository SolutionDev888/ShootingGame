using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDelayActive : MonoBehaviour {
    public GameObject[] targets;
    public bool flag = true;
    public float delayTime;
    public bool isStart= false;
	// Use this for initialization
	void Start () {
        if (isStart) OnAction();

    }
    public void OnAction()
    {
        StartCoroutine( DoProcess());
    }
    private IEnumerator DoProcess()
    {
        if (delayTime > 0f) {
            yield return null;
        } else {
            yield return new WaitForSeconds(delayTime);
        }
        foreach (GameObject child in targets)
        {
            child.SetActive(flag);
        }
    }
}
