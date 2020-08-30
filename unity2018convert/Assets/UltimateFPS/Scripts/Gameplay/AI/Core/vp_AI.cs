/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AI.cs
//	Â© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this is the main AI component that manages certain global
//					events and properties for the AI. Any fields that need to show
//					in the inspector automatically using vp_AIAttributes should be put
//					in this class and then referenced from the plugin by the auto
//					setup field m_AI (e.g. m_AI.DamageHandlerTakeDamageState).
//					Also, fields should exist here if they need to be altered at
//					runtime by the States and Presets system.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[RequireComponent(typeof(vp_AIEventHandler))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class vp_AI : vp_AIComponent
{
	
#region Plugins
	public vp_AIAnimation Animation 			= new vp_AIAnimation();
	public vp_AIAudioStates AudioStates 		= new vp_AIAudioStates();
	public vp_AIAStarPathfinding AStar 			= new vp_AIAStarPathfinding();
	public vp_AIMovement Movement 				= new vp_AIMovement();
	public vp_AIDamageHandler DamageHandler 	= new vp_AIDamageHandler();
	public vp_AICombat Combat 					= new vp_AICombat();
	public vp_AIPlaceOnGround PlaceOnGround 	= new vp_AIPlaceOnGround();
	public vp_AISenses Senses					= new vp_AISenses();
#endregion
	
	
#region Animation States Properties
	[vp_AICustomMethod("Animation")] public bool AnimationStatesFunction;
#endregion


#region Audio States Properties
	[vp_AICustomMethod("AudioStates")] public bool AudioStatesFunction;
    #endregion


#region Damage Handler Properties
    [vp_AIBeginFoldout("Animation/Sound States")]
	public bool DamageHandlerAnimStatesFoldout = false;
	
	[vp_AIAnimationStateField("Take Damage Animation", "Animation State for Taking Damage")]
	public vp_AIState DamageHandlerTakeDamageState = null;
	
	[vp_AIAnimationStateField("Death Animation", "Animation State for Death")]
	public vp_AIState DamageHandlerDeathAnimState = null;
	
	[vp_AIAudioStateField("Death Sound", "Audio State for the Death Sound")][vp_AIEndFoldout]
	public vp_AIState DamageHandlerDeathAudioState = null;
	
	[vp_AIBeginFoldout("Health")]
	public bool DamageHandlerHealthFoldout = false;
	
	[vp_AIRangeField("Min/Max Random Starting Health", "A random value will be picked at runtime between the below 2 values. If you'd like a higher value then the default max, enter it into the max field and it will become the new default.", 0, 1000)][vp_AIEndFoldout]
	public Vector2 DamageHandlerStartingHealth = new Vector2(1, 1);
	
	[vp_AIBeginFoldout("Spawning")]
	public bool DamageHandlerSpawningFoldout = false;
	
	[vp_AIField("Respawns", "Does this AI Respawn after death")]
	public bool DamageHandlerRespawns = true;
	
	[vp_AIRangeField("Min/Max Respawn Time", "A random time after death will be chosen between the 2 specified times below for respawning", 0, 600)]
	public Vector2 DamageHandlerRespawnTime = new Vector2(20, 40);
	
	[vp_AIField("Spawn Check Radius", "Area around the objects spawn location which must be clear of other objects before respawn", 1, 100)][vp_AIEndFoldout]
	public float DamageHandlerRespawnCheckRadius = 1.0f;
	
	[vp_AIBeginFoldout("Damaged")]
	public bool DamageHandlerDamagedFoldout = false;
	
	[vp_AIField("Dmg State Probability", "The probability that the Damaged state will be triggered", 0, 1)]
	public float DamageHandlerTakeDamageStateProbability = 0.5f;
	
	[vp_AIField("Damaged Again Delay", "Delay before the damage state can be triggered again", 0, 60)]
	public float DamageHandlerTakeDamageAgainDelay = .5f;
	
	[vp_AIField("Damaged Effect", "Effect to be instantiated when damage is taken")]
	public GameObject DamageHandlerTakeDamageEffect = null;
	
	[vp_AIField("Effect Probability", "The probability that the Damaged Effect will be shown", 0, 1)][vp_AIEndFoldout]
	public float DamageHandlerTakeDamageEffectProbability = 0.75f;
	
	[vp_AIBeginFoldout("Death")]
	public bool DamageHandlerDeathFoldout = false;
	
	[vp_AIField("Ragdoll", "A ragdoll game object to be instantiated upon death. If this field is filled, the Death State animation will not be used.")]
	public GameObject DamageHandlerRagdoll = null;
	
	[vp_AIField("Destroy Ragdoll", "Whether ragdoll should be destroyed on respawn or not")]
	public bool DamageHandlerDestroyRagdollOnRespawn = false;
	
	[vp_AIField("Disable Ragdoll Delay", "Delay in seconds before the rigidbodies on the ragdoll are disabled", 0, 300)]
	public float DamageHandlerDisableRagdollTime = 10f;
	
	[vp_AIField("Ragdoll Force", "The amount of force that gets applied to the rigidbodies when the ragdoll is instantiated", 0, 1000)]
	public float DamageHandlerRagdollForce = 100;
	
	[vp_AICustomMethod("DeathSpawnObjects")][vp_AIEndFoldout]
	public bool DamageHandlerOnlySpawnOneObject = true;
#endregion
	
	
#region Movement Properties
	public Transform MovementRoamingTarget = null;						// transform that is moved around setting the next waypoint for the AI
	
	[vp_AIBeginFoldout("Animation States")]
	public bool MovementAnimStatesFoldout = false;
	
	[vp_AIAnimationStateField("Idle", "Animation State for Idle")]
	public vp_AIState MovementIdleState = null;
	
	[vp_AIAnimationStateField("Walk", "Animation State for Walk")]
	public vp_AIState MovementWalkState = null;
	
	[vp_AIAnimationStateField("Run", "Animation State for Run")][vp_AIEndFoldout]
	public vp_AIState MovementRunState = null;
	
	[vp_AIBeginFoldout("Movement")]
	public bool MovementFoldout = false;
	
	[vp_AIField("Walk Speed", "Velocity at which the AI moves", 0, 50)]
	public float MovementWalkSpeed = 1.3f;
	
	[vp_AIField("Walk Turning Speed", "The speed at which the AI turns during the Walk State", 0, 100)]
	public float MovementWalkTurningSpeed = 3;
	
	[vp_AIField("Run Speed", "The speed at which the AI walks during the Run State.", 0, 50)]
	public float MovementRunSpeed = 4;
	
	[vp_AIField("Run Turning Speed", "The speed at which the AI turns during the Run State", 0, 100)]
	public float MovementRunTurningSpeed = 5;
	
	[vp_AIField("Slowdown Distance", "Distance from the target point where the AI will start to slow down", 0, 50)]
	public float MovementSlowdownDistance = 0.6F;
	
	[vp_AIField("End Reached Distance", "Distance threshold from the AI to the target to be considered reached", 0, 50)]
	public float MovementEndReachedDistance = 0.2F;
	
	[vp_AIField("Stationary Check Rate", "Rate at which being stationary/stuck is checked", 0, 60)]
	public float MovementStationaryCheckRate = 2;
	
	[vp_AIField("Stationary Timeout", "After stationary/stuck has been detected, timeout before another action can occur", 0, 60)][vp_AIEndFoldout]
	public float MovementStationaryTimeout = 1;
	
	[vp_AIBeginFoldout("Roaming")]
	public bool MovementRoamingFoldout = false;
	
	[vp_AIField("Allow Roaming", "Should the AI be allowed to Roam freely?")]
	public bool MovementAllowRoaming = true;
	
	[vp_AIField("Waypoints Parent", "GameObject that has children waypoints")]
	public Transform MovementWaypointsParent = null;
	
	[vp_AIRangeField("Min/Max Roaming Distance", "A random distance from current position will be chosen between the minimum and maximum where a new waypoint will be set", 5, 100)]
	public Vector2 MovementRoamingDistance = new Vector2(10, 15);
	
	[vp_AIRangeField("Min/Max Roaming Wait Time", "A random time in seconds will be chosen between the minimum and maximum for the AI to set at a waypoint", 0, 120)][vp_AIEndFoldout]
	public Vector2 MovementRoamingWaitTime = new Vector2(5, 10);
	
	[vp_AIBeginFoldout("Retreat")]
	public bool MovementRetreatFoldout = false;
	
	[vp_AIField("Allow Retreat", "Should the AI retreat?")]
	public bool MovementAllowRetreat = false;
	
	[vp_AIRangeField("Min/Max Retreat Distance From Target", "Minimum and maximum distance from target before retreating is allowed", 1, 500)]
	public Vector2 MovementRetreatDistFromTarget = new Vector2(15, 25);
	
	[vp_AIField("Retreat Health Threshold", "When AI's health reaches this threshold, they'll start retreating", 0, 1)]
	public float MovementRetreatHealthThreshold = .25f;
	
	[vp_AIField("Low Health Retreat Dist", "The distance to retreat when the low health threshold has been reached", 1, 10)]
	public float MovementRetreatLowHealthDistance = 25f;
	
	[vp_AIField("Delay Retreat Can Start", "Time in secs before retreat can start after attacking started", 1, 10)][vp_AIEndFoldout]
	public float MovementRetreatCanStartDelay = 1f;
#endregion
	
	
#region A*Pathfinding Properties
	[vp_AIBeginFoldout("Modifiers")]
	public bool AStarModifiersFoldout = false;
	
	[vp_AIField("Auto Prioritize", "Will automatically set recommended priority on active modifiers")]
	public bool AStarAutoPrioritize = false;
	
	[vp_AICustomMethod("ShowModifiers")][vp_AIEndFoldout]
	public bool AStarShowModifierButtons = false;
	
	[vp_AIField("Repath Rate", "Determines how often it will search for new paths", 0, 100)]
	public float AStarRepathRate = 0.5F;
	
	[vp_AIField("Can Search", "Enables or disables searching for paths")]
	public bool AStarCanSearch = false;
	
	[vp_AIField("Pick Next Waypoint Dist", "Determines within what range it will switch to target the next waypoint in the path", 0, 100)]
	public float AStarPickNextWaypointDist = 2;
	
	[vp_AIField("Forward Look", "Target point is Interpolated on the current segment in the path so that it has a distance of ForwardLook from the AI", 0, 100)]
	public float AStarForwardLook = 1;
	
	[vp_AIField("Closest On Path Check", "Do a closest point on path check when receiving path callback")]
	public bool AStarClosestOnPathCheck = true;
#endregion
	
	
#region Combat Properties
	[vp_AIBeginFoldout("Animation States")]
	public bool CombatAnimStatesFoldout = false;
	
	[vp_AIAnimationStateField("Idle In Combat", "Animation State for Idle In Combat")]
	public vp_AIState CombatIdleInCombatState = null;
	
	[vp_AIAnimationStateField("Alerted", "Animation State for Alerted")]
	public vp_AIState CombatAlertedState = null;
	
	[vp_AIAnimationStateField("Melee Attack", "Animation State for Melee Attack")]
	public vp_AIState CombatMeleeAttackState = null;
	
	[vp_AIAnimationStateField("Ranged Attack", "Animation State for Ranged Attack")]
	public vp_AIState CombatRangedAttackState = null;
	
	[vp_AIAnimationStateField("Reload", "Animation State for Reload")][vp_AIEndFoldout]
	public vp_AIState CombatReloadState = null;
	
	[vp_AIBeginFoldout("Settings")]
	public bool CombatSettingsFoldout = false;
	
	[vp_AICustomMethod("HostileLayersTags")]
	public LayerMask CombatHostileLayers = (1 << vp_Layer.LocalPlayer); 	// layer mask of hostiles
	public int CombatHostileTags = 0;										// hostile tags mask
	public List<string> CombatHostileTagsList = new List<string>(); 		// list of hostile tags
	
	[vp_AIField("Look For Hostile Dist", "Max distance to check for hostiles", 0, 1000)]
	public float CombatHostileCheckDistance = 500;
	
	[vp_AIField("Alert Friendly Dist", "Max distance to alert friendlies this AI is attacking", 0, 100)][vp_AIEndFoldout]
	public float CombatAlertFriendlyDistance = 10;
	
	[vp_AIBeginFoldout("Melee Combat")]
	public bool CombatMeleeCombatFoldout = false;
	
	[vp_AIField("Allow Melee Attacks", "Allows the AI to perform melee attacks")]
	public bool CombatAllowMeleeAttacks = false;
	
	[vp_AICustomMethod("MeleeIfRanged")]
	public int CombatMeleeProbability = 50;
	public float CombatOverrideRangedDistance = 10;
	
	[vp_AIRangeField("Min/Max Melee Damage", "The minimum and maximum amount of damage that can be applied for a melee attack",  0, 1)]
	public Vector2 CombatMeleeAttackDamage = new Vector2(.1f, .2f);
	
	[vp_AIRangeField("Min/Max Distance to Melee Attack", "The minimum and maximum distance from the target in order to perform a melee attack", 0, 100)]
	public Vector2 CombatMeleeAttackDistance = new Vector2(2, 4);
	
	[vp_AIField("Attack Again Time", "The time in secs it takes after a melee attack to attack again", 0, 100)][vp_AIEndFoldout]
	public float CombatMeleeAttackAgainTime = 2;
	
	[vp_AIBeginFoldout("Ranged Combat")]
	public bool CombatRangedCombatFoldout = false;
	
	[vp_AIField("Allow Ranged Attacks", "Allows the AI to perform ranged attacks")]
	public bool CombatAllowRangedAttacks = true;
	
	[vp_AICustomMethod("RangedIfMelee")]
	public int CombatRangedProbability = 50;
	
	[vp_AIRangeField("Min/Max Ranged Damage", "The minimum and maximum amount of damage that can be applied for a ranged attack",  0, 1)]
	public Vector2 CombatRangedAttackDamage = new Vector2(.1f, .3f);
	
	[vp_AIRangeField("Min/Max Distance to Ranged Attack", "The minimum and maximum distance from the target in order to perform a ranged attack", 0, 100)]
	public Vector2 CombatRangedAttackDistance = new Vector2(20, 25);
	
	[vp_AIField("Bullets Per Clip", "How many bullets can be dispenced before reloading occurs", 0, 200)]
	public int CombatBulletsPerClip = 25;
	
	[vp_AIField("Reload Time", "Time it takes to reload if no reload state is provided", 0, 50)][vp_AIEndFoldout]
	public float CombatReloadTime = 2;
#endregion
	
	
#region Place On Ground Properties
	[vp_AIField("Ground Layers", "Layers to check for ground placement")]
	public LayerMask PlaceOnGroundGroundLayers = 1<<vp_Layer.Default | 1<<vp_Layer.MovableObject;
	
	[vp_AIField("Offset", "An offset from the initial ground placement")]
	public Vector3 PlaceOnGroundOffset = new Vector3(0, .1f, 0);
#endregion
	
	
#region Senses Properties
	[vp_AIBeginFoldout("Vision")]
	public bool SensesVisionFoldout = false;
	
	[vp_AIField("Mask", "Layers AI can see")]
	public LayerMask SensesVisionMask = 1<<vp_Layer.LocalPlayer;
	
	[vp_AIField("Check Rate", "Frequency a check is performed for vision. Set to 0 to disable.", 0, 60)]
	public float SensesViewCheckRate = .5f;
	
	[vp_AIField("Viewing Distance", "Maximum distance AI can see", 0, 1000)]
	public float SensesViewDistance = 20;
	
	[vp_AIField("Viewing Angle", "Maximum angle of vision", 0, 360)][vp_AIEndFoldout]
	public float SensesViewingAngle = 80;
	
	[vp_AIBeginFoldout("Hearing")]
	public bool SensesHearingFoldout = false;
	
	[vp_AIField("Mask", "Layers AI can hear")]
	public LayerMask SensesHearingMask = 1<<vp_Layer.LocalPlayer;
	
	[vp_AIField("Check Rate", "Frequency a check is performed for hearing. Set to 0 to disable.", 0, 60)]
	public float SensesHearingCheckRate = .5f;
	
	[vp_AIField("Hearing Distance", "Maximum distance AI can hear", 0, 1000)][vp_AIEndFoldout]
	public float SensesHearingDistance = 20;
#endregion
	
#if UNITY_EDITOR
	// Editor Properties
	public bool StatesAndPresetsMainFoldout = false;
	public bool StateFoldout = true;
	public bool PresetFoldout;
#endif

	// plugin events
	public delegate void PluginEventCallback();
	public PluginEventCallback LateUpdateCallback;
	public PluginEventCallback UpdateCallback;
	public PluginEventCallback FixedUpdateCallback;
	
	/// <summary>
	///	class to hold cached properties for the AI's current target
	/// </summary>
	public class TargetProperties
	{
		public Collider Collider = null;
		public CharacterController CharacterController = null;
		public GameObject GameObject = null;
		public int Layer = -1;
	}
	
	protected List<vp_AIPlugin> m_Plugins = new List<vp_AIPlugin>(); 	// used at runtime to make plugins act like monobehaviours
	protected List<vp_Activity> m_Activities = new List<vp_Activity>();	// A cached list of vp_Activities for this AI
	protected Transform m_Target;										// Current Target.
	protected TargetProperties m_TargetProperties = null; 				// cached properties for the current target
	protected Transform m_HostileTarget;								// Current Hostile Target.
	protected TargetProperties m_HostileTargetProperties = null; 		// cached properties for the current hostile target
	protected Transform m_SpawnPoint = null;							// Cached spawn point transform
	protected Transform m_AIContainer = null;							// Container for all AI
	protected vp_AIEventHandler m_EventHandler = null;					// cached ai event handler
	
	protected static string m_ContainerName = "AI Container";			// empty gameobject that gets created to store all the AI
	
	
	/// <summary>
	/// in 'Awake' we do things that need to be run once at the
	/// very beginning. NOTE: as of Unity 4, gameobject hierarchy
	/// can not be altered in 'Awake'
	/// </summary>
	protected override void Awake()
	{
	
		base.Awake();
	
		m_EventHandler = (vp_AIEventHandler)EventHandler;
		
		// use reflection to loop through fields in this class
		foreach(FieldInfo info in this.GetType().GetFields())
		{
			// check if the field is a plugin
			if(info.FieldType.BaseType == typeof(vp_AIPlugin))
			{
				// get the value of the plugin
				vp_AIPlugin plugin = (vp_AIPlugin)info.GetValue(this);
				if(plugin == null)
					continue;
				
				// check if the plugin is enabled
				if(plugin.Enabled)
				{
					// setup the AIStates in the plugins so they have the correct values
					foreach(FieldInfo stateInfo in plugin.GetType().GetFields())
						if(stateInfo.FieldType == typeof(vp_AIState))
							stateInfo.SetValue( plugin, RefreshState( (vp_AIState)stateInfo.GetValue(plugin) ) );
							
					// add the plugin to the list
					m_Plugins.Add(plugin);
				}
			}
			
			// setup any AIStates in this class so they have the correct values
			if(info.FieldType == typeof(vp_AIState))
				info.SetValue( this, RefreshState( (vp_AIState)info.GetValue(this) ) );
			
		}
		
		// order the plugins by their sort order values
		m_Plugins = m_Plugins.OrderBy(p => p.SortOrder).ToList();
		
		// run Awake on all the enabled plugins
		foreach(vp_AIPlugin plugin in m_Plugins)
		{
			plugin.Awake(this);
			
			if(!enabled)
				plugin.OnDisable();
		}
			
		if(!enabled)
			if(PlaceOnGround != null)
				vp_Timer.In(0.1f, delegate { PlaceOnGround.PositionOnGround(true); });

	}


	/// <summary>
	/// sets up a heirarchy for this AI and
	/// it's dependant gameobjects.
	/// </summary>
	protected override void Start()
	{
	
		base.Start();
	
		// send Start
		foreach(vp_AIPlugin plugin in m_Plugins)
			plugin.Start();
			
		// create AI container
		GameObject container = GameObject.Find(m_ContainerName);
		if(container == null)
			container = new GameObject(m_ContainerName);
			
		m_AIContainer = container.transform;
		
		m_Parent = new GameObject(name.Replace("(Clone)","")).transform;
		m_Parent.gameObject.layer = gameObject.layer;
		
		m_Parent.parent = m_AIContainer;
		m_Transform.parent = m_Parent;
		
		m_SpawnPoint = new GameObject("SpawnPoint").transform;
		m_SpawnPoint.gameObject.layer = gameObject.layer;
		m_SpawnPoint.parent = m_Parent;
			
		// add all activities to list
		m_Activities.Clear();
		foreach(FieldInfo info in m_EventHandler.GetType().GetFields())
			if((info.FieldType == typeof(vp_Activity) || info.FieldType.BaseType == typeof(vp_Activity)))
				m_Activities.Add((vp_Activity)info.GetValue(m_EventHandler));
		
		// reset this AI
		m_EventHandler.Reset.Send(m_Transform.position);
		
	}
	
	
	/// <summary>
	/// registers this component with the event handler (if any)
	/// and registers with the AI Manager
	/// </summary>
	protected override void OnEnable()
	{
	
		base.OnEnable();
	
		if(m_EventHandler == null)
			return;
			
		vp_AIManager.Register(m_EventHandler);
			
		foreach(vp_AIPlugin plugin in m_Plugins)
			plugin.OnEnable();
			
		// register plugins with the event callbacks
		foreach(vp_AIPlugin plugin in m_Plugins)
		{
			UpdateCallback += plugin.Update;
			FixedUpdateCallback += plugin.FixedUpdate;
			LateUpdateCallback += plugin.LateUpdate;
		}
	
	}
	
	
	/// <summary>
	/// Unregisters this component from the event handler (if any)
	/// and Unregisters with the AI Manager
	/// </summary>
	protected override void OnDisable()
	{
	
		base.OnDisable();
	
		if(m_EventHandler == null)
			return;
		
		vp_AIManager.Unregister(m_EventHandler);
			
		foreach(vp_AIPlugin plugin in m_Plugins)
			plugin.OnDisable();
			
		// register plugins with the event callbacks
		foreach(vp_AIPlugin plugin in m_Plugins)
		{
			UpdateCallback -= plugin.Update;
			FixedUpdateCallback -= plugin.FixedUpdate;
			LateUpdateCallback -= plugin.LateUpdate;
		}
	
	}
	
	
	/// <summary>
	/// Destroys the parent gameobject to keep hierarchy clean
	/// </summary>
	protected override void OnDestroy()
	{
	
		base.OnDestroy();
		
		foreach(vp_AIPlugin plugin in m_Plugins)
			plugin.OnDestroy();
	
		if(enabled)
			Destroy(m_Parent.gameObject);
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected override void LateUpdate()
	{
	
		base.LateUpdate();
	
		// send LateUpdateCallback event
		if(LateUpdateCallback != null)
			LateUpdateCallback();
	
	}
	
	
	/// <summary>
	/// NOTE: to provide the 'Init' functionality, this method
	/// must be called using 'base.Update();' on the first line
	/// of the 'Update' method in the derived class
	/// </summary>
	protected override void Update()
	{
	
		base.Update();
	
		// send UpdateCallback event
		if(UpdateCallback != null)
			UpdateCallback();
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected override void FixedUpdate()
	{
	
		base.FixedUpdate();
	
		// send FixedUpdateCallback event
		if(FixedUpdateCallback != null)
			FixedUpdateCallback();
	
	}
	
	
	/// <summary>
	/// Refreshes a state from the States and Presets section
	/// </summary>
	public virtual vp_AIState RefreshState( vp_AIState state )
	{
	
		if(state.Name == null)
			return null;
			
		return States.Find(s => s.Name == state.Name);
	
	}
	
	
	/// <summary>
	/// Catches Damage SendMessage from previous
	/// UFPS setup. Any future events sent to
	/// plugins should use vp_NotificationCenter
	/// </summary>
	public virtual void Damage(float damage)
	{
        DamageHandler.OnMessage_TakeDamage(damage);

//        m_EventHandler.TakeDamage.Send(damage);
	
	}
	
	
	/// <summary>
	/// Sends the AI back to it's starting position
	/// </summary>
	protected virtual void OnMessage_ReturnToSpawn()
	{
	
		m_EventHandler.Stop.TryStart();
		m_EventHandler.Target.Set(m_SpawnPoint);
		m_EventHandler.Walk.Start();
	
	}
	
	
	/// <summary>
	/// if AI is returning home and has reached
	/// it's destination, start roaming
	/// </summary>
	protected virtual void OnMessage_TargetReached()
	{
	
		if(m_Target == null || m_SpawnPoint == null)
			return;
			
		if(m_Target != m_SpawnPoint)
			return;
			
		m_EventHandler.Idle.Start();
		m_EventHandler.Roaming.TryStart();
	
	}
	
	
	/// <summary>
	/// stops the AI upon death and plays
	/// the death animation or ragdoll
	/// </summary>
	protected virtual void OnStart_Dead()
	{
		
		m_EventHandler.Stop.TryStart(new List<object>(){ m_EventHandler.Dead });
		
	}
	
	
	/// <summary>
	/// Resets the AI to it's initial state
	/// and optionally positions the home transform
	/// to the specified new location
	/// </summary>
	protected virtual void OnMessage_Reset( Vector3 newSpawnPosition )
	{
	
		m_EventHandler.Idle.Start();
		
		if(newSpawnPosition != Vector3.zero)
			m_SpawnPoint.position = newSpawnPosition;
			
		m_EventHandler.Target.Set(m_SpawnPoint);
		m_EventHandler.PositionOnGround.Send(true);
	
	}
	
	
	/// <summary>
	/// stops all activities unless exceptions
	/// are provided as the argument
	/// </summary>
	protected virtual void OnStart_Stop()
	{
	
		List<object> ignoreActivites = (List<object>)m_EventHandler.Stop.Argument;
		if(ignoreActivites == null)
			ignoreActivites = new List<object>();
	
		m_Target = null;
		m_HostileTarget = null;
		foreach(vp_Activity activity in m_Activities)
			if(!ignoreActivites.Contains(activity))
				activity.Stop();
				
		m_EventHandler.Stop.Stop();
	
	}
	
	
	/// <summary>
	/// activate this object on respawn
	/// </summary>
	protected virtual void OnMessage_Respawn()
	{
	
		vp_Utility.Activate(gameObject);
	
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
		
		RefreshDefaultState();
	
		if(!properties.ContainsKey("Scale"))
			return;
	
		if(properties["Scale"] == null)
			return;
			
		if(properties["Scale"].GetType() != typeof(Vector3))
			return;
			
		m_Transform.localScale = (Vector3)properties["Scale"];
		
	}
	
	
	/// <summary>
	/// gets cached properties of the current target
	/// </summary>
	protected virtual object OnValue_TargetProperties
	{
		get{ return m_TargetProperties; }
	}
	
	
	/// <summary>
	/// cached spawn point
	/// </summary>
	protected virtual Transform OnValue_SpawnPoint
	{
	
		get{ return m_SpawnPoint; }
		set{ m_SpawnPoint = value; }
	
	}
	
	
	/// <summary>
	/// AI's current target
	/// </summary>
	protected virtual Transform OnValue_Target
	{
		get { return m_Target; }
		set { 
			if(value != null)
			{
				m_TargetProperties = new TargetProperties(){
					Collider = value.GetComponent<Collider>(),
					CharacterController = value.GetComponent<CharacterController>(),
					GameObject = value.gameObject,
					Layer = value.gameObject.layer
				};
			}
			else
				m_TargetProperties = null;
				
			m_Target = value;
		}
	}
	
	
	/// <summary>
	/// AI's current hostile target
	/// </summary>
	protected virtual Transform OnValue_HostileTarget
	{
		get { return m_HostileTarget; }
		set { 
			if(value != null)
			{
				m_HostileTargetProperties = new TargetProperties(){
					Collider = value.GetComponent<Collider>(),
					CharacterController = value.GetComponent<CharacterController>(),
					GameObject = value.gameObject,
					Layer = value.gameObject.layer
				};
			}
			else
				m_HostileTargetProperties = null;
				
			m_HostileTarget = value;
		}
	}
	
	
	/// <summary>
	/// AI's current hostile target
	/// </summary>
	protected virtual Transform OnValue_TrackedTarget
	{
		get { return m_EventHandler.Attacking.Active ? m_EventHandler.HostileTarget.Get() : m_EventHandler.Target.Get(); }
	}
	
}
