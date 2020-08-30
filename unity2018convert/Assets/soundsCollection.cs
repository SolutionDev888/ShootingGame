using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class soundsCollection : MonoBehaviour {


    public AudioSource[] audios;
    // Use this for initialization


    private void Awake()
    {
        if (audios.Length == 0)
            return;
        else
        {
            for (int i = 0; i < audios.Length; i++)
            {
                if (LevelSetting.isSound)
                    audios[i].mute = false;
                else
                    audios[i].mute = true;
            }
        }
    }
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
