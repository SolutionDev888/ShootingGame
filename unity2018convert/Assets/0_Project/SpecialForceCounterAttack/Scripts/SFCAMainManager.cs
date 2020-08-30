using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFCAMainManager : MainManagerBase
{
    public override void OnClickLevelBtn(int level)
    {
        base.OnClickLevelBtn(level);
    }
    
    public void OnBtnExit()
    {
        Debug.Log("Quit Game!");
        Application.Quit();
    }

}
