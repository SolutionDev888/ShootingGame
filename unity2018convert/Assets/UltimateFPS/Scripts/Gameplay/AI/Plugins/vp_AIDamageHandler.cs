/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIDamageHanlder.cs
//	Â© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class handles damage for AI and also death (loot drops)
//					and respawning
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[System.Serializable]
public class vp_AIDamageHandler : vp_AIPlugin
{

	public override string Title{ get{ return "Vitals, Spawning and Death"; } }
	public override int SortOrder{ get{ return 400; } }

	/// <summary>
	/// a class to hold properties for a death spawn object
	/// </summary>
	[System.Serializable]
	public class DeathSpawnObject
	{
	
		public bool Foldout = true;
		public GameObject Prefab = null;
		public float Probability = .15f;
	
	}

	public float CurrentHealth = 0.0f;				// current health of the object instance

	// health and death
	public List<DeathSpawnObject> DeathSpawnObjects = new List<DeathSpawnObject>();	// gameobjects to spawn when object dies.
															// TIP: could be fx, could 
	
	[System.NonSerialized]
	public Bounds AreaSpawnerBounds = new Bounds();			// cached spawner bounds if vp_AIAreaSpawner is used
	[System.NonSerialized]
	public bool RandomRespawnPosition = false;				// should the AI respawn in a random location within the area spawner bounds
	
#if UNITY_EDITOR
	// Editor Properties
	[HideInInspector]
	public bool DeathSpawnObjectsFoldout;
	[HideInInspector]
	public float MaxAllowedHealth = 100;
#endif
	
	protected Vector3 m_StartPosition;						// initial position detected and used for respawn
	protected Quaternion m_StartRotation;					// initial rotation detected and used for respawn
	protected vp_FPPlayerEventHandler m_Player = null;		// cached player event handler
	protected vp_Timer.Handle m_GetHitEndTimer = new vp_Timer.Handle();	// timer used for end of hit animation
	protected GameObject m_CachedRagdoll = null;
	protected float m_MaxHealth = 1.0f;						// initial health of the object instance, to be reset on respawn
	protected float m_TakeDamageAgainTime = 0;


	/// <summary>
	/// 
	/// </summary>
	public override void Awake(vp_AI ai)
	{
	
		base.Awake(ai);
	
	}


	/// <summary>
	/// 
	/// </summary>
	public override void Start()
	{
	
		CurrentHealth = Random.Range(m_AI.DamageHandlerStartingHealth.x, m_AI.DamageHandlerStartingHealth.y);
		m_MaxHealth = CurrentHealth;
	
		m_StartPosition = m_EventHandler.PositionOnGround.Send(true);
		m_StartRotation = m_Transform.rotation;

    }
	
	
	/// <summary>
	/// reduces current health by 'damage' points and kills the
	/// object if health runs out
	/// </summary>
	public virtual void OnMessage_TakeDamage(float damage)
	{

		if (!vp_Utility.IsActive(m_Transform.gameObject))
			return;

		if(m_EventHandler.Dead.Active)
			return;
			
		CurrentHealth = Mathf.Min(CurrentHealth - damage, m_MaxHealth);

		if (CurrentHealth <= 0.0f)
		{
			Die();
			return;
		}
		
		// if damage is taken, start attacking
		if(!m_EventHandler.Attacking.Active)
			m_EventHandler.Attacking.TryStart();
		
		// try to show damage effect if the animation didnt't succeed
		if(!m_EventHandler.Damaged.TryStart())
			DoTakeDamageEffect();

		// TIP: if you want to do things like play a special impact
		// sound upon every hit (but only if the object survives)
		// this is the place

	}
	
	
	/// <summary>
	/// Does the take damage effect.
	/// </summary>
	protected virtual void DoTakeDamageEffect( bool force = false )
	{
	
		if(m_AI.DamageHandlerTakeDamageEffect == null)
			return;
	
		if(Random.Range(0f, 1f) > m_AI.DamageHandlerTakeDamageEffectProbability && !force)
			return;
			
		Object.Instantiate(m_AI.DamageHandlerTakeDamageEffect, m_Transform.position, m_Transform.rotation);
	
	}
	
	
	/// <summary>
	/// checks if the damage activity should start
	/// </summary>
	protected virtual bool CanStart_Damaged()
	{

		if(m_EventHandler.Dead.Active)
			return false;

		if(m_GetHitEndTimer.Active)
			return false;
			
		if(Time.time < m_TakeDamageAgainTime)
			return false;

		if(Random.Range(0f, 1f) > m_AI.DamageHandlerTakeDamageStateProbability)
			return false;
		
		return true;
		
	}
	
	
	/// <summary>
	/// starts the damage animation and plays
	/// forces the damage effect to display
	/// </summary>
	protected virtual void OnStart_Damaged()
	{
		
		// return if no damage animation
		if(!m_EventHandler.Animate.Send(m_AI.DamageHandlerTakeDamageState))
			return;
			
		m_TakeDamageAgainTime = Time.time + m_AI.DamageHandlerTakeDamageAgainDelay;
			
		// force damage effect
		DoTakeDamageEffect(true);
			
		m_EventHandler.Attack.Stop();
		m_EventHandler.Run.Stop();
		m_EventHandler.Walk.Stop();
		
		// resets to the idle animation when damage animation is done and starts attacking again
		vp_Timer.In(m_AI.DamageHandlerTakeDamageState.LastAnimationClipLength, delegate(){
			if(m_EventHandler.Retreat.Active)
				m_EventHandler.Run.Start();
			else
				m_EventHandler.Attack.Start();
				
			m_EventHandler.Damaged.Stop();
		}, m_GetHitEndTimer);
		
	}


