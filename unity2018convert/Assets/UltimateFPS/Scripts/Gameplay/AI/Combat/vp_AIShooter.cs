/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIShooter.cs
//	Â© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this component can be added to any gameobject, giving it the capability
//					of firing projectiles. it handles firing rate, projectile spawning,
//					muzzle flashes, shell casings and shooting sound. call the 'TryFire'
//					method to fire the shooter (whether this succeeds is determined by
//					firing rate)
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_AIShooter : vp_Shooter {

	public vp_AICombat.AttackType AttackType = vp_AICombat.AttackType.None;	// The type of attack this shooter will be able to perform
	
	protected vp_AIEventHandler m_AI = null;
	protected float m_Damage = 0;
	protected float m_Range = 0;
	
	/// <summary>
	/// this message sets up the component for damage
	/// and range and executes the TryFire event
	/// </summary>
	protected virtual void OnMessage_CauseDamage( Dictionary<string, object> data )
	{
	
		int attackType = (int)data["AttackType"];
		
		if(attackType != (int)AttackType)
			return;
			
		if(m_AI == null)
			m_AI = (vp_AIEventHandler)EventHandler;
			
		if(m_AI.Target.Get() == null)
			return;
			
		m_Damage = (float)data["Damage"];
		m_Range = (float)data["Range"];
	
		Vector3 pos = (m_AI.Target.Get().GetComponent<Collider>() != null ? m_AI.Target.Get().GetComponent<Collider>().bounds.center : m_AI.Target.Get().position) - m_Transform.position;
		m_Transform.rotation = Quaternion.LookRotation(pos);
		TryFire();
		
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

        if (!m_AI.DepleteAmmo.Try() && AttackType != vp_AICombat.AttackType.Melee)
		{
			m_AI.Reload.TryStart();
			return true;
        }

		Fire();
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
				p.transform.localScale = new Vector3(ProjectileScale, ProjectileScale, ProjectileScale);	// preset defined scale
				vp_AIHitscanBullet bullet = p.GetComponent<vp_AIHitscanBullet>();
				if(bullet != null)
				{
					bullet.Damage = m_Damage;
					bullet.Range = m_Range;
				}

				// apply conical spread as defined in preset
				p.transform.Rotate(0, 0, Random.Range(0, 360));										// first, rotate up to 360 degrees around z for circular spread
				p.transform.Rotate(0, Random.Range(-ProjectileSpread, ProjectileSpread), 0);		// then rotate around y with user defined deviation
			}
		}
	
	}
	
}
