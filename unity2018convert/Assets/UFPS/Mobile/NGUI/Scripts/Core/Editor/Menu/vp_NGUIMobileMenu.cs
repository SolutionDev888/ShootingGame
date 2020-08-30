using UnityEditor;
using UnityEngine;
using System.Collections;

public class vp_NGUIMobileMenu : Editor
{

	[MenuItem("UFPS/Mobile/Create/NGUI UI Root", false, 123)]
	static void CreateUIRoot()
	{
	
		GameObject rootGO = new GameObject("UIRoot");
		vp_UIManager manager = rootGO.AddComponent<vp_NGUIManager>();
		rootGO.layer = manager.UILayer;
		rootGO.AddComponent<UIRoot>();
		
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
		
		UICamera nguiCamera = cameraGO.AddComponent<UICamera>();
		nguiCamera.eventReceiverMask = 1 << manager.UILayer;

		Undo.RegisterCreatedObjectUndo(rootGO, "Create UI Root");

		Selection.activeGameObject = rootGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Touch Controller/Dynamic Joystick", true)]
	static bool ValidateAddTouchControllerDynamic(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Touch Controller/Dynamic Joystick", false, 130)]
	static void AddTouchControllerDynamic(){ AddTouchController(vp_UITouchController.vp_TouchControllerType.DynamicJoystick, "Dynamic Joystick"); }
	
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Touch Controller/Static Joystick", true)]
	static bool ValidateAddTouchControllerStatic(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Touch Controller/Static Joystick", false, 130)]
	static void AddTouchControllerStatic(){ AddTouchController(vp_UITouchController.vp_TouchControllerType.StaticJoystick, "Static Joystick"); }
	
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Touch Controller/Touch Pad", true)]
	static bool ValidateAddTouchControllerTouchPad(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Touch Controller/Touch Pad", false, 130)]
	static void AddTouchControllerTouchPad(){ AddTouchController(vp_UITouchController.vp_TouchControllerType.TouchPad, "Touch Pad"); }
	
	static void AddTouchController( vp_UITouchController.vp_TouchControllerType type, string name )
	{

		GameObject controllerGO = new GameObject("Controller ("+name+")");
		controllerGO.MakeChild();
		
		GameObject panel = new GameObject("Panel");
		panel.AddComponent<UIPanel>();
		panel.MakeChild(controllerGO.transform);
		
		GameObject knob = new GameObject("Knob", typeof(UISprite), typeof(BoxCollider), typeof(ParticleSystem));
		knob.MakeChild(panel.transform);
		
		GameObject background = new GameObject("Background", typeof(UISprite));
		background.MakeChild(panel.transform);
		
		vp_UITouchController controller = controllerGO.AddComponent<vp_NGUITouchController>();
		controller.ControllerType = type;
		controller.Knob = knob.transform;
		controller.Background = background.transform;

		Undo.RegisterCreatedObjectUndo(controllerGO, "Create UI Controller");

		Selection.activeGameObject = controllerGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Weapon Switcher", true)]
	static bool ValidateAddWeaponSwitcher(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Weapon Switcher", false, 130)]
	static void AddWeaponSwitcher()
	{

		GameObject switcherGO = new GameObject("Weapon Switcher");
		switcherGO.MakeChild();
		
		GameObject background = new GameObject("Background", typeof(UISprite));
		background.MakeChild(switcherGO.transform);
		
		GameObject panel = new GameObject("Panel", typeof(UIPanel));
		panel.MakeChild(switcherGO.transform);
		
		GameObject scroller = new GameObject("WeaponScroller");
		scroller.MakeChild(panel.transform);
		
		// get player fp camera
		vp_FPCamera fpCamera = FindObjectOfType(typeof(vp_FPCamera)) as vp_FPCamera;
		if(fpCamera)
		{
			foreach(Transform t in fpCamera.transform)
			{
				if(t.GetComponent<Camera>())
					continue;
					
				GameObject go = new GameObject(t.name, typeof(UISprite));
				go.MakeChild(scroller.transform);
			}
		}
		
		vp_UITouchWeaponSwitcher switcher = switcherGO.AddComponent<vp_UITouchWeaponSwitcher>();
		switcher.WeaponScroller = scroller.transform;

		Undo.RegisterCreatedObjectUndo(switcherGO, "Create Weapon Switcher");

		Selection.activeGameObject = switcherGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Touch Button", true)]
	static bool ValidateAddTouchButton(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Touch Button", false, 130)]
	static void AddTouchButton()
	{

		GameObject buttonGO = new GameObject("Button", typeof(UISprite), typeof(vp_UITouchButton));
		buttonGO.MakeChild();

		Undo.RegisterCreatedObjectUndo(buttonGO, "Create Touch Button");

		Selection.activeGameObject = buttonGO;
	
	}
	
	
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Crosshair", true)]
	static bool ValidateAddCrosshair(){ return CheckIsValid(); }
	[MenuItem("UFPS/Mobile/Create/Controls/NGUI/Crosshair", false, 130)]
	static void AddCrosshair()
	{

		GameObject crosshairGO = new GameObject("Crosshair", typeof(UISprite), typeof(vp_NGUICrosshair));
		crosshairGO.MakeChild();

		Undo.RegisterCreatedObjectUndo(crosshairGO, "Create Crosshair");

		Selection.activeGameObject = crosshairGO;
	
	}
	
	
	static bool CheckIsValid()
	{
	
		GameObject go = Selection.activeGameObject;
		return go != null && go.transform.root.GetComponent<vp_UIManager>() != null;
	
	}
	
}
