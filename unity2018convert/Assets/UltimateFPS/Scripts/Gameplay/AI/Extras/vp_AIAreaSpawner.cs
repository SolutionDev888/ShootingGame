/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIAreaSpawner.cs
//	Â© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class allows for the spawning of an unlimited amount of
//					different types of AI within the colliders bounds. Optionally,
//					many of the AI's component properties can overridden if applicable.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[RequireComponent(typeof(BoxCollider))]
public class vp_AIAreaSpawner : MonoBehaviour
{

	/// <summary>
	/// A class containing all of the properties to be
	/// to be set from the provided Prefab for the spawned
	/// AI. This handles some functions for the editor
	/// as well.
	/// </summary>
	[System.Serializable]
	public class AISpawnerObject
	{
	
		public bool Enabled							= false;
		public bool Foldout 						= true;
		public GameObject Prefab 					= null;					// A prefab with a vp_AI component on it.
		public int AmountToSpawn					= 1;					// The amount of this particular AI to spawn.
		public Vector3 Scale						= Vector3.one;			// The scale of this AI when spawned. Set from the prefab when the prefab first gets set.
		public bool ShouldUpdate					= true;
		public string SpawnObjectName				= "New AI";
		public bool SpawnAtStart					= true;					// When enabled, all AI will spawn and be shown on scene start. If disabled, they'll be spawned, but hidden. Useful when used with vp_AISpawnerTrigger.
		[System.NonSerialized]
		public List<vp_AIEventHandler> CachedEventHandlers = new List<vp_AIEventHandler>();
		
		// vp_AIDamageHandler Overrides
		public GameObject DamageHandlerTakeDamageEffect = null;				// effect displayed when hit
		public vp_AIDamageHandler CachedDamageHandlerComponent	= null;
		public Vector2 DamageHandlerStartingHealth = new Vector2(1, 1);
		public bool DamageHandlerFoldout 					= true;
		public float DamageHandlerMaxHealth					= 5f;
		public GameObject DamageHandlerRagdoll							= null;
		public bool DamageHandlerDestroyRagdollOnRespawn = false;			// Whether ragdoll should be destroyed on respawn or not
		public float DamageHandlerDisableRagdollTime = 10f;					// time in seconds to disable rigidbodies on the ragdoll
		public float DamageHandlerRagdollForce = 100;						// force to be applied to ragdoll when instantiated
		public bool DamageHandlerDeathSpawnObjectsFoldout	= false;
		public List<vp_AIDamageHandler.DeathSpawnObject> DeathSpawnObjects = new List<vp_AIDamageHandler.DeathSpawnObject>();
		public bool DamageHandlerOnlySpawnOneObject			= true;
		public bool DamageHandlerRespawns		 			= true;
		public Vector2 DamageHandlerRespawnTime = new Vector2(20, 40);
		public float DamageHandlerRespawnCheckRadius		= 1f;
		public bool RandomRespawnPosition					= true;			// only available from the area spawner
		public float DamageHandlerTakeDamageAnimationProbability = 0.5f;	// probability that the TakeDamage animation will fire when hit
		public float DamageHandlerTakeDamageEffectProbability = 0.75f;		// probability of effect being displayed
		public float DamageHandlerTakeDamageAgainDelay 		= .5f;				// Delay before the damage state can be triggered again
		
		// vp_AIMovement Overrides
		public vp_AIMovement CachedMovementComponent	= null;
		public float MovementWalkSpeed 					= 1.3f;				// velocity at which the AI moves
		public float MovementWalkTurningSpeed 			= 3;				// speed at which the AI turns
		public float MovementRunSpeed 					= 4;				// velocity at which the AI moves
		public float MovementRunTurningSpeed 			= 5;				// speed at which the AI turns
		public bool MovementFoldout 					= false;
		public bool MovementAllowRoaming 				= true;
		public Transform MovementWaypointsParent	 			= null;
		public Vector2 MovementRoamingDistance = new Vector2(10, 15);
		public Vector2 MovementRoamingWaitTime = new Vector2(5, 10);
		public bool MovementAllowRetreat				= false;
		public Vector2 MovementRetreatDistFromTarget = new Vector2(15, 25);
		public float MovementRetreatHealthThreshold		= .25f;
		public float MovementRetreatLowHealthDistance	= 25f;
		public float MovementRetreatCanStartDelay		= 1f;
		