	/// <summary>
	/// removes the object, plays the death effect and schedules
	/// a respawn if enabled, otherwise destroys the object
	/// </summary>
	public virtual void Die()
	{

		if(!vp_Utility.IsActive(m_Transform.gameObject))
			return;
			
		if(m_EventHandler.Dead.Active)
			return;

		RemoveBulletHoles();
		
		DoRagdoll();
			
		m_EventHandler.Dead.Start();
		m_EventHandler.Animate.Send(m_AI.DamageHandlerDeathAnimState);
		m_GetHitEndTimer.Cancel();
		
		if(m_AI.DamageHandlerRagdoll != null)
			vp_Utility.Activate(m_Transform.gameObject, false);

		DeathSpawnObjects.RandomizeList();
		foreach (DeathSpawnObject o in DeathSpawnObjects)
		{
			if(o.Prefab != null)
				if(Random.Range(0f,1f) < o.Probability)
				{
					Object.Instantiate(o.Prefab, m_Transform.position, m_Transform.rotation);
					if(m_AI.DamageHandlerOnlySpawnOneObject)
						break;
				}
		}

		if (m_AI.DamageHandlerRespawns)
			vp_Timer.In(Random.Range(m_AI.DamageHandlerRespawnTime.x, m_AI.DamageHandlerRespawnTime.y), Respawn);
		else
		{
			if(m_EventHandler != null)
				vp_AIManager.Unregister(m_EventHandler);
				
			Object.Destroy(m_Transform.gameObject);
		}
		
	}


