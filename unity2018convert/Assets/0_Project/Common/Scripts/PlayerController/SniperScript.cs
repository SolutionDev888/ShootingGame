using UnityEngine;
using System.Collections;

public class SniperScript : MonoBehaviour {
	
	//DefaultCrosshair
	public Sprite DefaultCrosshair = null;//Default Crosshair - use UFPS
	public Sprite SniperCrosshair=null;//Sniper Crosshair - make your own
	public float FieldOfViewSetting=9f;//Maxmium Zoom amount
	public float fovMultiplier=2f;
	
	
	protected vp_FPPlayerEventHandler m_Player = null;
	protected vp_SimpleCrosshair m_crosshair=null;
	protected vp_FPCamera m_camera=null;
	Camera myCamera=null;
	
	float DefaultFOV=60f;//Default FieldOfView that your weapon is set to
	float maxFOV=60f;//Maximum FieldOfView will equal the DefaultFOV. You may want to cut this in half
	public float tempFOV=0f;//Temporary holder for the current Field of View
	bool hideZoom=true;
	bool resetZoom=false;
	
	protected virtual void Awake()
	{
		
		m_Player = GameObject.FindObjectOfType(typeof(vp_FPPlayerEventHandler)) as vp_FPPlayerEventHandler; // cache the player event handler
		m_crosshair = GameObject.FindObjectOfType(typeof(vp_SimpleCrosshair)) as vp_SimpleCrosshair; // cache the Simple Crosshair
		m_camera = GameObject.FindObjectOfType(typeof(vp_FPCamera)) as vp_FPCamera; // cache the Simple Crosshair
		myCamera=m_camera.GetComponent<Camera>();
		DefaultFOV=m_camera.RenderingFieldOfView;
		maxFOV=DefaultFOV;
		hideZoom=m_crosshair.HideOnFirstPersonZoom;
		
	}
	
	void OnGUI ()
	{
		if(m_Player.Zoom.Active)
		{
			m_crosshair.HideOnFirstPersonZoom=resetZoom;
			m_Player.Crosshair.Set(SniperCrosshair);
			tempFOV+=Input.GetAxis("Mouse ScrollWheel")*-fovMultiplier;//Assign temp zoom times -2. Scroll forward zoom in. Raise or lower -2 for quicker zooming
			//Handle the Zooming using the Mouse ScrollWheel.
			
			if(tempFOV>=maxFOV)//IF we have reached the Maximum, then don't zoom out anymore. Might want to cut it in half here
				tempFOV=maxFOV;
			else if(tempFOV<=FieldOfViewSetting)//If we reached closest zoom level, then stop zooming in
				tempFOV=FieldOfViewSetting;
			m_camera.RenderingFieldOfView=tempFOV;//Apply our Zoom amount
			myCamera.fieldOfView=tempFOV;
			
		}
		else
		{
			m_crosshair.HideOnFirstPersonZoom=hideZoom;
			m_crosshair.m_ImageCrosshair=DefaultCrosshair;
			m_camera.RenderingFieldOfView=DefaultFOV;
		}
	}
}