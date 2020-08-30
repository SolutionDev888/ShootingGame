using UnityEngine;
using System.Collections;

public class LevelLoadDelayTime : MonoBehaviour {
    public string levelName = "MainMenu";
    public float delayTime = 2f;
    public bool isStart = true;
	// Use this for initialization
	void Start () {
        if(isStart) DoLoadLevel();
    }
    public void DoLoadLevel()
    {
        StartCoroutine(_DoLoadLevel());
    }
    IEnumerator _DoLoadLevel()
    {
        yield return new WaitForSecondsRealtime(delayTime);
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }
}