		// vp_AISenses Overrides
		public vp_AISenses CachedSensesComponent	= null;
		public bool SensesFoldout					= false;
		public float SensesViewCheckRate			= .5f;					// frequency a check is performed for vision
		public float SensesViewDistance				= 20;					// maximum distance AI can see
		public float SensesViewingAngle 			= 80;					// maximum angle of vision
		public float SensesHearingCheckRate 		= .5f;					// frequency a check is performed for hearing
		public float SensesHearingDistance 			= 20;					// maximum distance AI can hear
		
		// vp_AICombat Overrides
		public vp_AICombat CachedCombatComponent	= null;
		public bool CombatFoldout 					= false;
		public LayerMask CombatHostileLayers				= (1 << vp_Layer.LocalPlayer);
		public int CombatHostileTags						= 0;
		public List<string> CombatHostileTagsList			= new List<string>();
		public float CombatHostileCheckDistance		= 500;
		public float CombatAlertFriendlyDistance	= 10;
		public bool CombatAllowMeleeAttacks 		= false;
		public float CombatMeleeAttackAgainTime 	= 2f;
		public int CombatMeleeProbability 			= 50;
		public Vector2 CombatMeleeAttackDamage = new Vector2(.1f, .2f);
		public Vector2 CombatMeleeAttackDistance = new Vector2(2, 4);
		public float CombatOverrideRangedDistance 	= 10f;
		public float CombatMeleeAttackSpeed 		= 1f;
		public bool CombatAllowRangedAttacks 		= false;
		public int CombatRangedProbability 			= 50;
		public Vector2 CombatRangedAttackDamage = new Vector2(.1f, .3f);
		public Vector2 CombatRangedAttackDistance = new Vector2(20, 25);
		public int CombatBulletsPerClip 			= 25;
		public float CombatReloadTime 				= 2f;
		
		
		/// <summary>
		/// Updates the cached components.
		/// </summary>
		public virtual void UpdateCachedComponents()
		{
		
			if(Prefab == null)
				return;
		
			vp_AI aiComponent = Prefab.GetComponent<vp_AI>();
			if(aiComponent.DamageHandler != null)
				CachedDamageHandlerComponent = aiComponent.DamageHandler;
			if(aiComponent.Movement != null)
				CachedMovementComponent = aiComponent.Movement;
			if(aiComponent.Combat != null)
				CachedCombatComponent = aiComponent.Combat;

		}
		
		
		/// <summary>
		/// Updates the cached component.
		/// </summary>
		public virtual void UpdateCachedComponent<T>( ref T component ) where T : UnityEngine.MonoBehaviour
		{
		
			if(Prefab == null)
				return;
		
			T[] components = Prefab.GetComponentsInChildren<T>(true);
			if(components.Length > 0)
				component = components[0];
		
		}
		
		
		/// <summary>
		/// Updates all the components.
		/// </summary>
		public virtual void UpdateComponents()
		{
		
			if(Prefab == null)
				return;
		
			if(!ShouldUpdate)
				return;
		
			ShouldUpdate = false;
			
			Scale = Prefab.transform.localScale;
			AmountToSpawn = 1;
			SpawnAtStart = true;
			
			UpdateCachedComponents();
			
			vp_AI aiComponent = Prefab.GetComponent<vp_AI>();
			UpdateValuesFromComponentNonMonoBehavior( ref CachedDamageHandlerComponent );
			UpdateValuesFromComponentNonMonoBehavior( ref CachedMovementComponent );
			UpdateValuesFromComponentNonMonoBehavior( ref CachedSensesComponent );
			UpdateValuesFromComponentNonMonoBehavior( ref CachedCombatComponent );
			UpdateValuesFromComponent( ref aiComponent );
		
		}
		
		
		/// <summary>
		/// Updates the values from components that are non mono behaviors.
		/// </summary>
		public virtual void UpdateValuesFromComponentNonMonoBehavior<T>( ref T field, string prefix = "" )
		{
		
			if(field == null)
				return;
			
			Dictionary<string, object> dict = new Dictionary<string, object>();
	
			foreach(FieldInfo info in field.GetType().GetFields())
				dict.Add( info.Name, info.GetValue(field) );
				
			foreach(string k in dict.Keys)
			{
				FieldInfo p = this.GetType().GetField(k);
				if(p != null)
					p.SetValue(this, dict[k]);
			}
			
			if( prefix == "")
				return;
				
			vp_AI ai = Prefab.GetComponent<vp_AI>();
			foreach(FieldInfo info in ai.GetType().GetFields())
				if(info.Name.IndexOf(prefix) != -1 && info.Name.IndexOf("Foldout") == -1)
				{
					FieldInfo p = this.GetType().GetField(info.Name);
					if(info != null && p != null)
						this.GetType()
						.GetField(info.Name)
						.SetValue(this, info.GetValue(ai));
				}
		
		}
		
		
		/// <summary>
		/// Updates the values from component.
		/// </summary>
		public virtual void UpdateValuesFromComponent<T>( ref T field ) where T : UnityEngine.MonoBehaviour
		{
		
			if(field == null)
				return;
			
			Dictionary<string, object> dict = new Dictionary<string, object>();
	
			foreach(FieldInfo info in field.GetType().GetFields())
				dict.Add( info.Name, info.GetValue(field) );
				
			foreach(string k in dict.Keys)
			{
				FieldInfo p = this.GetType().GetField(k);
				if(p != null)
					p.SetValue(this, dict[k]);
			}
		
		}
		
		
		/// <summary>
		/// Used to duplicate this instance.
		/// </summary>
		public virtual AISpawnerObject Copy()
	    {
	        Dictionary<string, object> dict = new Dictionary<string, object>();
			foreach(FieldInfo info in this.GetType().GetFields())
				dict.Add( info.Name, info.GetValue(this) );
			
			AISpawnerObject newProperties = new AISpawnerObject();	
			foreach(string k in dict.Keys)
			{
				FieldInfo p = newProperties.GetType().GetField(k);
				if(p != null)
				{	
					if(p.Name.IndexOf("Foldout") != -1 && p.Name != "Foldout")
						p.SetValue(newProperties, false);
					else if(p.Name == "Foldout")
						p.SetValue(newProperties, true);
					else if(p.Name == "SpawnObjectName")
						p.SetValue(newProperties, dict[k]+" Copy");
					else
						p.SetValue(newProperties, dict[k]);
				}
			}
			
			Foldout = false;
			
			return newProperties;
	    }
	
	}

