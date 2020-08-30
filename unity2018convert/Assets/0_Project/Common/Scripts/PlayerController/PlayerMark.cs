using UnityEngine;
using System.Collections;

public class PlayerMark : MonoBehaviour {
    static public PlayerMark instance;
    public Transform m_Trans;
    private void Awake()
    {
        m_Trans = this.GetComponent<Transform>();
        instance = this;
    }
}
