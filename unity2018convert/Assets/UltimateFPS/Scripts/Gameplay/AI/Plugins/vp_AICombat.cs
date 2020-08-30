/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AICombat.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class allows the AI to have ranged and melee combat
//					capabilities
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[System.Serializable]
public class vp_AICombat : vp_AIPlugin
{

	public override string Title{ get{ return "Combat"; } }
	public override int SortOrder{ get{ return 500; } }
	
	protected int m_CurrentAmmoCount = 0;		// current amount of ammo for ranged attacks
	protected AttackType m_NextAttackType = AttackType.None; // the attack type chosen for the next attack
	protected bool m_PlayerDead = false;
	protected vp_Timer.Handle m_ReloadTimer = new vp_Timer.Handle(); // timer for reloading
	protected vp_Timer.Handle m_MeleeAttackTimer = new vp_Timer.Handle(); // timer that coincides with MeleeAttackAgainTime
	protected Dictionary<GameObject, bool> m_ValidTargets = new Dictionary<GameObject, bool>();
	
	// type of attacks
	public enum AttackType{
		None,
		Melee,
		Ranged
	}
	
	
	/// <summary>
	/// NOTE: to provide the 'Init' functionality, this method
	/// must be called using 'base.Update();' on the first line
	/// of the 'Update' method in the derived class
	/// </summary>
	public override void Update()
	{
		
		base.Update();
		
		GetNextAttackTypeUpdate();
			
		AttackUpdate();
		
		DoDamageUpdate();
		
		CheckPlayerUpdate();
		
	}
		
	
	/// <summary>
	/// this manages whether the AI should attack
	/// </summary>
	protected virtual void AttackUpdate()
	{
	
		if(!m_EventHandler.Attacking.Active)
			return;
			
		if(m_EventHandler.Attack.Active)
			return;
			
		if(m_EventHandler.Alerted.Active)
			return;
			
		if(m_NextAttackType == AttackType.None)
			return;
			
		if(m_EventHandler.HostileTarget.Get() == null)
			return;
			
		float attackDistance = m_NextAttackType == AttackType.Melee ? m_AI.CombatMeleeAttackDistance.x : m_AI.CombatRangedAttackDistance.x;
			
		if(Vector3.Distance( m_Transform.position, m_EventHandler.HostileTarget.Get().position ) > attackDistance)
			return;
	
		m_EventHandler.Attack.TryStart();
	
	}
	
	
	/// <summary>
	/// decision process for choosing what type
	/// of attack to perform next
	/// </summary>
	protected virtual void GetNextAttackTypeUpdate()
	{
	
		if(!m_EventHandler.Attacking.Active)
			return;
			
		if((m_AI.CombatAllowMeleeAttacks && Vector3.Distance(m_Transform.position, ((Transform)m_EventHandler.Attacking.Argument).position) <= m_AI.CombatOverrideRangedDistance) && m_NextAttackType != AttackType.Melee)
		{
			m_EventHandler.Attack.Stop();
			m_NextAttackType = AttackType.Melee;
		}
		
		if(m_EventHandler.Attack.Active)
			return;
			
		if(m_NextAttackType != AttackType.None)
			return;
			
		if(!m_AI.CombatAllowMeleeAttacks && !m_AI.CombatAllowRangedAttacks)
			return;
			
		if(m_AI.CombatAllowMeleeAttacks && !m_AI.CombatAllowRangedAttacks)
			m_NextAttackType = AttackType.Melee;
		else if(!m_AI.CombatAllowMeleeAttacks && m_AI.CombatAllowRangedAttacks)
			m_NextAttackType = AttackType.Ranged;
		else
			m_NextAttackType = Random.Range(0, 101) <= m_AI.CombatMeleeProbability ? AttackType.Melee : AttackType.Ranged;
	
	}
	
	
	/// <summary>
	/// looks for a player and if the player is dead
	/// mark it as so and stop attacking
	/// </summary>
	protected virtual void CheckPlayerUpdate ()
	{
		
		if(m_PlayerDead)
			return;
		
		if(!m_EventHandler.Attacking.Active)
			return;
			
		if(m_EventHandler.HostileTarget.Get() == null)
			return;
			
		vp_FPPlayerEventHandler player = vp_AIManager.GetPlayer(m_EventHandler, m_EventHandler.HostileTarget.Get());
		if(player == null)
			return;
			
		if(player.Dead.Active)
		{
			m_PlayerDead = true;
			m_EventHandler.Reload.TryStart();
			m_EventHandler.Attacking.Stop();
		}
		
	}
	
	
	/// <summary>
	/// Determines whether an attack can start or not
	/// </summary>
	protected virtual bool CanStart_Attack()
	{
	
		List<vp_Activity> activities = new List<vp_Activity>(){ m_EventHandler.Dead, m_EventHandler.Retreat, m_EventHandler.Damaged, m_EventHandler.Attack, m_EventHandler.Reload, m_EventHandler.Alerted };
		foreach(vp_Activity activity in activities)
			if(activity.Active)
				return false;
		
		return true;
		
	}
	
	
	/// <summary>
	/// stop the enemy from moving when an attack starts
	/// </summary>
	protected virtual void OnStart_Attack()
	{
		
		m_EventHandler.Animate.Send(m_AI.CombatIdleInCombatState);
		m_EventHandler.Walk.Stop();
		m_EventHandler.Run.Stop();
		
	}
	
	
	/// <summary>
	/// set the attack type to none if an attack ends
	/// to allow for a new attack type to be chosen
	/// </summary>
	protected virtual void OnStop_Attack()
	{
	
		m_NextAttackType = AttackType.None;
	
	}
	
	
	/// <summary>
	/// determines whether damage should be applied
	/// to the current target and handles the type of attack.
	/// A message will get sent to a listener (vp_Shooter or derived class)
	/// to deal damage to the target
	/// </summary>
	protected virtual void DoDamageUpdate()
	{
	
		if(!m_EventHandler.Attacking.Active)
			return;
			
		if(m_EventHandler.Damaged.Active)
			return;
	
		if(m_EventHandler.HostileTarget.Get() == null)
			return;
			
		if(m_NextAttackType == AttackType.None)
			return;
			
		if(m_MeleeAttackTimer.Active)
			return;
			
		// determine the max attack distance of the current attack type
		float stopAttackDistance = m_NextAttackType == AttackType.Melee ? m_AI.CombatMeleeAttackDistance.y : m_AI.CombatRangedAttackDistance.y;
		
		// stop an attack if target distance is past threshold
		if( Vector3.Distance( m_Transform.position, m_EventHandler.HostileTarget.Get().position ) > stopAttackDistance )
		{
			m_EventHandler.Attack.Stop();
			m_EventHandler.Run.Start();
			return;
		}
		
		if (!m_EventHandler.Attack.Active)
			return;
		
		// play the attack animation according to the current attack type
		m_EventHandler.Animate.Send(m_NextAttackType == AttackType.Melee ? m_AI.CombatMeleeAttackState : m_AI.CombatRangedAttackState);
		
		// determine how much damage to apply
		float minDamage = m_NextAttackType == AttackType.Melee ? m_AI.CombatMeleeAttackDamage.x : m_AI.CombatRangedAttackDamage.x;
		float maxDamage = m_NextAttackType == AttackType.Melee ? m_AI.CombatMeleeAttackDamage.y : m_AI.CombatRangedAttackDamage.y;
		float damage = (float)Random.Range(minDamage, maxDamage);
		
		// determine the max range for the current attack type
		float range = m_NextAttackType == AttackType.Melee ? m_AI.CombatMeleeAttackDistance.y : m_AI.CombatRangedAttackDistance.y;
		
		// set the dictionary properties up to send
		Dictionary<string, object> data = new Dictionary<string, object>(){ { "AttackType", (int)m_NextAttackType }, { "Damage", (float)damage  }, { "Range", (float)range } };
		
		// determine the length for when an actual hit on the target should occur
		float hitTime = m_NextAttackType == AttackType.Melee ? 
							m_AI.CombatMeleeAttackState.LastAnimationClipLength * 0.5f :
							m_AI.CombatRangedAttackState.LastAnimationClipLength;
		
		vp_Timer.In(hitTime, delegate{
			m_EventHandler.CauseDamage.Send(data);
			m_EventHandler.AlertFriendly.Send();
		});
		
		// if a ranged attack, we're all done here
		if(m_NextAttackType == AttackType.Ranged)
			return;
			
		// when a melee attack is finished, play an idle animation while waiting for next attack
		vp_Timer.In(m_AI.CombatMeleeAttackState.LastAnimationClipLength, delegate {
			m_EventHandler.Animate.Send(m_AI.CombatIdleInCombatState);
		});
	
		// stop attacking and start chasing when a melee attack is allowed again
		vp_Timer.In(m_AI.CombatMeleeAttackAgainTime, delegate() {
			m_EventHandler.Attack.Stop();
			m_EventHandler.Run.Start();
		}, m_MeleeAttackTimer);
		
	}
	
	
	/// <summary>
	/// Determines whether reload can start
	/// </summary>
	protected virtual bool CanStart_Reload()
	{
	
		if(m_EventHandler.Dead.Active)
			return false;
			
		if(m_NextAttackType == AttackType.Melee)
			return false;
			
		if(m_EventHandler.Retreat.Active)
			return false;
	
		if(m_CurrentAmmoCount == m_AI.CombatBulletsPerClip)
			return false;
			
		return true;
	
	}
	
	
	/// <summary>
	/// stop attacking if reloading, reset
	/// the ammo count when reloading is finished
	/// and then start attacking again
	/// </summary>
	protected virtual void OnStart_Reload()
	{
	
		m_EventHandler.Attack.Stop();
		
		float reloadTime = m_AI.CombatReloadTime;
		
		if(!m_EventHandler.Animate.Send(m_AI.CombatReloadState))
			reloadTime = m_AI.CombatReloadState.LastAnimationClipLength;
			
		vp_Timer.In(reloadTime, delegate() {
			m_CurrentAmmoCount = m_AI.CombatBulletsPerClip;
			m_EventHandler.Reload.Stop();
			if(m_EventHandler.Attacking.Active)
				m_EventHandler.Attack.Start();
		}, m_ReloadTimer);
	
	}
	
	
	/// <summary>
	/// Determines whether attacking can start or not
	/// </summary>
	public virtual bool CanStart_Attacking()
	{
	
		if(m_EventHandler.Dead.Active)
			return false;
	
		Transform target = (Transform)m_EventHandler.Attacking.Argument;
		
		if(target == null)
		{
			Transform closestTarget = GetClosestHostile();
			if(closestTarget == null)
				return false;
			else
			{
				m_EventHandler.Attacking.TryStart(closestTarget);
				m_EventHandler.AlertFriendly.Send();
			}
		}
		else
			if(!m_EventHandler.IsValidTarget.Send(target.gameObject))
				return false;
			
		if(m_EventHandler.Retreat.Active)
			return false;
		
		return true;
		
	}
	
	
	/// <summary>
	/// Gets the closest hostile
	/// </summary>
	protected virtual Transform GetClosestHostile()
	{
		
		Collider[] aHostiles = Physics.OverlapSphere(m_Transform.position, m_AI.CombatHostileCheckDistance);
		List<Transform> hostiles = new List<Transform>();
		foreach(Collider col in aHostiles)
			if(m_EventHandler.IsValidTarget.Send(col.gameObject))
				hostiles.Add(col.transform);
			
		if(hostiles.Count == 0)
			return null;
			
		Vector3 pos = m_Transform.position;
		Transform target = hostiles.OrderBy(t => (pos - t.position).sqrMagnitude).First();
		
		return target;
		
	}
	
	
	/// <summary>
	/// check if the target is valid
	/// based on the hostiles tag and layer mask
	/// </summary>
	protected virtual bool OnMessage_IsValidTarget( GameObject go )
	{
	
		bool isValid = false;
		if(m_ValidTargets.TryGetValue(go, out isValid))
			return isValid;
	
		isValid = (m_AI.CombatHostileLayers.value & 1<<go.layer) != 0 || m_AI.CombatHostileTagsList.Contains(go.tag);
		m_ValidTargets.Add(go, isValid);
		
		return isValid;
		
	}
	
	
	/// <summary>
	/// the AI has a hostile target and will start to persue
	/// the target until close enough for an attack
	/// </summary>
	public void OnStart_Attacking()
	{
	
		m_EventHandler.Target.Set( (Transform)m_EventHandler.Attacking.Argument );
		m_EventHandler.HostileTarget.Set( (Transform)m_EventHandler.Attacking.Argument );
	
		if(!m_EventHandler.Alerted.TryStart())
			DoStartAttacking();
		
	}
	
	
	/// <summary>
	/// Starts attacking. Used because timers are set in
	/// multiple places so reducing code duplication.
	/// </summary>
	protected virtual void DoStartAttacking()
	{
	
		m_PlayerDead = false;
		m_EventHandler.Alerted.Stop();
		m_EventHandler.Roaming.Stop();
		m_EventHandler.Run.Start();
	
	}
	
	
	/// <summary>
	/// Checks if the alerted state can start
	/// </summary>
	protected virtual bool CanStart_Alerted()
	{
	
		if(m_EventHandler.Retreat.Active)
			return false;
	
		return true;
	
	}
	
	
	/// <summary>
	/// Starts the alerted activity
	/// </summary>
	protected virtual void OnStart_Alerted()
	{
	
		if(!m_EventHandler.Animate.Send(m_AI.CombatAlertedState))
		{
			DoStartAttacking();
			return;
		}
	
		vp_Timer.In(.1f, delegate{
			vp_Timer.In(m_AI.CombatAlertedState.LastAnimationClipLength, DoStartAttacking);
		});
	
	}
	
	
	/// <summary>
	/// Alerts friendlies in the specified distance
	/// that this AI is attacking
	/// </summary>
	protected virtual void OnMessage_AlertFriendly()
	{
		
		if(m_EventHandler.HostileTarget.Get() == null)
			return;
			
		Collider[] colliders = Physics.OverlapSphere(m_Transform.position, m_AI.CombatAlertFriendlyDistance, 1<<m_Transform.gameObject.layer);
	
		foreach(Collider col in colliders)
		{
			vp_AIEventHandler ai = col.GetComponent<vp_AIEventHandler>();
			if(ai != null)
				ai.Attacking.TryStart(m_EventHandler.HostileTarget.Get());
		}
	
	}
	
	
	/// <summary>
	/// send the AI to it's initial position
	/// when attacking ends
	/// </summary>
	protected virtual void OnStop_Attacking()
	{
	
		m_EventHandler.HostileTarget.Set(null);
		
		if(m_EventHandler.Reload.Active && !m_EventHandler.Retreat.Active)
		{
			vp_Timer.In(m_AI.CombatReloadState.LastAnimationClipLength, delegate {
				m_EventHandler.ReturnToLastWaypoint.Send();
			});
			return;
		}
		
		m_EventHandler.ReturnToLastWaypoint.Send();
	}
	
	
	/// <summary>
	/// if the AI is told to stop
	/// cancel timers for this class
	/// </summary>
	protected virtual void OnStart_Stop()
	{
	
		m_ReloadTimer.Cancel();
		m_MeleeAttackTimer.Cancel();
	
	}
	
	
	/// <summary>
	/// if attacking and within range (for pathfinding)
	/// attempt to perform an attack on the target
	/// </summary>
	protected virtual void OnMessage_TargetReached()
	{
	
		if(m_EventHandler.Attacking.Active && m_EventHandler.HostileTarget.Get())
			m_EventHandler.Attack.TryStart();
	
	}
	
	
	/// <summary>
	/// reset current ammo to the max ammo amount
	/// </summary>
	protected virtual void OnMessage_Reset(Vector3 newSpawnPosition)
	{
	
		m_CurrentAmmoCount = m_AI.CombatBulletsPerClip;
	
	}
	
	
	/// <summary>
	/// tries to remove one unit from ammo level of current weapon
	/// </summary>
	protected virtual bool OnAttempt_DepleteAmmo()
	{
	
		if(m_CurrentAmmoCount == 0)
			return false;
			
		if(m_NextAttackType != AttackType.Ranged)
			return false;

		// all ok: use one ammo
		m_CurrentAmmoCount--;

		return true;

	}
	
	
	/// <summary>
	/// receives a dictionary with properties and overrides
	/// those properties in this class if they exist
	/// </summary>
	protected virtual void OnMessage_SetProperties( Dictionary<string, object> properties )
	{
	
		foreach(string k in properties.Keys)
		{
			FieldInfo p = this.GetType().GetField(k);
			if(p != null)
				p.SetValue(this, properties[k]);
		}
		
	}
	
	
	/// <summary>
	/// AI's current ammo count
	/// </summary>
	protected virtual int OnValue_CurrentAmmoCount
	{
		get{ return m_CurrentAmmoCount; }
		set{ m_CurrentAmmoCount = value; }
	}
	
}
