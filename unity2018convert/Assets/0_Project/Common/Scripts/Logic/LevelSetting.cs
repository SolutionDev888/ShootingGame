using UnityEngine;
using System.Collections;

public class LevelSetting : MonoBehaviour {
    [System.Serializable]
    public class LevelSetInfo
    {
        

        public Material skybox;
        public Color ambientColor;
        public Transform playerStartPos;
		public GameObject briefingUI;
        public int enemyCount =0;
        public float time = 300f;
		public float maxHealth = 1f;
        public float canHitBulletCount=-1f;
        public GameObject[] levelObjs;
		public string[] weapons;
        public StepLevelObjectArray[] steps;
        private int stepIndex = 0;
        public void CheckSteps()
        {
            if (stepIndex == steps.Length - 1) return;
            for (int j = 0; j < steps[stepIndex].objs.Length; j++)
            {
                if (steps[stepIndex].objs[j] != null) return;
            }
            stepIndex++;
            for (int j = 0; j < steps[stepIndex].objs.Length; j++)
            {
                if (steps[stepIndex].objs[j] != null) steps[stepIndex].objs[j].SetActive(true);
            }
        }
        public void Set(bool flag)
        {

            //foreach (GameObject child in levelObjs)
            //{
            //    if (child != null) child.SetActive(flag);
            //    if (flag == false)
            //    {
            //        if (child != null) child.SetActive(flag);
            //        GameObject.Destroy(child);
            //    }
            //}
            stepIndex = 0;
            if (flag)
            {
                foreach (GameObject child in levelObjs)
                {
                    child.SetActive(true);
                }
                if (steps != null && steps.Length > 0)
                {
                    foreach (GameObject child in steps[0].objs)
                    {
                        child.SetActive(true);
                    }
                }
                RenderSettings.skybox = skybox;
                RenderSettings.ambientLight = ambientColor;
                LevelSetting.ME.player.gameObject.SetActive(true);
                LevelSetting.ME.player.GetComponent<vp_FPController>().SetPosition(playerStartPos.position);
                LevelSetting.ME.player.GetComponentInChildren<vp_FPCamera>().SetRotation(playerStartPos.rotation.eulerAngles);
				LevelSetting.ME.remainEnemyCount = enemyCount;
                LevelSetting.ME.time = time;
				vp_LocalPlayer.MaxHealth = maxHealth;
				vp_LocalPlayer.Health = vp_LocalPlayer.MaxHealth;
                Debug.Log("Player Health size:   " + vp_LocalPlayer.Health.ToString());
				if(briefingUI!=null){
					briefingUI.SetActive (true);
					foreach(UISprite child in briefingUI.GetComponentsInChildren<UISprite>()){
						if(child.spriteName=="Light")child.depth = 2;
						else if(child.spriteName=="M")child.depth = 1;
						else if(child.spriteName=="M_1")child.depth = 3;
						else if(child.spriteName=="Highlight")child.depth = 4;
					}
					foreach(UILabel child in briefingUI.GetComponentsInChildren<UILabel>()){
						child.depth = 5;
					}
				}
			}
        }

    }







	static public bool isSound{
		get{
			return PlayerPrefs.GetInt ("Sound",1)==1;
		}
		set{
			PlayerPrefs.SetInt ("Sound",value?1:0);
			if (value == false) {
                //AudioListener.volume = 0f;
                GameObject.Find("Canvas").GetComponent<AudioSource>().mute = true;
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SFCA_GamePlay")
                {
                    GameObject.Find("AdvancedPlayerMobileNGUI").GetComponent<AudioSource>().mute = true;
                }

            } else {
                //AudioListener.volume = 1f;
                GameObject.Find("Canvas").GetComponent<AudioSource>().mute = false;
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SFCA_GamePlay")
                {
                    GameObject.Find("AdvancedPlayerMobileNGUI").GetComponent<AudioSource>().mute = false;
                }
            }
		}
	}

