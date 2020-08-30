using UnityEngine;
using System.Collections;

public class HealthBehaviour : MonoBehaviour {
	private bool actionFlag = false;
	private float timeCounter = 0;

	public GameObject healthSprite;
	public float decVal = 0.1f;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

        
	}

	public void OnDamage(){
		if(!actionFlag){
			actionFlag = true;
			healthSprite.SetActive(true);
			healthSprite.GetComponent<TweenAlpha>().ResetToBeginning();
			healthSprite.GetComponent<TweenAlpha>().PlayForward();
			StartCoroutine(ActionDelay());
		}
	}

	IEnumerator ActionDelay(){
		yield return new WaitForSeconds(1f);
		actionFlag = false;
		healthSprite.SetActive(false);
	}
}
