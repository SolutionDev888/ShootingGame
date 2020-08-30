/////////////////////////////////////////////////////////////////////////////////
//
//	vp_UIDropdownList.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	This control creates a basic dropdown list from each line in
//					the Items string
//					
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class vp_UIDropdownList : vp_UIControl
{

	public string Items = "New Item 1\nNew Item 2\nNew Item 3";	// list of items on separate lines
	public Transform Background = null;							// background transform
	public TextMesh Label = null;								// label of the dropdown
	public Texture ItemBackground = null;						// background texture to use for each item
	public float ItemHeight = 20;								// the height of each item in the list
	public Vector2 ItemLabelPadding = new Vector2(5, 5);		// left and top padding for the label
	public Font ItemFont = null;								// font that'll get applied to items in the list
	public string CurrentItem = "";								// the currently selected item
	
	protected Dictionary<string, GameObject> m_Items = new Dictionary<string, GameObject>(); // list of items in the list and their GameObject
	protected GameObject ListContainer = null;					// the list container
	protected bool m_ListShown = true;							// whether or not the list is visible
	public bool ListShown{										// setting this will set the visibility of the list
		get{ return m_ListShown; }
		set{
			m_ListShown = value;
			vp_Utility.Activate(ListContainer, m_ListShown);
		}
	}
	

	
	/// <summary>
	/// 
	/// </summary>
	protected override void Awake()
	{
	
		base.Awake();
	
		// get a string array of items by separating the Items string by lines
		string[] items = Items.Split(new string[1]{ "\n" }, System.StringSplitOptions.None);
		
		// set dropdown label to the first item in the list
		if(items.Length > 0)
		{
			CurrentItem = items[0];
			Label.text = CurrentItem;
		}
		
		// create the list container object
		ListContainer = new GameObject("List");
		ListContainer.transform.parent = transform;
		ListContainer.transform.localScale = Vector3.one;
		ListContainer.transform.localPosition = Vector3.zero;
		
		// create all items from the Items list
		for(int i=0; i<items.Length; i++)
			CreateListItem(items[i], i).parent = ListContainer.transform;
			
		// position the list container
		ListContainer.transform.localPosition = new Vector3(Background.localPosition.x, Background.localPosition.y - ((Background.localScale.y * .5f) + (ItemHeight * .5f)), 0);
			
		// hide the list container
		ListShown = false;
	
	}
	
	
	/// <summary>
	/// When the control is pressed, show the list
	/// and set the colors for each item
	/// </summary>
	protected virtual void OnPressControl()
	{
	
		ListShown = !ListShown;
		if(ListShown)
			foreach(KeyValuePair<string, GameObject> pair in m_Items)
				if(pair.Key != CurrentItem)
					pair.Value.GetComponent<Renderer>().material.color = new Color(.9f, .9f, .9f, 1);
				else
					pair.Value.GetComponent<Renderer>().material.color = new Color(.8f, .8f, .8f, 1);
	
	}
	
	
	/// <summary>
	/// creates an item for the dropdown list
	/// </summary>
	protected virtual Transform CreateListItem( string itemName, int i )
	{
	
		// Create the item container
		GameObject item = new GameObject(itemName);
		item.layer = Background.gameObject.layer;
		item.transform.parent = transform;
		item.transform.localPosition = Vector3.zero;
		item.transform.localScale = Vector3.one;
		
		// create the background of the item
		GameObject background = new GameObject("Background");
		background.layer = Background.gameObject.layer;
		background.transform.parent = item.transform;
		MeshRenderer rend = background.AddComponent<MeshRenderer>();
		MeshFilter filter = background.AddComponent<MeshFilter>();
		filter.mesh = Background.GetComponent<MeshFilter>().mesh;
		rend.material.mainTexture = ItemBackground;
		rend.material.shader = Background.GetComponent<MeshRenderer>().material.shader;
		rend.receiveShadows = false;
		rend.castShadows = false;
		rend.material.color = Color.white;
		background.transform.localScale = new Vector3(Background.localScale.x, ItemHeight, 1);
		background.transform.localPosition = new Vector3(Background.localPosition.x, -(background.transform.localScale.y * i), Background.localPosition.z - 20);
		
		// creates the label for the item
		GameObject textgo = new GameObject("Label");
		textgo.layer = Background.gameObject.layer;
		textgo.transform.parent = item.transform;
		TextMesh text = textgo.AddComponent<TextMesh>();
		text.GetComponent<Renderer>().material = ItemFont.material;
		text.text = itemName;
		text.font = ItemFont;
		text.fontSize = 32;
		text.color = Color.black;
		text.anchor = TextAnchor.MiddleLeft;
		text.transform.localScale = Label.transform.localScale * .75f;
		text.transform.localPosition = new Vector3(
												(background.transform.localPosition.x - background.transform.localScale.x * .5f) + ItemLabelPadding.x,
												(background.transform.localPosition.y + text.transform.localScale.y * .5f) - ItemLabelPadding.y,
												Background.localPosition.z - 21
											);
				
		// add a box collider so the item can handle touch events							
		BoxCollider col = item.AddComponent<BoxCollider>();
		col.size = new Vector3(background.transform.localScale.x, background.transform.localScale.y, 1);
		col.center = background.transform.localPosition;
		
		// add a uicontrol so the item can register with the ReleaseControl event
		vp_UIControl control = item.AddComponent<vp_UIControl>();
		control.ReleaseControl += () => {
			SelectItem(itemName);
		};
		
		m_Items.Add(itemName, background);
		
		return item.transform;
	
	}
	
	
	/// <summary>
	/// When an item is selected from the dropdown
	/// </summary>
	protected virtual void SelectItem( string item )
	{
	
		Label.text = item;
		CurrentItem = item;
		if(ChangeControl != null)
			ChangeControl(this);
		ListShown = false;
	
	}
	
}