    static public bool isMusic
    {
        get
        {
            return PlayerPrefs.GetInt("Music", 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("Music", value ? 1 : 0);
            if (value == false)
            {
                //AudioListener.volume = 0f;
                GameObject.Find("backgroundMusic").GetComponent<AudioSource>().mute = true;
                if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SFCA_GamePlay")
                {
                    GameObject.Find("LevelSuccessSound").GetComponent<AudioSource>().mute = true;
                    GameObject.Find("LevelFailedSound").GetComponent<AudioSource>().mute = true;
                }
                
            }
            else
            {
                //AudioListener.volume = 1f;
                GameObject.Find("backgroundMusic").GetComponent<AudioSource>().mute = false;
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SFCA_GamePlay")
                {
                    GameObject.Find("LevelSuccessSound").GetComponent<AudioSource>().mute = false;
                    GameObject.Find("LevelFailedSound").GetComponent<AudioSource>().mute = false;
                }
                    
            }
        }
    }

    static public bool isStartedFight;
    static public bool _isBriefing = false;
    public enum StartState
    {
        MAINMENU,
        LEVELSELECT,
    }

    public static StartState curState = StartState.MAINMENU;

    static public bool isBriefing
    {
        get
        {
            return _isBriefing;
        }
        set
        {
			_isBriefing = value;
            if (LevelSetting.ME.briefingUI != null) LevelSetting.ME.briefingUI.SetActive(_isBriefing);
            if (LevelSetting.ME!=null && LevelSetting.ME.briefingPath != null) {
                if (_isBriefing)
                {
					RadarSystem.RadarSystem.Show = false;
                    LevelSetting.ME.briefingPath.Play();
					LevelSetting.ME.remainTimeUI.transform.parent.parent.parent.gameObject.SetActive (false);
                }
                else
                {
					RadarSystem.RadarSystem.Show = true;
                    LevelSetting.ME.briefingPath.Stop();
					LevelSetting.ME.remainTimeUI.transform.parent.parent.parent.gameObject.SetActive (true);
                }
                LevelSetting.ME.briefingPath.animationTarget.gameObject.SetActive(_isBriefing);
                LevelSetting.ME.briefingPath.gameObject.SetActive(_isBriefing);
            }
        }
    }
	public LayerMask enemybulletMask;
	public int testLevel = 1;
	public GameObject[] weaponImages;
    public GameObject briefingUI;
    [HideInInspector]
    public CameraPathBezierAnimator briefingPath;
    public UILabel remainEnemyUI;
    public UILabel remainTimeUI;
	public TweenAlpha ui_zoom;
	public GameObject ui_zoomHandle;
	public TweenAlpha ui_crossHair;
	public UILabel ui_zoomRate;
	public int[] zoomValues= new int[]{60,40,25};
	public int m_currentZoomIndex;
	public int currentZoomIndex{
		get{
			return m_currentZoomIndex;
		}
		set{
			m_currentZoomIndex = value;
			if (m_currentZoomIndex == 0) {
				ui_crossHair.PlayForward ();
			} else {
				ui_crossHair.PlayReverse();
			}
		}
	}

    public int currentlevel
    {
        get
        {
            return MainManagerBase.curreLevel;
        }
        set
        {
            MainManagerBase.curreLevel = value;
        }
        
    }
    public int weaponNumber = 0;
	public GameObject ui_lastLevelSuccess;
	public GameObject ui_LevelSuccess;
	public GameObject ui_LevelFailuer;
	public GameObject ui_Pause;
	public GameObject ui_Background;
    private int _remainEnemyCount;
    public int remainEnemyCount
    {
        get
        {
            return _remainEnemyCount;
        }
        set
        {
            _remainEnemyCount = value;
            if (remainEnemyUI != null) remainEnemyUI.text = "" + value;
            if (_remainEnemyCount == 0)
            {
                this.Invoke("OnSuccess", 2f);
            }
        }
    }
    private float _time;
    public float time
    {
        get
        {
            return _time;
        }
        set
        {
            _time = value;
            if (remainTimeUI != null)
            {
                remainTimeUI.text = "" + ((int)_time);
            }
        }
    }
    public Transform player;
    public float playerHealth
    {
        get
        {
			return vp_LocalPlayer.EventHandler.Health.Get() / vp_LocalPlayer.EventHandler.MaxHealth.Get();
        }
        set
        {
			vp_LocalPlayer.EventHandler.Health.Set(value* vp_LocalPlayer.EventHandler.MaxHealth.Get());
        }
    }
    
    public static LevelSetting ME;
    [System.Serializable]
    public class StepLevelObjectArray
    {
        public GameObject[] objs;
    }
 
    public LevelSetInfo[] levelSetInfos;
    public LevelSetInfo curLevel
    {
        get
        {
            return levelSetInfos[currentlevel];
        }
    }
    void Awake()
    {
        ME = this;
        isBriefing = false;
        Time.timeScale = 1f;
		isStartedFight = false;
		if (testLevel >= 0) {
			currentlevel = testLevel;
		}
		RadarSystem.RadarSystem.Show = false;
    }

    public bool isPause
    {
        get
        {
			if (Time.timeScale <= 0.0001f)
				return true;
			else
				return false;
        }
        set
        {
            if (value)
            {
				vp_LocalPlayer.DisableGameplayInput();
				Time.timeScale = 0.0001f;
				remainTimeUI.transform.parent.parent.parent.gameObject.SetActive (false);
				RadarSystem.RadarSystem.Show = false;
            }
            else
            {
				vp_LocalPlayer.EnableGameplayInput();
				Time.timeScale = 1f;
				remainTimeUI.transform.parent.parent.parent.gameObject.SetActive (true);
				RadarSystem.RadarSystem.Show = true;
            }
            //peh.InputAllowGameplay.Set(!value);
        }
    }
    private void Update()
    {
        curLevel.CheckSteps();
		if (isBriefing==false && time > 0f && RadarSystem.RadarSystem.Show==true)
        {
			time -= Time.deltaTime;
            if (time <= 0f)
            {
                OnPlayerDie();
            }
        }

    }
    private void Start()
    {
        for(int i=0;i< levelSetInfos.Length; i++)
        {
            levelSetInfos[i].Set(i == currentlevel);
        }
        AstarPath.active.Scan();
        System.GC.Collect(0, System.GCCollectionMode.Optimized);
        Resources.UnloadUnusedAssets();
		RefreshWeapon ();
    }
    public void OnPlayerDie()
    {
		OnFailure ();
    }
	vp_Timer.Handle timerHandle = new vp_Timer.Handle();
    public void OnBtnClick_Replay()
    {
		if (timerHandle.Active)
			return;
        Time.timeScale = 1f;
		ui_Background.GetComponent<TweenAlpha>().PlayForward();
		vp_Timer.In(0.1f, delegate() {  UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetSceneAt(0).name);}, timerHandle);
       
    }
    public void OnBtnClick_Menu()
    {
		if (timerHandle.Active)
			return;
        Time.timeScale = 1f;
		ui_Background.GetComponent<TweenAlpha>().PlayForward();
        //        if (LevelSetting.ME.briefingUI != null) LevelSetting.ME.briefingUI.SetActive(true);
        //        if (LevelSetting.ME != null && LevelSetting.ME.briefingPath != null)
        //        {
        //            LevelSetting.ME.briefingPath.animationTarget.gameObject.SetActive(true);
        //            LevelSetting.ME.briefingPath.animationTarget.SendMessage("setI", 2000f, SendMessageOptions.DontRequireReceiver);
        //            LevelSetting.ME.briefingPath.animationTarget.SendMessage("setC", 2000f, SendMessageOptions.DontRequireReceiver);
        //        }

        LevelSetting.curState = StartState.MAINMENU;
		vp_Timer.In(0.1f, delegate() {  UnityEngine.SceneManagement.SceneManager.LoadScene(MainManagerBase.MainSceneName);}, timerHandle);
        
    }
    public void OnBtnClick_Next()
    {
		if (timerHandle.Active)
			return;
        currentlevel++;
        if (currentlevel >= levelSetInfos.Length)
        {
            currentlevel= levelSetInfos.Length-1;
        }
		ui_Background.GetComponent<TweenAlpha>().PlayForward();
        Time.timeScale = 1f;

        LevelSetting.curState = StartState.LEVELSELECT;
        //vp_Timer.In(0.1f, delegate() {  UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetSceneAt(0).name);}, timerHandle);
        vp_Timer.In(0.1f, delegate () { UnityEngine.SceneManagement.SceneManager.LoadScene(MainManagerBase.MainSceneName); }, timerHandle);

    }
    public void OnSuccess()
    {

		MainManagerBase.clearLevel (currentlevel);
        GameObject.Find("backgroundMusic").GetComponent<AudioSource>().Stop();
        GameObject.Find("backgroundMusic").GetComponent<AudioSource>().loop = false;
        GameObject.Find("LevelSuccessSound").GetComponent<AudioSource>().Play();

        RadarSystem.RadarSystem.Show = false;
		vp_Timer.In(1f, delegate() { 
			isPause = true;
			if (currentlevel == levelSetInfos.Length - 1)
			{
				ui_lastLevelSuccess.GetComponent<TweenAlpha> ().PlayForward ();
			}
			else
			{
				ui_LevelSuccess.GetComponent<TweenAlpha> ().PlayForward ();
			}

		}, timerHandle);
    }
    public void OnFailure()
    {
		RadarSystem.RadarSystem.Show = false;
        GameObject.Find("backgroundMusic").GetComponent<AudioSource>().Stop();
        GameObject.Find("backgroundMusic").GetComponent<AudioSource>().loop = false;
        GameObject.Find("LevelFailedSound").GetComponent<AudioSource>().Play();
        vp_Timer.In(0.5f, delegate() {  isPause = true;ui_LevelFailuer.GetComponent<TweenAlpha> ().PlayForward ();}, timerHandle);
    }
    public void OnBriefingFinished()
    {
        isBriefing = false;
        playerHealth = 1.0f;
		RadarSystem.RadarSystem.Show = true;
        GameObject.Find("backgroundMusic").GetComponent<AudioSource>().Play();
        GameObject.Find("backgroundMusic").GetComponent<AudioSource>().loop = true;
    }
    public void OnShooted()
    {
        if (playerHealth <= 0f) return;
        if (curLevel.canHitBulletCount>0f)
            playerHealth -= 1f / curLevel.canHitBulletCount;
        if (playerHealth <= 0f) OnPlayerDie();
    }
    public void OnResume()
    {
		if (isPause) {
			RadarSystem.RadarSystem.Show = false;
			ui_Pause.GetComponent<TweenAlpha> ().PlayReverse ();
		} else {
			ui_Pause.GetComponent<TweenAlpha> ().PlayForward ();
			RadarSystem.RadarSystem.Show = true;
		}
        isPause = !isPause;
    }
	public void OnWeaponChange(){
		weaponNumber = (weaponNumber + 1);
		RefreshWeapon ();
	}
	private void RefreshWeapon(){
		weaponNumber = (weaponNumber) % curLevel.weapons.Length;
		vp_LocalPlayer.SetWeaponByName(curLevel.weapons[ weaponNumber]); 
		foreach (GameObject child in weaponImages) {
			child.SetActive(child.name == curLevel.weapons [(weaponNumber+1)%curLevel.weapons.Length]);
		}

		if (curLevel.weapons [weaponNumber].StartsWith ("6")) {
			ui_zoomHandle.SetActive (true);
		} else {
			ui_zoom.PlayReverse ();
			ui_zoomHandle.SetActive (false);
			currentZoomIndex = 0;
		}
	}
	public void OnTweenFinish(){
		
	}
    public void Test()
    {
        isPause = !isPause;
    }
}
