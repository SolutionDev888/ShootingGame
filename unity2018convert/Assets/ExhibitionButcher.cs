using UnityEngine;
using System.Collections;

public class ExhibitionButcher : MonoBehaviour {
	private Animator anim;
	public Transform waypoint;
	// Use this for initialization
	void Start () {
		anim = this.GetComponent<Animator>();
		anim.SetFloat("Speed",10f);
//		if(MainGameInfo.currentMission != 4){
//			this.gameObject.SetActive(false);
//		}
	}
	
	// Update is called once per frame
	void Update () {
		if(Vector3.Distance(transform.position,waypoint.position) < 3f){
			this.gameObject.SetActive(false);
		}
//		if(!MainGameInfo.pauseFlag){
//			this.gameObject.SetActive(false);
//		}
	}
}
