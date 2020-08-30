using UnityEditor;
using UnityEngine;
using System.Collections;

public class vp_MobileMenu : Editor
{
	
	[MenuItem("UFPS/Mobile/Manual", false, 121)]
	public static void Manual()
	{
		Application.OpenURL("http://www.visionpunk.com/hub/assets/ufpsmobile/manual");
	}

	[MenuItem("UFPS/Mobile/Create/UI Root", false, 123)]
	static void CreateUIRoot()
	{
	
		GameObject rootGO = new GameObject("UIRoot");
		vp_UIManager manager = rootGO.AddComponent<vp_UIManager>();
		rootGO.layer = manager.UILayer;
		
		GameObject cameraGO = new GameObject("UICamera");
		cameraGO.transform.parent = rootGO.transform;
		cameraGO.layer = rootGO.layer;
		Camera camera = cameraGO.AddComponent<Camera>();
		camera.cullingMask = 1 << manager.UILayer;
		camera.orthographic = true;
		camera.orthographicSize = 1;
		camera.nearClipPlane = -10;
		camera.farClipPlane = 10;
		
		cameraGO.AddComponent<vp_UICamera>();
		
		Undo.RegisterCreatedObjectUndo(rootGO, "Create UI Root");

		Selection.activeGameObject = rootGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/UI Anchor", true)]
	static bool ValidateAddUIAnchor(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/UI Anchor", false, 123)]
	static void AddUIAnchor()
	{
	
		GameObject anchor = new GameObject("Anchor (Middle)", typeof(vp_UIAnchor));
		anchor.MakeChild();

		Undo.RegisterCreatedObjectUndo(anchor, "Create UI Anchor");

		Selection.activeGameObject = anchor;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/Touch Controller/Dynamic Joystick", true)]
	static bool ValidateAddTouchControllerDynamic(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/Touch Controller/Dynamic Joystick", false, 125)]
	static void AddTouchControllerDynamic(){ AddTouchController(vp_UITouchController.vp_TouchControllerType.DynamicJoystick, "Dynamic Joystick"); }
	
	[MenuItem("UFPS/Mobile/Create/Controls/Touch Controller/Static Joystick", true)]
	static bool ValidateAddTouchControllerStatic(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/Touch Controller/Static Joystick", false, 125)]
	static void AddTouchControllerStatic(){ AddTouchController(vp_UITouchController.vp_TouchControllerType.StaticJoystick, "Static Joystick"); }
	
	[MenuItem("UFPS/Mobile/Create/Controls/Touch Controller/Touch Pad", true)]
	static bool ValidateAddTouchControllerTouchPad(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/Touch Controller/Touch Pad", false, 125)]
	static void AddTouchControllerTouchPad(){ AddTouchController(vp_UITouchController.vp_TouchControllerType.TouchPad, "Touch Pad"); }
	
	static void AddTouchController( vp_UITouchController.vp_TouchControllerType type, string name )
	{
			
		GameObject controllerGO = new GameObject("Controller ("+name+")");
		controllerGO.MakeChild();


		GameObject panel = new GameObject("Panel");
		panel.MakeChild(controllerGO.transform);
		
		GameObject knob = new GameObject("Knob", typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider), typeof(ParticleSystem));
		knob.MakeChild(panel.transform);
		SetupRenderer(knob);
		
		GameObject background = new GameObject("Background", typeof(MeshFilter), typeof(MeshRenderer));
		background.MakeChild(panel.transform);
		SetupRenderer(background);
		
		vp_UITouchController controller = controllerGO.AddComponent<vp_UITouchController>();
		controller.ControllerType = type;
		controller.Knob = knob.transform;
		controller.Background = background.transform;

		Undo.RegisterCreatedObjectUndo(controllerGO, "Create UI Controller");

		Selection.activeGameObject = controllerGO;


	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/Weapon Switcher", true)]
	static bool ValidateAddWeaponSwitcher(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/Weapon Switcher", false, 125)]
	static void AddWeaponSwitcher()
	{
	
		GameObject switcherGO = new GameObject("Weapon Switcher");
		switcherGO.MakeChild();
		
		GameObject background = new GameObject("Background", typeof(MeshFilter), typeof(MeshRenderer));
		background.MakeChild(switcherGO.transform);
		SetupRenderer(background);
		
		GameObject scroller = new GameObject("WeaponScroller");
		scroller.MakeChild(switcherGO.transform);
		
		// get player fp camera
		vp_FPCamera fpCamera = FindObjectOfType(typeof(vp_FPCamera)) as vp_FPCamera;
		if(fpCamera)
		{
			foreach(Transform t in fpCamera.transform)
			{
				if(t.GetComponent<Camera>())
					continue;
					
				GameObject go = new GameObject(t.name, typeof(MeshFilter), typeof(MeshRenderer));
				go.MakeChild(scroller.transform);
				go.transform.position = new Vector3(0, 0, -5);
				SetupRenderer(go);
			}
		}
		
		vp_UITouchWeaponSwitcher switcher = switcherGO.AddComponent<vp_UITouchWeaponSwitcher>();
		switcher.WeaponScroller = scroller.transform;
		
		Undo.RegisterCreatedObjectUndo(switcherGO, "Create Weapon Switcher");

		Selection.activeGameObject = switcherGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/Touch Button", true)]
	static bool ValidateAddTouchButton(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/Touch Button", false, 125)]
	static void AddTouchButton()
	{
	
		GameObject buttonGO = new GameObject("Button", typeof(MeshFilter), typeof(MeshRenderer), typeof(vp_UITouchButton));
		buttonGO.MakeChild();
		SetupRenderer(buttonGO);

		Undo.RegisterCreatedObjectUndo(buttonGO, "Create Touch Button");

		Selection.activeGameObject = buttonGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/Touch Look Pad", true)]
	static bool ValidateAddTouchLookPad(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/Touch Look Pad", false, 125)]
	static void AddTouchLookPad()
	{
	
		GameObject lookPadGO = new GameObject("LookPad", typeof(vp_UITouchLook));
		lookPadGO.MakeChild();

		Undo.RegisterCreatedObjectUndo(lookPadGO, "Create Touch Look Pad");

		Selection.activeGameObject = lookPadGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/Crosshair", true)]
	static bool ValidateAddCrosshair(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/Crosshair", false, 125)]
	static void AddCrosshair()
	{
	
		GameObject crosshairGO = new GameObject("Crosshair", typeof(MeshFilter), typeof(MeshRenderer), typeof(vp_UICrosshair));
		crosshairGO.MakeChild();
		SetupRenderer(crosshairGO);

		Undo.RegisterCreatedObjectUndo(crosshairGO, "Create Crosshair");

		Selection.activeGameObject = crosshairGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/Toggle", true)]
	static bool ValidateAddToggle(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/Toggle", false, 125)]
	static void AddToggle()
	{
	
		GameObject toggleGO = new GameObject("Toggle", typeof(MeshFilter), typeof(MeshRenderer), typeof(vp_UIToggle));
		toggleGO.MakeChild();
		
		GameObject background = new GameObject("Background", typeof(MeshFilter), typeof(MeshRenderer));
		SetupRenderer(background);
		background.transform.parent = toggleGO.transform;
		
		GameObject checkmark = new GameObject("Checkmark", typeof(MeshFilter), typeof(MeshRenderer));
		SetupRenderer(checkmark);
		checkmark.transform.parent = toggleGO.transform;
		
		vp_UIToggle toggle = toggleGO.GetComponent<vp_UIToggle>();
		toggle.Background = background;
		toggle.Checkmark = checkmark;
		
		Undo.RegisterCreatedObjectUndo(toggleGO, "Create Toggle");

		Selection.activeGameObject = toggleGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/Dropdown List", true)]
	static bool ValidateAddDropdown(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/Dropdown List", false, 125)]
	static void AddDropdown()
	{
	
		GameObject dropdownGO = new GameObject("Dropdown List", typeof(MeshFilter), typeof(MeshRenderer), typeof(vp_UIDropdownList));
		dropdownGO.MakeChild();
		
		GameObject background = new GameObject("Background", typeof(MeshFilter), typeof(MeshRenderer));
		SetupRenderer(background);
		background.transform.parent = dropdownGO.transform;
		background.GetComponent<Renderer>().sharedMaterial.color = Color.black;
		background.transform.localScale = new Vector2(1, .25f);
		
		GameObject label = new GameObject("Label", typeof(TextMesh));
		label.transform.parent = dropdownGO.transform;
		label.transform.localScale = new Vector2(.1f, .1f);
		label.transform.localPosition = new Vector3(-.45f, .075f, -.1f);
		
		vp_UIDropdownList dropdown = dropdownGO.GetComponent<vp_UIDropdownList>();
		dropdown.Background = background.transform;
		dropdown.Label = label.GetComponent<TextMesh>();
		dropdown.Label.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
		dropdown.Label.text = "New Item 1";
		label.GetComponent<Renderer>().sharedMaterial = dropdown.Label.font.material;
		
		Undo.RegisterCreatedObjectUndo(dropdownGO, "Create Toggle");

		Selection.activeGameObject = dropdownGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Attach/UI Sway", true)]
	static bool ValidateAttachSway(){ return CheckIsValid<vp_UISway>(); }
	[MenuItem("UFPS/Mobile/Attach/UI Sway", false, 126)]
	static void AttachSway()
	{
	
		Selection.activeGameObject.AddComponent<vp_UISway>();
	
	}
	
	
	[MenuItem("UFPS/Mobile/Attach/Collider", true)]
	static bool ValidateAttachCollider(){ return CheckIsValid<BoxCollider>() && Selection.activeGameObject.GetComponent<vp_UIManager>() == null; }
	[MenuItem("UFPS/Mobile/Attach/Collider", false, 126)]
	static void AttachCollider()
	{
	
		Selection.activeGameObject.AddComponent<vp_UISway>();
	
	}
	
	
	[MenuItem("UFPS/Mobile/Attach/Anchor", true)]
	static bool ValidateAttachAnchor(){ return CheckIsValid<vp_UIAnchor>() && Selection.activeGameObject.GetComponent<vp_UIManager>() == null; }
	[MenuItem("UFPS/Mobile/Attach/Anchor", false, 126)]
	static void AttachAnchor()
	{
	
		Selection.activeGameObject.AddComponent<vp_UIAnchor>();
	
	}
	
	
	static void SetupRenderer( GameObject go )
	{
	
		if(go.GetComponent<MeshFilter>() != null)
			go.GetComponent<MeshFilter>().mesh = CreateMesh();
			
		if(go.GetComponent<Renderer>() != null)
		{
			go.GetComponent<Renderer>().material = new Material(Shader.Find("Sprites/Transparent Unlit"));
			go.GetComponent<Renderer>().castShadows = false;
			go.GetComponent<Renderer>().receiveShadows = false;
		}
	
	}
	
	
	static Mesh CreateMesh()
	{
	
		Vector2 anchorOffset = Vector2.zero;
		int widthSegments = 1;
	    int lengthSegments = 1;
	    float width = 1.0f;
	    float length = 1.0f;
		
		Mesh m = new Mesh();
		m.name = "Plane";
		
		int hCount2 = widthSegments+1;
		int vCount2 = lengthSegments+1;
		int numTriangles = widthSegments * lengthSegments * 6;
		int numVertices = hCount2 * vCount2;
		
		Vector3[] vertices = new Vector3[numVertices];
		Vector2[] uvs = new Vector2[numVertices];
		int[] triangles = new int[numTriangles];
		
		int index = 0;
		float uvFactorX = 1.0f/widthSegments;
		float uvFactorY = 1.0f/lengthSegments;
		float scaleX = width/widthSegments;
		float scaleY = length/lengthSegments;
		for (float y = 0.0f; y < vCount2; y++)
		{
		for (float x = 0.0f; x < hCount2; x++)
		{
		    vertices[index] = new Vector3(x*scaleX - width/2f - anchorOffset.x, y*scaleY - length/2f - anchorOffset.y, 0.0f);
		    uvs[index++] = new Vector2(x*uvFactorX, y*uvFactorY);
		}
		}
		
		index = 0;
		for (int y = 0; y < lengthSegments; y++)
		{
		for (int x = 0; x < widthSegments; x++)
		{
		    triangles[index]   = (y     * hCount2) + x;
		    triangles[index+1] = ((y+1) * hCount2) + x;
		    triangles[index+2] = (y     * hCount2) + x + 1;
		
		    triangles[index+3] = ((y+1) * hCount2) + x;
		    triangles[index+4] = ((y+1) * hCount2) + x + 1;
		    triangles[index+5] = (y     * hCount2) + x + 1;
		    index += 6;
		}
		}
		
		m.vertices = vertices;
		m.uv = uvs;
		m.triangles = triangles;
		m.RecalculateNormals();
		
		return m;
	
	}
	
	
	static bool CheckIsValid()
	{
	
		GameObject go = Selection.activeGameObject;
		return go != null && go.transform.root.GetComponent<vp_UIManager>() != null;
	
	}
	
	
	static bool CheckIsValid<T>() where T : Component
	{
	
		GameObject go = Selection.activeGameObject;
		return CheckIsValid() && go.GetComponent<T>() == null;
	
	}

}


public static class vp_MobileUtilities
{

	public static void MakeChild( this GameObject childObject )
	{
	
		childObject.MakeChild("", null);
	
	}
	

	public static void MakeChild( this GameObject childObject, string childName )
	{
	
		childObject.MakeChild(childName, null);
	
	}
	
	
	public static void MakeChild( this GameObject childObject, Transform parent )
	{
	
		childObject.MakeChild("", parent);
	
	}
	
	
	public static void MakeChild( this GameObject childObject, string childName, Transform parent )
	{
	
		childObject.transform.parent = parent ? parent : Selection.activeTransform;
		if(childName != "") childObject.name = childName;
		childObject.transform.localPosition = Vector3.zero;
		childObject.transform.localRotation = Quaternion.identity;
		childObject.transform.localScale = Vector3.one;
		childObject.layer = Selection.activeGameObject.layer;
	
	}
	
	
	[MenuItem("GameObject/Selection/Add New Child #&n")]
	static void CreateChildGameObject()
	{

		GameObject go = new GameObject("GameObject");

		if (Selection.activeTransform != null)
			go.MakeChild("Child");

		Undo.RegisterCreatedObjectUndo(go, "Add New Child");

		Selection.activeGameObject = go;

	}


	[MenuItem("UFPS/Mobile/Check for Updates", false, 127)]
	public static void CheckForUpdates()
	{
		vp_UpdateDialog.Create("ufpsmobile", UFPSMobileInfo.Version);
	}


}
