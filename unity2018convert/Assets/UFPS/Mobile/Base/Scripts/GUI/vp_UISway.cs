/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UISway.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	sways specified objects based on player camera rotation
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vp_UISway : MonoBehaviour
{

	public bool BobStep = true;
	public float SwaySpeed = 10;										// speed at which objects sway
	public float SwayAmount = .5f;										// the amount objects are swayed
	public List<Transform> ObjectsToSway = new List<Transform>();		// list of objects to apply swaying logic to
	
	protected List<Vector3> m_ObjectPositions = new List<Vector3>();	// cached list of swayable objects position's
	protected List<Vector3> m_ObjectScales = new List<Vector3>();		// cached list of swayable objects position's
	protected vp_FPCamera m_Camera = null;								// cached fpcamera
	protected vp_UIManager m_Manager = null;							// cached uimanager
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void Awake()
	{
	
		m_Manager = transform.root.GetComponentInChildren<vp_UIManager>();
		m_Camera = m_Manager.Player.GetComponentInChildren<vp_FPCamera>();
	
		SetObjectPositions();
	
	}
	
	
	/// <summary>
	/// adds the swayable objects position's to a list
	/// </summary>
	protected virtual void SetObjectPositions()
	{
		
		m_ObjectPositions.Clear();
	
		for(int i=0;i<ObjectsToSway.Count;i++)
		{
			m_ObjectPositions.Add( ObjectsToSway[i].localPosition );
			m_ObjectScales.Add( ObjectsToSway[i].localScale );
		}
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void OnEnable()
	{
	
		if(m_Camera != null)
			m_Camera.BobStepCallback += Bob;
	
		vp_GlobalEvent.Register("Update UI Positions", SetObjectPositions);
	
	}
	
	
	/// <summary>
	/// 
	/// </summary>
	protected virtual void OnDisable()
	{
	
		if(m_Camera != null)
			m_Camera.BobStepCallback -= Bob;
	
		vp_GlobalEvent.Unregister("Update UI Positions", SetObjectPositions);
	
	}
	
	
	/// <summary>
	/// updates when an event is sent from vp_FPCamera's
	/// BobStepCallback
	/// </summary>
	protected virtual void Bob()
	{
	
		if(m_Manager == null || !BobStep)
			return;
	
		float y = m_Manager.Player.Run.Active ? 1 : m_Manager.Player.InputMoveVector.Get().y;
	
		
		for(int i=0;i<ObjectsToSway.Count;i++)
			ObjectsToSway[i].localPosition = Vector3.Slerp(new Vector3(ObjectsToSway[i].localPosition.x - (m_Manager.Player.InputMoveVector.Get().x * Time.deltaTime) * SwayAmount, ObjectsToSway[i].localPosition.y - (y * Time.deltaTime) * SwayAmount, m_ObjectPositions[i].z), m_ObjectPositions[i], Time.deltaTime * SwaySpeed );
	
	}
	
	
	/// <summary>
	/// handles movement of swayable objects
	/// </summary>
	protected virtual void Update()
	{
		
		if(vp_TimeUtility.Paused)
			return;
	
		for(int i=0;i<ObjectsToSway.Count;i++)
		{
			Vector3 newPosition = ObjectsToSway[i].localPosition;
			newPosition.x = Mathf.Lerp( newPosition.x - (vp_Input.GetAxisRaw("Mouse X") * Time.deltaTime) * SwayAmount, m_ObjectPositions[i].x, Time.deltaTime * SwaySpeed  );
			newPosition.y = Mathf.Lerp( newPosition.y - (vp_Input.GetAxisRaw("Mouse Y") * Time.deltaTime) * SwayAmount, m_ObjectPositions[i].y, Time.deltaTime * SwaySpeed );
			
			if(m_Manager == null)
				continue;
			
			newPosition.x = Mathf.Lerp( newPosition.x - (m_Manager.Player.InputMoveVector.Get().x * Time.deltaTime) * SwayAmount, m_ObjectPositions[i].x, Time.deltaTime * SwaySpeed );
			ObjectsToSway[i].localPosition = newPosition;
			
			float vertical = (m_Manager.Player.Run.Active ? 2 : m_Manager.Player.InputMoveVector.Get().y) * .25f;
			float size = Mathf.Lerp( ObjectsToSway[i].localScale.x + (vertical * Time.deltaTime), m_ObjectScales[i].x, Time.deltaTime * SwaySpeed );
			Vector3 newScale = new Vector3(size, size, size);
			
			ObjectsToSway[i].localScale = newScale;
		}
	
	}
	
}