	/// <summary>
	/// instantiates a ragdoll, playes a sound and adds force to
	/// the ragdoll upon death
	/// </summary>
	protected virtual void DoRagdoll()
	{
	
		if(m_AI.DamageHandlerRagdoll == null)
			return;
			
		m_CachedRagdoll = Object.Instantiate(m_AI.DamageHandlerRagdoll, m_Transform.position, m_Transform.rotation) as GameObject;
		AudioClip clip = m_EventHandler.GetAudioClipForState.Send(m_AI.DamageHandlerDeathAudioState);
		if(clip != null)
		{
			m_CachedRagdoll.AddComponent<AudioSource>();
			m_CachedRagdoll.GetComponent<AudioSource>().clip = clip;
			m_CachedRagdoll.GetComponent<AudioSource>().Play();
		}
		m_CachedRagdoll.transform.localScale = m_Transform.localScale;
		Transform[] transforms = m_CachedRagdoll.GetComponentsInChildren<Transform>();
		foreach(Transform t in transforms)
			if(t.GetComponent<Rigidbody>())
				t.GetComponent<Rigidbody>().AddForce(m_Controller.velocity * m_AI.DamageHandlerRagdollForce);
		
		vp_Timer.In(m_AI.DamageHandlerDisableRagdollTime > m_AI.DamageHandlerRespawnTime.x ? m_AI.DamageHandlerRespawnTime.x : m_AI.DamageHandlerDisableRagdollTime, delegate {
			transforms = m_CachedRagdoll.GetComponentsInChildren<Transform>();
			foreach(Transform t in transforms)
				if(t.GetComponent<Rigidbody>())
					t.GetComponent<Rigidbody>().isKinematic = true;
		});
	
	}
	
	
	/// <summary>
	/// respawns the object if no other object is occupying the
	/// respawn area. otherwise reschedules respawning
	/// </summary>
	protected virtual void Respawn()
	{

		vp_Utility.Activate(m_Transform.gameObject);
		m_EventHandler.Respawn.Send();
		
	}
	
	
	/// <summary>
	/// setup the AI to be respawned
	/// </summary>
	protected virtual void OnMessage_Respawn()
	{
	
		// return if the object has been destroyed (for example
		// as a result of loading a new level while it was gone)
		if (this == null)
			return;
		
		m_EventHandler.Dead.Stop();
		vp_Utility.Activate(m_Transform.gameObject, false);
			
		Vector3 newPos = m_StartPosition;
		
		Reroll:
		if(RandomRespawnPosition)
		{
			Vector3 min = AreaSpawnerBounds.min;
			Vector3 max = AreaSpawnerBounds.max;
			newPos = new Vector3(Random.Range(min.x, max.x), max.y, Random.Range(min.z, max.z));
		}

		// don't respawn if checkradius contains the local player or props
		// TIP: this can be expanded upon to check for alternative object layers
		if (Physics.CheckSphere(newPos, m_AI.DamageHandlerRespawnCheckRadius, vp_Layer.Mask.PhysicsBlockers))
		{
			if(RandomRespawnPosition)
				goto Reroll;
				
			// attempt to respawn again until the checkradius is clear
			vp_Timer.In(Random.Range(m_AI.DamageHandlerRespawnTime.x, m_AI.DamageHandlerRespawnTime.y), Respawn);
			return;
		}

		if(m_CachedRagdoll != null && m_AI.DamageHandlerDestroyRagdollOnRespawn)
			Object.Destroy(m_CachedRagdoll);

		m_StartPosition = newPos;
		ResetToDefault();
		
		vp_Utility.Activate(m_Transform.gameObject);
		
		m_EventHandler.Reset.Send(m_StartPosition);
	
	}
	
	
	/// <summary>
	/// resets health, position, angle and motion
	/// </summary>
	protected virtual void ResetToDefault()
	{
			
		CurrentHealth = m_MaxHealth;
		m_Transform.position = m_StartPosition;
		m_Transform.rotation = m_StartRotation;
		m_EventHandler.PositionOnGround.Send(true);
		if (m_Rigidbody != null && !m_Rigidbody.isKinematic)
		{
			m_Rigidbody.angularVelocity = Vector3.zero;
			m_Rigidbody.velocity = Vector3.zero;
		}

	}
	
	
	/// <summary>
	/// removes any bullet decals currently childed to this object
	/// </summary>
	protected virtual void RemoveBulletHoles()
	{

		foreach (Transform t in m_Transform)
		{
			Component[] c;
			c = t.GetComponents<vp_HitscanBullet>();
			if (c.Length != 0)
				Object.Destroy(t.gameObject);
		}

	}
	
	
	/// <summary>
	/// stops timer(s) when AI is told to stop
	/// </summary>
	protected virtual void OnStart_Stop()
	{
	
		m_GetHitEndTimer.Cancel();
	
	}


	/// <summary>
	/// prevents the AI from getting alerted when it's dead
	/// </summary>
	protected virtual bool CanStart_Alerted()
	{
	
		if(CurrentHealth < m_MaxHealth)
			return false;
			
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
	/// Gets or sets the health for the AI
	/// </summary>
	protected virtual float OnValue_Health
	{
	
		get{ return CurrentHealth; }
		set{ CurrentHealth = value; }
	
	}
	
}
