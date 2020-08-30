/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIManager.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this class collects the various AI in order to send actions
//					to them globally when events happen to the player
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_AIManager
{

	public Dictionary<vp_AIEventHandler, Dictionary<Transform, vp_FPPlayerEventHandler>> AIs = 
		new Dictionary<vp_AIEventHandler, Dictionary<Transform, vp_FPPlayerEventHandler>>(); // collection of AI and their cached players
	
	private static vp_AIManager instance = null;
	public static vp_AIManager Instance{
		get{
			if(instance == null)
				instance = new vp_AIManager();
			
			return instance;
		}
	}
	
	
	/// <summary>
	/// Register the specified AI with this manager
	/// </summary>
	public static void Register(vp_AIEventHandler ai)
	{
	
		if(!Instance.AIs.ContainsKey(ai))
			Instance.AIs.Add(ai, new Dictionary<Transform, vp_FPPlayerEventHandler>());
	
	}
	
	
	/// <summary>
	/// Unregister the specified AI from this manager
	/// </summary>
	public static void Unregister(vp_AIEventHandler ai)
	{
	
		if(Instance.AIs.ContainsKey(ai))
			Instance.AIs.Remove(ai);
	
	}
	
	
	/// <summary>
	/// try to add a player for the AI
	/// and the players properties
	/// </summary>
	public virtual vp_FPPlayerEventHandler AddPlayerForAI( vp_AIEventHandler ai, Transform t )
	{
	
		// if the AI is not registered, return
		if(!Instance.AIs.ContainsKey(ai))
			return null;
			
		// if the AI has cached this player, return the player
		if(Instance.AIs[ai].ContainsKey(t))
			return Instance.AIs[ai][t];
			
		// add this player for this AI
		vp_FPPlayerEventHandler player = t.root.GetComponentInChildren<vp_FPPlayerEventHandler>();
		Instance.AIs[ai].Add(t, player);
		
		return player;
	
	}
	
	
	/// <summary>
	/// Gets properties for the specified AI and player
	/// and adds the properties if none exist
	/// </summary>
	public static vp_FPPlayerEventHandler GetPlayer( vp_AIEventHandler ai, Transform t )
	{
	
		vp_FPPlayerEventHandler player = null;
	
		// if the AI is not registered, return
		if(!Instance.AIs.ContainsKey(ai))
			return null;
	
		// if a dictionary hasn't been created for some reason, create it and then add the player
		Dictionary<Transform, vp_FPPlayerEventHandler> dict = null;
		if(!Instance.AIs.TryGetValue(ai, out dict))
		{
			dict = new Dictionary<Transform, vp_FPPlayerEventHandler>();
			goto Add;
		}
			
		// if the player doesn't exist, add it
		if(!dict.TryGetValue(t, out player))
			goto Add;
			
		// if player was found, return the properties
		return player;
			
		// add the player for this AI
		Add:
		return Instance.AddPlayerForAI( ai, t );
	
	}
	
}
