using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayMngBase : MonoBehaviour {

    // Use this for initialization
    void Start()
    {
        start();
    }

    // Update is called once per frame
    void Update()
    {
        update();
    }
    virtual public void start()
    {

    }
    virtual public void update()
    {

    }
}
