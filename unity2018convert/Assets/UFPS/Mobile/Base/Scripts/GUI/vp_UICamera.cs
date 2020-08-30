using UnityEngine;

[RequireComponent( typeof( Camera ) )]
public class vp_UICamera : MonoBehaviour
{

	public void Awake()
	{

		GetComponent<Camera>().transparencySortMode = TransparencySortMode.Orthographic;

	}
	
	
	public void OnEnable(){}

}
