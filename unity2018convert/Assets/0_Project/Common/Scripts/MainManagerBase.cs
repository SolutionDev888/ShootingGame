using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManagerBase : MonoBehaviour {
	public static int clearLevel(){
		return PlayerPrefs.GetInt ("clearLevel", -1);
	}
	public static void clearLevel(int i){
		if (i > clearLevel ()) {
			PlayerPrefs.SetInt ("clearLevel", i);
		}
	}

    public GameObject goMainPanel;
    public GameObject goSelectLevel;
	public GameObject LoadingBar;
	public GameObject[] lockObjs;
    public string mainSceneName;
    public string[] sceneNames;
    public static int curreLevel;
	public static string MainSceneName = "SFCA_MainMenu";
	public AudioClip clickAudio;
	// Use this for initialization
	virtual public void Start () {
		if (lockObjs != null && lockObjs.Length > 0) {
			int cl = clearLevel ();
			for (int i = 0; i < lockObjs.Length; i++) {
				lockObjs [i].SetActive (!(i<=(cl+1)));
			}
		}

        if(LevelSetting.curState == LevelSetting.StartState.MAINMENU)
        {
            goMainPanel.SetActive(true);
            goSelectLevel.SetActive(false);
        }
        else if(LevelSetting.curState == LevelSetting.StartState.LEVELSELECT)
        {
            goMainPanel.SetActive(false);
            goSelectLevel.SetActive(true);
        }
    }
	
	// Update is called once per frame
	virtual public void Update () {
	}
    virtual public void OnClickLevelBtn(int level)
    {
		AudioSource aus = GetComponent<AudioSource> ();
		if (aus != null)
			aus.PlayOneShot (clickAudio);
        if (string.IsNullOrEmpty(MainSceneName))
        {
            MainSceneName = mainSceneName;
        }
        curreLevel = level;
		if (LoadingBar == null)
			UnityEngine.SceneManagement.SceneManager.LoadScene (sceneNames [level]);
		else {
			TweenAlpha ta = LoadingBar.GetComponent<TweenAlpha> ();
			if (ta != null) {
				ta.PlayForward ();
			} else {
				LoadingBar.SetActive (true);
			}
			vp_Timer.In(1f, delegate() {  UnityEngine.SceneManagement.SceneManager.LoadScene (sceneNames [level]);});
		}
    }
}
