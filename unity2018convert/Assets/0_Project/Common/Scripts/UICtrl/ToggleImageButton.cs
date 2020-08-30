using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleImageButton : MonoBehaviour {
    public UnityEngine.UI.Image target;
    public bool _Value;
    public bool Value
    {
        get
        {
            return _Value;
        }
        set
        {
            _Value = value;
            target.sprite = _Value == true?spriteTrue: spriteFalse;
        }
    }

    public UnityEngine.Sprite spriteTrue;
    public UnityEngine.Sprite spriteFalse;
    public void OnValueChange()
    {
        Value = !Value;
		if (target.gameObject.name.EndsWith ("Sound")) {
			LevelSetting.isSound = Value;
		}
        else if (target.gameObject.name.EndsWith("Music"))
        {
            LevelSetting.isMusic = Value;
        }
    }
	private void Start(){
		if (target.gameObject.name.EndsWith ("Sound")) {
			Value = LevelSetting.isSound ;
		}
        else if (target.gameObject.name.EndsWith("Music"))
        {
            Value = LevelSetting.isMusic;
        }
    }
}
