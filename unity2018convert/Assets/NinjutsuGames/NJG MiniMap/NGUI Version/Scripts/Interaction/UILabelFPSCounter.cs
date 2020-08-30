using UnityEngine;
using System.Collections;

// Attach this to a UILabel to make a frames/second indicator.
//
// It calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
//
// It is also fairly accurate at very low FPS counts (<10).
// We do this not by simply counting frames per interval, but
// by accumulating FPS for each frame. This way we end up with
// correct overall FPS even if the interval renders something like
// 5.5 frames.

[AddComponentMenu("NJG MiniMap/NGUI/Interaction/Label FPS Counter")]
[RequireComponent(typeof(UILabel))]
public class UILabelFPSCounter : MonoBehaviour {
	
	public float updateInterval = 1.0f;
	private UILabel label;
	
	private float accum = 0.0f; // FPS accumulated over the interval
	private float frames = 0f; // Frames drawn over the interval
	private float timeleft; // Left time for current interval
	private float fps = 15.0f; // Current FPS
	private double lastSample;
	private float gotIntervals = 0f;	
		
	// Use this for initialization
	void Start () {
		label = GetComponent<UILabel>();
		timeleft = updateInterval;
    	lastSample = Time.realtimeSinceStartup;
	}
	
	public float GetFPS(){ return fps; }
	public bool HasFPS(){ return gotIntervals > 2; }
	
	void Update () {
		++frames;
	    float newSample = Time.realtimeSinceStartup;
	    float deltaTime = (float)(newSample - lastSample);
	    lastSample = newSample;
	
	    timeleft -= deltaTime;
	    accum += (float)(1.0f / deltaTime);
	    
	    // Interval ended - update GUI text and start new interval
	    if( timeleft <= 0.0f )
	    {
	        // display two fractional digits (f2 format)
	        fps = accum/frames;
			if(label) label.text = fps.ToString("f2");
	        timeleft = updateInterval;
	        accum = 0.0f;
	        frames = 0;
	        ++gotIntervals;
	        
			if(label){
		        if( fps < 30 )
				{
			        label.color = Color.yellow;
				}
			    else 
				{
			        if( fps < 10 )
			            label.color = Color.red;
			        else
			            label.color = Color.green;
			    }
			}
	    }
	}
}

