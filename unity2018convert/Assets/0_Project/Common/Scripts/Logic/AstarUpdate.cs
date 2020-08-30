using UnityEngine;
using System.Collections;

public class AstarUpdate : MonoBehaviour {
    public float updateTime = 3f;
    // Use this for initialization
    AstarPath aStar;
    private float time = 0f;
	void Start () {
        time = updateTime;
		aStar = GetComponent<AstarPath> ();
    }
    
    // Update is called once per frame
    void LateUpdate() {
        if (time > 0f)
        {
            time -= Time.deltaTime;
        }
        else
        {
            time = updateTime;
			aStar.Scan();
            System.GC.Collect(0, System.GCCollectionMode.Optimized);
        }

    }
}
