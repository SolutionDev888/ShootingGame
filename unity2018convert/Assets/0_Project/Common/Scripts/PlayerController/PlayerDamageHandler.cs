using UnityEngine;
using System.Collections;

public class PlayerDamageHandler : MonoBehaviour {
    private float _health;
    public float health;
    public AudioClip[] hitSound;
    public AudioClip[] dieSound;
    public GameObject[] spawnObjects;
    private AudioSource _m_audioSource = null;
    private AudioSource m_audioSource
    {
        get
        {
            if (_m_audioSource == null)
            {
                _m_audioSource = GetComponentInParent<AudioSource>();
            }
            return _m_audioSource;
        }
    }

    public void Damage(float damage)
    {
        if (m_audioSource != null && hitSound!=null && hitSound.Length>0)
        {
            m_audioSource.PlayOneShot(hitSound[Random.Range(0, hitSound.Length * 2) % hitSound.Length]);
        }
        gameObject.SendMessage("OnMessage_HUDDamageFlash", new vp_DamageInfo(damage, null), SendMessageOptions.DontRequireReceiver);
    }
    public void Die()
    {
        if (m_audioSource != null && hitSound != null && hitSound.Length > 0)
        {
            m_audioSource.PlayOneShot(dieSound[Random.Range(0, hitSound.Length * 2) % hitSound.Length]);
        }
        if(spawnObjects!=null && spawnObjects.Length > 0)
        {
            foreach(GameObject child in spawnObjects)
            {
                GameObject.Instantiate(child, transform.position, transform.rotation);
            }
        }
    }
    private void Awake()
    {
        _health = health;
        if (LevelSetting.isSound)
            m_audioSource.mute = false;
        else
            m_audioSource.mute = true;
    }
}
