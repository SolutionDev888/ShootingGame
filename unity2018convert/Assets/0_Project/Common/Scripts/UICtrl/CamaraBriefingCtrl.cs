using UnityEngine;
using System.Collections;

public class CamaraBriefingCtrl : MonoBehaviour {
    // Use this for initialization
    void Start () {

        LevelSetting.ME.briefingPath = GetComponent<CameraPathBezierAnimator>();
        
        if (LevelSetting.ME.briefingPath != null)
        {
            LevelSetting.ME.briefingPath.AnimationFinished += LevelSetting.ME.OnBriefingFinished;
            LevelSetting.isBriefing = true;
        }
    }
}