	public bool SpawnAtStart = true; // whether or not all spawner objects will spawn at start
	public List<AISpawnerObject> AISpawnerObjects = new List<AISpawnerObject>(); // list of individual Spawn Objects
	
	protected BoxCollider m_Collider = null; // cached collider compoent
	
	
	/// <summary>
	/// setup the collider here as to not interfere
	/// with normal gameplay
	/// </summary>
	protected virtual void Awake()
	{
	
		m_Collider = GetComponent<Collider>() as BoxCollider;
		m_Collider.isTrigger = true;
		gameObject.layer = vp_Layer.Trigger;
		
		for(int i=0; i<AISpawnerObjects.Count; i++)
			InstantiateObject(i);
	
	}
	
	/// <summary>
	/// Instantiates an spawn object by it's index
	/// and sets the new AI's properties
	/// </summary>
	protected virtual void InstantiateObject( int index )
	{
		
		if(index > AISpawnerObjects.Count)
			return;
			
		AISpawnerObject spawnerObject = AISpawnerObjects[index];
		if(spawnerObject == null)
			return;
			
		if(spawnerObject.Prefab == null || !spawnerObject.Enabled)
			return;
				
		for(int i=0; i<spawnerObject.AmountToSpawn; i++)
		{
			GameObject go = Object.Instantiate(spawnerObject.Prefab) as GameObject;
		
			vp_AIEventHandler ai = go.GetComponent<vp_AIEventHandler>();
			if(ai != null)
			{
				Reroll:
			
				// get a random position within the spawners bounds
				Vector3 min = m_Collider.bounds.min;
				Vector3 max = m_Collider.bounds.max;
				Vector3 randPos = new Vector3(Random.Range(min.x, max.x), max.y, Random.Range(min.z, max.z));
				
				// check to see if this is a good position to spawn, if not, reroll
				if (Physics.CheckSphere(randPos, spawnerObject.DamageHandlerRespawnCheckRadius, vp_Layer.Mask.PhysicsBlockers))
					goto Reroll;
					
				// random rotation
				Quaternion randRot = Random.rotation;
				randRot.z = 0;
				randRot.x = 0;
				
				go.transform.position = randPos;
				go.transform.rotation = randRot;
				
				// set all the overridden properties to send to the various components
				Dictionary<string, object> dict = new Dictionary<string, object>();
				foreach(FieldInfo info in spawnerObject.GetType().GetFields())
					AddSpawnProperty( spawnerObject, info, dict );
							
				// send the spawner bounds with the other overrides
				dict.Add( "AreaSpawnerBounds", m_Collider.bounds );
				
				// send the overridden properties to the various components
				ai.SetProperties.Send(dict);
				
				spawnerObject.CachedEventHandlers.Add(ai);
				
				if(!SpawnAtStart || (SpawnAtStart && !spawnerObject.SpawnAtStart))
				{
					Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
					foreach(Renderer r in renderers)
						r.enabled = false;
							
					// the AI needs to be allowed to setup some stuff before being set to inactive
					vp_Timer.In(.1f, delegate {
						vp_Utility.Activate(go, false);
						foreach(Renderer r in renderers)
							r.enabled = true;
					});
				}
				
				ai.Reset.Send(Vector3.zero);
			}
		}
	
	}
	
	
	/// <summary>
	/// uses Reflection to check if a property is
	/// applicable and should be overriden
	/// before adding it to the overrides dictionary
	/// </summary>
	protected virtual void AddSpawnProperty( AISpawnerObject property, FieldInfo info, Dictionary<string, object> dict )
	{
	
		if(info.GetValue(property) == null)
			return;
		
		dict.Add( info.Name, info.GetValue(property) );
	
	}
	
	
	/// <summary>
	/// Spawns a spawn object by index
	/// </summary>
	public virtual void Spawn( int index )
	{
	
		// make sure index in range
		if(index > AISpawnerObjects.Count - 1)
			return;
	
		// check object exists
		AISpawnerObject spawnerObject = AISpawnerObjects[index];
		if(spawnerObject == null)
			return;
	
		// check if any AI are cached
		if(spawnerObject.CachedEventHandlers.Count == 0)
			return;
	
		// set each ai to active if applicable
		foreach(vp_AIEventHandler ai in spawnerObject.CachedEventHandlers)
		{
			if(ai.Dead.Active)
				continue;
			
			vp_Utility.Activate(ai.gameObject);
			ai.Respawn.Send();
		}
	
	}
	
	
	/// <summary>
	/// Spawns a spawn object by name
	/// </summary>
	public virtual void SpawnByName( string n )
	{
	
		AISpawnerObject spawnerObject = AISpawnerObjects.FirstOrDefault( ai => ai.SpawnObjectName == n );
		if(spawnerObject == null)
			return;
			
		Spawn( AISpawnerObjects.IndexOf(spawnerObject) );
	
	}
	
}
