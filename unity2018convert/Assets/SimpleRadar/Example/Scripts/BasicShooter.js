public var Crosshair:Texture;
public var DestroyObjectTag:String = "Enemy";

function Start () {

}

function OnGUI(){
	if(Crosshair){
		GUI.DrawTexture(Rect((Screen.width/2) - (Crosshair.width/2),(Screen.height/2)-(Crosshair.height/2),Crosshair.width,Crosshair.height),Crosshair);
	}
}

function Update () {
	if(Input.GetMouseButtonDown(0)){
		Shoot();
	}
}

function Shoot() {
	Screen.lockCursor = true;
	var posray:Vector2 = new Vector2(Screen.width/2,Screen.height/2);
 	var ray = Camera.main.ScreenPointToRay (posray);
    var hit : RaycastHit;
    
    if (Physics.Raycast (ray, hit, 100.0)) {
    	if(hit.collider.tag == DestroyObjectTag){
       	 	GameObject.Destroy(hit.collider.gameObject);
        }
    }
}