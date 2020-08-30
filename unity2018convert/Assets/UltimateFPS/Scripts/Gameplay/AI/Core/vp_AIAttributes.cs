/////////////////////////////////////////////////////////////////////////////////
//
//	vp_AIAttributes.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	this file contains many different attributes that can be used
//					to bring variables over to the editor for AI Plugins without
//					the need for editor scripting. Attributes can be added to a
//					field in a normal class like below
//					
//					[vp_AIField("Title", "Tooltip")]
//					public bool SampleBool = false;
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;


/// <summary>
/// This is the main class that all other AI
/// attributes inherit from. Valid types are
/// int, float, bool, string, Vector2, Vector3,
/// Vector4, Enum, LayerMask and any UnityEngine.Object
/// </summary>
[AttributeUsage( AttributeTargets.Field )]
public class vp_AIField : Attribute
{

	public string Title{ get; set; }	// The text that will be shown next to the field in the editor
	public string Tooltip{ get; set; }	// A tooltip that will show when the Title is hovered with the mouse
	public bool Slider{ get; set; }		// Whether or not this field will be a slider, if not, just an input box will show. Only applies to ints and floats.
	public float MinValue{ get; set; }	// Min value for int or float field. Only used when Slider is true.
	public float MaxValue{ get; set; }	// Max value for int or float field. Only used when Slider is true.
	
	/// <summary>
	/// Creates a normal input field based on the
	/// fields type
	/// </summary>
	public vp_AIField(){}
	
	/// <summary>
	/// Creates a normal input field with a
	/// title based on the fields type
	/// </summary>
	public vp_AIField( string title )
	{
		
		Title = title;
	
	}
	
	
	/// <summary>
	/// Creates a normal input field with a
	/// title and tooltip based on the fields type
	/// </summary>
	public vp_AIField( string title, string tooltip )
	{
		
		Title = title;
		Tooltip = tooltip;
	
	}
	
	
	/// <summary>
	/// Creates a slider input field with a
	/// title based on the fields type. Used for ints and floats.
	/// </summary>
	public vp_AIField( string title, float min, float max )
	{
		
		Title = title;
		MinValue = min;
		MaxValue = max;
		Slider = true;
	
	}
	
	
	/// <summary>
	/// Creates a slider input field with a
	/// title and tooltip based on the fields type. Used for ints and floats.
	/// </summary>
	public vp_AIField( string title, string tooltip, float min, float max )
	{
		
		Title = title;
		Tooltip = tooltip;
		MinValue = min;
		MaxValue = max;
		Slider = true;
	
	}
    
}


/// <summary>
/// This will start a foldout and should
/// be applied to a bool that will hold
/// the foldouts value
/// </summary>
[AttributeUsage( AttributeTargets.Field )]
public class vp_AIBeginFoldout : vp_AIField{
	
	/// <summary>
	/// This will start a foldout and should
	/// be applied to a bool that will hold
	/// the foldouts value. Accepts a value to
	/// display a title
	/// </summary>
	public vp_AIBeginFoldout( string title )
	{
		
		Title = title;
	
	}
	
}


/// <summary>
/// This ends a foldout and can be added
/// to a unused or used field
/// </summary>
[AttributeUsage( AttributeTargets.Field )]
public class vp_AIEndFoldout : vp_AIField{}


/// <summary>
/// This allows for executing custom code
/// in the editor. The MethodName field will
/// be used to look for a method in vp_AIEditor,
/// an editor class with a name such as
/// vp_AI{plugin_name}Editor or the class provided
/// as the ClassName field
/// </summary>
[AttributeUsage( AttributeTargets.Field )]
public class vp_AICustomMethod : vp_AIField{

	public string ClassName = "";
	public string MethodName = "";
	
	/// <summary>
	/// This allows for executing custom code
	/// in the editor. The MethodName field will
	/// be used to look for a method in vp_AIEditor,
	/// an editor class with a name such as
	/// vp_AI{plugin_name}Editor or the class provided
	/// as the ClassName field
	/// </summary>
	public vp_AICustomMethod( string methodName, string className = "" )
	{
	
		ClassName = className;
		MethodName = methodName;
	
	}

}


/// <summary>
/// This will display a range slider with inputs on
/// the left and right
/// </summary>
[AttributeUsage( AttributeTargets.Field )]
public class vp_AIRangeField : vp_AIField
{

	/// <summary>
	/// This will display a range slider with inputs on
	/// the left and right
	/// </summary>
	public vp_AIRangeField( string title, float min, float max )
	{
		
		Title = title;
		MinValue = min;
		MaxValue = max;
	
	}
	
	/// <summary>
	/// This will display a range slider with inputs on
	/// the left and right
	/// </summary>
	public vp_AIRangeField( string title, string tooltip, float min, float max )
	{
		
		Title = title;
		Tooltip = tooltip;
		MinValue = min;
		MaxValue = max;
	
	}

}


/// <summary>
/// This shows a popup with vp_AIStates that have been
/// enabled for animation
/// </summary>
[AttributeUsage( AttributeTargets.Field )]
public class vp_AIAnimationStateField : vp_AIField
{

	/// <summary>
	/// This shows a popup with a title
	/// for vp_AIStates that have been enabled for animation
	/// </summary>
	public vp_AIAnimationStateField( string title )
	{
		
		Title = title;
	
	}
	
	/// <summary>
	/// This shows a popup with a title and tooltip
	/// for vp_AIStates that have been enabled for animation
	/// </summary>
	public vp_AIAnimationStateField( string title, string tooltip )
	{
		
		Title = title;
		Tooltip = tooltip;
	
	}

}


/// <summary>
/// This shows a popup with vp_AIStates that have been
/// enabled for audio
/// </summary>
[AttributeUsage( AttributeTargets.Field )]
public class vp_AIAudioStateField : vp_AIField
{

	/// <summary>
	/// This shows a popup with a title
	/// for vp_AIStates that have been enabled for audio
	/// </summary>
	public vp_AIAudioStateField( string title )
	{
		
		Title = title;
	
	}

	/// <summary>
	/// This shows a popup with a title and tooltip
	/// for vp_AIStates that have been enabled for audio
	/// </summary>
	public vp_AIAudioStateField( string title, string tooltip )
	{
		
		Title = title;
		Tooltip = tooltip;
	
	}

}