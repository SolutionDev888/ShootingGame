using UnityEngine;
using System.Collections;

public class DamageHandlerBase : MonoBehaviour {
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
                if (_m_audioSource == null)
                {
                    _m_audioSource = Camera.main.GetComponent<AudioSource>();
                }
            }
            return _m_audioSource;
        }
    }

    virtual public void Damage(float damage)
    {
        if (health <= 0f) return;
        health -= damage;
        if (health > 0f)
        {
            if (m_audioSource != null && hitSound != null && hitSound.Length > 0)
            {
                m_audioSource.PlayOneShot(hitSound[Random.Range(0, hitSound.Length * 2) % hitSound.Length]);
            }
        }
        else
        {
            Die();
        }
    }
    virtual public void Die()
    {
        if (m_audioSource != null && hitSound != null && hitSound.Length > 0)
        {
            m_audioSource.PlayOneShot(dieSound[Random.Range(0, hitSound.Length * 2) % hitSound.Length]);
        }
        if (spawnObjects != null && spawnObjects.Length > 0)
        {
            foreach (GameObject child in spawnObjects)
            {
                GameObject.Instantiate(child, transform.position, transform.rotation);
            }
        }
        GameObject.Destroy(gameObject);
    }
    private void Awake()
    {
        _health = health;
        //if (LevelSetting.isSound)
        //    m_audioSource.mute = false;
        //else
        //    m_audioSource.mute = true;
    }
}
