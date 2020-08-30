using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Shooter : vp_Shooter
{
    public int currentAmmo = 20;
    public int maxAmmo = 20;
    public int clipSize = 20;
    public float throwForce = 100f;
    protected vp_AIEventHandler m_AI = null;
    public float m_Damage = 0;
    public  float m_Range = 0;

    private Transform _m_Trans = null;
    public Transform m_Trans
    {
        get
        {
            if (_m_Trans == null) _m_Trans = GetComponent<Transform>();
            return _m_Trans;
        }
    }
    protected override void Awake()
    {
        base.Awake();
    }
	protected override void Start ()
	{
		base.Start ();
	}
    private AI_Move _root;
    protected AI_Move root
    {
        get
        {
            if (_root == null) _root = GetComponentInParent<AI_Move>();
            return _root;
        }
    }
    public virtual void weaponThrow()
    {
        Rigidbody _rigidbody = this.GetComponent<Rigidbody>();
        if (_rigidbody == null) return;
        this.GetComponent<Transform>().SetParent(null);
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        if (throwForce > 0)
        {
            _rigidbody.AddForce(Random.onUnitSphere * throwForce);
        }
        GameObject.Destroy(this.gameObject,5f);
        //vp_Timer.In(5f, delegate () {
        //    vp_Utility.Destroy(this.gameObject);
        //});
        this.enabled = false;
        
    }
    /// <summary>
    /// calls the fire method if the firing rate of this shooter
    /// allows it. override this method to add further rules
    /// </summary>
    public override bool TryFire()
    {

        // return if we can't fire yet
        if (Time.time < m_NextAllowedFireTime)
            return true;
        if (currentAmmo==0)
        {
            root.TryReload();
            return true;
        }
		m_ProjectileSpawnPoint.transform.LookAt (PlayerMark.instance.m_Trans);
        Fire();
        currentAmmo--;
        return true;
    }
    /// <summary>
    /// spawns one or more projectiles in a customizable conical
    /// pattern. NOTE: this does not send the projectiles flying.
    /// the spawned gameobjects need to have their own movement
    /// logic
    /// </summary>
    protected override void SpawnProjectiles()
    {
	    for (int v = 0; v < ProjectileCount; v++)
        {
            if (ProjectilePrefab != null)
            {
                GameObject p = null;
                p = (GameObject)Object.Instantiate(ProjectilePrefab, m_ProjectileSpawnPoint.transform.position, m_ProjectileSpawnPoint.transform.rotation);
                p.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, ProjectileScale);    // preset defined scale
                vp_FXBullet bullet = p.GetComponent<vp_FXBullet>();
                if (bullet != null)
                {
                    bullet.Damage = m_Damage;
                    bullet.Range = m_Range;
                }
				p.SendMessage("SetSource", (ProjectileSourceIsRoot ? Root : Transform), SendMessageOptions.DontRequireReceiver);
                // apply conical spread as defined in preset
                p.transform.Rotate(0, 0, Random.Range(0, 360));                                     // first, rotate up to 360 degrees around z for circular spread
                p.transform.Rotate(0, Random.Range(-ProjectileSpread, ProjectileSpread), 0);        // then rotate around y with user defined deviation
            }
        }

    }

}
